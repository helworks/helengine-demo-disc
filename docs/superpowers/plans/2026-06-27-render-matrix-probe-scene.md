# Render Matrix Probe Scene Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a visual-only probe scene for rotated and scaled cube rendering and boot Windows directly into it.

**Architecture:** Extend the existing `PhysicsSceneCatalog` and `PhysicsSceneFactory` with one additional authored scene that uses only mesh entities and existing demo materials. Keep the change isolated to authored scene generation, one source test, and the Windows build config so the runtime can be verified without touching BEPU or general rendering code yet.

**Tech Stack:** C#, xUnit, helengine scene generation, Windows platform build config JSON

---

### Task 1: Add Source Coverage For The New Probe Scene

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityPhysicsSceneSourceTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
[Fact]
public void City_render_matrix_probe_scene_source_is_exposed_as_render_only_visual_cases() {
    string catalogSourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\physics.tools\PhysicsSceneCatalog.cs";
    string catalogSource = File.ReadAllText(catalogSourcePath);
    string factorySourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\physics.tools\PhysicsSceneFactory.cs";
    string factorySource = File.ReadAllText(factorySourcePath);

    Assert.Contains("public const string RenderMatrixProbeSceneId = \"scenes/physics/test_scene_render_matrix_probe.helen\";", catalogSource, StringComparison.Ordinal);
    Assert.Contains("RenderMatrixProbeSceneId,", catalogSource, StringComparison.Ordinal);
    Assert.Contains("CreateRenderMatrixProbeScene()", factorySource, StringComparison.Ordinal);
    Assert.Contains("\"render_matrix_probe.flat_control\"", factorySource, StringComparison.Ordinal);
    Assert.Contains("\"render_matrix_probe.rotated_only\"", factorySource, StringComparison.Ordinal);
    Assert.Contains("\"render_matrix_probe.scaled_only\"", factorySource, StringComparison.Ordinal);
    Assert.Contains("\"render_matrix_probe.rotated_scaled\"", factorySource, StringComparison.Ordinal);
    Assert.DoesNotContain("CreatePhysicsBoxMeshEntity(\"render_matrix_probe", factorySource, StringComparison.Ordinal);
    Assert.DoesNotContain("CreatePhysicsSphereMeshEntity(\"render_matrix_probe", factorySource, StringComparison.Ordinal);
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter FullyQualifiedName~City_render_matrix_probe_scene_source_is_exposed_as_render_only_visual_cases -v minimal`
Expected: `FAIL` because the catalog constant and factory method do not exist yet.

- [ ] **Step 3: Implement the minimal source additions**

```csharp
public const string RenderMatrixProbeSceneId = "scenes/physics/test_scene_render_matrix_probe.helen";
```

```csharp
return CreateRenderMatrixProbeScene();
```

- [ ] **Step 4: Run test to verify it passes**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter FullyQualifiedName~City_render_matrix_probe_scene_source_is_exposed_as_render_only_visual_cases -v minimal`
Expected: `PASS`

### Task 2: Author The Render-Only Probe Scene

**Files:**
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\physics.tools\PhysicsSceneCatalog.cs`
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\physics.tools\PhysicsSceneFactory.cs`

- [ ] **Step 1: Add the new scene id to the catalog**

```csharp
/// <summary>
/// Relative scene id for the render-only matrix probe validation scene.
/// </summary>
public const string RenderMatrixProbeSceneId = "scenes/physics/test_scene_render_matrix_probe.helen";
```

- [ ] **Step 2: Add the scene id to the stable ordered scene list**

```csharp
StrictRotatedBoxCompareSceneId,
RenderOnlySlopeSceneId,
RenderMatrixProbeSceneId,
DynamicMixedStackSceneId,
```

- [ ] **Step 3: Route factory creation through the new scene method**

```csharp
} else if (string.Equals(sceneId, PhysicsSceneCatalog.RenderMatrixProbeSceneId, StringComparison.Ordinal)) {
    return CreateRenderMatrixProbeScene();
}
```

- [ ] **Step 4: Author the render-only scene body**

```csharp
SceneAsset CreateRenderMatrixProbeScene() {
    SceneEntityAsset scenarioEntity = CreateScenarioRoot(
        "render_matrix_probe.scenario",
        new[] {
            CreateCubeMeshEntity("render_matrix_probe.ground", "Ground", new float3(0f, -0.5f, 0f), new float3(24f, 1f, 14f), float4.Identity, CreatePhysicsDemoMaterialReference(PhysicsDemoGroundMaterialRelativePath)),
            CreateCubeMeshEntity("render_matrix_probe.flat_control", "FlatControlCube", new float3(-6f, 1f, 0f), new float3(2f, 2f, 2f), float4.Identity, CreatePhysicsDemoMaterialReference(PhysicsDemoNeutralMaterialRelativePath)),
            CreateCubeMeshEntity("render_matrix_probe.rotated_only", "RotatedOnlyCube", new float3(-2f, 1f, 0f), new float3(2f, 2f, 2f), CreateYawPitchRollDegrees(0.0, 0.0, 18.0), CreatePhysicsDemoMaterialReference(PhysicsDemoBlueMaterialRelativePath)),
            CreateCubeMeshEntity("render_matrix_probe.scaled_only", "ScaledOnlyCube", new float3(2f, 1f, 0f), new float3(4f, 1f, 2f), float4.Identity, CreatePhysicsDemoMaterialReference(PhysicsDemoYellowMaterialRelativePath)),
            CreateCubeMeshEntity("render_matrix_probe.rotated_scaled", "RotatedScaledCube", new float3(6f, 1f, 0f), new float3(4f, 1f, 2f), CreateYawPitchRollDegrees(0.0, 0.0, 18.0), CreatePhysicsDemoMaterialReference(PhysicsDemoRedMaterialRelativePath))
        });
    SceneEntityAsset cameraEntity = CreateCameraEntity("render_matrix_probe.camera", new float3(0f, 6.5f, 14f), CreateYawPitchRollDegrees(0.0, -20.0, 0.0));
    return CreateSceneAsset(PhysicsSceneCatalog.RenderMatrixProbeSceneId, cameraEntity, scenarioEntity);
}
```

- [ ] **Step 5: Run the targeted source test again**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter FullyQualifiedName~City_render_matrix_probe_scene_source_is_exposed_as_render_only_visual_cases -v minimal`
Expected: `PASS`

### Task 3: Boot Windows Directly Into The Probe Scene

**Files:**
- Modify: `C:\dev\helprojs\demodisc\user_settings\build_config.json`

- [ ] **Step 1: Replace the Windows selected scenes with the new probe scene**

```json
"selectedSceneIds": [
  "test_scene_render_matrix_probe"
],
"sceneOrders": [
  {
    "sceneId": "test_scene_render_matrix_probe",
    "orderNumber": 1
  }
]
```

- [ ] **Step 2: Regenerate authored physics scenes**

Run: `rtk powershell -NoProfile -Command "dotnet run --project C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\demodisc\project.heproj --editor-command menu.generate-physics-scenes"`
Expected: output ends with `Editor command 'menu.generate-physics-scenes' executed successfully.`

- [ ] **Step 3: Build the Windows package**

Run: `rtk powershell -NoProfile -ExecutionPolicy Bypass -Command "& 'C:\dev\helworks\helengine\artifacts\build-platform.ps1' -Project 'C:\dev\helprojs\demodisc\project.heproj' -Platform 'windows' -Output 'C:\dev\helprojs\demodisc\windows-build'"`
Expected: `C:\dev\helprojs\demodisc\windows-build\helengine_windows.exe` exists with fresh timestamp.

- [ ] **Step 4: Launch the package and verify direct boot**

Run: `Start-Process -FilePath 'C:\dev\helprojs\demodisc\windows-build\helengine_windows.exe' -WorkingDirectory 'C:\dev\helprojs\demodisc\windows-build'`
Expected: runtime boots directly into the new four-cube probe scene.
