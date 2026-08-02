# Demo Disc Scene Labels Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** Add the approved "$. Scene Name" overlay to the 12 non-game Demo Scenes and Physics Scenes entries, while leaving game screens and DS/3DS generators unchanged.

**Architecture:** Add one shared DemoDiscSceneLabelOverlayFactory beside the existing overlay factories. Targeted rendering factories attach explicit labels to their existing UI roots; PhysicsSceneFactory resolves labels only for Matrix Render and the five curated physics entries. The existing generated-scene writer continues to exclude common roots from DS/3DS.

**Tech Stack:** C#, Helengine editor scene authoring APIs, xUnit source-level regression tests, generated .NET projects, PowerShell.

---

### Task 1: Add the failing source contract

**Files:**
- Create: assets/codebase/rendering.tools.tests/DemoDiscSceneLabelOverlaySourceTests.cs
- Test: user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj

- [ ] **Step 1: Write the failing tests**

Create the test file with these assertions:

    namespace city.tests {
        public sealed class DemoDiscSceneLabelOverlaySourceTests {
            const string ProjectRootPath = @"C:\dev\helprojs\demodisc";

            [Fact]
            public void Shared_label_overlay_uses_fixed_top_right_body_font_layout() {
                string sourcePath = Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "DemoDiscSceneLabelOverlayFactory.cs");
                Assert.True(File.Exists(sourcePath), $"Expected '{sourcePath}' to exist.");
                string source = File.ReadAllText(sourcePath);
                Assert.Contains("const int ReferenceViewportWidth = 1280;", source, StringComparison.Ordinal);
                Assert.Contains("const int ReferenceViewportHeight = 720;", source, StringComparison.Ordinal);
                Assert.Contains("const float SceneLabelRight = 24f;", source, StringComparison.Ordinal);
                Assert.Contains("const float SceneLabelTop = 24f;", source, StringComparison.Ordinal);
                Assert.Contains("BindingMode = ViewportComponent.ScreenBindingMode", source, StringComparison.Ordinal);
                Assert.Contains("Alignment = TextAlignment.Right", source, StringComparison.Ordinal);
                Assert.Contains("DemoDiscSceneComponentRecordFactory.CreateEditorFontReference()", source, StringComparison.Ordinal);
                Assert.Contains("RenderOrder2D = SceneLabelRenderOrder", source, StringComparison.Ordinal);
            }

            [Fact]
            public void Curated_rendering_factories_contain_the_approved_labels() {
                (string FileName, string Label)[] expected = [
                    ("CubeTestSceneFactory.cs", "1. Cube Test"),
                    ("ColoredCubeGridSceneFactory.cs", "2. Colored Cubes"),
                    ("TexturedCubeGridSceneFactory.cs", "3. Textured Cubes"),
                    ("AxisTestSceneFactory.cs", "4. Axis 1"),
                    ("AxisTest2SceneFactory.cs", "5. Axis 2"),
                    ("DirectionalShadowPlazaSceneFactory.cs", "7. Shadow Plaza")
                ];
                foreach ((string fileName, string label) in expected) {
                    string source = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", fileName));
                    Assert.Contains("DemoDiscSceneLabelOverlayFactory", source, StringComparison.Ordinal);
                    Assert.Contains($"\"{label}\"", source, StringComparison.Ordinal);
                }
            }

            [Fact]
            public void Physics_factory_contains_only_the_curated_physics_labels() {
                string source = File.ReadAllText(Path.Combine(ProjectRootPath, "assets", "codebase", "physics.tools", "PhysicsSceneFactory.cs"));
                Assert.Contains("DemoDiscSceneLabelOverlayFactory", source, StringComparison.Ordinal);
                foreach (string label in new[] {
                    "6. Matrix Render", "8. Stacked Boxes", "9. Sphere Stack",
                    "10. Mixed Stack", "11. Static Mesh", "12. Simple Mesh"
                }) {
                    Assert.Contains($"\"{label}\"", source, StringComparison.Ordinal);
                }
                Assert.Contains("ResolveDemoDiscSceneLabel", source, StringComparison.Ordinal);
            }

            [Fact]
            public void Game_and_handheld_generators_do_not_reference_the_label_overlay() {
                string[] paths = [
                    Path.Combine(ProjectRootPath, "assets", "codebase", "game.tools", "GameSceneFactory.cs"),
                    Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "NintendoDsRenderingSceneScaffoldFactory.cs"),
                    Path.Combine(ProjectRootPath, "assets", "codebase", "physics.tools", "PhysicsNintendoDsSceneGenerator.cs")
                ];
                foreach (string path in paths) {
                    Assert.DoesNotContain("DemoDiscSceneLabelOverlayFactory", File.ReadAllText(path), StringComparison.Ordinal);
                }
            }

            [Fact]
            public void Non_menu_rendering_factories_remain_without_scene_labels() {
                string[] paths = [
                    Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "GroundCubeProbeSceneFactory.cs"),
                    Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "ScaledCubeSceneFactory.cs"),
                    Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "SpotlightStreetSliceSceneFactory.cs"),
                    Path.Combine(ProjectRootPath, "assets", "codebase", "rendering.tools", "SceneMemoryProbeSceneFactory.cs")
                ];
                foreach (string path in paths) {
                    Assert.DoesNotContain("DemoDiscSceneLabelOverlayFactory", File.ReadAllText(path), StringComparison.Ordinal);
                }
            }
        }
    }

