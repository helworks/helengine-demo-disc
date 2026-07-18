# Demo Follow Camera Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a new `DemoFollowCameraComponent` for the city static-mesh showcase scene so the camera follows the player sphere position and supports free orbit in both packaged and direct-launch playable paths.

**Architecture:** Introduce one generic runtime scene-entity identity component inside `helengine.core` so runtime-authored components can resolve stable `SceneEntityReference` ids back to live entities without scene-specific glue. Then author one city-local `DemoFollowCameraComponent` that stores a serialized `SceneEntityReference`, lazily resolves the tracked entity through the runtime object graph, and reuses the existing demo-orbit input model while following the ball position instead of its rotation.

**Tech Stack:** C#, `helengine.core`, `helengine.editor.tests`, city asset-source scene factories, xUnit source/runtime tests

---

### Task 1: Add Generic Runtime Scene-Entity Identity

**Files:**
- Create: `C:\dev\helworks\helengine\engine\helengine.core\components\SceneEntityRuntimeIdComponent.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\scene\runtime\RuntimeSceneLoadService.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\serialization\scene\RuntimeSceneLoadServiceTests.cs`

- [ ] **Step 1: Write the failing runtime-load test**

```csharp
[Fact]
public void Load_WhenSceneEntityIdsExist_AttachesRuntimeSceneEntityIdComponents() {
    RuntimeSceneAssetReferenceResolver resolver = new RuntimeSceneAssetReferenceResolver(
        Core.Instance.ContentManager,
        TempRootPath,
        ShaderCompileTarget.DirectX11);
    RuntimeSceneLoadService loadService = new RuntimeSceneLoadService(resolver, RuntimeComponentRegistry.CreateDefault());
    SceneAsset sceneAsset = new SceneAsset {
        RootEntities = new[] {
            new SceneEntityAsset {
                Id = 17u,
                Name = "Root",
                Children = new[] {
                    new SceneEntityAsset {
                        Id = 23u,
                        Name = "Child",
                        Children = Array.Empty<SceneEntityAsset>()
                    }
                }
            }
        }
    };

    Entity loadedRoot = Assert.Single(loadService.Load(sceneAsset));
    SceneEntityRuntimeIdComponent rootId = Assert.IsType<SceneEntityRuntimeIdComponent>(Assert.Single(loadedRoot.Components, component => component is SceneEntityRuntimeIdComponent));
    Entity loadedChild = Assert.Single(loadedRoot.Children);
    SceneEntityRuntimeIdComponent childId = Assert.IsType<SceneEntityRuntimeIdComponent>(Assert.Single(loadedChild.Components, component => component is SceneEntityRuntimeIdComponent));

    Assert.Equal(17u, rootId.SceneEntityId);
    Assert.Equal(23u, childId.SceneEntityId);
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~RuntimeSceneLoadServiceTests.Load_WhenSceneEntityIdsExist_AttachesRuntimeSceneEntityIdComponents"`

Expected: FAIL because `SceneEntityRuntimeIdComponent` does not exist and the runtime loader does not attach it.

- [ ] **Step 3: Write the minimal runtime identity implementation**

```csharp
namespace helengine {
    /// <summary>
    /// Stores the stable serialized scene-entity id on one live runtime entity so runtime-authored systems can resolve scene references without editor-only metadata.
    /// </summary>
    public sealed class SceneEntityRuntimeIdComponent : Component {
        /// <summary>
        /// Gets or sets the stable serialized scene-entity id restored for the owning runtime entity.
        /// </summary>
        public uint SceneEntityId { get; set; }
    }
}
```

```csharp
Entity entity = new Entity {
    Static = entityAsset.IsStatic,
    LayerMask = entityAsset.LayerMask,
    LocalPosition = entityAsset.LocalPosition,
    LocalScale = entityAsset.LocalScale,
    LocalOrientation = entityAsset.LocalOrientation
};
entity.InitComponents();
entity.InitChildren();
entity.AddComponent(new SceneEntityRuntimeIdComponent {
    SceneEntityId = entityAsset.Id
});
```

