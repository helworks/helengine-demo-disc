# Matrix Render Scene Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a new render-only Windows probe scene with one animated hero cube and a few static reference cubes so transform bugs can be inspected through move, rotate, scale, pairwise combinations, and the full triple combination.

**Architecture:** Keep the probe isolated from BEPU by authoring it entirely through `CreateCubeMeshEntity(...)` plus one new `UpdateComponent`-derived city runtime script that drives the hero cube through a fixed phase loop. Expose the scene through the existing physics scene catalog/factory, then point the Windows build config directly at it for fast rebuild-and-run debugging.

**Tech Stack:** C#, xUnit source-audit tests, helengine scene generation, city runtime script components, Windows loose-file platform build

---

## File Structure

- `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityPhysicsSceneSourceTests.cs`
  Adds one source-audit test that locks the new scene id, scene factory method, hero/reference entity ids, and render-only component wiring into place before implementation.
- `C:\dev\helprojs\city\assets\codebase\rendering\MatrixRenderComponent.cs`
  Defines the dedicated runtime animation component that applies the seven deterministic move/rotate/scale phases to the hero cube without introducing any physics dependencies.
- `C:\dev\helprojs\city\assets\codebase\physics.tools\PhysicsSceneCatalog.cs`
  Exposes the new `test_scene_matrix_render` scene id and inserts it into the stable generated-physics scene order.
- `C:\dev\helprojs\city\assets\codebase\physics.tools\PhysicsSceneFactory.cs`
  Routes the new scene id, authors the static reference cubes plus fixed camera, and creates the hero cube entity with `MatrixRenderComponent`.
- `C:\dev\helprojs\city\user_settings\build_config.json`
  Points the Windows loose-file build directly at the new motion probe scene so each rebuild launches straight into the visual harness.

