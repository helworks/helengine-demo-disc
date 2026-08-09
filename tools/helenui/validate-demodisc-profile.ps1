[CmdletBinding()]
param(
    [string]$ProfilePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ProfilePath)) {
    $ProfilePath = Join-Path $PSScriptRoot '..\..\helenui\demodisc.json'
}

function Fail([string]$Message) {
    [Console]::Error.WriteLine("FAIL: $Message")
    exit 1
}

function Get-RequiredProperty($Object, [string]$PropertyName, [string]$Context) {
    if ($null -eq $Object) {
        Fail "$Context is required"
    }

    $property = $Object.PSObject.Properties[$PropertyName]
    if ($null -eq $property -or $null -eq $property.Value) {
        Fail "$Context.$PropertyName is required"
    }

    return ,$property.Value
}

function Get-RequiredStringProperty($Object, [string]$PropertyName, [string]$Context) {
    $value = Get-RequiredProperty $Object $PropertyName $Context
    if ($value -isnot [string] -or [string]::IsNullOrWhiteSpace([string]$value)) {
        Fail "$Context.$PropertyName must be a non-empty string"
    }
    return [string]$value
}

function Get-OptionalStringProperty($Object, [string]$PropertyName, [string]$Context) {
    if ($null -eq $Object) {
        return $null
    }

    $property = $Object.PSObject.Properties[$PropertyName]
    if ($null -eq $property) {
        return $null
    }
    if ($null -eq $property.Value) {
        Fail "$Context.$PropertyName cannot be null when present"
    }
    if ($property.Value -isnot [string] -or [string]::IsNullOrWhiteSpace([string]$property.Value)) {
        Fail "$Context.$PropertyName must be a non-empty string when present"
    }
    return [string]$property.Value
}

function Get-RequiredIntegerProperty($Object, [string]$PropertyName, [string]$Context) {
    $value = Get-RequiredProperty $Object $PropertyName $Context
    $integerTypeNames = @(
        'System.Byte', 'System.SByte', 'System.Int16', 'System.UInt16',
        'System.Int32', 'System.UInt32', 'System.Int64', 'System.UInt64'
    )
    if ($integerTypeNames -notcontains $value.GetType().FullName) {
        Fail "$Context.$PropertyName must be an integer"
    }

    return [long]$value
}

function Get-RequiredArray($Object, [string]$PropertyName, [string]$Context, [bool]$AllowEmpty) {
    $value = Get-RequiredProperty $Object $PropertyName $Context
    if ($value -isnot [array]) {
        Fail "$Context.$PropertyName must be an array"
    }

    $arrayValue = @($value)
    if (-not $AllowEmpty -and $arrayValue.Count -eq 0) {
        if ($Context -eq 'profile' -and $PropertyName -eq 'surfaces') {
            Fail 'profile.surfaces must contain at least one surface'
        }
        Fail "$Context.$PropertyName must contain at least one entry"
    }
    return ,$arrayValue
}

try {
    $resolvedProfilePath = [System.IO.Path]::GetFullPath($ProfilePath)
} catch {
    Fail "profile path is invalid: $($_.Exception.Message)"
}

try {
    $profileExists = Test-Path -LiteralPath $resolvedProfilePath -PathType Leaf
} catch {
    Fail "profile path could not be checked: $($_.Exception.Message)"
}
if (-not $profileExists) {
    Fail "profile not found at $resolvedProfilePath"
}

try {
    $profileJson = Get-Content -LiteralPath $resolvedProfilePath -Raw
} catch {
    Fail "profile could not be read: $($_.Exception.Message)"
}

try {
    $document = $profileJson | ConvertFrom-Json
} catch {
    Fail "profile is not valid JSON: $($_.Exception.Message)"
}

if ($null -eq $document -or $document -isnot [pscustomobject]) {
    Fail 'profile root must be a JSON object'
}

$project = Get-RequiredProperty $document 'project' 'profile'
$schemaVersion = Get-RequiredIntegerProperty $project 'schemaVersion' 'profile.project'
$projectId = Get-RequiredStringProperty $project 'id' 'profile.project'
if ($schemaVersion -ne 7) {
    Fail "profile.project.schemaVersion must be 7, got $schemaVersion"
}
if ($projectId -cne 'demodisc') {
    Fail "profile.project.id must be demodisc, got $projectId"
}

$surfaces = Get-RequiredArray $document 'surfaces' 'profile' $false

