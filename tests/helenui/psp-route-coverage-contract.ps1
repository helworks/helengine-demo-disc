$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$runnerPath = Join-Path $projectRoot 'tools\helenui\run-psp-route-coverage.ps1'

if (-not (Test-Path -LiteralPath $runnerPath -PathType Leaf)) {
    throw "PSP route runner was not found: $runnerPath"
}

$planJson = & $runnerPath -PlanOnly
if ($LASTEXITCODE -ne 0) {
    throw "PSP route runner plan mode failed with exit code $LASTEXITCODE."
}

$plan = @((($planJson -join [Environment]::NewLine) | ConvertFrom-Json))
if ($plan.Count -eq 0) {
    throw 'Expected at least one planned PSP route.'
}

foreach ($route in $plan) {
    if (([string]::IsNullOrWhiteSpace([string]$route.SourceSurfaceId)) -or ([string]::IsNullOrWhiteSpace([string]$route.NodeId)) -or ([string]::IsNullOrWhiteSpace([string]$route.InteractionId)) -or ([string]::IsNullOrWhiteSpace([string]$route.TargetSurfaceId))) {
        throw "Route plan entry is incomplete: $($route | ConvertTo-Json -Compress)"
    }

    if ([string]$route.SourceSurfaceId -match 'clear-results|time-up') {
        throw "Metadata-only result route '$($route.InteractionId)' must not be part of the required PSP plan."
    }

    if ([string]::IsNullOrWhiteSpace([string]$route.SourceSurfaceName) -or [string]::IsNullOrWhiteSpace([string]$route.TargetSurfaceName)) {
        throw "Route plan entry must map its profile surface IDs to HelenUI-recognized surface names: $($route | ConvertTo-Json -Compress)"
    }
}

$profile = Get-Content -LiteralPath (Join-Path $projectRoot 'helenui\demodisc.json') -Raw | ConvertFrom-Json
$mainMenu = @($profile.surfaces | Where-Object { $_.id -eq 'surface-demodisc-main-menu' }) | Select-Object -First 1
$mainMenuClue = @($mainMenu.recognition.clues | Where-Object { $_.type -eq 'at_least_texts' }) | Select-Object -First 1
if ($null -eq $mainMenuClue -or [int]$mainMenuClue.params.minimumMatches -ne 2) {
    throw 'PSP main-menu recognition must require two stable top-level labels.'
}
foreach ($node in @($mainMenu.uiNodes)) {
    $selectedState = @($node.states | Where-Object { $_.name -eq 'selected' }) | Select-Object -First 1
    $selectionClue = @($selectedState.recognition.clues | Where-Object { $_.type -eq 'highlighted_text' }) | Select-Object -First 1
    $region = $selectionClue.params.region
    if ($null -eq $region -or [double]$region.width -ne 0.01 -or [double]$region.height -ne 0.01) {
        throw "PSP main-menu selection clue '$($node.id)' must use its dedicated normalized focus region."
    }
    if ($selectionClue.params.includeArrowRegion -or $selectionClue.params.requireArrowRegion) {
        throw "PSP main-menu selection clue '$($node.id)' must not use the non-unique arrow heuristic."
    }
    if ([double]$selectionClue.params.minimumColorCoverage -ne 0.5) {
        throw "PSP main-menu selection clue '$($node.id)' must require 50 percent highlight-color coverage."
    }
}

foreach ($surfaceId in @('surface-demodisc-demo-scenes-menu', 'surface-demodisc-physics-scenes-menu')) {
    $surface = @($profile.surfaces | Where-Object { $_.id -eq $surfaceId }) | Select-Object -First 1
    $catalogClue = @($surface.recognition.clues | Where-Object { $_.type -eq 'at_least_texts' }) | Select-Object -First 1
    if ($null -eq $catalogClue -or [int]$catalogClue.params.minimumMatches -ne 2) {
        throw "PSP catalog '$surfaceId' must require two distinctive visible entries."
    }
}

$targetSurfaceIds = @($plan | ForEach-Object { [string]$_.TargetSurfaceId })
foreach ($requiredTarget in @(
    'surface-demodisc-demo-scenes-menu',
    'surface-demodisc-physics-scenes-menu',
    'surface-demodisc-games-menu',
    'surface-demodisc-options-menu')) {
    if ($targetSurfaceIds -notcontains $requiredTarget) {
        throw "Expected route target '$requiredTarget' was missing from the PSP plan."
    }
}

foreach ($metadataOnlyTarget in @(
    'surface-tilt-trial-console-clear-results-level-1',
    'surface-tilt-trial-handheld-clear-results-level-1',
    'surface-tilt-trial-time-up-level-1')) {
    if ($targetSurfaceIds -contains $metadataOnlyTarget) {
        throw "Optional result-overlay target '$metadataOnlyTarget' must not be part of the required PSP plan."
    }
}

$runnerSource = Get-Content -LiteralPath $runnerPath -Raw
if (-not $runnerSource.Contains("[string]`$OcrConfigPath = 'C:\dev\helenui\plugins\recognition-cli\recognition-config.sample.json'")) {
    throw 'The PSP runner must default to HelenUI''s checked-in PaddleOCR configuration.'
}
if ($runnerSource.Contains("'Escape'")) {
    throw 'The PSP runner must not send the emulator save-state/menu key.'
}

foreach ($forbidden in @('/keys', 'Send-GamepadBack', '$gamepadControls')) {
    if ($runnerSource.Contains($forbidden)) { throw "The PSP runner must not contain raw recovery input '$forbidden'." }
}

foreach ($forbiddenDeadline in @('$NavigationTimeoutMilliseconds', '$RequestTimeoutSeconds', 'timeoutMs =', '-TimeoutSec', 'Stopwatch')) {
    if ($runnerSource.Contains($forbiddenDeadline)) {
        throw "The PSP runner must be state-driven and must not contain deadline mechanism '$forbiddenDeadline'."
    }
}
if (-not $runnerSource.Contains('function Wait-ForPpssppTarget')) {
    throw 'The PSP runner must wait for HelenUI to discover PPSSPP instead of failing on a transient target snapshot.'
}
if ($runnerSource.Contains("throw 'No running PPSSPP target was discovered by HelenUI.'")) {
    throw 'The PSP runner must not fail immediately when PPSSPP is temporarily absent from a target snapshot.'
}
if ([regex]::Matches($runnerSource, 'return Invoke-RestMethod').Count -ne 2) {
    throw 'The PSP runner must retain both HelenUI REST calls while omitting only their time limits.'
}
if (-not $runnerSource.Contains("inputClass = 'gamepad'")) { throw 'The PSP runner must request gamepad navigation.' }
if (-not $runnerSource.Contains('function Save-SceneScreenshot')) {
    throw 'The PSP runner must define one RAM-frame screenshot capture operation per recognized scene.'
}
if (-not $runnerSource.Contains('/sessions/$SessionId/latest-image')) {
    throw 'The PSP runner must obtain report screenshots through HelenUI''s RAM-resident latest-image endpoint.'
}
if (-not $runnerSource.Contains('$capturedSceneScreenshots')) {
    throw 'The PSP runner must track captured scene screenshots by surface ID so revisits do not duplicate artifacts.'
}
if (-not $runnerSource.Contains('sceneScreenshots = @($capturedSceneScreenshots.Values)')) {
    throw 'The PSP route report must list every saved scene screenshot artifact.'
}

Write-Output "PASS: PSP route-plan contract validated $($plan.Count) required routes."