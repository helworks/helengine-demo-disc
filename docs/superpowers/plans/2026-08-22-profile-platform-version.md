# Per-platform profile version Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the editor Profiles modal the source of each platform's runtime version string, so builds can stamp values such as `1.0.0` for PS2 and `1.0.1` for PSP.

**Architecture:** Add a normalized `Version` property to each `EditorPlatformProfileSettingsDocument`, expose it as a platform-wide text field in `ProfilesDialog`, and have `EditorPlatformAssetCookService` resolve the selected platform profile before constructing `PlatformBuildManifest`. Existing runtime manifest generation and footer lookup remain unchanged.

**Tech Stack:** C#/.NET, System.Text.Json, helengine editor UI components, xUnit editor tests.

---

### Task 1: Add the per-platform version model and persistence default

**Files:**
- Modify: `C:/dev/helworks/helengine/engine/helengine.editor/managers/project/EditorPlatformProfileSettingsDocument.cs`
- Modify: `C:/dev/helworks/helengine/engine/helengine.editor/managers/project/EditorProfileSettingsService.cs`
- Test: `C:/dev/helworks/helengine/engine/helengine.editor.tests/managers/project/EditorProfileSettingsServiceTests.cs`

- [ ] **Step 1: Write failing persistence tests**

Add tests to `EditorProfileSettingsServiceTests` that establish the required behavior:

```csharp
[Fact]
public void Load_WhenPlatformFilesAreMissing_SeedsDefaultVersion() {
    EditorProfileSettingsService service = new EditorProfileSettingsService(TempRootPath);

    EditorProfileSettingsDocument document = service.Load(new[] { "ps2", "psp" });

    Assert.Equal("1.0.0", document.Platforms[0].Version);
    Assert.Equal("1.0.0", document.Platforms[1].Version);
}

[Fact]
public void SaveAndReload_PreservesDistinctPlatformVersions() {
    EditorProfileSettingsService service = new EditorProfileSettingsService(TempRootPath);
    EditorProfileSettingsDocument document = CreateProfileDocument("ps2", "psp");
    document.Platforms[0].Version = "1.0.0";
    document.Platforms[1].Version = "1.0.1";

    service.Save(document);
    EditorProfileSettingsDocument reloaded = service.Load(new[] { "ps2", "psp" });

    Assert.Equal("1.0.0", reloaded.Platforms[0].Version);
    Assert.Equal("1.0.1", reloaded.Platforms[1].Version);
}

[Fact]
public void Load_WhenExistingPlatformFileOmitsVersion_UsesDefaultVersion() {
    Directory.CreateDirectory(Path.Combine(TempRootPath, "settings"));
    File.WriteAllText(
        Path.Combine(TempRootPath, "settings", "platform.ps2.json"),
        "{\"platformId\":\"ps2\",\"build\":{},\"graphics\":{},\"codegen\":{},\"input\":{}}");

    EditorProfileSettingsService service = new EditorProfileSettingsService(TempRootPath);
    EditorProfileSettingsDocument document = service.Load(new[] { "ps2" });

    Assert.Equal("1.0.0", Assert.Single(document.Platforms).Version);
}
```

- [ ] **Step 2: Run the focused tests and verify they fail**

Run from `C:/dev/helworks/helengine`:

```powershell
rtk dotnet test engine/helengine.editor.tests/helengine.editor.tests.csproj --no-restore --filter "FullyQualifiedName~EditorProfileSettingsServiceTests" --verbosity minimal
```

Expected: compilation failures because `EditorPlatformProfileSettingsDocument.Version` does not exist.

- [ ] **Step 3: Add the model property and normalization**

Add one shared default and serialized property to `EditorPlatformProfileSettingsDocument`:

```csharp
public const string DefaultVersion = "1.0.0";

public string Version { get; set; } = DefaultVersion;
```

In `EditorProfileSettingsService.NormalizePlatform`, immediately after assigning `platform.PlatformId`, normalize old or blank JSON values:

```csharp
platform.Version = string.IsNullOrWhiteSpace(platform.Version)
    ? EditorPlatformProfileSettingsDocument.DefaultVersion
    : platform.Version.Trim();
```

This makes newly seeded profiles and old files without the property resolve to `1.0.0`, while preserving an explicitly configured value such as `1.0.1`. `Save` will serialize the property using the existing camel-case JSON policy as `version`.

- [ ] **Step 4: Run the persistence tests and verify they pass**

