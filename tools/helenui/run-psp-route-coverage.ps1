[CmdletBinding()]
param(
    [switch]$PlanOnly,

    [string]$ServiceUrl = 'http://localhost:38406',

    [string]$PpssppProfilePath = 'C:\dev\helenui\ppsspp.json',

    [string]$OcrConfigPath = 'C:\dev\helenui\plugins\recognition-cli\recognition-config.sample.json',

    [int]$NavigationTimeoutMilliseconds = 30000,

    [int]$RetryLimit = 2,

    [int]$RequestTimeoutSeconds = 45
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$profilePath = Join-Path $projectRoot 'helenui\demodisc.json'
$reportDirectory = Join-Path $projectRoot 'output\psp'
$mainMenuSurfaceId = 'surface-demodisc-main-menu'

function Get-OptionalPropertyValue {
    param(
        [Parameter(Mandatory = $true)]
        [object]$InputObject,

        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    $property = $InputObject.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

function Test-PspSurface {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SurfaceId
    )

    return $SurfaceId -notmatch 'console|clear-results|time-up'
}

function Get-RequiredRoutePlan {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProfilePath
    )

    $profile = Get-Content -LiteralPath $ProfilePath -Raw | ConvertFrom-Json
    $routes = foreach ($surface in @($profile.surfaces)) {
        if (-not (Test-PspSurface -SurfaceId ([string]$surface.id))) {
            continue
        }

        foreach ($node in @($surface.uiNodes)) {
            if ([bool](Get-OptionalPropertyValue -InputObject $node -Name 'isOptional')) {
                continue
            }

            foreach ($interaction in @($node.interactions)) {
                $targetSurfaceId = [string](Get-OptionalPropertyValue -InputObject $interaction -Name 'targetSurfaceId')
                if ([string]::IsNullOrWhiteSpace($targetSurfaceId) -or -not (Test-PspSurface -SurfaceId $targetSurfaceId)) {
                    continue
                }

                [pscustomobject]@{
                    SourceSurfaceId = [string]$surface.id
                    SourceSurfaceName = [string]$surface.name
                    NodeId = [string]$node.id
                    InteractionId = [string]$interaction.id
                    TargetSurfaceId = $targetSurfaceId
                    TargetSurfaceName = [string]($profile.surfaces | Where-Object { $_.id -eq $targetSurfaceId } | Select-Object -First 1).name
                    ActivationCount = @($node.interactions | Where-Object { $_.kind -eq 'activate' }).Count
                }
            }
        }
    }

    return @($routes | Sort-Object SourceSurfaceId, NodeId, InteractionId)
}

function Invoke-HelenRequest {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Get', 'Post', 'Delete')]
        [string]$Method,

        [Parameter(Mandatory = $true)]
        [string]$Path,

        [object]$Body
    )

    $uri = "$($ServiceUrl.TrimEnd('/'))$Path"
    if ($PSBoundParameters.ContainsKey('Body')) {
        return Invoke-RestMethod -Method $Method -Uri $uri -TimeoutSec $RequestTimeoutSeconds -ContentType 'application/json' -Body ($Body | ConvertTo-Json -Depth 12)
    }

    return Invoke-RestMethod -Method $Method -Uri $uri -TimeoutSec $RequestTimeoutSeconds
}

function Get-RecognizedGameState {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SessionId
    )

    Invoke-HelenRequest -Method Post -Path "/sessions/$SessionId/recognize" | Out-Null
    $summary = Invoke-HelenRequest -Method Get -Path "/sessions/$SessionId"
    if ($null -ne $summary.lastError) {
        throw "HelenUI reported a session error: $($summary.lastError.message)"
    }

    $gameScope = @($summary.profileScopes) | Where-Object { $_.scopeId -eq 'game' } | Select-Object -First 1
    $gameScopeMatched = $null -ne $gameScope -and (([string]$gameScope.recognitionStatus -eq 'Matched') -or ([int]$gameScope.recognitionStatus -eq 0))
    if (-not $gameScopeMatched -or $null -eq $gameScope.recognizedState) {
        $scopeDiagnostic = @($summary.profileScopes | ForEach-Object {
            $recognizedState = Get-OptionalPropertyValue -InputObject $_ -Name 'recognizedState'
            $screen = if ($null -eq $recognizedState) { '' } else { [string](Get-OptionalPropertyValue -InputObject $recognizedState -Name 'currentScreenName') }
            $scopeError = Get-OptionalPropertyValue -InputObject $_ -Name 'error'
            $errorMessage = if ($null -eq $scopeError) { '' } else { [string](Get-OptionalPropertyValue -InputObject $scopeError -Name 'message') }
            "scope=$($_.scopeId); status=$($_.recognitionStatus); screen=$screen; error=$errorMessage"
        }) -join ' | '
        throw "HelenUI did not recognize the Demo Disc game scope. $scopeDiagnostic"
    }

    $recognizedSurfaceName = [string]$gameScope.recognizedState.currentScreenName
    $surfaceId = if ($surfaceIdsByName.ContainsKey($recognizedSurfaceName)) {
        [string]$surfaceIdsByName[$recognizedSurfaceName]
    }
    else {
        $recognizedSurfaceName
    }

    return [pscustomobject]@{
        Summary = $summary
        SurfaceId = $surfaceId
    }
}