- [ ] **Step 2: Run the new test and verify the expected red state**

Run:

    dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter FullyQualifiedName~DemoDiscSceneLabelOverlaySourceTests

Expected: the test project builds and the new tests fail because the shared factory and targeted call sites do not exist. Fix only test setup errors before continuing.

### Task 2: Implement the shared overlay

**Files:**
- Create: assets/codebase/rendering.tools/DemoDiscSceneLabelOverlayFactory.cs

- [ ] **Step 1: Add the minimal shared factory**

Create the factory with the existing overlay conventions:

    using helengine.editor;

    namespace city.rendering.tools {
        public sealed class DemoDiscSceneLabelOverlayFactory {
            public const string LabelViewportEntityName = "DemoDiscSceneLabelViewport";
            public const string LabelEntityName = "DemoDiscSceneLabelText";
            const string FontReferenceName = "Font";
            const int ReferenceViewportWidth = 1280;
            const int ReferenceViewportHeight = 720;
            const float SceneLabelRight = 24f;
            const float SceneLabelTop = 24f;
            const int SceneLabelWidth = 420;
            const int SceneLabelHeight = 32;
            const float SceneLabelFontScale = 1.5f;
            const int SceneLabelRenderOrder = 255;

            public void AttachToSceneUi(Entity sceneUiEntity, FontAsset font, string labelText) {
                if (sceneUiEntity == null) {
                    throw new ArgumentNullException(nameof(sceneUiEntity));
                } else if (font == null) {
                    throw new ArgumentNullException(nameof(font));
                } else if (string.IsNullOrWhiteSpace(labelText)) {
                    throw new ArgumentException("Scene label text must be provided.", nameof(labelText));
                }

                ushort overlayLayerMask = sceneUiEntity.LayerMask;
                Entity viewportEntity = Core.Instance.EntityFactory.CreateChild(sceneUiEntity, LabelViewportEntityName);
                viewportEntity.LayerMask = overlayLayerMask;
                viewportEntity.AddComponent(new ViewportComponent {
                    BindingMode = ViewportComponent.ScreenBindingMode,
                    FixedSize = new int2(ReferenceViewportWidth, ReferenceViewportHeight)
                });

                Entity labelEntity = Core.Instance.EntityFactory.CreateChild(viewportEntity, LabelEntityName);
                labelEntity.LocalPosition = new float3(
                    ReferenceViewportWidth - SceneLabelRight - SceneLabelWidth,
                    SceneLabelTop,
                    0.1f);
                labelEntity.LayerMask = overlayLayerMask;
                TextComponent labelComponent = new TextComponent {
                    Text = labelText,
                    Font = font,
                    FontScale = SceneLabelFontScale,
                    Alignment = TextAlignment.Right,
                    Color = new byte4(255, 255, 255, 255),
                    Size = new int2(SceneLabelWidth, SceneLabelHeight),
                    RenderOrder2D = SceneLabelRenderOrder
                };
                labelEntity.AddComponent(labelComponent);
                EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(labelEntity);
                saveComponent.SetAssetReference(
                    labelComponent,
                    FontReferenceName,
                    DemoDiscSceneComponentRecordFactory.CreateEditorFontReference());
            }

            EntitySaveComponent FindRequiredEntitySaveComponent(Entity entity) {
                if (entity == null) {
                    throw new ArgumentNullException(nameof(entity));
                } else if (entity.Components == null) {
                    throw new InvalidOperationException("Generated entities must expose initialized component collections.");
                }
                for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                    if (entity.Components[componentIndex] is EntitySaveComponent saveComponent) {
                        return saveComponent;
                    }
                }
                throw new InvalidOperationException("Generated entity is missing required save state.");
            }
        }
    }