Run the same focused command from Step 2. Expected: all `EditorProfileSettingsServiceTests` pass.

- [ ] **Step 5: Commit the model and persistence change**

```powershell
git add -- C:/dev/helworks/helengine/engine/helengine.editor/managers/project/EditorPlatformProfileSettingsDocument.cs C:/dev/helworks/helengine/engine/helengine.editor/managers/project/EditorProfileSettingsService.cs C:/dev/helworks/helengine/engine/helengine.editor.tests/managers/project/EditorProfileSettingsServiceTests.cs
git commit -m "Add per-platform profile versions"
```

### Task 2: Add the Version field to the Profiles modal

**Files:**
- Modify: `C:/dev/helworks/helengine/engine/helengine.editor/components/ui/ProfilesDialog.cs`
- Test: `C:/dev/helworks/helengine/engine/helengine.editor.tests/ProfilesDialogTests.cs`

- [ ] **Step 1: Write failing dialog tests**

Extend `ProfilesDialogTests` so the selected platform's version is loaded, switching platforms swaps it, and Save returns the edited value without mutating the source document:

```csharp
[Fact]
public void Show_WhenPlatformSelectionChanges_LoadsTheSelectedPlatformVersion() {
    ProfilesDialog dialog = new ProfilesDialog(CreateFont());
    EditorProfileSettingsDocument document = CreateProfileDocument();
    document.Platforms[0].Version = "1.0.0";
    document.Platforms[1].Version = "1.0.1";

    dialog.Show(document, new[] { "windows", "ps2" }, "windows", CreateSelectionModel());

    TextBoxComponent versionTextBox = GetPrivateField<TextBoxComponent>(dialog, "VersionTextBox");
    Assert.Equal("1.0.0", versionTextBox.Text);

    ComboBoxComponent platformComboBox = GetPrivateField<ComboBoxComponent>(dialog, "PlatformComboBox");
    platformComboBox.SelectedIndex = 1;

    Assert.Equal("1.0.1", versionTextBox.Text);
}

[Fact]
public void HandleSaveClicked_PreservesEditedPlatformVersionInTheReturnedDocument() {
    ProfilesDialog dialog = new ProfilesDialog(CreateFont());
    EditorProfileSettingsDocument document = CreateProfileDocument();
    document.Platforms[0].Version = "1.0.0";

    dialog.Show(document, new[] { "windows", "ps2" }, "windows", CreateSelectionModel());
    TextBoxComponent versionTextBox = GetPrivateField<TextBoxComponent>(dialog, "VersionTextBox");
    versionTextBox.Text = "1.0.1";

    ProfilesDialogSelection selection = null;
    dialog.ConfirmRequested += value => selection = value;
    InvokePrivate(dialog, "HandleSaveClicked");

    Assert.Equal("1.0.0", document.Platforms[0].Version);
    Assert.Equal("1.0.1", selection.ProfileSettingsDocument.Platforms[0].Version);
}
```

Update the existing scaled-layout assertions to include the new Version row, and assert the field is positioned after the platform selector and before the tabs.

- [ ] **Step 2: Run the focused dialog tests and verify they fail**

```powershell
rtk dotnet test engine/helengine.editor.tests/helengine.editor.tests.csproj --no-restore --filter "FullyQualifiedName~ProfilesDialogTests" --verbosity minimal
```

Expected: compilation failures because `VersionTextBox` is not present.

- [ ] **Step 3: Add the platform-wide Version controls and layout**

In `ProfilesDialog`, add a label host/text pair and a `TextBoxComponent VersionTextBox`, using the same render-order setup as the existing platform settings text boxes:

```csharp
readonly EditorEntity VersionLabelHost;
readonly TextComponent VersionLabelText;
readonly EditorEntity VersionTextBoxHost;
readonly TextBoxComponent VersionTextBox;
```

Construct them after the platform combo box:

```csharp
VersionLabelHost = CreateTextHost();
DialogPanelRoot.AddChild(VersionLabelHost);
VersionLabelText = CreateLabelText("Version");
VersionLabelHost.AddComponent(VersionLabelText);

VersionTextBoxHost = CreateTextHost();
DialogPanelRoot.AddChild(VersionTextBoxHost);
VersionTextBox = new TextBoxComponent(new int2(GetSettingValueWidth(), GetFieldRowHeightPixels()), DialogFontValue, EditorPlatformProfileSettingsDocument.DefaultVersion);
VersionTextBox.SetRenderOrders(DialogPanelOrder, DialogTextOrder);
VersionTextBoxHost.AddComponent(VersionTextBox);
```

