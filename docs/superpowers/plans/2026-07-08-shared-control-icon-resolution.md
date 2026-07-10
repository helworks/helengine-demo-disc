# Shared Control Icon Resolution Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add one shared raw control-icon resolver backed by the generated manifest and migrate the shared rendering/physics instruction overlay to one scene-authored prompt surface that swaps icon textures per platform through editor component overrides.

**Architecture:** Keep the generated control pack under `assets/images/instructions/controls/generated` as the only source of truth. The shared utility resolves `platform id + raw control id` into both the generated PNG path and the imported texture asset id, but the scene-authoring layer must still persist file-backed texture references because `SceneAssetReferenceValidationService` currently rejects generated texture references. The shared overlay migration therefore uses `ComponentPlatformEditingService` to author per-platform `SpriteComponent` overrides for texture and size while leaving layout ownership in `DemoSceneInstructionOverlayFactory`.

**Tech Stack:** C#, `System.Text.Json`, HelEngine editor import pipeline, HelEngine component platform override authoring APIs, xUnit source/behavior tests, editor-command scene regeneration

---

### Task 1: Lock The Shared Raw Resolver Contract In Failing Tests

**Files:**
- Create: `C:\dev\helprojs\city\tests\gameplay.tests\GeneratedControlIconAssetResolverTests.cs`
- Test: `C:\dev\helprojs\city\tests\gameplay.tests\gameplay.tests.csproj`

- [ ] **Step 1: Write the failing platform-map and manifest tests**

Create `GeneratedControlIconAssetResolverTests.cs` with direct coverage for platform normalization, manifest lookup, and strict failure behavior.

```csharp
namespace city.tests {
    /// <summary>
    /// Verifies generated control-icon lookup stays manifest-driven and strict.
    /// </summary>
    public sealed class GeneratedControlIconAssetResolverTests {
        [Fact]
        public void Platform_map_defaults_windows_and_win32_to_keyboard() {
            Assert.Equal("keyboard", city.rendering.tools.GeneratedControlIconPlatformMap.ResolveFamilyId("windows"));
            Assert.Equal("keyboard", city.rendering.tools.GeneratedControlIconPlatformMap.ResolveFamilyId("win32"));
        }

        [Fact]
        public void Catalog_returns_generated_png_path_for_known_family_and_control() {
            city.rendering.tools.GeneratedControlIconCatalog catalog = city.rendering.tools.GeneratedControlIconCatalog.Load(
                @"C:\dev\helprojs\city");

            string relativePath = catalog.RequireControlPath("keyboard", "wasd");

            Assert.Equal("images/instructions/controls/generated/keyboard/wasd.png", relativePath);
        }

        [Fact]
        public void Catalog_throws_for_missing_control() {
            city.rendering.tools.GeneratedControlIconCatalog catalog = city.rendering.tools.GeneratedControlIconCatalog.Load(
                @"C:\dev\helprojs\city");

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => catalog.RequireControlPath("ps2", "not-a-real-control"));

            Assert.Contains("ps2", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("not-a-real-control", exception.Message, StringComparison.Ordinal);
        }
    }
}
```

- [ ] **Step 2: Add the failing import-resolution tests**

Extend the same file with behavior tests for the shared resolver entry point.

```csharp
        [Fact]
        public void Resolver_returns_generated_png_path_and_imported_texture_asset_id() {
            city.rendering.tools.GeneratedControlIconAssetResolver resolver = new city.rendering.tools.GeneratedControlIconAssetResolver();

            city.rendering.tools.ResolvedControlIcon resolved = resolver.RequireIcon(
                @"C:\dev\helprojs\city",
                "ps2",
                "r1");

            Assert.Equal("ps2", resolved.PlatformId);
            Assert.Equal("ps2", resolved.FamilyId);
            Assert.Equal("r1", resolved.ControlId);
            Assert.Equal("images/instructions/controls/generated/ps2/r1.png", resolved.SourcePngRelativePath);
            Assert.False(string.IsNullOrWhiteSpace(resolved.ImportedTextureAssetId));
        }

        [Fact]
        public void Resolver_throws_for_unknown_platform() {
            city.rendering.tools.GeneratedControlIconAssetResolver resolver = new city.rendering.tools.GeneratedControlIconAssetResolver();

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => resolver.RequireIcon(@"C:\dev\helprojs\city", "saturn", "a"));

            Assert.Contains("saturn", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
```

