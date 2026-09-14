[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ExpectedBranchName = "codex/helphysics-demodisc-stack-test"
$DemoMainRootPath = "C:\dev\helprojs\demodisc"
$EngineMainRootPath = "C:\dev\helworks\helengine"
$WindowsMainRootPath = "C:\dev\helworks\helengine-windows"
$CodegenMainRootPath = "C:\dev\helworks\csharpcodegen"
$DemoWorktreeRootPath = "C:\dev\helprojs\demodisc\.worktrees\helphysics-demodisc-stack-test"
$EngineWorktreeRootPath = "C:\dev\helworks\helengine\.worktrees\helphysics-demodisc-stack-test"
$WindowsWorktreeRootPath = "C:\dev\helworks\helengine-windows\.worktrees\helphysics-demodisc-stack-test"
$CodegenWorktreeRootPath = "C:\dev\helworks\csharpcodegen\.worktrees\helphysics-demodisc-stack-test"
$DemoBaselineCommit = "d716776b6ec6d719c7448fe57652cf342d0b09d6"
$EngineBaselineCommit = "25374214867e8e5deeb3cd9e19893caeb2278b64"
$WindowsBaselineCommit = "b9590af9bece2c824b3531bdc5cc237853879188"
$CodegenBaselineCommit = "87958b3ac4f5b2e6221551874e8bf0a7a5620254"
$VisibleBuildRootPath = "C:\dev\helworks\builds\helphysics-demodisc-stack-test"
$FinalOutputParentRootPath = "C:\dev\helprojs\output"
$FinalOutputRootPath = "C:\dev\helprojs\output\windows-helphysics-stack-test"
$StackSceneRelativePath = "assets/scenes/physics/test_scene_dynamic_stack_boxes.helen"
$StackSceneId = "test_scene_dynamic_stack_boxes"

function Invoke-GitText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRootPath,

        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    $Output = & git -C $RepositoryRootPath @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "git -C '$RepositoryRootPath' $($Arguments -join ' ') failed: $($Output -join [Environment]::NewLine)"
    }

    return ($Output -join "`n").TrimEnd()
}

function Get-MainRepositorySnapshot {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRootPath
    )

    return [pscustomobject]@{
        RepositoryRootPath = [System.IO.Path]::GetFullPath($RepositoryRootPath)
        Head = Invoke-GitText -RepositoryRootPath $RepositoryRootPath -Arguments @("rev-parse", "HEAD")
        Status = Invoke-GitText -RepositoryRootPath $RepositoryRootPath -Arguments @("status", "--porcelain=v1", "--untracked-files=all")
    }
}

function Assert-MainRepositorySnapshotsUnchanged {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Snapshots
    )

    $ChangedRepositories = New-Object System.Collections.Generic.List[string]
    foreach ($Snapshot in $Snapshots) {
        $CurrentHead = Invoke-GitText -RepositoryRootPath $Snapshot.RepositoryRootPath -Arguments @("rev-parse", "HEAD")
        $CurrentStatus = Invoke-GitText -RepositoryRootPath $Snapshot.RepositoryRootPath -Arguments @("status", "--porcelain=v1", "--untracked-files=all")
        if ($CurrentHead -cne $Snapshot.Head -or $CurrentStatus -cne $Snapshot.Status) {
            $ChangedRepositories.Add($Snapshot.RepositoryRootPath)
        }
    }

    if ($ChangedRepositories.Count -gt 0) {
        throw "Main checkout state changed during the isolated build: $($ChangedRepositories -join ', ')."
    }
}

function Assert-IsolatedWorktree {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRootPath,

        [Parameter(Mandatory = $true)]
        [string]$BaselineCommit
    )

    $BranchName = Invoke-GitText -RepositoryRootPath $RepositoryRootPath -Arguments @("branch", "--show-current")
    if ($BranchName -cne $ExpectedBranchName) {
        throw "Worktree '$RepositoryRootPath' is on '$BranchName' instead of '$ExpectedBranchName'."
    }

    $null = & git -C $RepositoryRootPath merge-base --is-ancestor $BaselineCommit HEAD 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Worktree '$RepositoryRootPath' does not descend from required baseline '$BaselineCommit'."
    }

    $WorktreeStatus = Invoke-GitText -RepositoryRootPath $RepositoryRootPath -Arguments @("status", "--porcelain=v1", "--untracked-files=all")
    if (-not [string]::IsNullOrWhiteSpace($WorktreeStatus)) {
        throw "Worktree '$RepositoryRootPath' has uncommitted source changes: $WorktreeStatus"
    }
}

function Assert-SafeWorkspaceOwnedPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$TargetPath,

        [Parameter(Mandatory = $true)]
        [string]$WorkspaceOwnedRootPath
    )

    $FullOwnedRootPath = [System.IO.Path]::GetFullPath($WorkspaceOwnedRootPath).TrimEnd('\', '/')
    $FullTargetPath = [System.IO.Path]::GetFullPath($TargetPath).TrimEnd('\', '/')
    $RequiredPrefix = $FullOwnedRootPath + [System.IO.Path]::DirectorySeparatorChar
    if ($FullTargetPath -ceq $FullOwnedRootPath -or
        -not $FullTargetPath.StartsWith($RequiredPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Path '$FullTargetPath' is not a child of workspace-owned root '$FullOwnedRootPath'."
    }

    return $FullTargetPath
}

function Invoke-CheckedCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$DisplayName,

        [Parameter(Mandatory = $true)]
        [scriptblock]$Command
    )

    Write-Host "[build] $DisplayName"
    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "$DisplayName failed with exit code $LASTEXITCODE."
    }
}

function Get-OnlyFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SearchRootPath,

        [Parameter(Mandatory = $true)]
        [string]$FileName
    )

    $Matches = @(Get-ChildItem -LiteralPath $SearchRootPath -Recurse -File -Filter $FileName)
    if ($Matches.Count -ne 1) {
        throw "Expected exactly one '$FileName' beneath '$SearchRootPath', but found '$($Matches.Count)'."
    }

    return $Matches[0].FullName
}

function Enter-VerifiedPackagePublicationLock {
    param(
        [Parameter(Mandatory = $true)]
        [string]$DestinationRootPath,

        [Parameter(Mandatory = $true)]
        [string]$DestinationParentRootPath
    )

    $SafeDestinationRootPath = Assert-SafeWorkspaceOwnedPath -TargetPath $DestinationRootPath -WorkspaceOwnedRootPath $DestinationParentRootPath
    $LockFilePath = Assert-SafeWorkspaceOwnedPath `
        -TargetPath ($SafeDestinationRootPath + ".publication.lock") `
        -WorkspaceOwnedRootPath $DestinationParentRootPath
    $null = [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($LockFilePath))
    try {
        $LockStream = [System.IO.File]::Open(
            $LockFilePath,
            [System.IO.FileMode]::OpenOrCreate,
            [System.IO.FileAccess]::ReadWrite,
            [System.IO.FileShare]::None)
    } catch [System.IO.IOException] {
        throw "Another build owns the final-package publication lock '$LockFilePath'."
    }

    return [pscustomobject]@{
        LockFilePath = $LockFilePath
        LockStream = $LockStream
    }
}

function Exit-VerifiedPackagePublicationLock {
    param(
        [Parameter(Mandatory = $true)]
        [object]$PublicationLock
    )

    $PublicationLock.LockStream.Dispose()
}

function Assert-VerifiedPackagePublicationOwnership {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Publication
    )

    $OwnershipMarkerPath = Join-Path $Publication.DestinationRootPath ".helengine-publication-owner"
    if (-not (Test-Path -LiteralPath $OwnershipMarkerPath -PathType Leaf)) {
        throw "Published package ownership marker is missing from '$($Publication.DestinationRootPath)'."
    }

    $ActualPublicationId = (Get-Content -LiteralPath $OwnershipMarkerPath -Raw).Trim()
    if ($ActualPublicationId -cne $Publication.PublicationId) {
        throw "Published package '$($Publication.DestinationRootPath)' is owned by publication '$ActualPublicationId', not '$($Publication.PublicationId)'."
    }
}

