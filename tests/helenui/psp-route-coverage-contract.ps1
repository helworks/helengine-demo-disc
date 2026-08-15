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
foreach ($node in @($mainMenu.uiNodes)) {
    $selectedState = @($node.states | Where-Object { $_.name -eq 'selected' }) | Select-Object -First 1
    $selectionClue = @($selectedState.recognition.clues | Where-Object { $_.type -eq 'highlighted_text' }) | Select-Object -First 1
    if ($selectionClue.params.includeArrowRegion -or $selectionClue.params.requireArrowRegion) {
        throw "PSP main-menu selection clue '$($node.id)' must use its configured region instead of OCR-derived arrow geometry."
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

if (-not $runnerSource.Contains("inputClass = 'gamepad'")) { throw 'The PSP runner must request gamepad navigation.' }

Write-Output "PASS: PSP route-plan contract validated $($plan.Count) required routes."