- [ ] **Step 3: Run the resolver tests to verify they fail**

Run:

```bash
rtk dotnet test C:\dev\helprojs\city\tests\gameplay.tests\gameplay.tests.csproj --filter "GeneratedControlIconAssetResolverTests" -v minimal
```

Expected: `FAIL` because `GeneratedControlIconPlatformMap`, `GeneratedControlIconCatalog`, `GeneratedControlIconAssetResolver`, and `ResolvedControlIcon` do not exist yet.

- [ ] **Step 4: Commit the failing-test checkpoint**

```bash
rtk git -C C:\dev\helprojs\city add tests/gameplay.tests/GeneratedControlIconAssetResolverTests.cs
rtk git -C C:\dev\helprojs\city commit -m "test: define generated control icon resolver contract"
```

### Task 2: Implement The Shared Manifest-Driven Resolver

**Files:**
- Create: `C:\dev\helprojs\city\assets\codebase\rendering.tools\ResolvedControlIcon.cs`
- Create: `C:\dev\helprojs\city\assets\codebase\rendering.tools\GeneratedControlIconPlatformMap.cs`
- Create: `C:\dev\helprojs\city\assets\codebase\rendering.tools\GeneratedControlIconCatalog.cs`
- Create: `C:\dev\helprojs\city\assets\codebase\rendering.tools\GeneratedControlIconAssetResolver.cs`
- Modify: `C:\dev\helprojs\city\tests\gameplay.tests\GeneratedControlIconAssetResolverTests.cs`

- [ ] **Step 1: Add the resolved-icon DTO and strict platform map**

Create `ResolvedControlIcon.cs` and `GeneratedControlIconPlatformMap.cs`.

```csharp
namespace city.rendering.tools {
    /// <summary>
    /// Describes one generated control icon resolved from the manifest and editor import pipeline.
    /// </summary>
    public sealed class ResolvedControlIcon {
        public string PlatformId { get; init; } = string.Empty;
        public string FamilyId { get; init; } = string.Empty;
        public string ControlId { get; init; } = string.Empty;
        public string SourcePngRelativePath { get; init; } = string.Empty;
        public string ImportedTextureAssetId { get; init; } = string.Empty;
    }

    /// <summary>
    /// Maps authored platform ids to generated control-icon families.
    /// </summary>
    public static class GeneratedControlIconPlatformMap {
        static readonly Dictionary<string, string> FamilyIdsByPlatformId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            ["windows"] = "keyboard",
            ["win32"] = "keyboard",
            ["xbox360"] = "xbox360",
            ["switch"] = "switch",
            ["gamecube"] = "gamecube",
            ["wii"] = "wii",
            ["ds"] = "ds",
            ["3ds"] = "3ds",
            ["psp"] = "psp",
            ["ps2"] = "ps2",
            ["psvita"] = "psvita",
            ["n64"] = "n64",
            ["dreamcast"] = "dreamcast",
            ["ps1"] = "ps1",
            ["ps3"] = "ps3",
            ["xbox"] = "xbox",
            ["steamdeck"] = "steamdeck"
        };

        public static string ResolveFamilyId(string platformId) {
            if (string.IsNullOrWhiteSpace(platformId)) {
                throw new ArgumentException("Platform id must be provided.", nameof(platformId));
            }

            if (FamilyIdsByPlatformId.TryGetValue(platformId.Trim(), out string familyId)) {
                return familyId;
            }

            throw new InvalidOperationException($"Generated control icon family mapping was not found for platform '{platformId}'.");
        }

        public static IReadOnlyList<string> EnumerateMappedPlatformIds() {
            return FamilyIdsByPlatformId.Keys.OrderBy(static value => value, StringComparer.OrdinalIgnoreCase).ToArray();
        }
    }
}
```

- [ ] **Step 2: Add the manifest loader without duplicating catalog data in source**

Create `GeneratedControlIconCatalog.cs` and parse `assets/images/instructions/controls/generated/manifest.json` with `System.Text.Json`.