$inputCatalog = Get-RequiredProperty $document 'inputCatalog' 'profile'
$inputControls = Get-RequiredArray $inputCatalog 'controls' 'profile.inputCatalog' $false
$inputActions = Get-RequiredArray $inputCatalog 'actions' 'profile.inputCatalog' $false

$ordinalComparer = [System.StringComparer]::Ordinal
$surfaceIds = [System.Collections.Generic.Dictionary[string,object]]::new($ordinalComparer)
$nodeIds = [System.Collections.Generic.Dictionary[string,string]]::new($ordinalComparer)
$inputControlIds = [System.Collections.Generic.Dictionary[string,bool]]::new($ordinalComparer)
$inputActionIds = [System.Collections.Generic.Dictionary[string,bool]]::new($ordinalComparer)
foreach ($control in $inputControls) {
    if ($null -eq $control -or $control -isnot [pscustomobject]) {
        Fail 'profile.inputCatalog.controls must contain JSON objects'
    }
    $controlId = Get-RequiredStringProperty $control 'id' 'input control'
    Get-RequiredStringProperty $control 'family' "input control '$controlId'" | Out-Null
    Get-RequiredStringProperty $control 'label' "input control '$controlId'" | Out-Null
    if ($inputControlIds.ContainsKey($controlId)) {
        Fail "duplicate input control id '$controlId'"
    }
    $inputControlIds[$controlId] = $true
}
foreach ($action in $inputActions) {
    if ($null -eq $action) {
        Fail 'profile.inputCatalog.actions cannot contain null entries'
    }
    $actionId = Get-RequiredStringProperty $action 'id' 'input action'
    if ($inputActionIds.ContainsKey($actionId)) {
        Fail "duplicate input action id '$actionId'"
    }
    $inputActionIds[$actionId] = $true

    $bindings = Get-RequiredArray $action 'bindings' "input action '$actionId'" $false
    $bindingIds = [System.Collections.Generic.Dictionary[string,bool]]::new($ordinalComparer)
    foreach ($binding in $bindings) {
        if ($null -eq $binding -or $binding -isnot [pscustomobject]) {
            Fail "input action '$actionId' bindings must contain JSON objects"
        }
        $bindingId = Get-RequiredStringProperty $binding 'id' "input action '$actionId' binding"
        if ($bindingIds.ContainsKey($bindingId)) {
            Fail "duplicate binding id '$bindingId' on input action '$actionId'"
        }
        $bindingIds[$bindingId] = $true
        $parts = Get-RequiredArray $binding 'parts' "input binding '$bindingId'" $false
        foreach ($part in $parts) {
            if ($null -eq $part -or $part -isnot [pscustomobject]) {
                Fail "input binding '$bindingId' parts must contain JSON objects"
            }
            Get-RequiredStringProperty $part 'family' "input binding '$bindingId' part" | Out-Null
            $controlId = Get-RequiredStringProperty $part 'controlId' "input binding '$bindingId' part"
            if (-not $inputControlIds.ContainsKey($controlId)) {
                Fail "input binding '$bindingId' references unknown control '$controlId'"
            }
        }
    }
}

