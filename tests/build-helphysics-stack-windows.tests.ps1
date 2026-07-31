[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Assert-ContainsText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Source,

        [Parameter(Mandatory = $true)]
        [string]$Expected,

        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    if ($Source.IndexOf($Expected, [System.StringComparison]::Ordinal) -lt 0) {
        throw "Missing build-script contract: $Description ('$Expected')."
    }
}

function Assert-DoesNotContainText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Source,

        [Parameter(Mandatory = $true)]
        [string]$Unexpected,

        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    if ($Source.IndexOf($Unexpected, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        throw "Forbidden build-script behavior: $Description ('$Unexpected')."
    }
}

function Assert-ContainsCount {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Source,

        [Parameter(Mandatory = $true)]
        [string]$Expected,

        [Parameter(Mandatory = $true)]
        [int]$Count,

        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    $Matches = [regex]::Matches($Source, [regex]::Escape($Expected))
    if ($Matches.Count -ne $Count) {
        throw "Incorrect build-script contract count for $Description ('$Expected'): expected $Count, found $($Matches.Count)."
    }
}

$BuildScriptPath = Join-Path (Split-Path -Parent $PSScriptRoot) "build-helphysics-stack-windows.ps1"
if (-not (Test-Path -LiteralPath $BuildScriptPath -PathType Leaf)) {
    throw "Guarded build script was not found at '$BuildScriptPath'."
}

$Source = Get-Content -LiteralPath $BuildScriptPath -Raw