```csharp
using System.Text.Json;

namespace city.rendering.tools {
    /// <summary>
    /// Loads and validates the generated control-icon manifest.
    /// </summary>
    public sealed class GeneratedControlIconCatalog {
        const string ManifestRelativePath = "assets/images/instructions/controls/generated/manifest.json";
        readonly Dictionary<string, HashSet<string>> ControlIdsByFamilyId;

        GeneratedControlIconCatalog(Dictionary<string, HashSet<string>> controlIdsByFamilyId) {
            ControlIdsByFamilyId = controlIdsByFamilyId;
        }

        public static GeneratedControlIconCatalog Load(string projectRootPath) {
            string fullProjectRootPath = Path.GetFullPath(projectRootPath ?? string.Empty);
            string fullManifestPath = Path.Combine(fullProjectRootPath, ManifestRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullManifestPath)) {
                throw new InvalidOperationException($"Generated control icon manifest was not found at '{fullManifestPath}'.");
            }

            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(fullManifestPath));
            JsonElement platformsElement = document.RootElement.GetProperty("platforms");
            Dictionary<string, HashSet<string>> controlsByFamily = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (JsonProperty platformProperty in platformsElement.EnumerateObject()) {
                HashSet<string> controls = new HashSet<string>(StringComparer.Ordinal);
                foreach (JsonElement controlElement in platformProperty.Value.GetProperty("controls").EnumerateArray()) {
                    controls.Add(controlElement.GetString() ?? string.Empty);
                }

                controlsByFamily.Add(platformProperty.Name, controls);
            }

            return new GeneratedControlIconCatalog(controlsByFamily);
        }

        public string RequireControlPath(string familyId, string controlId) {
            if (string.IsNullOrWhiteSpace(familyId)) {
                throw new ArgumentException("Family id must be provided.", nameof(familyId));
            }
            if (string.IsNullOrWhiteSpace(controlId)) {
                throw new ArgumentException("Control id must be provided.", nameof(controlId));
            }
            if (!ControlIdsByFamilyId.TryGetValue(familyId, out HashSet<string> controls)) {
                throw new InvalidOperationException($"Generated control icon family '{familyId}' was not found in the manifest.");
            }
            if (!controls.Contains(controlId)) {
                throw new InvalidOperationException($"Generated control icon '{familyId}/{controlId}' was not found in the manifest.");
            }

            return "images/instructions/controls/generated/" + familyId + "/" + controlId + ".png";
        }
    }
}
```

- [ ] **Step 3: Add the editor-import-backed resolver**

Create `GeneratedControlIconAssetResolver.cs` and reuse `GeneratedAuthoringSceneWriteService.CreateGeneratedSceneAssetImportManager(...)` so the resolver uses the same importer registrations as generated-scene authoring.

```csharp
namespace city.rendering.tools {
    /// <summary>
    /// Resolves generated control icons into both source paths and imported texture asset ids.
    /// </summary>
    public sealed class GeneratedControlIconAssetResolver {
        public ResolvedControlIcon RequireIcon(string projectRootPath, string platformId, string controlId) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            string familyId = GeneratedControlIconPlatformMap.ResolveFamilyId(platformId);
            GeneratedControlIconCatalog catalog = GeneratedControlIconCatalog.Load(projectRootPath);
            string relativePath = catalog.RequireControlPath(familyId, controlId);

            string fullProjectRootPath = Path.GetFullPath(projectRootPath);
            string fullAssetsRootPath = Path.Combine(fullProjectRootPath, "assets");
            string fullSourcePath = Path.Combine(fullAssetsRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullSourcePath)) {
                throw new InvalidOperationException($"Generated control icon source '{relativePath}' was not found for platform '{platformId}' and control '{controlId}'.");
            }

            AssetImportManager importManager = GeneratedAuthoringSceneWriteService.CreateGeneratedSceneAssetImportManager(fullProjectRootPath);
            TextureAssetImportSettings settings = importManager.LoadOrCreateTextureImportSettings(fullSourcePath);
            if (settings == null || settings.Importer == null || string.IsNullOrWhiteSpace(settings.Importer.AssetId)) {
                throw new InvalidOperationException($"Generated control icon '{relativePath}' did not produce a persisted imported texture asset id.");
            }

            return new ResolvedControlIcon {
                PlatformId = platformId,
                FamilyId = familyId,
                ControlId = controlId,
                SourcePngRelativePath = relativePath,
                ImportedTextureAssetId = settings.Importer.AssetId
            };
        }
    }
}
```