$routeTargetCount = 0
$nodeCount = 0
foreach ($surface in $surfaces) {
    if ($null -eq $surface) {
        Fail 'profile.surfaces cannot contain null entries'
    }
    $surfaceId = Get-RequiredStringProperty $surface 'id' 'surface'
    if ($surfaceIds.ContainsKey($surfaceId)) {
        Fail "duplicate surface id '$surfaceId'"
    }
    $surfaceIds[$surfaceId] = $surface

    $surfaceNodes = Get-RequiredArray $surface 'uiNodes' "surface '$surfaceId'" $false
    $orders = @{}
    foreach ($node in $surfaceNodes) {
        if ($null -eq $node) {
            Fail "surface '$surfaceId' cannot contain null UI nodes"
        }
        $nodeId = Get-RequiredStringProperty $node 'id' "surface '$surfaceId' UI node"
        if ($nodeIds.ContainsKey($nodeId)) {
            Fail "duplicate UI node id '$nodeId'"
        }
        $nodeIds[$nodeId] = $surfaceId

        $orderValue = Get-RequiredIntegerProperty $node 'order' "node '$nodeId'"
        if ($orderValue -lt 0) {
            Fail "node '$nodeId' order must be non-negative"
        }
        $order = [string]$orderValue
        if ($orders.ContainsKey($order)) {
            Fail "surface '$surfaceId' has duplicate node order '$order'"
        }
        $orders[$order] = $nodeId
        $nodeCount++

        $interactions = Get-RequiredArray $node 'interactions' "node '$nodeId'" $true
        foreach ($interaction in $interactions) {
            if ($null -eq $interaction) {
                Fail "node '$nodeId' cannot contain null interactions"
            }
            if ($interaction -isnot [pscustomobject]) {
                Fail "node '$nodeId' interactions must be JSON objects"
            }
            $targetSurfaceId = Get-OptionalStringProperty $interaction 'targetSurfaceId' "interaction on node '$nodeId'"
            if ($null -ne $targetSurfaceId) {
                $routeTargetCount++
                if (-not $surfaceIds.ContainsKey($targetSurfaceId)) {
                    # The target may be declared later; defer final target validation.
                }
            }
            $interactionInputActionId = Get-OptionalStringProperty $interaction 'inputActionId' "interaction on node '$nodeId'"
            if ($null -ne $interactionInputActionId -and -not $inputActionIds.ContainsKey($interactionInputActionId)) {
                Fail "node '$nodeId' references unknown input action '$interactionInputActionId'"
            }
        }
        $nodeInputActionId = Get-OptionalStringProperty $node 'inputActionId' "node '$nodeId'"
        if ($null -ne $nodeInputActionId -and -not $inputActionIds.ContainsKey($nodeInputActionId)) {
            Fail "node '$nodeId' references unknown input action '$nodeInputActionId'"
        }
        $nodeType = Get-RequiredStringProperty $node 'type' "node '$nodeId'"
        if ($nodeType -eq 'hidden_button') {
            if ($null -eq $nodeInputActionId) {
                Fail "hidden button '$nodeId' must reference an input action id"
            }
            if ($interactions.Count -eq 0) {
                Fail "hidden button '$nodeId' must declare at least one interaction"
            }
        }
    }

    for ($expectedOrder = 0; $expectedOrder -lt $surfaceNodes.Count; $expectedOrder++) {
        if (-not $orders.ContainsKey([string]$expectedOrder)) {
            Fail "surface '$surfaceId' node orders must be contiguous starting at 0; missing order '$expectedOrder'"
        }
    }

    $surfaceNodeIds = @($surfaceNodes | ForEach-Object { Get-RequiredStringProperty $_ 'id' "surface '$surfaceId' UI node" })
    $initialSelectedNodeId = Get-OptionalStringProperty $surface 'initialSelectedNodeId' "surface '$surfaceId'"
    if ($null -ne $initialSelectedNodeId -and $surfaceNodeIds -cnotcontains $initialSelectedNodeId) {
        Fail "surface '$surfaceId' has unknown initialSelectedNodeId '$initialSelectedNodeId'"
    }
}

foreach ($surfaceId in $surfaceIds.Keys) {
    $surfaceNodes = @($surfaceIds[$surfaceId].uiNodes)
    foreach ($node in $surfaceNodes) {
        $nodeId = [string](Get-RequiredProperty $node 'id' "surface '$surfaceId' UI node")
        $interactions = @($node.interactions)
        foreach ($interaction in $interactions) {
            $targetSurfaceId = Get-OptionalStringProperty $interaction 'targetSurfaceId' "interaction on node '$nodeId'"
            if ($null -ne $targetSurfaceId -and -not $surfaceIds.ContainsKey($targetSurfaceId)) {
                Fail "node '$nodeId' references unknown target surface '$targetSurfaceId'"
            }
        }
    }
}

$mainMenuId = 'surface-demodisc-main-menu'
if (-not $surfaceIds.ContainsKey($mainMenuId)) {
    Fail "canonical main menu '$mainMenuId' is missing"
}

$reachable = [System.Collections.Generic.Dictionary[string,bool]]::new($ordinalComparer)
$adjacency = [System.Collections.Generic.Dictionary[string,string[]]]::new($ordinalComparer)
foreach ($surfaceId in $surfaceIds.Keys) {
    $targets = @()
    foreach ($node in @($surfaceIds[$surfaceId].uiNodes)) {
        foreach ($interaction in @($node.interactions)) {
            $targetSurfaceId = Get-OptionalStringProperty $interaction 'targetSurfaceId' "interaction on surface '$surfaceId'"
            if ($null -ne $targetSurfaceId) {
                $targets += $targetSurfaceId
            }
        }
    }
    $adjacency[$surfaceId] = [string[]]$targets
}

