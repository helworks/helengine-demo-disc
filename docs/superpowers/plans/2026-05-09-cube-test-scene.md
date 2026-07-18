# Cube Test Scene Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add one committed cube-test scene to `city`, make it the active startup target, and temporarily reduce the rendering scene export set to only that scene.

**Architecture:** Follow the existing generated rendering-scene pattern in `city.rendering.tools`. Add one new `CubeTestSceneFactory`, update `RenderingSceneGenerator` to emit only that scene, and update the menu/catalog and startup-facing scene selection content so the build boots directly into the cube scene for now.

**Tech Stack:** C#, city generated-scene tooling, helengine scene assets, PS2 build/export flow

---

### Task 1: Add a Generated Cube-Test Scene Factory

**Files:**
- Create: `C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\CubeTestSceneFactory.cs`
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\RenderingSceneGenerator.cs`
- Test: `C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\GeneratedRuntimeSceneViewportSourceTests.cs`

- [ ] **Step 1: Write the failing test**

Add this test to `GeneratedRuntimeSceneViewportSourceTests.cs`:

```csharp
        /// <summary>
        /// Ensures the city rendering generator defines one minimal cube-test scene with a normalized fullscreen camera viewport.
        /// </summary>
        [Fact]
        public void Cube_test_scene_source_uses_fullscreen_camera_and_no_runtime_motion_scripts() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\CubeTestSceneFactory.cs");

            Assert.Contains("public const string SceneId = RenderingSceneGenerator.CubeTestSceneId;", source, StringComparison.Ordinal);
            Assert.Contains("writer.WriteField(\"Viewport\", fieldWriter => fieldWriter.WriteFloat4(new float4(0f, 0f, 1f, 1f)));", source, StringComparison.Ordinal);
            Assert.DoesNotContain("CreateOrbitRecord", source, StringComparison.Ordinal);
            Assert.DoesNotContain("CreateSunSweepRecord", source, StringComparison.Ordinal);
            Assert.DoesNotContain("CreateCameraOrbitRecord", source, StringComparison.Ordinal);
        }