- [ ] **Step 2: Run the shared-layout test**

Run the single Shared_label_overlay_uses_fixed_top_right_body_font_layout test. Expected: PASS; factory coverage tests remain red until call sites are added.

### Task 3: Attach labels to curated rendering scenes

**Files:**
- Modify: assets/codebase/rendering.tools/CubeTestSceneFactory.cs
- Modify: assets/codebase/rendering.tools/ColoredCubeGridSceneFactory.cs
- Modify: assets/codebase/rendering.tools/TexturedCubeGridSceneFactory.cs
- Modify: assets/codebase/rendering.tools/AxisTestSceneFactory.cs
- Modify: assets/codebase/rendering.tools/AxisTest2SceneFactory.cs
- Modify: assets/codebase/rendering.tools/DirectionalShadowPlazaSceneFactory.cs

- [ ] **Step 1: Add the shared call to each targeted UI root**

Immediately before each targeted UI-root method returns its entity, add:

    DemoDiscSceneLabelOverlayFactory sceneLabelOverlayFactory = new DemoDiscSceneLabelOverlayFactory();
    sceneLabelOverlayFactory.AttachToSceneUi(entity, ResolveRequiredEditorFont(), "1. Cube Test");

Use these exact labels:

    CubeTestSceneFactory.cs                 "1. Cube Test"
    ColoredCubeGridSceneFactory.cs          "2. Colored Cubes"
    TexturedCubeGridSceneFactory.cs         "3. Textured Cubes"
    AxisTestSceneFactory.cs                 "4. Axis 1"
    AxisTest2SceneFactory.cs                "5. Axis 2"
    DirectionalShadowPlazaSceneFactory.cs   "7. Shadow Plaza"

Do not add the helper to GroundCubeProbeSceneFactory, ScaledCubeSceneFactory, SpotlightStreetSliceSceneFactory, or SceneMemoryProbeSceneFactory.

- [ ] **Step 2: Run the rendering coverage test**

Run the Curated_rendering_factories_contain_the_approved_labels test. Expected: PASS for all six factories.

### Task 4: Attach Matrix Render and curated physics labels

**Files:**
- Modify: assets/codebase/physics.tools/PhysicsSceneFactory.cs

- [ ] **Step 1: Add the stable physics-id label resolver**

Add:

    static string ResolveDemoDiscSceneLabel(string sceneId) {
        if (string.IsNullOrWhiteSpace(sceneId)) {
            throw new ArgumentException("Scene id must be provided.", nameof(sceneId));
        } else if (string.Equals(sceneId, PhysicsSceneCatalog.DynamicStackBoxesSceneId, StringComparison.Ordinal)) {
            return "8. Stacked Boxes";
        } else if (string.Equals(sceneId, PhysicsSceneCatalog.DynamicSphereStackSceneId, StringComparison.Ordinal)) {
            return "9. Sphere Stack";
        } else if (string.Equals(sceneId, PhysicsSceneCatalog.DynamicMixedStackSceneId, StringComparison.Ordinal)) {
            return "10. Mixed Stack";
        } else if (string.Equals(sceneId, PhysicsSceneCatalog.StaticMeshShowcaseSceneId, StringComparison.Ordinal)) {
            return "11. Static Mesh";
        } else if (string.Equals(sceneId, PhysicsSceneCatalog.StaticMeshMinimalSceneId, StringComparison.Ordinal)) {
            return "12. Simple Mesh";
        }
        return string.Empty;
    }

- [ ] **Step 2: Pass the resolver result through both physics UI construction paths**

Change both existing calls from CreateLivePhysicsShowcaseUiEntity() to:

    CreateLivePhysicsShowcaseUiEntity(ResolveDemoDiscSceneLabel(sceneId))

Use normalizedSceneId in the regenerated playable-scene method. Change the method to accept string sceneLabel, keep its existing FPS/light behavior, and add:

    if (!string.IsNullOrWhiteSpace(sceneLabel)) {
        city.rendering.tools.DemoDiscSceneLabelOverlayFactory sceneLabelOverlayFactory = new city.rendering.tools.DemoDiscSceneLabelOverlayFactory();
        sceneLabelOverlayFactory.AttachToSceneUi(entity, ResolveRequiredEditorFont(), sceneLabel);
    }