function Assert-ExactTextOrder([string]$SurfaceId, [string[]]$ExpectedTexts) {
    if (-not $surfaceIds.ContainsKey($SurfaceId)) {
        Fail "catalog audit surface '$SurfaceId' is missing"
    }

    $orderedNodes = @($surfaceIds[$SurfaceId].uiNodes | Sort-Object -Property @{ Expression = { Get-RequiredIntegerProperty $_ 'order' "catalog audit node on '$SurfaceId'" } })
    $actualTexts = @()
    foreach ($node in $orderedNodes) {
        $text = Get-OptionalStringProperty $node 'text' "catalog audit node on '$SurfaceId'"
        if ($null -ne $text) {
            $actualTexts += $text
        }
    }
    if ($actualTexts.Count -ne $ExpectedTexts.Count) {
        Fail "catalog audit surface '$SurfaceId' expected $($ExpectedTexts.Count) visible text nodes, found $($actualTexts.Count)"
    }
    for ($index = 0; $index -lt $ExpectedTexts.Count; $index++) {
        if ($actualTexts[$index] -cne $ExpectedTexts[$index]) {
            Fail "catalog audit surface '$SurfaceId' expected '$($ExpectedTexts[$index])' at order $index, found '$($actualTexts[$index])'"
        }
    }
}

function Assert-Route([string]$SourceSurfaceId, [string]$TargetSurfaceId, [string]$Description) {
    if (-not $surfaceIds.ContainsKey($SourceSurfaceId)) {
        Fail "return-path audit source surface '$SourceSurfaceId' is missing"
    }
    if ($adjacency[$SourceSurfaceId] -cnotcontains $TargetSurfaceId) {
        Fail "return-path audit '$Description' is missing route '$SourceSurfaceId' -> '$TargetSurfaceId'"
    }
}

function Assert-NodeRoute([string]$SourceSurfaceId, [string]$NodeId, [string]$TargetSurfaceId, [string]$Description) {
    if (-not $surfaceIds.ContainsKey($SourceSurfaceId)) {
        Fail "node-route audit source surface '$SourceSurfaceId' is missing"
    }
    $matchingNodes = @($surfaceIds[$SourceSurfaceId].uiNodes | Where-Object {
        (Get-RequiredStringProperty $_ 'id' "node-route audit surface '$SourceSurfaceId'") -ceq $NodeId
    })
    if ($matchingNodes.Count -ne 1) {
        Fail "node-route audit expected one node '$NodeId' on '$SourceSurfaceId', found $($matchingNodes.Count)"
    }
    $targetIds = @()
    foreach ($interaction in @($matchingNodes[0].interactions)) {
        $targetId = Get-OptionalStringProperty $interaction 'targetSurfaceId' "node-route audit node '$NodeId'"
        if ($null -ne $targetId) {
            $targetIds += $targetId
        }
    }
    if ($targetIds -cnotcontains $TargetSurfaceId) {
        Fail "node-route audit '$Description' is missing route '$SourceSurfaceId/$NodeId' -> '$TargetSurfaceId'"
    }
}

function Assert-RecognitionText([string]$SurfaceId, [string]$ExpectedText, [string]$Description) {
    if (-not $surfaceIds.ContainsKey($SurfaceId)) {
        Fail "recognition audit surface '$SurfaceId' is missing"
    }
    $found = $false
    foreach ($clue in @($surfaceIds[$SurfaceId].recognition.clues)) {
        $paramsProperty = $clue.PSObject.Properties['params']
        if ($null -eq $paramsProperty -or $null -eq $paramsProperty.Value) {
            continue
        }
        $params = $paramsProperty.Value
        $text = Get-OptionalStringProperty $params 'text' "recognition audit clue on '$SurfaceId'"
        if ($null -ne $text -and $text -ceq $ExpectedText) {
            $found = $true
        }
        $textsProperty = $params.PSObject.Properties['texts']
        if ($null -ne $textsProperty -and $null -ne $textsProperty.Value -and $textsProperty.Value -is [array]) {
            foreach ($candidate in @($textsProperty.Value)) {
                if ([string]$candidate -ceq $ExpectedText) {
                    $found = $true
                }
            }
        }
    }
    if (-not $found) {
        Fail "recognition audit '$Description' is missing '$ExpectedText' on '$SurfaceId'"
    }
}