```

- [ ] **Step 2: Run the test to verify it fails**

Run:

```powershell
rtk dotnet test "C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\helengine.editor.tests.csproj" -c Debug --filter FullyQualifiedName~Cube_test_scene_source_uses_fullscreen_camera_and_no_runtime_motion_scripts
```

Expected: `FAIL` because `CubeTestSceneFactory.cs` does not exist yet.

- [ ] **Step 3: Create the minimal scene factory**

Create `CubeTestSceneFactory.cs` using the same style as the other rendering scene factories. The scene must contain exactly:

- one camera entity
- one directional light entity
- one cube entity

Use this structure:

```csharp
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the canonical authored scene asset for the minimal cube rendering test.
    /// </summary>
    public sealed class CubeTestSceneFactory {
        /// <summary>
        /// Stable scene id used by the generated cube-test asset.
        /// </summary>
        public const string SceneId = RenderingSceneGenerator.CubeTestSceneId;

        const string MeshComponentTypeId = "helengine.MeshComponent";
        const string CameraComponentTypeId = "helengine.CameraComponent";
        const string DirectionalLightComponentTypeId = "helengine.DirectionalLightComponent";
        const ushort SceneObjectsLayerMask = 0b0100000000000000;
        const string MeshModelReferenceName = "Model";
        const string MeshMaterialReferenceName = "Material";

        readonly MeshComponentPersistenceDescriptor MeshDescriptor;
        readonly DirectionalLightComponentPersistenceDescriptor DirectionalLightDescriptor;
        readonly AuthoringPlaceholderRuntimeModel PlaceholderModel;
        readonly RuntimeMaterial PlaceholderMaterial;

        /// <summary>
        /// Initializes the cube-test scene factory.
        /// </summary>
        public CubeTestSceneFactory() {
            MeshDescriptor = new MeshComponentPersistenceDescriptor();
            DirectionalLightDescriptor = new DirectionalLightComponentPersistenceDescriptor();
            PlaceholderModel = new AuthoringPlaceholderRuntimeModel();
            PlaceholderMaterial = new RuntimeMaterial();
        }

        /// <summary>
        /// Creates the canonical cube-test scene asset.
        /// </summary>
        public SceneAsset CreateSceneAsset(SceneAssetReference cubeReference, SceneAssetReference standardMaterialReference) {
            if (cubeReference == null) {
                throw new ArgumentNullException(nameof(cubeReference));
            } else if (standardMaterialReference == null) {
                throw new ArgumentNullException(nameof(standardMaterialReference));
            }

            return new SceneAsset {
                Id = SceneId,
                AssetReferences = new[] {
                    cubeReference,
                    standardMaterialReference
                },
                RootEntities = new[] {
                    CreateCameraEntity(),
                    CreateDirectionalLightEntity(),
                    CreateCubeEntity(cubeReference, standardMaterialReference)
                }
            };
        }
```

Continue the file in the same existing style with:

- static camera
- no script components
- one directional light
- one cube at the origin with a simple visible scale
- normalized camera viewport `0,0,1,1`

- [ ] **Step 4: Update the rendering scene generator to define and use the new scene id**

In `RenderingSceneGenerator.cs`, add:

```csharp
        /// <summary>
        /// Stable scene id used by the minimal cube rendering test.
        /// </summary>
        public const string CubeTestSceneId = "scenes/rendering/cube_test.helen";
```

Replace the factory fields and constructor setup with:

```csharp
        /// <summary>
        /// Factory used to author the minimal cube rendering test scene.
        /// </summary>
        readonly CubeTestSceneFactory CubeTestFactory;

        /// <summary>
        /// Initializes one city rendering scene generator.
        /// </summary>
        public RenderingSceneGenerator() {
            SceneWriteService = new GeneratedSceneWriteService();
            CubeTestFactory = new CubeTestSceneFactory();
        }
```

Replace the current `Generate(...)` body with:

```csharp
        public void Generate(string projectRootPath) {
            SceneAssetReference cubeReference = CreateGeneratedReference(EngineGeneratedAssetProvider.CubeRelativePath, EngineGeneratedModelCache.CubeAssetId);
            SceneAssetReference standardMaterialReference = CreateGeneratedReference(EngineGeneratedAssetProvider.StandardMaterialRelativePath, EngineGeneratedMaterialCache.StandardAssetId);

            SceneAsset cubeTestSceneAsset = CubeTestFactory.CreateSceneAsset(
                cubeReference,
                standardMaterialReference);
            SceneWriteService.WriteScene(projectRootPath, CubeTestSceneId, cubeTestSceneAsset);
        }
```

- [ ] **Step 5: Run the focused test to verify it passes**

Run:

```powershell
rtk dotnet test "C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\helengine.editor.tests.csproj" -c Debug --filter FullyQualifiedName~Cube_test_scene_source_uses_fullscreen_camera_and_no_runtime_motion_scripts
```

Expected: `PASS`

- [ ] **Step 6: Commit the scene factory task**

```powershell
rtk git -C "C:\dev\helprojs\demodisc" add -- "assets/codebase/rendering.tools/CubeTestSceneFactory.cs" "assets/codebase/rendering.tools/RenderingSceneGenerator.cs"
rtk git -C "C:\dev\helprojs\demodisc" commit -m "feat: add cube test rendering scene"
```

### Task 2: Reduce the Generated Rendering Export Set to the Cube Scene

**Files:**
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\menu\DemoDiscSceneCatalog.cs`
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\RenderingSceneGenerator.cs`
- Test: `C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\tools\DemoDiscSceneWriterTests.cs`

- [ ] **Step 1: Write the failing test**

Add this test to `DemoDiscSceneWriterTests.cs`:

```csharp
        /// <summary>
        /// Ensures the city demo scene catalog points at the cube-test scene only during the minimal rendering debug configuration.
        /// </summary>
        [Fact]
        public void Demo_disc_scene_catalog_source_lists_only_cube_test_scene() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\menu\DemoDiscSceneCatalog.cs");

            Assert.Contains("scenes/rendering/cube_test.helen", source, StringComparison.Ordinal);
            Assert.DoesNotContain("scenes/rendering/directional_shadow_plaza.helen", source, StringComparison.Ordinal);
            Assert.DoesNotContain("scenes/rendering/spotlight_street_slice.helen", source, StringComparison.Ordinal);
        }