function Start-VerifiedPackagePublication {
    param(
        [Parameter(Mandatory = $true)]
        [string]$VerifiedOutputRootPath,

        [Parameter(Mandatory = $true)]
        [string]$DestinationRootPath,

        [Parameter(Mandatory = $true)]
        [string]$DestinationParentRootPath,

        [Parameter(Mandatory = $true)]
        [string]$WorkspaceOwnedRootPath
    )

    $SafeDestinationRootPath = Assert-SafeWorkspaceOwnedPath -TargetPath $DestinationRootPath -WorkspaceOwnedRootPath $DestinationParentRootPath
    $PublishId = [Guid]::NewGuid().ToString("N")
    $StagingRootPath = Assert-SafeWorkspaceOwnedPath `
        -TargetPath (Join-Path $WorkspaceOwnedRootPath "final-package-staging") `
        -WorkspaceOwnedRootPath $WorkspaceOwnedRootPath
    $BackupRootPath = Assert-SafeWorkspaceOwnedPath `
        -TargetPath (Join-Path $DestinationParentRootPath ("windows-helphysics-stack-test.backup-" + $PublishId)) `
        -WorkspaceOwnedRootPath $DestinationParentRootPath
    $HadPreviousPackage = $false
    Copy-Item -LiteralPath $VerifiedOutputRootPath -Destination $StagingRootPath -Recurse
    Set-Content -LiteralPath (Join-Path $StagingRootPath ".helengine-publication-owner") -Value $PublishId -NoNewline
    if (Test-Path -LiteralPath $SafeDestinationRootPath) {
        Move-Item -LiteralPath $SafeDestinationRootPath -Destination $BackupRootPath
        $HadPreviousPackage = $true
    }

    try {
        Move-Item -LiteralPath $StagingRootPath -Destination $SafeDestinationRootPath
    } catch {
        $PromotionFailure = $_
        if ($HadPreviousPackage -and -not (Test-Path -LiteralPath $SafeDestinationRootPath)) {
            try {
                Move-Item -LiteralPath $BackupRootPath -Destination $SafeDestinationRootPath
            } catch {
                throw "Package promotion failed: $($PromotionFailure.Exception.Message) Restoring the previous package also failed: $($_.Exception.Message) The previous package remains at '$BackupRootPath'."
            }
        }

        throw $PromotionFailure
    }

    return [pscustomobject]@{
        PublicationId = $PublishId
        DestinationRootPath = $SafeDestinationRootPath
        BackupRootPath = $BackupRootPath
        HadPreviousPackage = $HadPreviousPackage
        WorkspaceOwnedRootPath = $WorkspaceOwnedRootPath
    }
}

function Undo-VerifiedPackagePublication {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Publication
    )

    Assert-VerifiedPackagePublicationOwnership -Publication $Publication
    $FailedPackageRootPath = Assert-SafeWorkspaceOwnedPath `
        -TargetPath (Join-Path $Publication.WorkspaceOwnedRootPath "failed-published-package") `
        -WorkspaceOwnedRootPath $Publication.WorkspaceOwnedRootPath
    if (Test-Path -LiteralPath $Publication.DestinationRootPath) {
        Move-Item -LiteralPath $Publication.DestinationRootPath -Destination $FailedPackageRootPath
    }
    if ($Publication.HadPreviousPackage) {
        Move-Item -LiteralPath $Publication.BackupRootPath -Destination $Publication.DestinationRootPath
    }
}

function Complete-VerifiedPackagePublication {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Publication
    )

    Assert-VerifiedPackagePublicationOwnership -Publication $Publication
    if (-not $Publication.HadPreviousPackage) {
        return
    }

    $ArchivedPackageRootPath = Assert-SafeWorkspaceOwnedPath `
        -TargetPath (Join-Path $Publication.WorkspaceOwnedRootPath "previous-final-package") `
        -WorkspaceOwnedRootPath $Publication.WorkspaceOwnedRootPath
    Move-Item -LiteralPath $Publication.BackupRootPath -Destination $ArchivedPackageRootPath
}