$expectedRenderingCatalog = @('Cube Test','Colored Cubes','Textured Cubes','Axis 1','Axis 2','Matrix Render','Directional Shadow Plaza','PBR Material Gallery','PBR Textured Showcase','PBR Shadow Theater','Back')
$expectedPhysicsCatalog = @('Stacked Boxes','Sphere Stack','Mixed Stack','Static Mesh','Static Mesh Simple','Back')
Assert-ExactTextOrder 'surface-demodisc-main-menu' @('Demo Scenes','Physics Scenes','Games','Options')
Assert-ExactTextOrder 'surface-demodisc-demo-scenes-menu' $expectedRenderingCatalog
Assert-ExactTextOrder 'surface-demodisc-physics-scenes-menu' $expectedPhysicsCatalog
Assert-ExactTextOrder 'surface-demodisc-games-menu' @('Tilt Trial','Back')
Assert-ExactTextOrder 'surface-demodisc-options-menu' @('Display','Audio','Controls','Back')
Assert-ExactTextOrder 'surface-tilt-trial-console-title' @('Play','Options','Demo Disc')
Assert-ExactTextOrder 'surface-tilt-trial-options' @('OPTIONS','Settings coming soon','BACK')
Assert-ExactTextOrder 'surface-tilt-trial-console-selector' @('Level 1','Level 2','Level 3','Level 4','Level 5','Back','Play')
Assert-ExactTextOrder 'surface-tilt-trial-handheld-list' @('Level 1','Level 2','Level 3','Level 4','Level 5')

Assert-Route 'surface-demodisc-demo-scenes-menu' 'surface-demodisc-main-menu' 'rendering menu back'
Assert-Route 'surface-demodisc-physics-scenes-menu' 'surface-demodisc-main-menu' 'physics menu back'
Assert-Route 'surface-demodisc-games-menu' 'surface-demodisc-main-menu' 'games menu back'
Assert-Route 'surface-demodisc-options-menu' 'surface-demodisc-main-menu' 'options back'
Assert-Route 'surface-demodisc-showcase-scene' 'surface-demodisc-main-menu' 'showcase back'
Assert-Route 'surface-tilt-trial-console-title' 'surface-demodisc-main-menu' 'title Demo Disc action'
Assert-Route 'surface-tilt-trial-options' 'surface-tilt-trial-console-title' 'Tilt Play options back'
Assert-Route 'surface-tilt-trial-console-selector' 'surface-tilt-trial-console-title' 'console selector back'
Assert-Route 'surface-tilt-trial-handheld-list' 'surface-demodisc-main-menu' 'handheld return'
Assert-NodeRoute 'surface-demodisc-games-menu' 'node-demodisc-games-tilt-trial' 'surface-tilt-trial-console-title' 'Games Tilt Trial console entry'
Assert-NodeRoute 'surface-demodisc-games-menu' 'node-demodisc-games-tilt-trial' 'surface-tilt-trial-handheld-list' 'Games Tilt Trial handheld entry'
Assert-NodeRoute 'surface-demodisc-demo-scenes-menu' 'node-demodisc-rendering-back' 'surface-demodisc-main-menu' 'rendering Back node'
Assert-NodeRoute 'surface-demodisc-physics-scenes-menu' 'node-demodisc-physics-back' 'surface-demodisc-main-menu' 'physics Back node'
Assert-NodeRoute 'surface-demodisc-games-menu' 'node-demodisc-games-back' 'surface-demodisc-main-menu' 'games Back node'
Assert-NodeRoute 'surface-demodisc-options-menu' 'node-demodisc-options-back' 'surface-demodisc-main-menu' 'options Back node'
Assert-NodeRoute 'surface-demodisc-showcase-scene' 'node-demodisc-showcase-back' 'surface-demodisc-main-menu' 'showcase Back node'
Assert-NodeRoute 'surface-tilt-trial-console-title' 'node-tilt-title-play' 'surface-tilt-trial-console-selector' 'title Play node'
Assert-NodeRoute 'surface-tilt-trial-console-title' 'node-tilt-title-options' 'surface-tilt-trial-options' 'title Options node'
Assert-NodeRoute 'surface-tilt-trial-console-title' 'node-tilt-title-demo-disc' 'surface-demodisc-main-menu' 'title Demo Disc node'
Assert-NodeRoute 'surface-tilt-trial-options' 'node-tilt-options-back' 'surface-tilt-trial-console-title' 'Tilt Play options Back node'
Assert-NodeRoute 'surface-tilt-trial-console-selector' 'node-tilt-console-back' 'surface-tilt-trial-console-title' 'console selector Back node'
Assert-NodeRoute 'surface-tilt-trial-handheld-list' 'node-tilt-handheld-back' 'surface-demodisc-main-menu' 'handheld Back node'
for ($catalogIndex = 0; $catalogIndex -lt 10; $catalogIndex++) {
    $renderingSlug = ($expectedRenderingCatalog[$catalogIndex].ToLower() -replace '[^a-z0-9]+','-')
    Assert-NodeRoute 'surface-demodisc-demo-scenes-menu' "node-demodisc-rendering-$renderingSlug" 'surface-demodisc-showcase-scene' "rendering $($expectedRenderingCatalog[$catalogIndex]) route"
}
for ($catalogIndex = 0; $catalogIndex -lt 5; $catalogIndex++) {
    $physicsSlug = ($expectedPhysicsCatalog[$catalogIndex].ToLower() -replace '[^a-z0-9]+','-')
    Assert-NodeRoute 'surface-demodisc-physics-scenes-menu' "node-demodisc-physics-$physicsSlug" 'surface-demodisc-showcase-scene' "physics $($expectedPhysicsCatalog[$catalogIndex]) route"
}