- [ ] **Step 4: Run the runtime-load test to verify it passes**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~RuntimeSceneLoadServiceTests.Load_WhenSceneEntityIdsExist_AttachesRuntimeSceneEntityIdComponents"`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add C:/dev/helworks/helengine/engine/helengine.core/components/SceneEntityRuntimeIdComponent.cs C:/dev/helworks/helengine/engine/helengine.core/scene/runtime/RuntimeSceneLoadService.cs C:/dev/helworks/helengine/engine/helengine.editor.tests/serialization/scene/RuntimeSceneLoadServiceTests.cs
git commit -m "feat: restore runtime scene entity ids"
```

### Task 2: Add The City Follow Camera Component

**Files:**
- Create: `C:\dev\helprojs\demodisc\assets\codebase\rendering\DemoFollowCameraComponent.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityPhysicsSceneSourceTests.cs`

- [ ] **Step 1: Write the failing source test for the new component contract**

```csharp
[Fact]
public void City_static_mesh_follow_camera_source_uses_scene_entity_reference_and_runtime_id_lookup() {
    string sourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\rendering\DemoFollowCameraComponent.cs";
    string source = File.ReadAllText(sourcePath);

    Assert.Contains("public SceneEntityReference TargetEntityReference { get; set; }", source, StringComparison.Ordinal);
    Assert.Contains("SceneEntityRuntimeIdComponent", source, StringComparison.Ordinal);
    Assert.Contains("Core.Instance.ObjectManager.Entities", source, StringComparison.Ordinal);
    Assert.Contains("TargetEntityReference.EntityId", source, StringComparison.Ordinal);
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~CityPhysicsSceneSourceTests.City_static_mesh_follow_camera_source_uses_scene_entity_reference_and_runtime_id_lookup"`

Expected: FAIL because the new camera component file does not exist yet.

- [ ] **Step 3: Write the minimal follow-camera component**

```csharp
public sealed class DemoFollowCameraComponent : UpdateComponent {
    public SceneEntityReference TargetEntityReference { get; set; }
    public float3 TargetOffset { get; set; }
    public float ManualYawSpeedRadians { get; set; }
    public float ManualPitchSpeedRadians { get; set; }
    public float MinimumPitchRadians { get; set; }
    public float MaximumPitchRadians { get; set; }

    Entity TargetEntity;
    float CurrentYawRadians;
    float CurrentPitchRadians;
    float CurrentOrbitRadius;
    bool IsOrbitInitialized;

    public override void Update() {
        base.Update();
        ResolveTargetEntityWhenNeeded();
        EnsureOrbitInitialized();
        ApplyManualOrbitInput();
        ApplyOrbitPose();
    }
}
```

- [ ] **Step 4: Run the source test to verify it passes**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~CityPhysicsSceneSourceTests.City_static_mesh_follow_camera_source_uses_scene_entity_reference_and_runtime_id_lookup"`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add C:/dev/helprojs/demodisc/assets/codebase/rendering/DemoFollowCameraComponent.cs C:/dev/helworks/helengine/engine/helengine.editor.tests/CityPhysicsSceneSourceTests.cs
git commit -m "feat: add static mesh demo follow camera"
```

### Task 3: Rewire Static-Mesh Showcase Camera Authoring

**Files:**
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\physics.tools\PhysicsSceneFactory.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityPhysicsSceneSourceTests.cs`

- [ ] **Step 1: Write the failing source test for static-mesh showcase camera wiring**

```csharp
[Fact]
public void City_static_mesh_showcase_source_uses_demo_follow_camera_for_packaged_and_live_paths() {
    string sourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\physics.tools\PhysicsSceneFactory.cs";
    string source = File.ReadAllText(sourcePath);

    Assert.Contains("CreateStaticMeshShowcaseCameraEntity(", source, StringComparison.Ordinal);
    Assert.Contains("new city.rendering.DemoFollowCameraComponent", source, StringComparison.Ordinal);
    Assert.Contains("TargetEntityReference = new SceneEntityReference {", source, StringComparison.Ordinal);
    Assert.Contains("StaticMeshShowcaseCamera", source, StringComparison.Ordinal);
    Assert.DoesNotContain("CreateLivePhysicsShowcaseCameraEntity(\r\n                    \"StaticMeshShowcaseCamera\"", source, StringComparison.Ordinal);
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~CityPhysicsSceneSourceTests.City_static_mesh_showcase_source_uses_demo_follow_camera_for_packaged_and_live_paths"`

Expected: FAIL because the static-mesh showcase still uses `DemoDiscOrbitCameraComponent`.

- [ ] **Step 3: Write the minimal scene-factory changes**

```csharp
SceneEntityAsset playerSphereEntity = CreatePhysicsSphereMeshEntity(...);
SceneEntityAsset cameraEntity = CreateStaticMeshShowcaseCameraEntity(playerSphereEntity.Id);
```

```csharp
CreateAutomaticComponentRecord(new city.rendering.DemoFollowCameraComponent {
    TargetEntityReference = new SceneEntityReference {
        EntityId = targetEntityId
    },
    TargetOffset = new float3(0f, 1.4f, 0f)
}, 1)
```

```csharp
cameraEntity = CreateLiveStaticMeshShowcaseCameraEntity("StaticMeshShowcaseCamera", ..., targetEntityId);
```

- [ ] **Step 4: Run the source test to verify it passes**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~CityPhysicsSceneSourceTests.City_static_mesh_showcase_source_uses_demo_follow_camera_for_packaged_and_live_paths"`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add C:/dev/helprojs/demodisc/assets/codebase/physics.tools/PhysicsSceneFactory.cs C:/dev/helworks/helengine/engine/helengine.editor.tests/CityPhysicsSceneSourceTests.cs
git commit -m "feat: wire static mesh showcase follow camera"
```

### Task 4: Verify The Runtime And Scene Paths Together

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityStaticMeshShowcasePackagedSceneTests.cs`

- [ ] **Step 1: Write the failing packaged-scene test assertion**

```csharp
SceneEntityAsset cameraEntityAsset = FindEntityAssetWithComponent(sceneAsset.RootEntities, AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(SceneEntityRuntimeIdComponent)));
```

Replace that placeholder with a real packaged-scene assertion that the static-mesh showcase camera record payload contains `DemoFollowCameraComponent` and the scene still binds successfully after loading.

- [ ] **Step 2: Run the targeted packaged-scene test to verify it fails for the camera assertion**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~CityStaticMeshShowcasePackagedSceneTests.GameCube_packaged_static_mesh_showcase_scene_loads_and_binds"`

Expected: FAIL until the camera assertion matches the new packaged scene output.

- [ ] **Step 3: Update the packaged-scene test to assert both follow-camera presence and static-mesh bind success**

```csharp
SceneEntityAsset cameraEntityAsset = FindEntityAssetWithComponent(
    sceneAsset.RootEntities,
    AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(city.rendering.DemoFollowCameraComponent)));

Assert.NotNull(cameraEntityAsset);
world.BindScene(new[] { colliderEntity });
```

- [ ] **Step 4: Run the focused verification set**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~RuntimeSceneLoadServiceTests.Load_WhenSceneEntityIdsExist_AttachesRuntimeSceneEntityIdComponents|FullyQualifiedName~CityPhysicsSceneSourceTests.City_static_mesh_follow_camera_source_uses_scene_entity_reference_and_runtime_id_lookup|FullyQualifiedName~CityPhysicsSceneSourceTests.City_static_mesh_showcase_source_uses_demo_follow_camera_for_packaged_and_live_paths|FullyQualifiedName~CityStaticMeshShowcasePackagedSceneTests.GameCube_packaged_static_mesh_showcase_scene_loads_and_binds"`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add C:/dev/helworks/helengine/engine/helengine.editor.tests/CityStaticMeshShowcasePackagedSceneTests.cs
git commit -m "test: verify static mesh showcase follow camera"
```
