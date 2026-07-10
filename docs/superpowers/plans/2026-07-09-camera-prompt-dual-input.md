# Camera Prompt Dual-Input Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Update the shared rendering and physics scene `Camera` prompt so each platform shows one row with the correct raw camera inputs: single-icon where only one input exists, dual-icon where D-pad and stick-style input both exist.

**Architecture:** Keep one shared prompt overlay scene in `DemoSceneInstructionOverlayFactory`. Replace the single-icon camera row model with a two-slot shared row that persists per-platform sprite overrides, source rects, and visibility for each slot. Reuse the existing raw generated icon resolver and leave runtime camera components unchanged unless verification proves a behavior gap.

**Tech Stack:** C#/.NET 9, xUnit, Helengine editor scene authoring APIs, generated control-icon manifest/PNG assets, `dotnet test`, editor command scene regeneration.

---

### Task 1: Lock the Dual-Input Prompt Contract in Tests

**Files:**
- Modify: `assets/codebase/rendering.tools.tests/PromptIconOverlaySourceTests.cs`
- Modify: `assets/codebase/rendering.tools.tests/GeneratedControlIconAssetResolverTests.cs`
- Test: `user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj`

- [ ] **Step 1: Write the failing source-contract tests**

```csharp
[Fact]
public void Demo_scene_instruction_overlay_source_authors_dual_input_camera_row_specs() {
    string source = File.ReadAllText(
        @"C:\dev\helprojs\city\assets\codebase\rendering.tools\DemoSceneInstructionOverlayFactory.cs");

    Assert.Contains("CameraIconSpecs", source, StringComparison.Ordinal);
    Assert.Contains("\"3ds\", \"dpad\"", source, StringComparison.Ordinal);
    Assert.Contains("\"3ds\", \"circle_pad\"", source, StringComparison.Ordinal);
    Assert.Contains("\"psp\", \"analog\"", source, StringComparison.Ordinal);
    Assert.Contains("\"dreamcast\", \"analog\"", source, StringComparison.Ordinal);
    Assert.Contains("\"xbox360\", \"left_stick\"", source, StringComparison.Ordinal);
}

[Fact]
public void Demo_scene_instruction_overlay_source_authors_two_camera_icon_slots() {
    string source = File.ReadAllText(
        @"C:\dev\helprojs\city\assets\codebase\rendering.tools\DemoSceneInstructionOverlayFactory.cs");

    Assert.Contains("CameraIconPrimary", source, StringComparison.Ordinal);
    Assert.Contains("CameraIconSecondary", source, StringComparison.Ordinal);
    Assert.Contains("nameof(SpriteComponent.Visible)", source, StringComparison.Ordinal);
}
```

```csharp
[Fact]
public void Catalog_returns_generated_png_paths_for_circle_pad_and_analog_controls() {
    city.rendering.tools.GeneratedControlIconCatalog catalog =
        city.rendering.tools.GeneratedControlIconCatalog.Load(@"C:\dev\helprojs\city");

    Assert.Equal(
        "images/instructions/controls/generated/3ds/circle_pad.png",
        catalog.RequireControlPath("3ds", "circle_pad"));
    Assert.Equal(
        "images/instructions/controls/generated/psp/analog.png",
        catalog.RequireControlPath("psp", "analog"));
}
```

- [ ] **Step 2: Run the targeted tests to verify they fail for the expected reason**

Run:

```powershell
rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helprojs\city\user_settings\generated_code\projects\rendering.tools.tests\rendering.tools.tests.csproj' --filter 'PromptIconOverlaySourceTests|GeneratedControlIconAssetResolverTests' -v minimal"
```

Expected:

- `PromptIconOverlaySourceTests` fails because `CameraIconSpecs`, `CameraIconPrimary`, `CameraIconSecondary`, and `nameof(SpriteComponent.Visible)` are not authored yet.
- `GeneratedControlIconAssetResolverTests` may already pass for manifest lookups, which is acceptable if the new catalog assertions are satisfied.

- [ ] **Step 3: Commit the red tests**

```bash
git add assets/codebase/rendering.tools.tests/PromptIconOverlaySourceTests.cs assets/codebase/rendering.tools.tests/GeneratedControlIconAssetResolverTests.cs
git commit -m "test: lock dual-input camera prompt contract"
```

### Task 2: Implement the Two-Slot Shared Camera Row

**Files:**
- Modify: `assets/codebase/rendering.tools/DemoSceneInstructionOverlayFactory.cs`
- Test: `assets/codebase/rendering.tools.tests/PromptIconOverlaySourceTests.cs`
- Test: `assets/codebase/rendering.tools.tests/GeneratedControlIconAssetResolverTests.cs`

- [ ] **Step 1: Add the failing implementation seam by switching the camera row call site**

Update `CreateDesktopInstructionOverlayRoot(...)` so the first row becomes a dedicated camera-row path instead of the old single-icon `Rotate` row:

```csharp
CreateDesktopInstructionCameraRow(
    panelEntity,
    projectRootPath,
    font,
    "Camera",
    DesktopInstructionFirstRowTop,
    DesktopInstructionRotateTextTopAdjustment);
CreateDesktopInstructionRow(
    panelEntity,
    projectRootPath,
    font,
    "LightIcon",
    "Light",
    DesktopInstructionSecondRowTop,
    DesktopInstructionToggleTextTopAdjustment,
    LightIconSpecs);
```

- [ ] **Step 2: Implement the new camera row specs and slot model**

Add a row-spec model that can represent one or two raw icons per platform:

```csharp
readonly struct DesktopInstructionPlatformIconSlotSpec {
    public DesktopInstructionPlatformIconSlotSpec(string platformId, string controlId, int2 size, int slotIndex) {
        PlatformId = platformId;
        ControlId = controlId;
        Size = size;
        SlotIndex = slotIndex;
    }

    public string PlatformId { get; }
    public string ControlId { get; }
    public int2 Size { get; }
    public int SlotIndex { get; }
}

static readonly DesktopInstructionPlatformIconSlotSpec[] CameraIconSpecs = new[] {
    new DesktopInstructionPlatformIconSlotSpec("windows", "wasd", new int2(76, 52), 0),
    new DesktopInstructionPlatformIconSlotSpec("win32", "wasd", new int2(76, 52), 0),
    new DesktopInstructionPlatformIconSlotSpec("ds", "dpad", new int2(48, 48), 0),
    new DesktopInstructionPlatformIconSlotSpec("3ds", "dpad", new int2(48, 48), 0),
    new DesktopInstructionPlatformIconSlotSpec("3ds", "circle_pad", new int2(48, 48), 1),
    new DesktopInstructionPlatformIconSlotSpec("psp", "dpad", new int2(48, 48), 0),
    new DesktopInstructionPlatformIconSlotSpec("psp", "analog", new int2(48, 48), 1),
    new DesktopInstructionPlatformIconSlotSpec("dreamcast", "dpad", new int2(48, 48), 0),
    new DesktopInstructionPlatformIconSlotSpec("dreamcast", "analog", new int2(48, 48), 1),
    new DesktopInstructionPlatformIconSlotSpec("xbox360", "dpad", new int2(48, 48), 0),
    new DesktopInstructionPlatformIconSlotSpec("xbox360", "left_stick", new int2(48, 48), 1),
    new DesktopInstructionPlatformIconSlotSpec("switch", "dpad", new int2(48, 48), 0),
    new DesktopInstructionPlatformIconSlotSpec("switch", "left_stick", new int2(48, 48), 1),
    new DesktopInstructionPlatformIconSlotSpec("gamecube", "dpad", new int2(48, 48), 0),
    new DesktopInstructionPlatformIconSlotSpec("gamecube", "control_stick", new int2(48, 48), 1),
    new DesktopInstructionPlatformIconSlotSpec("wii", "dpad", new int2(48, 48), 0),
    new DesktopInstructionPlatformIconSlotSpec("wii", "stick", new int2(48, 48), 1),
    new DesktopInstructionPlatformIconSlotSpec("ps2", "dpad", new int2(48, 48), 0),
    new DesktopInstructionPlatformIconSlotSpec("ps2", "left_stick", new int2(48, 48), 1),
    new DesktopInstructionPlatformIconSlotSpec("psvita", "dpad", new int2(48, 48), 0),
    new DesktopInstructionPlatformIconSlotSpec("psvita", "left_stick", new int2(48, 48), 1),
    new DesktopInstructionPlatformIconSlotSpec("ps1", "dpad", new int2(48, 48), 0),
    new DesktopInstructionPlatformIconSlotSpec("ps1", "left_stick", new int2(48, 48), 1),
    new DesktopInstructionPlatformIconSlotSpec("ps3", "dpad", new int2(48, 48), 0),
    new DesktopInstructionPlatformIconSlotSpec("ps3", "left_stick", new int2(48, 48), 1),
    new DesktopInstructionPlatformIconSlotSpec("xbox", "dpad", new int2(48, 48), 0),
    new DesktopInstructionPlatformIconSlotSpec("xbox", "left_stick", new int2(48, 48), 1),
    new DesktopInstructionPlatformIconSlotSpec("steamdeck", "dpad", new int2(48, 48), 0),
    new DesktopInstructionPlatformIconSlotSpec("steamdeck", "left_stick", new int2(48, 48), 1),
    new DesktopInstructionPlatformIconSlotSpec("n64", "dpad", new int2(48, 48), 0),
    new DesktopInstructionPlatformIconSlotSpec("n64", "control_stick", new int2(48, 48), 1)
};
```

- [ ] **Step 3: Implement the minimal two-slot row builder**

Add a narrow camera-specific builder that authors two shared sprite entities and toggles slot visibility through per-platform overrides:

```csharp
void CreateDesktopInstructionCameraRow(
    Entity panelEntity,
    string projectRootPath,
    FontAsset font,
    string text,
    float topOffset,
    float textTopAdjustment) {
    CreateInstructionIconEntity(projectRootPath, panelEntity, "CameraIconPrimary", topOffset, CameraIconSpecs, 0, 201);
    CreateInstructionIconEntity(projectRootPath, panelEntity, "CameraIconSecondary", topOffset, CameraIconSpecs, 1, 201);

    Entity textEntity = Core.Instance.EntityFactory.CreateChild(panelEntity, "CameraText");
    textEntity.LocalPosition = new float3(DesktopInstructionTextLeft, topOffset + textTopAdjustment, 0.1f);
    textEntity.LayerMask = DesktopOverlayLayerMask;
    TextComponent textComponent = new TextComponent {
        Text = text,
        Font = font,
        FontScale = DesktopInstructionLabelFontScale,
        Color = new byte4(255, 255, 255, 255),
        Size = new int2(DesktopInstructionTextWidth, DesktopInstructionTextHeight),
        RenderOrder2D = 202,
        LayerMask = OverlayDrawableLayerMask
    };
    textEntity.AddComponent(textComponent);
    ApplyFontReference(textEntity, textComponent);
}
```

Update `CreateInstructionIconEntity(...)` so it accepts a `slotIndex`, selects the matching common Windows slot, hides the secondary slot by default, and persists `SpriteComponent.Visible`, `SpriteComponent.Size`, `SpriteComponent.SourceRect`, and texture-reference overrides for every non-common platform.

- [ ] **Step 4: Run the targeted tests to verify the implementation turns green**

Run:

```powershell
rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helprojs\city\user_settings\generated_code\projects\rendering.tools.tests\rendering.tools.tests.csproj' --filter 'PromptIconOverlaySourceTests|GeneratedControlIconAssetResolverTests' -v minimal"
```

Expected:

- all updated `PromptIconOverlaySourceTests` pass
- all updated `GeneratedControlIconAssetResolverTests` pass

- [ ] **Step 5: Run the full rendering-tools test project**

Run:

```powershell
rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helprojs\city\user_settings\generated_code\projects\rendering.tools.tests\rendering.tools.tests.csproj' -v minimal"
```

Expected:

- `Passed`
- `Failed: 0`

- [ ] **Step 6: Commit the implementation**

```bash
git add assets/codebase/rendering.tools/DemoSceneInstructionOverlayFactory.cs assets/codebase/rendering.tools.tests/PromptIconOverlaySourceTests.cs assets/codebase/rendering.tools.tests/GeneratedControlIconAssetResolverTests.cs
git commit -m "feat: add dual-input camera prompt row"
```

### Task 3: Regenerate Scenes and Verify Shared Output

**Files:**
- Modify: `assets/scenes/rendering/*.helen`
- Modify: `assets/scenes/physics/*.helen`
- Modify: any touched generated control-icon `.hasset` sidecars under `assets/images/instructions/controls/generated/`

- [ ] **Step 1: Regenerate the rendering scenes**

Run:

```powershell
rtk powershell -NoProfile -Command "dotnet run --project 'C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj' -- --project 'C:\dev\helprojs\city\project.heproj' --editor-command menu.generate-rendering-scenes"
```

Expected:

- `Editor command 'menu.generate-rendering-scenes' executed successfully.`

- [ ] **Step 2: Regenerate the physics scenes**

Run:

```powershell
rtk powershell -NoProfile -Command "dotnet run --project 'C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj' -- --project 'C:\dev\helprojs\city\project.heproj' --editor-command menu.generate-physics-scenes"
```

Expected:

- `Editor command 'menu.generate-physics-scenes' executed successfully.`

- [ ] **Step 3: Verify the workspace changes are constrained to the expected prompt/scene outputs**

Run:

```powershell
rtk powershell -NoProfile -Command "git diff --stat -- assets/codebase/rendering.tools assets/codebase/rendering.tools.tests assets/scenes assets/images/instructions/controls/generated"
```

Expected:

- changes in `DemoSceneInstructionOverlayFactory.cs`
- changes in rendering-tools tests
- regenerated rendering and physics `.helen` assets
- possible touched generated control-icon `.hasset` sidecars

- [ ] **Step 4: Commit the regenerated scene outputs**

```bash
git add assets/scenes assets/images/instructions/controls/generated
git commit -m "chore: regenerate dual-input camera prompt scenes"
```

### Task 4: Final Verification and Review Summary

**Files:**
- Review only

- [ ] **Step 1: Run the final verification commands one more time**

Run:

```powershell
rtk powershell -NoProfile -Command "dotnet test 'C:\dev\helprojs\city\user_settings\generated_code\projects\rendering.tools.tests\rendering.tools.tests.csproj' -v minimal"
rtk powershell -NoProfile -Command "git status --short"
```

Expected:

- `Passed`
- `Failed: 0`
- only intentional dirty files remain, or the worktree is clean if everything was committed

- [ ] **Step 2: Summarize the result**

Report:

- which code files changed
- whether runtime camera behavior needed code changes
- which tests passed
- which scene-generation commands were run
- whether any unrelated dirty files remain in the worktree