function Wait-ForSurface {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SessionId,

        [Parameter(Mandatory = $true)]
        [string]$ExpectedSurfaceId
    )

    $watch = [System.Diagnostics.Stopwatch]::StartNew()
    $lastSurfaceId = $null
    $lastFailure = $null
    while ($watch.ElapsedMilliseconds -lt $NavigationTimeoutMilliseconds) {
        try {
            $state = Get-RecognizedGameState -SessionId $SessionId
            $lastSurfaceId = $state.SurfaceId
            if ($lastSurfaceId -eq $ExpectedSurfaceId) {
                return $state
            }
        }
        catch {
            $lastFailure = $_.Exception.Message
        }

        Start-Sleep -Milliseconds 750
    }

    $detail = if ($lastFailure) { $lastFailure } else { "last recognized surface '$lastSurfaceId'" }
    throw "Timed out waiting for '$ExpectedSurfaceId': $detail"
}

function Get-NavigationTarget {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Route
    )

    if ([int]$Route.ActivationCount -eq 1) {
        return [string]$Route.NodeId
    }

    return [string]$Route.TargetSurfaceId
}

function Invoke-Route {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SessionId,

        [Parameter(Mandatory = $true)]
        [object]$Route
    )

    Invoke-HelenRequest -Method Post -Path "/sessions/$SessionId/navigate" -Body @{
        scopeId = 'game'
        inputClass = 'gamepad'
        targetScreen = Get-NavigationTarget -Route $Route
        timeoutMs = $NavigationTimeoutMilliseconds
        retryLimit = $RetryLimit
    } | Out-Null

    return Wait-ForSurface -SessionId $SessionId -ExpectedSurfaceId ([string]$Route.TargetSurfaceId)
}

function Ensure-MainMenu {
    param([Parameter(Mandatory = $true)][string]$SessionId)
    $state = Get-RecognizedGameState -SessionId $SessionId
    if ($state.SurfaceId -ne $mainMenuSurfaceId) { throw 'PSP route coverage must start at the main menu. No recovery input was sent.' }
    return $state
}

function Get-NavigationPath {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$RoutePlan,

        [Parameter(Mandatory = $true)]
        [string]$SourceSurfaceId,

        [Parameter(Mandatory = $true)]
        [string]$TargetSurfaceId
    )

    if ($SourceSurfaceId -eq $TargetSurfaceId) {
        return @()
    }

    $queue = [System.Collections.Generic.Queue[object]]::new()
    $queue.Enqueue([pscustomobject]@{ SurfaceId = $SourceSurfaceId; Path = @() })
    $visited = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    $visited.Add($SourceSurfaceId) | Out-Null

    while ($queue.Count -gt 0) {
        $candidate = $queue.Dequeue()
        foreach ($route in @($RoutePlan | Where-Object { $_.SourceSurfaceId -eq $candidate.SurfaceId })) {
            $path = @($candidate.Path) + $route
            if ($route.TargetSurfaceId -eq $TargetSurfaceId) {
                return $path
            }

            if ($visited.Add([string]$route.TargetSurfaceId)) {
                $queue.Enqueue([pscustomobject]@{ SurfaceId = [string]$route.TargetSurfaceId; Path = $path })
            }
        }
    }

    throw "No PSP navigation path exists from '$SourceSurfaceId' to '$TargetSurfaceId'."
}