- [ ] **Step 4: Run the resolver tests to verify they pass**

Run:

```bash
rtk dotnet test C:\dev\helprojs\city\tests\gameplay.tests\gameplay.tests.csproj --filter "GeneratedControlIconAssetResolverTests" -v minimal
```

Expected: `PASS`

- [ ] **Step 5: Commit the shared resolver implementation**

```bash
rtk git -C C:\dev\helprojs\city add assets/codebase/rendering.tools/ResolvedControlIcon.cs assets/codebase/rendering.tools/GeneratedControlIconPlatformMap.cs assets/codebase/rendering.tools/GeneratedControlIconCatalog.cs assets/codebase/rendering.tools/GeneratedControlIconAssetResolver.cs tests/gameplay.tests/GeneratedControlIconAssetResolverTests.cs
rtk git -C C:\dev\helprojs\city commit -m "feat: add generated control icon resolver"
```

### Task 3: Add Failing Source Audits For The Shared Overlay Migration

**Files:**
- Create: `C:\dev\helprojs\city\tests\gameplay.tests\PromptIconOverlaySourceTests.cs`
- Test: `C:\dev\helprojs\city\tests\gameplay.tests\gameplay.tests.csproj`

- [ ] **Step 1: Write the failing rendering-overlay source audit**

Create `PromptIconOverlaySourceTests.cs` and lock the overlay migration onto manifest-backed raw ids plus editor platform overrides.

```csharp
namespace city.tests {
    /// <summary>
    /// Verifies rendering and physics prompt overlays consume the shared generated control-icon resolver.
    /// </summary>
    public sealed class PromptIconOverlaySourceTests {
        [Fact]
        public void Demo_scene_instruction_overlay_source_uses_shared_resolver_and_editor_platform_overrides() {
            string source = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\rendering.tools\DemoSceneInstructionOverlayFactory.cs");

            Assert.Contains("GeneratedControlIconAssetResolver", source, StringComparison.Ordinal);
            Assert.Contains("ComponentPlatformEditingService", source, StringComparison.Ordinal);
            Assert.Contains("EnsurePlatformOverrideComponent", source, StringComparison.Ordinal);
            Assert.Contains("PersistPlatformOverride", source, StringComparison.Ordinal);
            Assert.DoesNotContain("DemoScenePlatformInstructionIconSetComponent", source, StringComparison.Ordinal);
            Assert.DoesNotContain("images/instructions/controls/xbox360_dpad.png", source, StringComparison.Ordinal);
            Assert.DoesNotContain("images/instructions/controls/ps2_r1.png", source, StringComparison.Ordinal);
            Assert.DoesNotContain("images/instructions/controls/switch_r.png", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Demo_scene_instruction_overlay_source_uses_raw_control_ids_for_keyboard_and_console_rows() {
            string source = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\rendering.tools\DemoSceneInstructionOverlayFactory.cs");

            Assert.Contains("\"wasd\"", source, StringComparison.Ordinal);
            Assert.Contains("\"key_l\"", source, StringComparison.Ordinal);
            Assert.Contains("\"dpad\"", source, StringComparison.Ordinal);
            Assert.Contains("\"rb\"", source, StringComparison.Ordinal);
            Assert.Contains("\"r1\"", source, StringComparison.Ordinal);
            Assert.Contains("\"r\"", source, StringComparison.Ordinal);
        }
```

- [ ] **Step 2: Add the failing physics source audit**

Extend the same file with a guardrail around the shared physics consumer.

```csharp
        [Fact]
        public void Physics_scene_factory_source_still_delegates_instruction_overlay_to_shared_rendering_factory() {
            string source = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\physics.tools\PhysicsSceneFactory.cs");

            Assert.Contains("instructionOverlayFactory.CreateDesktopInstructionOverlayRoot", source, StringComparison.Ordinal);
            Assert.DoesNotContain("images/instructions/controls/xbox360_dpad.png", source, StringComparison.Ordinal);
            Assert.DoesNotContain("images/instructions/controls/generated/", source, StringComparison.Ordinal);
        }
    }
}
```

- [ ] **Step 3: Run the source audits to verify they fail**

Run:

```bash
rtk dotnet test C:\dev\helprojs\city\tests\gameplay.tests\gameplay.tests.csproj --filter "PromptIconOverlaySourceTests" -v minimal
```

Expected: `FAIL` because the overlay still hard-codes legacy controller PNG paths and still uses `DemoScenePlatformInstructionIconSetComponent`.

- [ ] **Step 4: Commit the failing source-audit checkpoint**

```bash
rtk git -C C:\dev\helprojs\city add tests/gameplay.tests/PromptIconOverlaySourceTests.cs
rtk git -C C:\dev\helprojs\city commit -m "test: lock shared prompt icon overlay migration"
```

### Task 4: Migrate The Shared Rendering/Physics Overlay To Per-Platform Scene Overrides

**Files:**
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\DemoSceneInstructionOverlayFactory.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\CubeTestSceneFactory.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\ScaledCubeSceneFactory.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\ColoredCubeGridSceneFactory.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\DirectionalShadowPlazaSceneFactory.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\TexturedCubeGridSceneFactory.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\SpotlightStreetSliceSceneFactory.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\AxisTestSceneFactory.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\AxisTest2SceneFactory.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\RenderingSceneGenerator.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\physics.tools\PhysicsSceneFactory.cs`
- Delete: `C:\dev\helprojs\city\assets\codebase\rendering\DemoScenePlatformInstructionIconSetComponent.cs`
- Modify: `C:\dev\helprojs\city\tests\gameplay.tests\PromptIconOverlaySourceTests.cs`

- [ ] **Step 1: Replace hard-coded path constants with raw row definitions**

Refactor `DemoSceneInstructionOverlayFactory.cs` so it defines one common icon entity per row plus raw per-platform bindings instead of per-platform child groups.

```csharp
readonly GeneratedControlIconAssetResolver ControlIconResolver = new GeneratedControlIconAssetResolver();
readonly ComponentPlatformEditingService PlatformEditingService = new ComponentPlatformEditingService();

readonly struct DesktopInstructionPlatformIconSpec {
    public DesktopInstructionPlatformIconSpec(string platformId, string controlId, int2 size) {
        PlatformId = platformId;
        ControlId = controlId;
        Size = size;
    }

    public string PlatformId { get; }
    public string ControlId { get; }
    public int2 Size { get; }
}

static readonly DesktopInstructionPlatformIconSpec[] RotateIconSpecs = new[] {
    new DesktopInstructionPlatformIconSpec("windows", "wasd", new int2(76, 52)),
    new DesktopInstructionPlatformIconSpec("win32", "wasd", new int2(76, 52)),
    new DesktopInstructionPlatformIconSpec("xbox360", "dpad", new int2(48, 48)),
    new DesktopInstructionPlatformIconSpec("switch", "dpad", new int2(48, 48)),
    new DesktopInstructionPlatformIconSpec("gamecube", "dpad", new int2(48, 48)),
    new DesktopInstructionPlatformIconSpec("wii", "dpad", new int2(48, 48)),
    new DesktopInstructionPlatformIconSpec("ds", "dpad", new int2(48, 48)),
    new DesktopInstructionPlatformIconSpec("3ds", "dpad", new int2(48, 48)),
    new DesktopInstructionPlatformIconSpec("psp", "dpad", new int2(48, 48)),
    new DesktopInstructionPlatformIconSpec("ps2", "dpad", new int2(48, 48)),
    new DesktopInstructionPlatformIconSpec("psvita", "dpad", new int2(48, 48)),
    new DesktopInstructionPlatformIconSpec("n64", "dpad", new int2(48, 48)),
    new DesktopInstructionPlatformIconSpec("dreamcast", "dpad", new int2(48, 48)),
    new DesktopInstructionPlatformIconSpec("ps1", "dpad", new int2(48, 48)),
    new DesktopInstructionPlatformIconSpec("ps3", "dpad", new int2(48, 48)),
    new DesktopInstructionPlatformIconSpec("xbox", "dpad", new int2(48, 48)),
    new DesktopInstructionPlatformIconSpec("steamdeck", "dpad", new int2(48, 48))
};