Increase `PanelHeight` enough to accommodate the extra form row while retaining the footer spacing, and update the layout helpers so the order is:

```text
Platform row -> Version row -> tab buttons -> active settings -> status -> footer
```

Use a shared row-top helper for the Version row and have `LayoutTabs`, `LayoutSettingsSections`, and `LayoutStatus` start after the Version row. Keep the field width aligned with the existing settings value column.

- [ ] **Step 4: Load, store, clone, and reset the Version value**

When loading a platform, set the field from the normalized profile:

```csharp
VersionTextBox.Text = string.IsNullOrWhiteSpace(platform.Version)
    ? EditorPlatformProfileSettingsDocument.DefaultVersion
    : platform.Version;
```

At the start of `TryStoreCurrentPlatformFields`, store the trimmed value and fall back to the default if the field is blank:

```csharp
platform.Version = string.IsNullOrWhiteSpace(VersionTextBox.Text)
    ? EditorPlatformProfileSettingsDocument.DefaultVersion
    : VersionTextBox.Text.Trim();
```

Copy `Version = platform.Version` in `ClonePlatformDocument`, and clear/reset the text box in `Hide` alongside the other transient controls. Platform selection and tab changes already call `TryStoreCurrentPlatformFields`, so the value will follow the existing draft-edit behavior.

- [ ] **Step 5: Run the dialog tests and verify they pass**

Run the focused command from Step 2. Expected: all `ProfilesDialogTests` pass, including the updated layout assertions.

- [ ] **Step 6: Commit the Profiles modal change**

```powershell
git add -- C:/dev/helworks/helengine/engine/helengine.editor/components/ui/ProfilesDialog.cs C:/dev/helworks/helengine/engine/helengine.editor.tests/ProfilesDialogTests.cs
git commit -m "Expose platform version in Profiles dialog"
```

### Task 3: Stamp the profile version into cooked runtime metadata

**Files:**
- Modify: `C:/dev/helworks/helengine/engine/helengine.editor/managers/project/EditorPlatformAssetCookService.cs`
- Test: `C:/dev/helworks/helengine/engine/helengine.editor.tests/managers/project/EditorPlatformAssetCookServiceTests.cs`
- Verify: `C:/dev/helworks/helengine/engine/helengine.editor.tests/managers/project/EditorRuntimeNativeManifestWriterTests.cs`

- [ ] **Step 1: Write a failing cook test**

Add a test that writes `settings/platform.windows.json` with `"version": "1.0.1"`, cooks with the existing Windows test builder, and asserts the resulting manifest uses the profile value:

```csharp
[Fact]
public void Cook_UsesPlatformProfileVersionForRuntimeManifest() {
    Directory.CreateDirectory(Path.Combine(ProjectRootPath, "settings"));
    File.WriteAllText(
        Path.Combine(ProjectRootPath, "settings", "platform.windows.json"),
        "{\"platformId\":\"windows\",\"version\":\"1.0.1\",\"build\":{},\"graphics\":{},\"codegen\":{},\"input\":{}}");
    WriteSceneAsset("Scenes/MainMenu.helen", Array.Empty<SceneAssetReference>());

    EditorPlatformAssetCookService service = new(
        ProjectRootPath,
        "1.0.0-engine",
        "game",
        "1.0.0",
        Array.Empty<IAssetImporterRegistration>(),
        PackagedFontAssetFactory.Create());
    TestPlatformMaterialAssetBuilder builder = new();

    PlatformBuildManifest manifest = service.Cook(
        builder.Definition,
        new[] { "MainMenu" },
        BuildRootPath,
        new[] { "windows" },
        builder);

    Assert.Equal("1.0.1", manifest.PlatformVersion);
}
```

The existing `EditorRuntimeNativeManifestWriterTests` assertion for `"2026.05.12"` already proves that `PlatformBuildManifest.PlatformVersion` is emitted into `runtime_startup_manifest.cpp`; retain it as the downstream pass-through check.

- [ ] **Step 2: Run the focused cook test and verify it fails**

```powershell
rtk dotnet test engine/helengine.editor.tests/helengine.editor.tests.csproj --no-restore --filter "FullyQualifiedName~EditorPlatformAssetCookServiceTests.Cook_UsesPlatformProfileVersionForRuntimeManifest" --verbosity minimal
```