This leaves Strict Rotated Box Compare unlabeled because the resolver returns an empty string.

- [ ] **Step 3: Add Matrix Render’s dedicated label**

In CreateLiveMatrixRenderUiEntity, after the phase-status component is attached, add:

    city.rendering.tools.DemoDiscSceneLabelOverlayFactory sceneLabelOverlayFactory = new city.rendering.tools.DemoDiscSceneLabelOverlayFactory();
    sceneLabelOverlayFactory.AttachToSceneUi(entity, ResolveRequiredEditorFont(), "6. Matrix Render");

- [ ] **Step 4: Run all focused source tests**

Run the full DemoDiscSceneLabelOverlaySourceTests filter. Expected: all five tests pass.

### Task 5: Build, regenerate, and verify generated scenes

**Files:**
- Generated: targeted assets/scenes/rendering/*.helen and assets/scenes/physics/*.helen outputs.

- [ ] **Step 1: Build the affected tool projects**

Run:

    dotnet build user_settings/generated_code/projects/rendering.tools/rendering.tools.csproj
    dotnet build user_settings/generated_code/projects/physics.tools/physics.tools.csproj
    dotnet build user_settings/generated_code/projects/menu.tools/menu.tools.csproj

Expected: all three commands exit 0 with no compiler errors.

- [ ] **Step 2: Regenerate rendering and physics scenes**

Run:

    dotnet run --project C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\demodisc\project.heproj --editor-command menu.generate-rendering-scenes
    dotnet run --project C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\demodisc\project.heproj --editor-command menu.generate-physics-scenes

Expected: both editor commands report successful generation.

- [ ] **Step 3: Inspect generated text and protected sources**

Run:

    rg -n "1\. Cube Test|2\. Colored Cubes|3\. Textured Cubes|4\. Axis 1|5\. Axis 2|6\. Matrix Render|7\. Shadow Plaza|8\. Stacked Boxes|9\. Sphere Stack|10\. Mixed Stack|11\. Static Mesh|12\. Simple Mesh" assets/scenes
    rg -n "DemoDiscSceneLabelViewport|DemoDiscSceneLabelText" assets/scenes
    git diff --name-only -- assets/codebase/game.tools/GameSceneFactory.cs assets/codebase/rendering.tools/NintendoDsRenderingSceneScaffoldFactory.cs assets/codebase/physics.tools/PhysicsNintendoDsSceneGenerator.cs

Expected: all 12 labels occur in targeted common scene assets, and the protected-source diff is empty.

### Task 6: Run complete affected verification and commit the feature

**Files:**
- Verify: assets/codebase/rendering.tools.tests
- Verify: assets/codebase/menu.tools.tests

- [ ] **Step 1: Run focused and complete affected test projects**

Run:

    dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj --filter FullyQualifiedName~DemoDiscSceneLabelOverlaySourceTests
    dotnet test user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj
    dotnet test user_settings/generated_code/projects/menu.tools.tests/menu.tools.tests.csproj

Expected: all commands pass with zero failed tests.

- [ ] **Step 2: Review the feature-only diff**

Run git diff --check and inspect git diff --name-only for the helper, six rendering factories, physics factory, focused test, generated targeted scenes, and this plan. Confirm unrelated existing staged and unstaged work remains untouched.

- [ ] **Step 3: Commit only feature files**

Stage the feature-owned files explicitly and commit:

    git add assets/codebase/rendering.tools/DemoDiscSceneLabelOverlayFactory.cs assets/codebase/rendering.tools.tests/DemoDiscSceneLabelOverlaySourceTests.cs assets/codebase/rendering.tools/CubeTestSceneFactory.cs assets/codebase/rendering.tools/ColoredCubeGridSceneFactory.cs assets/codebase/rendering.tools/TexturedCubeGridSceneFactory.cs assets/codebase/rendering.tools/AxisTestSceneFactory.cs assets/codebase/rendering.tools/AxisTest2SceneFactory.cs assets/codebase/rendering.tools/DirectionalShadowPlazaSceneFactory.cs assets/codebase/physics.tools/PhysicsSceneFactory.cs assets/scenes/rendering assets/scenes/physics docs/superpowers/plans/2026-08-02-demo-disc-scene-labels.md
    git commit -m "feat: label demo disc scenes"

Do not stage or modify GameSceneFactory.cs, NintendoDsRenderingSceneScaffoldFactory.cs, PhysicsNintendoDsSceneGenerator.cs, or unrelated pre-existing changes.