function Invoke-VerifiedPackagePublicationLifecycle {
    param(
        [Parameter(Mandatory = $true)]
        [string]$VerifiedOutputRootPath,

        [Parameter(Mandatory = $true)]
        [string]$DestinationRootPath,

        [Parameter(Mandatory = $true)]
        [string]$DestinationParentRootPath,

        [Parameter(Mandatory = $true)]
        [string]$WorkspaceOwnedRootPath,

        [Parameter(Mandatory = $true)]
        [scriptblock]$ValidatePublishedPackage,

        [Parameter(Mandatory = $true)]
        [scriptblock]$AuditMainRepositories
    )

    $PublicationLock = $null
    $Publication = $null
    $OperationFailure = $null
    $AuditFailure = $null
    $RollbackFailure = $null
    try {
        try {
            $PublicationLock = Enter-VerifiedPackagePublicationLock `
                -DestinationRootPath $DestinationRootPath `
                -DestinationParentRootPath $DestinationParentRootPath
            $Publication = Start-VerifiedPackagePublication `
                -VerifiedOutputRootPath $VerifiedOutputRootPath `
                -DestinationRootPath $DestinationRootPath `
                -DestinationParentRootPath $DestinationParentRootPath `
                -WorkspaceOwnedRootPath $WorkspaceOwnedRootPath
            & $ValidatePublishedPackage $Publication.DestinationRootPath
        } catch {
            $OperationFailure = $_
        }

        try {
            & $AuditMainRepositories
        } catch {
            $AuditFailure = $_
        }

        if ($null -ne $OperationFailure -or $null -ne $AuditFailure) {
            if ($null -ne $Publication) {
                try {
                    Undo-VerifiedPackagePublication -Publication $Publication
                } catch {
                    $RollbackFailure = $_
                }
            }

            $FailureMessages = New-Object System.Collections.Generic.List[string]
            if ($null -ne $OperationFailure) {
                $FailureMessages.Add("Final package publication failed: $($OperationFailure.Exception.Message)")
            }
            if ($null -ne $AuditFailure) {
                $FailureMessages.Add("The final main-checkout audit failed: $($AuditFailure.Exception.Message)")
            }
            if ($null -ne $RollbackFailure) {
                $FailureMessages.Add("Restoring the previous final package failed: $($RollbackFailure.Exception.Message)")
            }

            throw ($FailureMessages -join " ")
        }

        Complete-VerifiedPackagePublication -Publication $Publication
        return $Publication
    } finally {
        if ($null -ne $PublicationLock) {
            Exit-VerifiedPackagePublicationLock -PublicationLock $PublicationLock
        }
    }
}

function Assert-ConversionReport {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ReportPath
    )

    $Report = Get-Content -LiteralPath $ReportPath -Raw | ConvertFrom-Json
    $ErrorDiagnostics = @($Report.diagnostics | Where-Object { $_.severity -eq "Error" })
    if ($Report.hasErrors -or [int]$Report.errorCount -ne 0 -or $ErrorDiagnostics.Count -ne 0) {
        throw "Generated C++ conversion report contains errors: '$ReportPath'."
    }
}

# Validate every generated module report, including packaged copies.
function Assert-ConversionReports {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SearchRootPath
    )

    $Reports = @(Get-ChildItem -LiteralPath $SearchRootPath -Recurse -File -Filter 'cpp-conversion-report.json')
    if ($Reports.Count -eq 0) {
        throw "No C++ conversion reports found beneath '$SearchRootPath'."
    }
    foreach ($Report in $Reports) {
        Assert-ConversionReport -ReportPath $Report.FullName
    }
    return $Reports.FullName
}

function Assert-BackendManifest {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ManifestPath
    )

    $Manifest = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
    if ($Manifest.backend -cne "helphysics" -or
        [int]$Manifest.fixedStepHertz -ne 20 -or
        [int]$Manifest.velocityIterations -ne 4 -or
        [int]$Manifest.penetrationCorrectionPasses -ne 1) {
        throw "Packaged runtime physics manifest does not describe HelPhysics 20/4/1: '$ManifestPath'."
    }
}

function Assert-OneSceneBuildManifest {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ManifestPath
    )

    $Manifest = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
    $Scenes = @($Manifest.Scenes)
    if ($Manifest.StartupSceneId -cne $StackSceneId -or
        $Scenes.Count -ne 1 -or
        $Scenes[0].ItemId -cne $StackSceneId) {
        throw "Windows build manifest does not contain exactly '$StackSceneId' as its only startup scene: '$ManifestPath'."
    }
}