$routePlan = Get-RequiredRoutePlan -ProfilePath $profilePath
$profile = Get-Content -LiteralPath $profilePath -Raw | ConvertFrom-Json
$surfaceIdsByName = @{}
foreach ($surface in @($profile.surfaces)) {
    $surfaceId = [string]$surface.id
    $surfaceName = [string]$surface.name
    if (-not [string]::IsNullOrWhiteSpace($surfaceId) -and -not [string]::IsNullOrWhiteSpace($surfaceName)) {
        $surfaceIdsByName[$surfaceName] = $surfaceId
    }
}

if ($PlanOnly) {
    $routePlan | ConvertTo-Json -Depth 5
    exit 0
}

foreach ($requiredPath in @($PpssppProfilePath, $OcrConfigPath, $profilePath)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required HelenUI file was not found: $requiredPath"
    }
}

$targetsResponse = Invoke-HelenRequest -Method Get -Path '/targets'
$targets = if ($null -ne $targetsResponse.PSObject.Properties['value']) {
    @($targetsResponse.value)
}
else {
    @($targetsResponse)
}
$target = $targets |
    Where-Object { $_.processName -match '^PPSSPP' } |
    Select-Object -First 1
if ($null -eq $target) {
    throw 'No running PPSSPP target was discovered by HelenUI.'
}

$sessionId = $null
$results = [System.Collections.Generic.List[object]]::new()
$runFailure = $null
try {
    $session = Invoke-HelenRequest -Method Post -Path '/sessions' -Body @{
        targetId = $target.targetId
        projectPath = $PpssppProfilePath
        ocrConfigPath = $OcrConfigPath
        captureIntervalMs = 500
        recognitionIntervalMs = 750
        profiles = @(
            @{ scopeId = 'emulator'; profilePath = $PpssppProfilePath; parentScopeId = $null },
            @{ scopeId = 'game'; profilePath = $profilePath; parentScopeId = 'emulator' }
        )
    }
    $sessionId = [string]$session.sessionId
    if ([string]::IsNullOrWhiteSpace($sessionId)) {
        throw 'HelenUI did not return a session identifier.'
    }

    foreach ($route in $routePlan) {
        Ensure-MainMenu -SessionId $sessionId | Out-Null
        $path = Get-NavigationPath -RoutePlan $routePlan -SourceSurfaceId $mainMenuSurfaceId -TargetSurfaceId ([string]$route.SourceSurfaceId)
        foreach ($pathRoute in $path) {
            Invoke-Route -SessionId $sessionId -Route $pathRoute | Out-Null
        }

        Invoke-Route -SessionId $sessionId -Route $route | Out-Null
        $results.Add([pscustomobject]@{
            interactionId = $route.InteractionId
            sourceSurfaceId = $route.SourceSurfaceId
            targetSurfaceId = $route.TargetSurfaceId
            status = 'passed'
        })
    }
}
catch {
    $runFailure = $_.Exception.Message
    $results.Add([pscustomobject]@{
        interactionId = $null
        sourceSurfaceId = $null
        targetSurfaceId = $null
        status = 'blocked'
        detail = $runFailure
    })
}
finally {
    if (-not [string]::IsNullOrWhiteSpace($sessionId)) {
        try { Invoke-HelenRequest -Method Delete -Path "/sessions/$sessionId" | Out-Null } catch { Write-Warning "Could not delete HelenUI session ${sessionId}: $($_.Exception.Message)" }
    }
}

New-Item -ItemType Directory -Path $reportDirectory -Force | Out-Null
$report = [pscustomobject]@{
    generatedAtUtc = [DateTime]::UtcNow.ToString('o')
    status = if ($null -eq $runFailure) { 'passed' } else { 'blocked' }
    target = [pscustomobject]@{ processName = $target.processName; processId = $target.processId; title = $target.title }
    profilePath = $profilePath
    routeCount = $routePlan.Count
    completedRouteCount = @($results | Where-Object { $_.status -eq 'passed' }).Count
    results = @($results)
}
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $reportDirectory 'helenui-route-coverage.json') -Encoding utf8
$report | Format-List | Out-String | Set-Content -LiteralPath (Join-Path $reportDirectory 'helenui-route-coverage.txt') -Encoding utf8

if ($null -ne $runFailure) {
    throw $runFailure
}

Write-Output "PASS: PSP HelenUI route coverage completed $($report.completedRouteCount) routes."