static readonly DesktopInstructionPlatformIconSpec[] LightIconSpecs = new[] {
    new DesktopInstructionPlatformIconSpec("windows", "key_l", new int2(46, 46)),
    new DesktopInstructionPlatformIconSpec("win32", "key_l", new int2(46, 46)),
    new DesktopInstructionPlatformIconSpec("xbox360", "rb", new int2(78, 45)),
    new DesktopInstructionPlatformIconSpec("switch", "r", new int2(89, 41)),
    new DesktopInstructionPlatformIconSpec("gamecube", "r", new int2(82, 43)),
    new DesktopInstructionPlatformIconSpec("wii", "b", new int2(58, 46)),
    new DesktopInstructionPlatformIconSpec("ds", "r", new int2(74, 42)),
    new DesktopInstructionPlatformIconSpec("3ds", "r", new int2(74, 42)),
    new DesktopInstructionPlatformIconSpec("psp", "r1", new int2(74, 42)),
    new DesktopInstructionPlatformIconSpec("ps2", "r1", new int2(65, 48)),
    new DesktopInstructionPlatformIconSpec("psvita", "r1", new int2(74, 42)),
    new DesktopInstructionPlatformIconSpec("n64", "r", new int2(70, 42)),
    new DesktopInstructionPlatformIconSpec("dreamcast", "r", new int2(70, 42)),
    new DesktopInstructionPlatformIconSpec("ps1", "r1", new int2(65, 48)),
    new DesktopInstructionPlatformIconSpec("ps3", "r1", new int2(65, 48)),
    new DesktopInstructionPlatformIconSpec("xbox", "rb", new int2(78, 45)),
    new DesktopInstructionPlatformIconSpec("steamdeck", "r1", new int2(78, 45))
};

static DesktopInstructionPlatformIconSpec FindRequiredCommonSpec(DesktopInstructionPlatformIconSpec[] specs, string platformId) {
    for (int index = 0; index < specs.Length; index++) {
        if (string.Equals(specs[index].PlatformId, platformId, StringComparison.OrdinalIgnoreCase)) {
            return specs[index];
        }
    }

    throw new InvalidOperationException($"Common prompt icon spec '{platformId}' was not found.");
}
```

- [ ] **Step 2: Author one common sprite plus platform overrides through the editor API**

Replace `CreatePlatformIconEntity(...)` with one helper that writes the common texture reference and then applies per-platform overrides for both `SpriteComponent.Size` and `Texture`.

```csharp
public Entity CreateDesktopInstructionOverlayRoot(string projectRootPath, FontAsset font) {
    if (string.IsNullOrWhiteSpace(projectRootPath)) {
        throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
    }
    if (font == null) {
        throw new ArgumentNullException(nameof(font));
    }

    CreateDesktopInstructionRow(panelEntity, projectRootPath, font, "RotateIcon", "Rotate", DesktopInstructionFirstRowTop, DesktopInstructionRotateTextTopAdjustment, RotateIconSpecs);
    CreateDesktopInstructionRow(panelEntity, projectRootPath, font, "LightIcon", "Light", DesktopInstructionSecondRowTop, DesktopInstructionToggleTextTopAdjustment, LightIconSpecs);
    return viewportRootEntity;
}

void CreateInstructionIconEntity(string projectRootPath, Entity panelEntity, string entityName, float topOffset, DesktopInstructionPlatformIconSpec[] specs, byte renderOrder2D) {
    Entity entity = Core.Instance.EntityFactory.CreateChild(panelEntity, entityName);
    entity.LocalPosition = new float3(DesktopInstructionIconLeft, topOffset, 0.1f);
    entity.LayerMask = DesktopOverlayLayerMask;

    DesktopInstructionPlatformIconSpec commonSpec = FindRequiredCommonSpec(specs, "windows");
    SpriteComponent spriteComponent = new SpriteComponent {
        Size = commonSpec.Size,
        RenderOrder2D = renderOrder2D,
        LayerMask = OverlayDrawableLayerMask
    };
    entity.AddComponent(spriteComponent);

    ResolvedControlIcon commonIcon = ControlIconResolver.RequireIcon(projectRootPath, commonSpec.PlatformId, commonSpec.ControlId);
    ApplyTextureReference(entity, spriteComponent, commonIcon.SourcePngRelativePath);

    for (int index = 0; index < specs.Length; index++) {
        DesktopInstructionPlatformIconSpec spec = specs[index];
        if (string.Equals(spec.PlatformId, commonSpec.PlatformId, StringComparison.OrdinalIgnoreCase)) {
            continue;
        }

        ApplyPlatformSpriteOverride(projectRootPath, entity, spriteComponent, spec);
    }
}