### Task 1: Add Failing Source Coverage For The New Matrix Render Scene

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityPhysicsSceneSourceTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
/// <summary>
/// Ensures the matrix render scene is exposed by the catalog and factory as a visual-only scene with one animated hero cube.
/// </summary>
[Fact]
public void City_matrix_render_scene_source_is_exposed_as_visual_only_with_matrix_render_component() {
    string catalogSourcePath = @"C:\dev\helprojs\city\assets\codebase\physics.tools\PhysicsSceneCatalog.cs";
    string catalogSource = File.ReadAllText(catalogSourcePath);
    string factorySourcePath = @"C:\dev\helprojs\city\assets\codebase\physics.tools\PhysicsSceneFactory.cs";
    string factorySource = File.ReadAllText(factorySourcePath);

    Assert.Contains("public const string MatrixRenderSceneId = \"scenes/physics/test_scene_matrix_render.helen\";", catalogSource, StringComparison.Ordinal);
    Assert.Contains("MatrixRenderSceneId,", catalogSource, StringComparison.Ordinal);
    Assert.Contains("CreateMatrixRenderScene()", factorySource, StringComparison.Ordinal);
    Assert.Contains("\"matrix_render.hero\"", factorySource, StringComparison.Ordinal);
    Assert.Contains("\"matrix_render.flat_control\"", factorySource, StringComparison.Ordinal);
    Assert.Contains("\"matrix_render.rotated_scaled_reference\"", factorySource, StringComparison.Ordinal);
    Assert.Contains("new city.rendering.MatrixRenderComponent", factorySource, StringComparison.Ordinal);
    Assert.DoesNotContain("CreatePhysicsBoxMeshEntity(\"matrix_render", factorySource, StringComparison.Ordinal);
    Assert.DoesNotContain("CreatePhysicsSphereMeshEntity(\"matrix_render", factorySource, StringComparison.Ordinal);
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter FullyQualifiedName~City_matrix_render_scene_source_is_exposed_as_visual_only_with_matrix_render_component -v minimal
```

Expected: `FAIL` because the catalog constant, factory method, and matrix render component usage do not exist yet.

- [ ] **Step 3: Re-run after each implementation slice until the test passes**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter FullyQualifiedName~City_matrix_render_scene_source_is_exposed_as_visual_only_with_matrix_render_component -v minimal
```

Expected: `PASS`

### Task 2: Add The Matrix Render Runtime Component

**Files:**
- Create: `C:\dev\helprojs\city\assets\codebase\rendering\MatrixRenderComponent.cs`

- [ ] **Step 1: Create the component skeleton**

```csharp
namespace city.rendering {
    /// <summary>
    /// Drives one render-only probe cube through a deterministic transform phase loop for matrix-order debugging.
    /// </summary>
    public sealed class MatrixRenderComponent : UpdateComponent {
        /// <summary>
        /// Gets or sets the local origin that all probe phases animate around.
        /// </summary>
        public float3 BaseLocalPosition { get; set; }

        /// <summary>
        /// Gets or sets the local offset applied by motion-enabled phases.
        /// </summary>
        public float3 MotionOffset { get; set; }

        /// <summary>
        /// Gets or sets the neutral local scale used by non-scaling phases.
        /// </summary>
        public float3 BaseLocalScale { get; set; }

        /// <summary>
        /// Gets or sets the non-uniform local scale used by scaling phases.
        /// </summary>
        public float3 ScaledLocalScale { get; set; }

        /// <summary>
        /// Gets or sets the quaternion used by rotation-enabled phases.
        /// </summary>
        public float4 RotatedLocalOrientation { get; set; }

        /// <summary>
        /// Gets or sets the duration of each phase in seconds.
        /// </summary>
        public double PhaseDurationSeconds { get; set; }

        /// <summary>
        /// Advances the parent cube through the fixed move/rotate/scale phase sequence.
        /// </summary>
        public override void Update() {
        }
    }
}
```

- [ ] **Step 2: Implement the exact seven-phase loop**

```csharp
public override void Update() {
    base.Update();

    if (Parent == null) {
        throw new InvalidOperationException("MatrixRenderComponent requires an attached parent entity.");
    } else if (PhaseDurationSeconds <= 0d) {
        throw new InvalidOperationException("MatrixRenderComponent requires a positive phase duration.");
    }

    const int PhaseCount = 7;
    double totalPhaseSeconds = PhaseDurationSeconds * PhaseCount;
    double wrappedSeconds = Core.Instance.TotalElapsedSeconds % totalPhaseSeconds;
    int phaseIndex = (int)(wrappedSeconds / PhaseDurationSeconds);
    double phaseProgress = (wrappedSeconds - (phaseIndex * PhaseDurationSeconds)) / PhaseDurationSeconds;
    float easedProgress = (float)(0.5d - (0.5d * Math.Cos(phaseProgress * Math.PI)));

    float3 position = BaseLocalPosition;
    float3 scale = BaseLocalScale;
    float4 orientation = float4.Identity;

    if (phaseIndex == 0) {
        position = float3.Lerp(BaseLocalPosition, BaseLocalPosition + MotionOffset, easedProgress);
    } else if (phaseIndex == 1) {
        orientation = float4.Lerp(float4.Identity, RotatedLocalOrientation, easedProgress);
    } else if (phaseIndex == 2) {
        scale = float3.Lerp(BaseLocalScale, ScaledLocalScale, easedProgress);
    } else if (phaseIndex == 3) {
        position = float3.Lerp(BaseLocalPosition, BaseLocalPosition + MotionOffset, easedProgress);
        orientation = float4.Lerp(float4.Identity, RotatedLocalOrientation, easedProgress);
    } else if (phaseIndex == 4) {
        position = float3.Lerp(BaseLocalPosition, BaseLocalPosition + MotionOffset, easedProgress);
        scale = float3.Lerp(BaseLocalScale, ScaledLocalScale, easedProgress);
    } else if (phaseIndex == 5) {
        orientation = float4.Lerp(float4.Identity, RotatedLocalOrientation, easedProgress);
        scale = float3.Lerp(BaseLocalScale, ScaledLocalScale, easedProgress);
    } else if (phaseIndex == 6) {
        position = float3.Lerp(BaseLocalPosition, BaseLocalPosition + MotionOffset, easedProgress);
        orientation = float4.Lerp(float4.Identity, RotatedLocalOrientation, easedProgress);
        scale = float3.Lerp(BaseLocalScale, ScaledLocalScale, easedProgress);
    } else {
        throw new InvalidOperationException($"Unsupported matrix render phase index '{phaseIndex}'.");
    }

    orientation.Normalize();
    Parent.LocalPosition = position;
    Parent.LocalScale = scale;
    Parent.LocalOrientation = orientation;
}
```

- [ ] **Step 3: Use the existing math APIs exactly as they are already implemented in the engine**

Keep these exact calls in the component:

```csharp
float3.Lerp(...)
float4.Lerp(...)
float4.Identity
```

- [ ] **Step 4: Build the city project through the editor command path**

Run:

```powershell
rtk powershell -NoProfile -Command "dotnet run --project C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\city\project.heproj --editor-command menu.generate-physics-scenes"
```

Expected: the city script assembly compiles and the command ends with `Editor command 'menu.generate-physics-scenes' executed successfully.`

### Task 3: Author The Matrix Render Scene

**Files:**
- Modify: `C:\dev\helprojs\city\assets\codebase\physics.tools\PhysicsSceneCatalog.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\physics.tools\PhysicsSceneFactory.cs`

- [ ] **Step 1: Add the new scene id to the catalog**

```csharp
/// <summary>
/// Relative scene id for the render-only matrix render validation scene.
/// </summary>
public const string MatrixRenderSceneId = "scenes/physics/test_scene_matrix_render.helen";
```

- [ ] **Step 2: Add the scene id to the ordered scene list**

```csharp
RenderOnlySlopeSceneId,
RenderMatrixProbeSceneId,
MatrixRenderSceneId,
DynamicMixedStackSceneId,
```

- [ ] **Step 3: Route `CreateSceneAsset` to the new scene factory method**

```csharp
} else if (string.Equals(sceneId, PhysicsSceneCatalog.MatrixRenderSceneId, StringComparison.Ordinal)) {
    return CreateMatrixRenderScene();
}
```

- [ ] **Step 4: Add one helper that creates the animated hero cube entity**

```csharp
SceneEntityAsset CreateMatrixRenderHeroEntity() {
    return new SceneEntityAsset {
        Id = AllocateSceneEntityId(),
        Name = "HeroMotionCube",
        LayerMask = EditorLayerMasks.SceneObjects,
        LocalPosition = new float3(6f, 1f, -3.5f),
        LocalScale = new float3(2f, 2f, 2f),
        LocalOrientation = float4.Identity,
        Components = new[] {
            CreateMeshComponentRecord(CreatePhysicsDemoMaterialReference(PhysicsDemoRedMaterialRelativePath)),
            CreateAutomaticComponentRecord(new city.rendering.MatrixRenderComponent {
                BaseLocalPosition = new float3(6f, 1f, -3.5f),
                MotionOffset = new float3(0f, 0f, 5f),
                BaseLocalScale = new float3(2f, 2f, 2f),
                ScaledLocalScale = new float3(4f, 1f, 2f),
                RotatedLocalOrientation = CreateYawPitchRollDegrees(0.0, 0.0, 18.0),
                PhaseDurationSeconds = 1.5d
            }, 1)
        },
        Children = Array.Empty<SceneEntityAsset>()
    };
}
```

- [ ] **Step 5: Add the full scene method with static references and fixed camera**

```csharp
SceneAsset CreateMatrixRenderScene() {
    SceneEntityAsset scenarioEntity = CreateScenarioRoot(
        "matrix_render.scenario",
        new[] {
            CreateCubeMeshEntity("matrix_render.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(28f, 1f, 18f), float4.Identity, CreatePhysicsDemoMaterialReference(PhysicsDemoGroundMaterialRelativePath)),
            CreateCubeMeshEntity("matrix_render.flat_control", "FlatControlCube", new float3(-8f, 1f, 0f), new float3(2f, 2f, 2f), float4.Identity, CreatePhysicsDemoMaterialReference(PhysicsDemoNeutralMaterialRelativePath)),
            CreateCubeMeshEntity("matrix_render.rotated_only", "RotatedOnlyReferenceCube", new float3(-3f, 1f, 0f), new float3(2f, 2f, 2f), CreateYawPitchRollDegrees(0.0, 0.0, 18.0), CreatePhysicsDemoMaterialReference(PhysicsDemoBlueMaterialRelativePath)),
            CreateCubeMeshEntity("matrix_render.scaled_only", "ScaledOnlyReferenceCube", new float3(2f, 1f, 0f), new float3(4f, 1f, 2f), float4.Identity, CreatePhysicsDemoMaterialReference(PhysicsDemoYellowMaterialRelativePath)),
            CreateCubeMeshEntity("matrix_render.rotated_scaled_reference", "RotatedScaledReferenceCube", new float3(7f, 1f, 0f), new float3(4f, 1f, 2f), CreateYawPitchRollDegrees(0.0, 0.0, 18.0), CreatePhysicsDemoMaterialReference(PhysicsDemoMagentaMaterialRelativePath)),
            CreateMatrixRenderHeroEntity()
        });
    SceneEntityAsset cameraEntity = CreateCameraEntity("matrix_render.camera", new float3(0f, 8f, 18f), CreateYawPitchRollDegrees(0.0, -20.0, 0.0));
    return CreateSceneAsset(PhysicsSceneCatalog.MatrixRenderSceneId, cameraEntity, scenarioEntity);
}
```

- [ ] **Step 6: Re-run the source-audit test**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter FullyQualifiedName~City_matrix_render_scene_source_is_exposed_as_visual_only_with_matrix_render_component -v minimal
```

Expected: `PASS`

### Task 4: Boot Windows Directly Into Matrix Render And Validate It

**Files:**
- Modify: `C:\dev\helprojs\city\user_settings\build_config.json`

- [ ] **Step 1: Switch the Windows selected scene to the new motion probe**

```json
"selectedSceneIds": [
  "test_scene_matrix_render"
],
"sceneOrders": [
  {
    "sceneId": "test_scene_matrix_render",
    "orderNumber": 1
  }
]
```

- [ ] **Step 2: Regenerate authored physics scenes**

Run:

```powershell
rtk powershell -NoProfile -Command "dotnet run --project C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\city\project.heproj --editor-command menu.generate-physics-scenes"
```

Expected: `Editor command 'menu.generate-physics-scenes' executed successfully.`

- [ ] **Step 3: Build the Windows package**

Run:

```powershell
rtk powershell -NoProfile -ExecutionPolicy Bypass -Command "& 'C:\dev\helworks\helengine\artifacts\build-platform.ps1' -Project 'C:\dev\helprojs\city\project.heproj' -Platform 'windows' -Output 'C:\dev\helprojs\city\windows-build'"
```

Expected: `C:\dev\helprojs\city\windows-build\helengine_windows.exe` exists and `C:\dev\helprojs\city\windows-build\cooked\scenes\physics\test_scene_matrix_render.hasset` exists.

- [ ] **Step 4: Launch the Windows build**

Run:

```powershell
Start-Process -FilePath 'C:\dev\helprojs\city\windows-build\helengine_windows.exe' -WorkingDirectory 'C:\dev\helprojs\city\windows-build'
```

Expected: runtime boots directly into `test_scene_matrix_render`.

- [ ] **Step 5: Verify direct boot in the startup log**

Run:

```powershell
rtk powershell -NoProfile -Command "Get-Content 'C:\dev\helprojs\city\windows-build\helengine_windows.startup.log' | Select-Object -First 20"
```

Expected line:

```text
[Host] Loading startup scene from runtime scene catalog entry 'test_scene_matrix_render'.
```