foreach ($levelNumber in 1..5) {
    $catalogValuesByLevel = @{
        1 = @('18.00','28.00','40.00')
        2 = @('20.00','31.00','44.00')
        3 = @('23.00','35.00','48.00')
        4 = @('25.00','38.00','52.00')
        5 = @('27.00','41.00','56.00')
    }
    $levelTargets = $catalogValuesByLevel[$levelNumber]
    $targetTuple = "Gold  $($levelTargets[0])`nSilver $($levelTargets[1])`nBronze $($levelTargets[2])"
    Assert-RecognitionText "surface-tilt-trial-handheld-details-level-$levelNumber" $targetTuple "handheld Level $levelNumber target tuple"
    Assert-RecognitionText 'surface-tilt-trial-console-selector' $targetTuple "console selector Level $levelNumber target tuple"
    Assert-ExactTextOrder "surface-tilt-trial-handheld-details-level-$levelNumber" @('BACK','PLAY')
    Assert-ExactTextOrder "surface-tilt-trial-handheld-clear-results-level-$levelNumber" @('RETRY','EXIT','NEXT')
    Assert-ExactTextOrder "surface-tilt-trial-time-up-level-$levelNumber" @('Retry','Level Select')
    Assert-Route 'surface-tilt-trial-console-selector' "surface-tilt-trial-level-$levelNumber" "console Level $levelNumber selection"
    Assert-Route 'surface-tilt-trial-handheld-list' "surface-tilt-trial-handheld-details-level-$levelNumber" "handheld Level $levelNumber selection"
    Assert-NodeRoute 'surface-tilt-trial-console-selector' "node-tilt-console-level-$levelNumber" "surface-tilt-trial-level-$levelNumber" "console Level $levelNumber row"
    Assert-NodeRoute 'surface-tilt-trial-handheld-list' "node-tilt-handheld-level-$levelNumber" "surface-tilt-trial-handheld-details-level-$levelNumber" "handheld Level $levelNumber row"
    Assert-NodeRoute "surface-tilt-trial-handheld-details-level-$levelNumber" "node-tilt-handheld-details-$levelNumber-back" 'surface-tilt-trial-handheld-list' "handheld Level $levelNumber back"
    Assert-NodeRoute "surface-tilt-trial-handheld-details-level-$levelNumber" "node-tilt-handheld-details-$levelNumber-play" "surface-tilt-trial-level-$levelNumber" "handheld Level $levelNumber play"
    Assert-NodeRoute "surface-tilt-trial-level-$levelNumber" "node-tilt-level-$levelNumber-console-clear-route" "surface-tilt-trial-console-clear-results-level-$levelNumber" "Level $levelNumber console clear transition"
    Assert-NodeRoute "surface-tilt-trial-level-$levelNumber" "node-tilt-level-$levelNumber-handheld-clear-route" "surface-tilt-trial-handheld-clear-results-level-$levelNumber" "Level $levelNumber handheld clear transition"
    Assert-NodeRoute "surface-tilt-trial-level-$levelNumber" "node-tilt-level-$levelNumber-time-up-route" "surface-tilt-trial-time-up-level-$levelNumber" "Level $levelNumber time-up transition"
    Assert-NodeRoute "surface-tilt-trial-console-clear-results-level-$levelNumber" "node-tilt-console-clear-$levelNumber-retry" "surface-tilt-trial-level-$levelNumber" "console Level $levelNumber retry"
    Assert-NodeRoute "surface-tilt-trial-console-clear-results-level-$levelNumber" "node-tilt-console-clear-$levelNumber-level-select" 'surface-tilt-trial-console-selector' "console Level $levelNumber level select"
    Assert-NodeRoute "surface-tilt-trial-console-clear-results-level-$levelNumber" "node-tilt-console-clear-$levelNumber-next" $(if ($levelNumber -lt 5) { "surface-tilt-trial-level-$($levelNumber + 1)" } else { 'surface-tilt-trial-console-selector' }) "console Level $levelNumber next"
    Assert-NodeRoute "surface-tilt-trial-handheld-clear-results-level-$levelNumber" "node-tilt-handheld-clear-$levelNumber-retry" "surface-tilt-trial-level-$levelNumber" "handheld Level $levelNumber retry"
    Assert-NodeRoute "surface-tilt-trial-handheld-clear-results-level-$levelNumber" "node-tilt-handheld-clear-$levelNumber-exit" 'surface-tilt-trial-handheld-list' "handheld Level $levelNumber exit"
    Assert-NodeRoute "surface-tilt-trial-handheld-clear-results-level-$levelNumber" "node-tilt-handheld-clear-$levelNumber-next" $(if ($levelNumber -lt 5) { "surface-tilt-trial-level-$($levelNumber + 1)" } else { 'surface-tilt-trial-handheld-list' }) "handheld Level $levelNumber next"
    Assert-NodeRoute "surface-tilt-trial-time-up-level-$levelNumber" "node-tilt-time-up-$levelNumber-retry" "surface-tilt-trial-level-$levelNumber" "time-up Level $levelNumber retry"
    Assert-NodeRoute "surface-tilt-trial-time-up-level-$levelNumber" "node-tilt-time-up-$levelNumber-level-select" 'surface-tilt-trial-console-selector' "time-up Level $levelNumber console selector"
    Assert-NodeRoute "surface-tilt-trial-time-up-level-$levelNumber" "node-tilt-time-up-$levelNumber-level-select" 'surface-tilt-trial-handheld-list' "time-up Level $levelNumber handheld selector"
}