Expected: FAIL because the cook service still resolves `BuilderVersion` (`1.0.0`) instead of the persisted profile value (`1.0.1`).

- [ ] **Step 3: Resolve the normalized profile before constructing the manifest**

In `EditorPlatformAssetCookService.Cook`, resolve the platform name once and use it for both the manifest name and version:

```csharp
string platformName = ResolvePlatformName(platformDefinition, materialBuilder);
string platformVersion = ResolvePlatformVersion(platformName);

PlatformBuildManifest manifest = new PlatformBuildManifest(
    2,
    ProjectId,
    ProjectVersion,
    RequiredEngineVersion,
    platformName,
    platformVersion,
    orderedSceneIds[0],
    scenes,
    Array.Empty<PlatformBuildAsset>(),
    cookedArtifacts,
    Array.Empty<PlatformBuildCodeModule>(),
    Array.Empty<PlatformArtifactPlacement>(),
    new PlatformContainerWritePlan(string.Empty, Array.Empty<PlatformContainerArtifact>()),
    platformCookWorkItems);
```

Replace the builder-only version resolver with a profile-backed resolver:

```csharp
string ResolvePlatformVersion(string platformId) {
    EditorPlatformProfileSettingsDocument platform = ResolvePlatformProfile(platformId);
    return string.IsNullOrWhiteSpace(platform.Version)
        ? EditorPlatformProfileSettingsDocument.DefaultVersion
        : platform.Version.Trim();
}

EditorPlatformProfileSettingsDocument ResolvePlatformProfile(string platformId) {
    EditorProfileSettingsDocument document = ProfileSettingsService.Load(new[] { platformId });
    for (int index = 0; index < document.Platforms.Count; index++) {
        EditorPlatformProfileSettingsDocument platform = document.Platforms[index];
        if (platform != null && string.Equals(platform.PlatformId, platformId, StringComparison.OrdinalIgnoreCase)) {
            return platform;
        }
    }

    throw new InvalidOperationException($"Platform profile '{platformId}' could not be resolved.");
}
```

Use the same helper in `ResolveStandardPlatformInputConfiguration` so the cook path has one normalized profile lookup. Do not change `EditorRuntimeNativeManifestWriter`; it already emits `manifest.PlatformVersion`, and do not change the menu footer; it already reads `PlatformInfo.Version`.

- [ ] **Step 4: Run the cook and native-manifest tests**

```powershell
rtk dotnet test engine/helengine.editor.tests/helengine.editor.tests.csproj --no-restore --filter "FullyQualifiedName~EditorPlatformAssetCookServiceTests|FullyQualifiedName~EditorRuntimeNativeManifestWriterTests" --verbosity minimal
```

Expected: all selected tests pass, including the new `1.0.1` profile stamping assertion and the existing generated-source assertion.

- [ ] **Step 5: Commit the build metadata change**

```powershell
git add -- C:/dev/helworks/helengine/engine/helengine.editor/managers/project/EditorPlatformAssetCookService.cs C:/dev/helworks/helengine/engine/helengine.editor.tests/managers/project/EditorPlatformAssetCookServiceTests.cs C:/dev/helworks/helengine/engine/helengine.editor.tests/managers/project/EditorRuntimeNativeManifestWriterTests.cs
git commit -m "Stamp profile version into runtime manifest"
```

### Task 4: Run the complete focused verification and inspect the diff

**Files:**
- Verify: all files changed in Tasks 1-3

- [ ] **Step 1: Run the combined regression suite**

From `C:/dev/helworks/helengine`:

```powershell
rtk dotnet test engine/helengine.editor.tests/helengine.editor.tests.csproj --no-restore --filter "FullyQualifiedName~EditorProfileSettingsServiceTests|FullyQualifiedName~ProfilesDialogTests|FullyQualifiedName~EditorPlatformAssetCookServiceTests|FullyQualifiedName~EditorRuntimeNativeManifestWriterTests" --verbosity minimal
```

Expected: all selected tests pass.

- [ ] **Step 2: Verify formatting and scope**

From `C:/dev/helprojs/demodisc`:

```powershell
git diff --check HEAD~3..HEAD
git status --short
```

Expected: no whitespace errors; only the implementation commits and the user's pre-existing worktree changes are present. Do not rebuild PS2/PSP or regenerate levels as part of this change.

- [ ] **Step 3: Report the implementation and verification results**

Summarize the changed files, the per-platform JSON field (`version`), the `1.0.0` migration default, and the focused test command/results. Explicitly note that no platform build was run.