Assert-ContainsText $Source 'C:\dev\helprojs\demodisc\.worktrees\helphysics-demodisc-stack-test' "isolated Demodisc worktree"
Assert-ContainsText $Source 'C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test' "isolated engine worktree"
Assert-ContainsText $Source 'C:\dev\helworks\helengine-windows\.worktrees\helphysics-demodisc-stack-test' "isolated Windows worktree"
Assert-ContainsText $Source 'C:\dev\helworks\csharpcodegen\.worktrees\helphysics-demodisc-stack-test' "isolated codegen worktree"
Assert-ContainsText $Source 'codex/helphysics-demodisc-stack-test' "exact worktree branch validation"
Assert-ContainsText $Source 'd716776b6ec6d719c7448fe57652cf342d0b09d6' "Demodisc baseline ancestry"
Assert-ContainsText $Source '25374214867e8e5deeb3cd9e19893caeb2278b64' "engine baseline ancestry"
Assert-ContainsText $Source 'b9590af9bece2c824b3531bdc5cc237853879188' "Windows baseline ancestry"
Assert-ContainsText $Source '87958b3ac4f5b2e6221551874e8bf0a7a5620254' "codegen baseline ancestry"
Assert-ContainsText $Source 'Assert-IsolatedWorktree -RepositoryRootPath $DemoWorktreeRootPath -BaselineCommit $DemoBaselineCommit' "Demodisc path-to-baseline association"
Assert-ContainsText $Source 'Assert-IsolatedWorktree -RepositoryRootPath $EngineWorktreeRootPath -BaselineCommit $EngineBaselineCommit' "engine path-to-baseline association"
Assert-ContainsText $Source 'Assert-IsolatedWorktree -RepositoryRootPath $WindowsWorktreeRootPath -BaselineCommit $WindowsBaselineCommit' "Windows path-to-baseline association"
Assert-ContainsText $Source 'Assert-IsolatedWorktree -RepositoryRootPath $CodegenWorktreeRootPath -BaselineCommit $CodegenBaselineCommit' "codegen path-to-baseline association"
Assert-ContainsText $Source 'Get-MainRepositorySnapshot' "four-main snapshot capture"
Assert-ContainsCount $Source 'Get-MainRepositorySnapshot -RepositoryRootPath $' 4 "exactly four main snapshot invocations"
Assert-ContainsText $Source 'Assert-MainRepositorySnapshotsUnchanged' "four-main finally comparison"
Assert-ContainsText $Source 'hash-object' "source scene blob verification"
Assert-ContainsText $Source 'platforms.json' "temporary platform installation manifest"
Assert-ContainsText $Source 'HELENGINE_SHARED_USER_SETTINGS_ROOT' "isolated platform settings override"
Assert-ContainsText $Source 'codegen\codegen.csproj' "isolated release codegen build"
Assert-ContainsText $Source '& dotnet build $CodegenProjectPath -c Release' "release codegen configuration"
Assert-ContainsText $Source 'builder\helengine.windows.builder.csproj' "isolated Windows builder build"
Assert-ContainsText $Source '& dotnet build $WindowsBuilderProjectPath -c Debug --property:HelEngineRoot=$EngineWorktreeRootPath' "debug builder configuration and isolated engine reference"
Assert-ContainsText $Source 'scripts\build-platform.ps1' "isolated engine build wrapper"
Assert-ContainsText $Source '-Configuration Release' "release player configuration"
Assert-ContainsText $Source '-BuildProfile release' "release player profile"
Assert-ContainsText $Source '-WorkspaceRoot' "visible explicit build workspace"
Assert-ContainsText $Source 'C:\dev\helworks\builds\helphysics-demodisc-stack-test' "visible workspace-owned output root"
Assert-ContainsText $Source 'C:\dev\helprojs\output\windows-helphysics-stack-test' "documented final package output"
Assert-ContainsText $Source '[Guid]::NewGuid().ToString("N").Substring(0, 8)' "unique build root generation"
Assert-ContainsText $Source '$OutputRootPath = Assert-SafeWorkspaceOwnedPath -TargetPath (Join-Path $BuildRootPath "output") -WorkspaceOwnedRootPath $BuildRootPath' "unique contained output path"
Assert-ContainsText $Source 'builderAssemblyPath = $WindowsBuilderAssemblyPath' "isolated manifest builder assembly"
Assert-ContainsText $Source 'playerSourceRootPath = $WindowsWorktreeRootPath' "isolated manifest player source"
Assert-ContainsText $Source 'generatedCoreCppRootPath = $DescriptorGeneratedCoreRootPath' "contained descriptor generated-core root"
Assert-ContainsText $Source 'codegenToolPath = $CodegenToolPath' "isolated manifest codegen tool"
Assert-ContainsText $Source 'Publish-VerifiedPackage' "transactional final package publication"
Assert-ContainsText $Source 'Get-Command powershell.exe -CommandType Application' "caller-independent Windows PowerShell resolution"
Assert-ContainsText $Source 'cpp-conversion-report.json' "conversion report validation"
Assert-ContainsText $Source 'hasErrors' "conversion error rejection"
Assert-ContainsText $Source 'runtime-physics-backend.json' "packaged backend evidence"
Assert-ContainsText $Source 'fixedStepHertz' "20 Hz backend validation"
Assert-ContainsText $Source 'velocityIterations' "4-iteration backend validation"
Assert-ContainsText $Source 'penetrationCorrectionPasses' "1-pass backend validation"
Assert-ContainsText $Source 'windows-build-manifest.json' "one-scene build manifest validation"
Assert-ContainsText $Source 'test_scene_dynamic_stack_boxes' "exact startup scene validation"
Assert-ContainsText $Source 'HelPhysicsRuntimeComponentRegistration::RegisterConfigured' "generated HelPhysics registration evidence"
Assert-ContainsText $Source 'Physics3DRuntimeComponentRegistration::Register' "generated BEPU registration rejection"
Assert-ContainsText $Source 'helengine_windows.exe' "executable existence validation"
Assert-ContainsText $Source 'Assert-SafeWorkspaceOwnedPath' "recursive cleanup containment guard"
Assert-ContainsText $Source '$BuildFailure = $_' "primary build failure preservation"
Assert-ContainsText $Source '$AuditFailure = $_' "main-checkout audit failure preservation"
Assert-DoesNotContainText $Source 'Start-Process' "the build script must never launch the player"
Assert-DoesNotContainText $Source 'Invoke-Item' "the build script must never open the player"
Assert-DoesNotContainText $Source '& $ExecutablePath' "the build script must never invoke the produced executable"
Assert-DoesNotContainText $Source '& (Join-Path $OutputRootPath' "the build script must never invoke an output-root executable expression"
Assert-DoesNotContainText $Source 'AppData\Local\Temp' "agent-controlled output must not use the system temp tree"
Assert-DoesNotContainText $Source '[System.IO.Path]::GetTempPath' "agent-controlled output must not use GetTempPath"

Write-Host "Guarded HelPhysics Windows build script contract passed."