void ApplyPlatformSpriteOverride(string projectRootPath, Entity entity, SpriteComponent commonComponent, DesktopInstructionPlatformIconSpec spec) {
    EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
    SpriteComponent overrideComponent = (SpriteComponent)PlatformEditingService.EnsurePlatformOverrideComponent(
        commonComponent,
        saveComponent,
        spec.PlatformId);
    overrideComponent.Size = spec.Size;
    PlatformEditingService.MarkPropertyOverride(commonComponent, saveComponent, spec.PlatformId, nameof(SpriteComponent.Size));

    ResolvedControlIcon resolvedIcon = ControlIconResolver.RequireIcon(projectRootPath, spec.PlatformId, spec.ControlId);
    PlatformEditingService.StoreAssetReference(
        commonComponent,
        overrideComponent,
        saveComponent,
        spec.PlatformId,
        TextureAssetScenePersistenceSupport.TextureReferenceName,
        BuildFileReference(resolvedIcon.SourcePngRelativePath));
    PlatformEditingService.PersistPlatformOverride(commonComponent, overrideComponent, saveComponent, spec.PlatformId);
}
```

- [ ] **Step 3: Thread `projectRootPath` through every prompt-bearing rendering and physics caller**

Update the rendering scene factories and physics overlay call sites so every prompt-bearing scene passes the real project root into the shared overlay authoring helper.

```csharp
public GeneratedAuthoringSceneDefinition CreateSceneDefinition(string projectRootPath, RuntimeModel cubeModel, RuntimeMaterial solidColorMaterial) {
    if (string.IsNullOrWhiteSpace(projectRootPath)) {
        throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
    }

    FontAsset instructionFont = ResolveRequiredEditorFont();
    DemoSceneInstructionOverlayFactory instructionOverlayFactory = new DemoSceneInstructionOverlayFactory();
    Entity instructionOverlayEntity = instructionOverlayFactory.CreateDesktopInstructionOverlayRoot(projectRootPath, instructionFont);
}
```

Update `RenderingSceneGenerator.cs` similarly:

```csharp
GeneratedAuthoringSceneDefinition cubeTestSceneDefinition = CubeTestFactory.CreateSceneDefinition(
    projectRootPath,
    assets.GeneratedCubeModel,
    assets.GeneratedCubeTestSolidMaterial);
```

Update `PhysicsSceneFactory.cs` similarly by storing the active project root during `WriteScenes(...)` and reusing it when the shared overlay root is authored:

```csharp
string CurrentProjectRootPath = string.Empty;

public void WriteScenes(string projectRootPath) {
    if (string.IsNullOrWhiteSpace(projectRootPath)) {
        throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
    }

    CurrentProjectRootPath = Path.GetFullPath(projectRootPath);
}

EditorEntity CreatePhysicsShowcaseDesktopInstructionOverlayRoot() {
    DemoSceneInstructionOverlayFactory instructionOverlayFactory = new DemoSceneInstructionOverlayFactory();
    Entity overlayRootEntity = instructionOverlayFactory.CreateDesktopInstructionOverlayRoot(CurrentProjectRootPath, ResolveRequiredEditorFont());
    return (EditorEntity)overlayRootEntity;
}
```

- [ ] **Step 4: Collapse the row builder onto one shared scene-authored layout**

Update the row builder so the label stays shared and only the sprite component varies by platform.

```csharp
CreateDesktopInstructionRow(
    panelEntity,
    projectRootPath,
    font,
    "RotateIcon",
    "Rotate",
    DesktopInstructionFirstRowTop,
    DesktopInstructionRotateTextTopAdjustment,
    RotateIconSpecs);

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

Also delete `DemoScenePlatformInstructionIconSetComponent.cs`, because the runtime child-group switcher is obsolete once the scene stores platform overrides directly.

- [ ] **Step 5: Run the overlay source audits to verify they pass**

Run:

```bash
rtk dotnet test C:\dev\helprojs\city\tests\gameplay.tests\gameplay.tests.csproj --filter "PromptIconOverlaySourceTests" -v minimal
```