$expectedCatalogValues = @('18.00','28.00','40.00','20.00','31.00','44.00','23.00','35.00','48.00','25.00','38.00','52.00','27.00','41.00','56.00')
foreach ($catalogValue in $expectedCatalogValues) {
    if ($profileJson -cnotmatch [regex]::Escape($catalogValue)) {
        Fail "catalog audit profile is missing target value '$catalogValue'"
    }
}

$queue = [System.Collections.Generic.Queue[string]]::new()
$queue.Enqueue($mainMenuId)
while ($queue.Count -gt 0) {
    $currentId = $queue.Dequeue()
    if ($reachable.ContainsKey($currentId)) {
        continue
    }
    $reachable[$currentId] = $true
    foreach ($node in @($surfaceIds[$currentId].uiNodes)) {
        foreach ($interaction in @($node.interactions)) {
            $targetSurfaceId = Get-OptionalStringProperty $interaction 'targetSurfaceId' "interaction on surface '$currentId'"
            if ($null -ne $targetSurfaceId -and -not $reachable.ContainsKey($targetSurfaceId)) {
                $queue.Enqueue($targetSurfaceId)
            }
        }
    }
}

$routeSurfacePattern = '^surface-(demodisc-(main-menu|demo-scenes-menu|physics-scenes-menu|games-menu|options-menu|showcase-scene)|tilt-trial-(console-title|options|console-selector|handheld-list|handheld-details-level-\d+|console-clear-results-level-\d+|handheld-clear-results-level-\d+|time-up-level-\d+))$'
foreach ($surfaceId in $surfaceIds.Keys) {
    if ($surfaceId -cne $mainMenuId -and $surfaceId -cmatch $routeSurfacePattern -and $adjacency[$surfaceId].Count -eq 0) {
        Fail "route surface '$surfaceId' has no outgoing target route"
    }
}