```

- [ ] **Step 2: Run the test to verify it fails**

Run:

```powershell
rtk dotnet test "C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\helengine.editor.tests.csproj" -c Debug --filter FullyQualifiedName~Demo_disc_scene_catalog_source_lists_only_cube_test_scene
```

Expected: `FAIL` because the catalog still references the old rendering scenes.

- [ ] **Step 3: Reduce the menu scene catalog to the cube scene**

In `DemoDiscSceneCatalog.cs`, replace the rendering scene entries with one cube-scene item:

```csharp
                new MenuItemDefinition("scene-cube-test", "Cube Test", "Minimal cube, camera, and light scene for renderer debugging.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "scenes/rendering/cube_test.helen")),
```

Remove the directional-shadow and spotlight-street entries from the current list.

- [ ] **Step 4: Run the focused test to verify it passes**

Run:

```powershell
rtk dotnet test "C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\helengine.editor.tests.csproj" -c Debug --filter FullyQualifiedName~Demo_disc_scene_catalog_source_lists_only_cube_test_scene
```

Expected: `PASS`

- [ ] **Step 5: Commit the reduced export/list task**

```powershell
rtk git -C "C:\dev\helprojs\demodisc" add -- "assets/codebase/menu/DemoDiscSceneCatalog.cs"
rtk git -C "C:\dev\helprojs\demodisc" commit -m "feat: point demo scene catalog to cube test"
```

### Task 3: Make the Cube Scene the Active Startup Target and Verify Export

**Files:**
- Modify: `C:\dev\helprojs\demodisc\project.heproj`
- Use: `C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\helengine.ui\helengine.editor.app\bin\Debug\net9.0-windows\helengine.editor.app.dll`
- Output: `C:\dev\helprojs\output\ps2-cube-test`

- [ ] **Step 1: Write the failing test**

Add this test to `DemoDiscSceneWriterTests.cs`:

```csharp
        /// <summary>
        /// Ensures the city project startup scene is redirected to the cube-test rendering scene during the minimal renderer debug configuration.
        /// </summary>
        [Fact]
        public void City_project_startup_scene_points_to_cube_test_scene() {
            string projectSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\project.heproj");

            Assert.Contains("scenes/rendering/cube_test.helen", projectSource, StringComparison.Ordinal);
        }
```

- [ ] **Step 2: Run the test to verify it fails**

Run:

```powershell
rtk dotnet test "C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\helengine.editor.tests.csproj" -c Debug --filter FullyQualifiedName~City_project_startup_scene_points_to_cube_test_scene
```

Expected: `FAIL` because `project.heproj` still points somewhere else.

- [ ] **Step 3: Update the city project startup scene**

Edit `project.heproj` so the configured startup scene points to:

```text
scenes/rendering/cube_test.helen
```

Preserve the existing project file structure and formatting; change only the startup-scene value.

- [ ] **Step 4: Run both focused startup/list tests**

Run:

```powershell
rtk dotnet test "C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\helengine.editor.tests.csproj" -c Debug --filter "FullyQualifiedName~City_project_startup_scene_points_to_cube_test_scene|FullyQualifiedName~Demo_disc_scene_catalog_source_lists_only_cube_test_scene|FullyQualifiedName~Cube_test_scene_source_uses_fullscreen_camera_and_no_runtime_motion_scripts"
```

Expected: `PASS`

- [ ] **Step 5: Export the PS2 build**

Run:

```powershell
rtk proxy powershell.exe -NoProfile -Command "dotnet 'C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\helengine.ui\helengine.editor.app\bin\Debug\net9.0-windows\helengine.editor.app.dll' --build ps2 --project 'C:\dev\helprojs\demodisc\project.heproj' --output 'C:\dev\helprojs\output\ps2-cube-test'"
```

Expected:

```text
Build completed for platform 'ps2': C:\dev\helprojs\output\ps2-cube-test
```

- [ ] **Step 6: Verify the ISO artifact exists**

Run:

```powershell
rtk proxy powershell.exe -NoProfile -Command "Get-Item 'C:\dev\helprojs\output\ps2-cube-test\game.iso' | Select-Object FullName,Length,LastWriteTime"
```

Expected: one `game.iso` entry with a fresh timestamp.

- [ ] **Step 7: Manual runtime verification**

Manual checks:

1. Boot `C:\dev\helprojs\output\ps2-cube-test\game.iso`
2. Confirm it goes directly to the cube scene with no menu
3. Confirm only the cube, camera, and light are present
4. Record how the cube renders on PS2

- [ ] **Step 8: Commit the startup/export task**

```powershell
rtk git -C "C:\dev\helprojs\demodisc" add -- "project.heproj"
rtk git -C "C:\dev\helprojs\demodisc" commit -m "feat: boot city into cube test scene"
```