Expected: `PASS`

- [ ] **Step 6: Commit the overlay migration**

```bash
rtk git -C C:\dev\helprojs\city add assets/codebase/rendering.tools/DemoSceneInstructionOverlayFactory.cs assets/codebase/rendering.tools/CubeTestSceneFactory.cs assets/codebase/rendering.tools/ScaledCubeSceneFactory.cs assets/codebase/rendering.tools/ColoredCubeGridSceneFactory.cs assets/codebase/rendering.tools/DirectionalShadowPlazaSceneFactory.cs assets/codebase/rendering.tools/TexturedCubeGridSceneFactory.cs assets/codebase/rendering.tools/SpotlightStreetSliceSceneFactory.cs assets/codebase/rendering.tools/AxisTestSceneFactory.cs assets/codebase/rendering.tools/AxisTest2SceneFactory.cs assets/codebase/rendering.tools/RenderingSceneGenerator.cs assets/codebase/physics.tools/PhysicsSceneFactory.cs tests/gameplay.tests/PromptIconOverlaySourceTests.cs
rtk git -C C:\dev\helprojs\city rm assets/codebase/rendering/DemoScenePlatformInstructionIconSetComponent.cs
rtk git -C C:\dev\helprojs\city commit -m "feat: author shared prompt icons with platform scene overrides"
```

### Task 5: Regenerate Rendering And Physics Scenes And Verify The New Prompt Paths

**Files:**
- Generated output: `C:\dev\helprojs\city\assets\scenes\rendering\`
- Generated output: `C:\dev\helprojs\city\assets\scenes\physics\`
- Verify against: `C:\dev\helprojs\city\tests\gameplay.tests\gameplay.tests.csproj`

- [ ] **Step 1: Run the focused city tests**

Run:

```bash
rtk dotnet test C:\dev\helprojs\city\tests\gameplay.tests\gameplay.tests.csproj --filter "GeneratedControlIconAssetResolverTests|PromptIconOverlaySourceTests" -v minimal
```

Expected: `PASS`

- [ ] **Step 2: Regenerate the rendering scenes**

Run:

```bash
rtk dotnet run --project C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\city\project.heproj --editor-command menu.generate-rendering-scenes
```

Expected: the editor command exits successfully after rewriting the rendering scene assets.

- [ ] **Step 3: Regenerate the physics scenes**

Run:

```bash
rtk dotnet run --project C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\city\project.heproj --editor-command menu.generate-physics-scenes
```

Expected: the editor command exits successfully after rewriting the physics scene assets.

- [ ] **Step 4: Verify legacy prompt paths are gone and generated paths are present**

Run:

```bash
rtk powershell -NoProfile -Command "& { rg -a 'images/instructions/controls/(xbox360_|ps2_|switch_)' 'C:\dev\helprojs\city\assets\scenes\rendering' 'C:\dev\helprojs\city\assets\scenes\physics'; if ($LASTEXITCODE -eq 0) { throw 'Legacy control prompt paths are still present in generated scenes.' } }"
rtk powershell -NoProfile -Command "& { rg -a 'images/instructions/controls/generated/' 'C:\dev\helprojs\city\assets\scenes\rendering' 'C:\dev\helprojs\city\assets\scenes\physics' }"
```

Expected:

```text
first command: no matches
second command: matches under rendering and physics scene assets
```

- [ ] **Step 5: Commit the regenerated scenes and final implementation**

```bash
rtk git -C C:\dev\helprojs\city add assets/codebase/rendering.tools/ResolvedControlIcon.cs assets/codebase/rendering.tools/GeneratedControlIconPlatformMap.cs assets/codebase/rendering.tools/GeneratedControlIconCatalog.cs assets/codebase/rendering.tools/GeneratedControlIconAssetResolver.cs assets/codebase/rendering.tools/DemoSceneInstructionOverlayFactory.cs tests/gameplay.tests/GeneratedControlIconAssetResolverTests.cs tests/gameplay.tests/PromptIconOverlaySourceTests.cs assets/scenes/rendering assets/scenes/physics docs/superpowers/plans/2026-07-08-shared-control-icon-resolution.md
rtk git -C C:\dev\helprojs\city commit -m "feat: migrate shared prompt icons to generated control assets"
```