function Assert-GeneratedBackendSelection {
    param(
        [Parameter(Mandatory = $true)]
        [string]$GeneratedManifestHeaderPath
    )

    $HeaderSource = Get-Content -LiteralPath $GeneratedManifestHeaderPath -Raw
    if ($HeaderSource.IndexOf("HELENGINE_RUNTIME_PHYSICS_BACKEND_HELPHYSICS", [System.StringComparison]::Ordinal) -lt 0 -or
        $HeaderSource.IndexOf("HELENGINE_RUNTIME_PHYSICS_BACKEND_BEPU", [System.StringComparison]::Ordinal) -ge 0) {
        throw "Generated native backend header is not exclusively HelPhysics: '$GeneratedManifestHeaderPath'."
    }

    $GeneratedCoreRootPath = Split-Path -Parent (Split-Path -Parent $GeneratedManifestHeaderPath)
    $HelPhysicsRegistrationSourcePath = Join-Path $GeneratedCoreRootPath "HelPhysicsRuntimeComponentRegistration.cpp"
    if (-not (Test-Path -LiteralPath $HelPhysicsRegistrationSourcePath -PathType Leaf)) {
        throw "Generated HelPhysics registration source is missing from '$GeneratedCoreRootPath'."
    }

    $HelPhysicsRegistrationCall = "HelPhysicsRuntimeComponentRegistration::RegisterConfigured"
    $WindowsHostSourcePath = Join-Path $WindowsWorktreeRootPath "src\platform\windows\win32\win32_application.cpp"
    $WindowsHostSource = Get-Content -LiteralPath $WindowsHostSourcePath -Raw
    if ($WindowsHostSource.IndexOf($HelPhysicsRegistrationCall, [System.StringComparison]::Ordinal) -lt 0) {
        throw "Windows host does not contain the selected HelPhysics registration call."
    }

    $ForbiddenBepuRegistrationCall = "Physics3DRuntimeComponentRegistration::Register"
    $GeneratedBepuEvidence = @(Get-ChildItem -LiteralPath $GeneratedCoreRootPath -Recurse -File -Include "*.cpp", "*.hpp" |
        Select-String -SimpleMatch $ForbiddenBepuRegistrationCall)
    if ($GeneratedBepuEvidence.Count -ne 0) {
        throw "HelPhysics generated core contains BEPU registration evidence."
    }
}

$MainRepositorySnapshots = @(
    Get-MainRepositorySnapshot -RepositoryRootPath $DemoMainRootPath
    Get-MainRepositorySnapshot -RepositoryRootPath $EngineMainRootPath
    Get-MainRepositorySnapshot -RepositoryRootPath $WindowsMainRootPath
    Get-MainRepositorySnapshot -RepositoryRootPath $CodegenMainRootPath
)

