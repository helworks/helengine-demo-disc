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

function Get-BuildScriptFunctionDefinition {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourcePath,

        [Parameter(Mandatory = $true)]
        [string]$FunctionName
    )

    $Tokens = $null
    $ParseErrors = $null
    $SyntaxTree = [System.Management.Automation.Language.Parser]::ParseFile($SourcePath, [ref]$Tokens, [ref]$ParseErrors)
    if ($ParseErrors.Count -ne 0) {
        throw "Unable to parse build script for functional contract tests: $($ParseErrors[0].Message)"
    }

    $Definitions = @($SyntaxTree.FindAll({
        param($Node)
        return $Node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $Node.Name -ceq $FunctionName
    }, $true))
    if ($Definitions.Count -ne 1) {
        throw "Expected one function named '$FunctionName' in '$SourcePath', but found $($Definitions.Count)."
    }

    return [scriptblock]::Create($Definitions[0].Extent.Text)
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
Assert-ContainsText $Source 'Get-MainRepositorySnapshot -RepositoryRootPath $DemoMainRootPath' "Demodisc main snapshot association"
Assert-ContainsText $Source 'Get-MainRepositorySnapshot -RepositoryRootPath $EngineMainRootPath' "engine main snapshot association"
Assert-ContainsText $Source 'Get-MainRepositorySnapshot -RepositoryRootPath $WindowsMainRootPath' "Windows main snapshot association"
Assert-ContainsText $Source 'Get-MainRepositorySnapshot -RepositoryRootPath $CodegenMainRootPath' "codegen main snapshot association"
Assert-ContainsCount $Source 'Get-MainRepositorySnapshot -RepositoryRootPath $' 4 "exactly four distinct main snapshot invocations"
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
Assert-ContainsText $Source 'Start-VerifiedPackagePublication' "two-phase final package publication"
Assert-ContainsText $Source 'Undo-VerifiedPackagePublication' "failed publication rollback"
Assert-ContainsText $Source 'Complete-VerifiedPackagePublication' "post-audit publication completion"
Assert-ContainsText $Source '[System.IO.FileShare]::None' "exclusive final-package publication lock"
Assert-ContainsText $Source '.helengine-publication-owner' "per-publication destination ownership marker"
Assert-ContainsCount $Source 'Assert-VerifiedPackagePublicationOwnership -Publication $Publication' 2 "ownership checks before rollback and completion moves"
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
Assert-DoesNotContainText $Source '& $VerificationExecutablePath' "the build script must never invoke the verified workspace executable"
Assert-DoesNotContainText $Source '& (Join-Path $OutputRootPath' "the build script must never invoke an output-root executable expression"
Assert-DoesNotContainText $Source 'Remove-Item' "the build script must preserve publication candidates and backups without recursive deletion"
Assert-DoesNotContainText $Source 'AppData\Local\Temp' "agent-controlled output must not use the system temp tree"
Assert-DoesNotContainText $Source '[System.IO.Path]::GetTempPath' "agent-controlled output must not use GetTempPath"

$PublicationFunctionNames = @(
    "Assert-SafeWorkspaceOwnedPath",
    "Enter-VerifiedPackagePublicationLock",
    "Exit-VerifiedPackagePublicationLock",
    "Assert-VerifiedPackagePublicationOwnership",
    "Start-VerifiedPackagePublication",
    "Undo-VerifiedPackagePublication",
    "Complete-VerifiedPackagePublication",
    "Invoke-VerifiedPackagePublicationLifecycle"
)
foreach ($FunctionName in $PublicationFunctionNames) {
    . (Get-BuildScriptFunctionDefinition -SourcePath $BuildScriptPath -FunctionName $FunctionName)
}

$FunctionalTestRootPath = Join-Path "C:\dev\helworks\builds\helphysics-demodisc-stack-test\publication-contract-tests" ([Guid]::NewGuid().ToString("N"))
$null = New-Item -ItemType Directory -Path $FunctionalTestRootPath
$LockDestinationRootPath = Join-Path $FunctionalTestRootPath "locked-final-package"
$FirstPublicationLock = Enter-VerifiedPackagePublicationLock `
    -DestinationRootPath $LockDestinationRootPath `
    -DestinationParentRootPath $FunctionalTestRootPath
$SecondLockFailure = $null
try {
    try {
        $null = Enter-VerifiedPackagePublicationLock `
            -DestinationRootPath $LockDestinationRootPath `
            -DestinationParentRootPath $FunctionalTestRootPath
    } catch {
        $SecondLockFailure = $_
    }

    if ($null -eq $SecondLockFailure) {
        throw "A second publication unexpectedly acquired the same final-package lock."
    }
} finally {
    Exit-VerifiedPackagePublicationLock -PublicationLock $FirstPublicationLock
}

$ReacquiredPublicationLock = Enter-VerifiedPackagePublicationLock `
    -DestinationRootPath $LockDestinationRootPath `
    -DestinationParentRootPath $FunctionalTestRootPath