$gameplaySurfaceIds = @($surfaceIds.Keys | Where-Object { $_ -cmatch '^surface-tilt-trial-level-(\d+)$' })
foreach ($gameplaySurfaceId in $gameplaySurfaceIds) {
    $levelNumber = [regex]::Match($gameplaySurfaceId, '^surface-tilt-trial-level-(\d+)$', [System.Text.RegularExpressions.RegexOptions]::CultureInvariant).Groups[1].Value
    $gameplaySurface = $surfaceIds[$gameplaySurfaceId]
    $pauseNodes = @($gameplaySurface.uiNodes | Where-Object {
        (Get-OptionalStringProperty $_ 'inputActionId' "gameplay surface '$gameplaySurfaceId' UI node") -ceq 'pause'
    })
    $pauseInteractions = @(
        foreach ($gameplayNode in @($gameplaySurface.uiNodes)) {
            foreach ($gameplayInteraction in @($gameplayNode.interactions)) {
                if ((Get-OptionalStringProperty $gameplayInteraction 'inputActionId' "gameplay surface '$gameplaySurfaceId' interaction") -ceq 'pause') {
                    $gameplayInteraction
                }
            }
        }
    )
    if ($pauseNodes.Count -eq 0 -and $pauseInteractions.Count -eq 0) {
        Fail "gameplay surface '$gameplaySurfaceId' is missing a pause action"
    }

    $consoleClearSurfaceIds = @($surfaceIds.Keys | Where-Object { $_ -cmatch "^surface-tilt-trial-console-clear-results-level-$levelNumber$" })
    if ($consoleClearSurfaceIds.Count -eq 0) {
        Fail "gameplay surface '$gameplaySurfaceId' has no matching console clear-results surface"
    }
    $handheldClearSurfaceIds = @($surfaceIds.Keys | Where-Object { $_ -cmatch "^surface-tilt-trial-handheld-clear-results-level-$levelNumber$" })
    if ($handheldClearSurfaceIds.Count -eq 0) {
        Fail "gameplay surface '$gameplaySurfaceId' has no matching handheld clear-results surface"
    }
    foreach ($clearSurfaceId in @($consoleClearSurfaceIds + $handheldClearSurfaceIds)) {
        if ($adjacency[$clearSurfaceId].Count -eq 0) {
            Fail "clear-results surface '$clearSurfaceId' has no return route"
        }
    }
    foreach ($consoleClearSurfaceId in $consoleClearSurfaceIds) {
        if ($adjacency[$consoleClearSurfaceId] -cnotcontains $gameplaySurfaceId) {
            Fail "console clear-results surface '$consoleClearSurfaceId' is missing a retry route to '$gameplaySurfaceId'"
        }
        if ($adjacency[$consoleClearSurfaceId] -cnotcontains 'surface-tilt-trial-console-selector') {
            Fail "console clear-results surface '$consoleClearSurfaceId' is missing a level-select route"
        }
    }
    foreach ($handheldClearSurfaceId in $handheldClearSurfaceIds) {
        if ($adjacency[$handheldClearSurfaceId] -cnotcontains $gameplaySurfaceId) {
            Fail "handheld clear-results surface '$handheldClearSurfaceId' is missing a retry route to '$gameplaySurfaceId'"
        }
        if ($adjacency[$handheldClearSurfaceId] -cnotcontains 'surface-tilt-trial-handheld-list') {
            Fail "handheld clear-results surface '$handheldClearSurfaceId' is missing an exit route"
        }
    }

    $timeUpSurfaceIds = @($surfaceIds.Keys | Where-Object { $_ -cmatch "^surface-tilt-trial-time-up-level-$levelNumber$" })
    if ($timeUpSurfaceIds.Count -eq 0) {
        Fail "gameplay surface '$gameplaySurfaceId' has no matching time-up surface"
    }
    foreach ($timeUpSurfaceId in $timeUpSurfaceIds) {
        if ($adjacency[$timeUpSurfaceId].Count -eq 0) {
            Fail "time-up surface '$timeUpSurfaceId' has no return route"
        }
        if ($adjacency[$timeUpSurfaceId] -cnotcontains $gameplaySurfaceId) {
            Fail "time-up surface '$timeUpSurfaceId' is missing a retry route to '$gameplaySurfaceId'"
        }
        if ($adjacency[$timeUpSurfaceId] -cnotcontains 'surface-tilt-trial-console-selector' -or $adjacency[$timeUpSurfaceId] -cnotcontains 'surface-tilt-trial-handheld-list') {
            Fail "time-up surface '$timeUpSurfaceId' is missing a level-select route"
        }
    }
}

foreach ($surfaceId in $surfaceIds.Keys) {
    if (-not $reachable.ContainsKey($surfaceId)) {
        Fail "surface '$surfaceId' is not reachable from '$mainMenuId'"
    }
}

Write-Output "PASS: $($surfaceIds.Count) surfaces, $nodeCount UI nodes, $routeTargetCount route targets validated."