$OriginalSharedUserSettingsRoot = $env:HELENGINE_SHARED_USER_SETTINGS_ROOT
$BuildFailure = $null
$AuditFailure = $null
$Publication = $null
$BuildRootPath = $null
$ExecutablePath = $null
$BackendManifestPath = $null
$WindowsBuildManifestPath = $null
$ConversionReportPaths = @()
try {
    Assert-IsolatedWorktree -RepositoryRootPath $DemoWorktreeRootPath -BaselineCommit $DemoBaselineCommit
    Assert-IsolatedWorktree -RepositoryRootPath $EngineWorktreeRootPath -BaselineCommit $EngineBaselineCommit
    Assert-IsolatedWorktree -RepositoryRootPath $WindowsWorktreeRootPath -BaselineCommit $WindowsBaselineCommit
    Assert-IsolatedWorktree -RepositoryRootPath $CodegenWorktreeRootPath -BaselineCommit $CodegenBaselineCommit

    $ExpectedSceneBlob = Invoke-GitText -RepositoryRootPath $DemoWorktreeRootPath -Arguments @("rev-parse", ($DemoBaselineCommit + ":" + $StackSceneRelativePath))
    $SourceScenePath = Join-Path $DemoWorktreeRootPath ($StackSceneRelativePath.Replace('/', '\'))
    $CurrentSceneBlob = Invoke-GitText -RepositoryRootPath $DemoWorktreeRootPath -Arguments @("hash-object", "--", $SourceScenePath)
    if ($CurrentSceneBlob -cne $ExpectedSceneBlob) {
        throw "Authored stack scene differs from baseline '$DemoBaselineCommit'."
    }

    $BuildId = (Get-Date -Format "yyyyMMdd-HHmmssfff") + "-" + [Guid]::NewGuid().ToString("N").Substring(0, 8)
    $BuildRootPath = Assert-SafeWorkspaceOwnedPath -TargetPath (Join-Path $VisibleBuildRootPath $BuildId) -WorkspaceOwnedRootPath $VisibleBuildRootPath
    $WorkspaceRootPath = Assert-SafeWorkspaceOwnedPath -TargetPath (Join-Path $BuildRootPath "workspace") -WorkspaceOwnedRootPath $BuildRootPath
    $OutputRootPath = Assert-SafeWorkspaceOwnedPath -TargetPath (Join-Path $BuildRootPath "output") -WorkspaceOwnedRootPath $BuildRootPath
    $SharedUserSettingsRootPath = Assert-SafeWorkspaceOwnedPath -TargetPath (Join-Path $BuildRootPath "user-settings") -WorkspaceOwnedRootPath $BuildRootPath
    $DescriptorGeneratedCoreRootPath = Assert-SafeWorkspaceOwnedPath -TargetPath (Join-Path $BuildRootPath "descriptor-generated-core") -WorkspaceOwnedRootPath $BuildRootPath
    $null = New-Item -ItemType Directory -Path $WorkspaceRootPath, $OutputRootPath, $SharedUserSettingsRootPath, $DescriptorGeneratedCoreRootPath

    $CodegenProjectPath = Join-Path $CodegenWorktreeRootPath "codegen\codegen.csproj"
    Invoke-CheckedCommand -DisplayName "isolated codegen Release" -Command {
        & dotnet build $CodegenProjectPath -c Release
    }
    $CodegenToolPath = Join-Path $CodegenWorktreeRootPath "codegen\bin\Release\net9.0\codegen.exe"
    if (-not (Test-Path -LiteralPath $CodegenToolPath -PathType Leaf)) {
        throw "Isolated release codegen executable was not produced at '$CodegenToolPath'."
    }

    $WindowsBuilderProjectPath = Join-Path $WindowsWorktreeRootPath "builder\helengine.windows.builder.csproj"
    Invoke-CheckedCommand -DisplayName "isolated Windows builder Debug" -Command {
        & dotnet build $WindowsBuilderProjectPath -c Debug --property:HelEngineRoot=$EngineWorktreeRootPath
    }
    $WindowsBuilderAssemblyPath = Join-Path $WindowsWorktreeRootPath "builder\bin\Debug\net9.0\helengine.windows.builder.dll"
    if (-not (Test-Path -LiteralPath $WindowsBuilderAssemblyPath -PathType Leaf)) {
        throw "Isolated Windows builder assembly was not produced at '$WindowsBuilderAssemblyPath'."
    }

    $ProjectDocument = Get-Content -LiteralPath (Join-Path $DemoWorktreeRootPath "project.heproj") -Raw | ConvertFrom-Json
    $PlatformManifest = @{
        platforms = @(
            @{
                engineVersion = $ProjectDocument.requiredEngineVersion
                platformId = "windows"
                displayName = "Windows DirectX HelPhysics Stack Proof"
                builderAssemblyPath = $WindowsBuilderAssemblyPath
                playerSourceRootPath = $WindowsWorktreeRootPath
                generatedCoreCppRootPath = $DescriptorGeneratedCoreRootPath
                codegenToolPath = $CodegenToolPath
            }
        )
    }
    $PlatformManifestPath = Join-Path $SharedUserSettingsRootPath "platforms.json"
    $PlatformManifest | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $PlatformManifestPath -Encoding UTF8
    $env:HELENGINE_SHARED_USER_SETTINGS_ROOT = $SharedUserSettingsRootPath

    $EngineBuildScriptPath = Join-Path $EngineWorktreeRootPath "scripts\build-platform.ps1"
    $EditorProjectPath = Join-Path $EngineWorktreeRootPath "helengine.ui\helengine.editor.app\helengine.editor.app.csproj"
    $PowerShellExecutablePath = (Get-Command powershell.exe -CommandType Application -ErrorAction Stop).Path
    Invoke-CheckedCommand -DisplayName "guarded isolated Windows package" -Command {
        & $PowerShellExecutablePath -NoProfile -ExecutionPolicy Bypass -File $EngineBuildScriptPath `
            -Project (Join-Path $DemoWorktreeRootPath "project.heproj") `
            -Platform windows `
            -Configuration Release `
            -BuildProfile release `
            -Output $OutputRootPath `
            -EditorProject $EditorProjectPath `
            -WorkspaceRoot $WorkspaceRootPath
    }

    $ConversionReportPaths = @(Assert-ConversionReports -SearchRootPath $WorkspaceRootPath)
    $BackendManifestPath = Join-Path $OutputRootPath "runtime\runtime-physics-backend.json"
    Assert-BackendManifest -ManifestPath $BackendManifestPath
    $WindowsBuildManifestPath = Get-OnlyFile -SearchRootPath $WorkspaceRootPath -FileName "windows-build-manifest.json"
    Assert-OneSceneBuildManifest -ManifestPath $WindowsBuildManifestPath
    $GeneratedBackendHeaderPath = Get-OnlyFile -SearchRootPath $WorkspaceRootPath -FileName "runtime_physics_backend_manifest.hpp"
    Assert-GeneratedBackendSelection -GeneratedManifestHeaderPath $GeneratedBackendHeaderPath

    $VerificationExecutablePath = Join-Path $OutputRootPath "helengine_windows.exe"
    if (-not (Test-Path -LiteralPath $VerificationExecutablePath -PathType Leaf)) {
        throw "Windows executable was not produced at '$VerificationExecutablePath'."
    }

    $CurrentSceneBlobAfterBuild = Invoke-GitText -RepositoryRootPath $DemoWorktreeRootPath -Arguments @("hash-object", "--", $SourceScenePath)
    if ($CurrentSceneBlobAfterBuild -cne $ExpectedSceneBlob) {
        throw "Authored stack scene changed during the isolated build."
    }

} catch {
    $BuildFailure = $_
} finally {
    $env:HELENGINE_SHARED_USER_SETTINGS_ROOT = $OriginalSharedUserSettingsRoot
}

if ($null -ne $BuildFailure) {
    try {
        Assert-MainRepositorySnapshotsUnchanged -Snapshots $MainRepositorySnapshots
    } catch {
        $AuditFailure = $_
    }

    $FailureMessages = New-Object System.Collections.Generic.List[string]
    $FailureMessages.Add("The isolated build failed: $($BuildFailure.Exception.Message)")
    if ($null -ne $AuditFailure) {
        $FailureMessages.Add("The final main-checkout audit failed: $($AuditFailure.Exception.Message)")
    }

    throw ($FailureMessages -join " ")
}

$Publication = Invoke-VerifiedPackagePublicationLifecycle `
    -VerifiedOutputRootPath $OutputRootPath `
    -DestinationRootPath $FinalOutputRootPath `
    -DestinationParentRootPath $FinalOutputParentRootPath `
    -WorkspaceOwnedRootPath $BuildRootPath `
    -ValidatePublishedPackage {
        param($PublishedOutputRootPath)
        $PublishedExecutablePath = Join-Path $PublishedOutputRootPath "helengine_windows.exe"
        $PublishedBackendManifestPath = Join-Path $PublishedOutputRootPath "runtime\runtime-physics-backend.json"
        if (-not (Test-Path -LiteralPath $PublishedExecutablePath -PathType Leaf)) {
            throw "Published Windows executable is missing from '$PublishedOutputRootPath'."
        }

        Assert-BackendManifest -ManifestPath $PublishedBackendManifestPath
    } `
    -AuditMainRepositories {
        Assert-MainRepositorySnapshotsUnchanged -Snapshots $MainRepositorySnapshots
    }
$PublishedOutputRootPath = $Publication.DestinationRootPath
$ExecutablePath = Join-Path $PublishedOutputRootPath "helengine_windows.exe"
$BackendManifestPath = Join-Path $PublishedOutputRootPath "runtime\runtime-physics-backend.json"

Write-Host "HelPhysics stack Windows build complete."
Write-Host "Executable: $ExecutablePath"
Write-Host "Backend manifest: $BackendManifestPath"
Write-Host "Build manifest: $WindowsBuildManifestPath"
Write-Host "Conversion reports: $($ConversionReportPaths -join ', ')"