Exit-VerifiedPackagePublicationLock -PublicationLock $ReacquiredPublicationLock

$LifecycleWorkspaceRootPath = Join-Path $FunctionalTestRootPath "lifecycle-workspace"
$LifecycleVerifiedOutputRootPath = Join-Path $FunctionalTestRootPath "lifecycle-verified-output"
$LifecycleDestinationRootPath = Join-Path $FunctionalTestRootPath "lifecycle-final-package"
$null = New-Item -ItemType Directory -Path $LifecycleWorkspaceRootPath, $LifecycleVerifiedOutputRootPath
Set-Content -LiteralPath (Join-Path $LifecycleVerifiedOutputRootPath "artifact.txt") -Value "verified" -NoNewline
$LifecycleObservations = [pscustomobject]@{
    ValidationLockWasHeld = $false
    AuditLockWasHeld = $false
}
$LifecyclePublication = Invoke-VerifiedPackagePublicationLifecycle `
    -VerifiedOutputRootPath $LifecycleVerifiedOutputRootPath `
    -DestinationRootPath $LifecycleDestinationRootPath `
    -DestinationParentRootPath $FunctionalTestRootPath `
    -WorkspaceOwnedRootPath $LifecycleWorkspaceRootPath `
    -ValidatePublishedPackage {
        param($PublishedOutputRootPath)
        if (-not (Test-Path -LiteralPath (Join-Path $PublishedOutputRootPath "artifact.txt") -PathType Leaf)) {
            throw "Lifecycle validation did not receive the promoted package."
        }

        $CompetingLock = $null
        $CompetingLockFailure = $null
        try {
            $CompetingLock = Enter-VerifiedPackagePublicationLock `
                -DestinationRootPath $LifecycleDestinationRootPath `
                -DestinationParentRootPath $FunctionalTestRootPath
        } catch {
            $CompetingLockFailure = $_
        }
        if ($null -ne $CompetingLock) {
            Exit-VerifiedPackagePublicationLock -PublicationLock $CompetingLock
            throw "Validation acquired the publication lock before the transaction completed."
        }
        if ($null -eq $CompetingLockFailure) {
            throw "Validation did not observe publication lock contention."
        }

        $LifecycleObservations.ValidationLockWasHeld = $true
    } `
    -AuditMainRepositories {
        $CompetingLock = $null
        $CompetingLockFailure = $null
        try {
            $CompetingLock = Enter-VerifiedPackagePublicationLock `
                -DestinationRootPath $LifecycleDestinationRootPath `
                -DestinationParentRootPath $FunctionalTestRootPath
        } catch {
            $CompetingLockFailure = $_
        }
        if ($null -ne $CompetingLock) {
            Exit-VerifiedPackagePublicationLock -PublicationLock $CompetingLock
            throw "Audit acquired the publication lock before the transaction completed."
        }
        if ($null -eq $CompetingLockFailure) {
            throw "Audit did not observe publication lock contention."
        }

        $LifecycleObservations.AuditLockWasHeld = $true
    }
if (-not $LifecycleObservations.ValidationLockWasHeld -or
    -not $LifecycleObservations.AuditLockWasHeld) {
    throw "The publication lifecycle did not hold its lock through validation and audit."
}
Assert-VerifiedPackagePublicationOwnership -Publication $LifecyclePublication

$BlockedLifecycleDestinationRootPath = Join-Path $FunctionalTestRootPath "blocked-lifecycle-final-package"
$BlockingLifecycleLock = Enter-VerifiedPackagePublicationLock `
    -DestinationRootPath $BlockedLifecycleDestinationRootPath `
    -DestinationParentRootPath $FunctionalTestRootPath
$BlockedLifecycleObservations = [pscustomobject]@{
    AuditWasRun = $false
}
$BlockedLifecycleFailure = $null
try {
    try {
        $null = Invoke-VerifiedPackagePublicationLifecycle `
            -VerifiedOutputRootPath $LifecycleVerifiedOutputRootPath `
            -DestinationRootPath $BlockedLifecycleDestinationRootPath `
            -DestinationParentRootPath $FunctionalTestRootPath `
            -WorkspaceOwnedRootPath $LifecycleWorkspaceRootPath `
            -ValidatePublishedPackage {
                param($PublishedOutputRootPath)
                throw "A lock-blocked publication unexpectedly reached validation for '$PublishedOutputRootPath'."
            } `
            -AuditMainRepositories {
                $BlockedLifecycleObservations.AuditWasRun = $true
            }
    } catch {
        $BlockedLifecycleFailure = $_
    }
} finally {
    Exit-VerifiedPackagePublicationLock -PublicationLock $BlockingLifecycleLock
}
if ($null -eq $BlockedLifecycleFailure -or -not $BlockedLifecycleObservations.AuditWasRun) {
    throw "A lock acquisition failure did not preserve the final main-repository audit."
}

$OwnershipWorkspaceRootPath = Join-Path $FunctionalTestRootPath "ownership-workspace"
$OwnershipDestinationRootPath = Join-Path $FunctionalTestRootPath "owned-final-package"
$OwnershipBackupRootPath = Join-Path $FunctionalTestRootPath "owned-backup-package"
$null = New-Item -ItemType Directory -Path $OwnershipWorkspaceRootPath, $OwnershipDestinationRootPath, $OwnershipBackupRootPath
Set-Content -LiteralPath (Join-Path $OwnershipDestinationRootPath ".helengine-publication-owner") -Value "replacement-publication" -NoNewline
$MismatchedPublication = [pscustomobject]@{
    PublicationId = "original-publication"
    DestinationRootPath = $OwnershipDestinationRootPath
    BackupRootPath = $OwnershipBackupRootPath
    HadPreviousPackage = $true
    WorkspaceOwnedRootPath = $OwnershipWorkspaceRootPath
}
$OwnershipFailure = $null
try {
    Undo-VerifiedPackagePublication -Publication $MismatchedPublication
} catch {
    $OwnershipFailure = $_
}

if ($null -eq $OwnershipFailure) {
    throw "Rollback unexpectedly moved a final package owned by another publication."
}
if (-not (Test-Path -LiteralPath $OwnershipDestinationRootPath -PathType Container) -or
    -not (Test-Path -LiteralPath $OwnershipBackupRootPath -PathType Container)) {
    throw "Ownership rejection changed a destination or backup package before failing."
}

$CompletionWorkspaceRootPath = Join-Path $FunctionalTestRootPath "completion-workspace"
$CompletionDestinationRootPath = Join-Path $FunctionalTestRootPath "completion-final-package"
$CompletionBackupRootPath = Join-Path $FunctionalTestRootPath "completion-backup-package"
$null = New-Item -ItemType Directory -Path $CompletionWorkspaceRootPath, $CompletionDestinationRootPath, $CompletionBackupRootPath
Set-Content -LiteralPath (Join-Path $CompletionDestinationRootPath ".helengine-publication-owner") -Value "replacement-publication" -NoNewline
$MismatchedCompletion = [pscustomobject]@{
    PublicationId = "original-publication"
    DestinationRootPath = $CompletionDestinationRootPath
    BackupRootPath = $CompletionBackupRootPath
    HadPreviousPackage = $true
    WorkspaceOwnedRootPath = $CompletionWorkspaceRootPath
}
$CompletionOwnershipFailure = $null
try {
    Complete-VerifiedPackagePublication -Publication $MismatchedCompletion
} catch {
    $CompletionOwnershipFailure = $_
}

if ($null -eq $CompletionOwnershipFailure) {
    throw "Completion unexpectedly archived a backup for a final package owned by another publication."
}
if (-not (Test-Path -LiteralPath $CompletionDestinationRootPath -PathType Container) -or
    -not (Test-Path -LiteralPath $CompletionBackupRootPath -PathType Container)) {
    throw "Completion ownership rejection changed a destination or backup package before failing."
}

Write-Host "Guarded HelPhysics Windows build script contract passed."

# Every generated module and its packaged copy must be checked.
. (Get-BuildScriptFunctionDefinition -SourcePath $BuildScriptPath -FunctionName 'Assert-ConversionReport')
. (Get-BuildScriptFunctionDefinition -SourcePath $BuildScriptPath -FunctionName 'Assert-ConversionReports')
$ReportTestRoot = Join-Path $FunctionalTestRootPath 'conversion-reports'
$null = New-Item -ItemType Directory -Path $ReportTestRoot
$EmptyRejected = $false
try { Assert-ConversionReports -SearchRootPath $ReportTestRoot } catch { $EmptyRejected = $true }
if (-not $EmptyRejected) { throw 'Missing conversion reports must fail verification.' }
foreach ($Module in @('generated-core', 'code/gameplay', 'package/code/gameplay')) {
    $ModuleRoot = Join-Path $ReportTestRoot $Module
    $null = New-Item -ItemType Directory -Force -Path $ModuleRoot
    Set-Content -LiteralPath (Join-Path $ModuleRoot 'cpp-conversion-report.json') -Value '{"hasErrors":false,"errorCount":0,"diagnostics":[]}'
}
$VerifiedReports = @(Assert-ConversionReports -SearchRootPath $ReportTestRoot)
if ($VerifiedReports.Count -ne 3) { throw 'All three conversion reports must be verified.' }
Set-Content -LiteralPath (Join-Path $ReportTestRoot 'package/code/gameplay/cpp-conversion-report.json') -Value '{"hasErrors":true,"errorCount":1,"diagnostics":[{"severity":"Error"}]}'
$ErrorRejected = $false
try { Assert-ConversionReports -SearchRootPath $ReportTestRoot } catch { $ErrorRejected = $true }
if (-not $ErrorRejected) { throw 'An error in any conversion report must fail verification.' }
Write-Host 'Multiple conversion report regression tests passed.'
