# Software Trace Scene Full-Cube Remediation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the CPU trace-scene ingestion path consume the actual packaged generated cube and derive the Cornell ceiling light from one inward-facing face while retaining all twelve cube triangles.

**Architecture:** The engine packaging seam will normalize only generated CPU-readable primitive companions with one default submesh covering their active index buffer; global `ModelUtils` and GPU model behavior remain unchanged. DemoDisc will test the real 24-vertex/36-index generated cube shape and select the largest rectangular face whose existing geometric winding points from the emitter toward the non-emitter scene interior.

**Tech Stack:** C# 13 / .NET 9, xUnit, HelenEngine automatic packaging, DemoDisc generated gameplay projects

---

### Task 1: Give packaged generated CPU companions one default submesh

**Files:**
- Modify: `engine/helengine.editor.tests/managers/project/SceneComponentPackagingTransformServiceTests.cs` in `C:\dev\helprojs\.worktrees\helengine-software-path-tracer-engine-seams`
- Modify: `engine/helengine.editor/managers/project/SceneComponentPackagingTransformService.cs` in `C:\dev\helprojs\.worktrees\helengine-software-path-tracer-engine-seams`

- [ ] **Step 1: Strengthen the existing generated-companion tests**

After deserializing the cube, plane, or sphere companion, assert:

```csharp
ModelSubmeshAsset submesh = Assert.Single(model.Submeshes);
Assert.Equal(0, submesh.IndexStart);
int activeIndexCount = model.Indices16?.Length > 0
    ? model.Indices16.Length
    : model.Indices32.Length;
Assert.Equal(activeIndexCount, submesh.IndexCount);
```

- [ ] **Step 2: Run the focused tests and verify RED**

```powershell
rtk dotnet test engine\helengine.editor.tests\helengine.editor.tests.csproj --no-restore --filter "FullyQualifiedName~SceneComponentPackagingTransformServiceTests.TryTransform_Cpu_readable_model_reference_Generated" -v:minimal
```

Expected: FAIL because generated companions currently deserialize with null `Submeshes`.

- [ ] **Step 3: Normalize only generated CPU-readable companions**

Immediately before `WriteAsset` in `RewriteGeneratedCpuReadableModelReference`, call a private helper that validates the active index width and assigns one default submesh only when `Submeshes` is null or empty:

```csharp
static void EnsureGeneratedCpuReadableModelSubmesh(ModelAsset modelAsset) {
    ModelAssetIndexData indexData = ModelAssetIndexData.Resolve(modelAsset);
    if (indexData.IndexCount <= 0) {
        throw new InvalidOperationException("Generated CPU-readable model companions require one populated index buffer.");
    }
    if (modelAsset.Submeshes == null || modelAsset.Submeshes.Length == 0) {
        modelAsset.Submeshes = new[] {
            new ModelSubmeshAsset {
                MaterialSlotName = "DefaultMaterial",
                IndexStart = 0,
                IndexCount = indexData.IndexCount
            }
        };
    }
}
```

Do not modify `ModelUtils`, normal GPU model creation, filesystem imports, or any renderer.

- [ ] **Step 4: Run the focused tests and verify GREEN**

Run the command from Step 2. Expected: all selected generated-companion cases PASS.

- [ ] **Step 5: Commit the engine fix**

```powershell
rtk git add -- engine/helengine.editor.tests/managers/project/SceneComponentPackagingTransformServiceTests.cs engine/helengine.editor/managers/project/SceneComponentPackagingTransformService.cs
rtk git commit -m "Normalize generated CPU model companions"
```

### Task 2: Prove and select the inward face of the real closed cube

**Files:**
- Modify: `assets/codebase/gameplay.tests/SoftwareTraceSceneTests.cs`
- Modify: `assets/codebase/rendering/SoftwareTraceScene.cs`

- [ ] **Step 1: Replace the plane-like emitter fixture with packaged generated-cube geometry**

Create the test asset from `ModelUtils.GenerateCubeMesh(float3.Zero, float3.One)`, then add the same default submesh emitted by Task 1:

```csharp
ModelAsset cube = ModelUtils.GenerateCubeMesh(float3.Zero, float3.One);
cube.Submeshes = new[] {
    new ModelSubmeshAsset {
        MaterialSlotName = "DefaultMaterial",
        IndexStart = 0,
        IndexCount = cube.Indices16.Length
    }
};
return cube;
```

Author one non-emitter scene anchor around the Cornell interior and transform the emitter above it with a thin Y scale. Assert:

```csharp
Assert.Equal(12, emitterTriangleCount);
Assert.Equal(12, scene.Triangles.Length - nonEmitterTriangleCount);
Assert.Equal(0.55f * 0.45f, scene.AreaLight.Area, 4);
Assert.True(scene.AreaLight.InwardNormal.Y < -0.99f);
Assert.InRange(scene.AreaLight.FirstTriangleIndex, emitterOffset, emitterOffset + 11);
Assert.InRange(scene.AreaLight.SecondTriangleIndex, emitterOffset, emitterOffset + 11);
```

Also assert at least one non-selected emitter triangle remains in `scene.Triangles`.

- [ ] **Step 2: Run the full-cube test and verify RED**

```powershell
rtk dotnet test user_settings\generated_code\editor-command\EditorFull\projects\gameplay.tests\gameplay.tests.csproj --no-restore --filter "FullyQualifiedName~SoftwareTraceSceneTests.Cube_emitter_derives_one_rectangular_area_light" -v:minimal
```

Expected: FAIL because the existing algorithm uses the emitter's own AABB center and rejects or ambiguously selects the closed cube.

- [ ] **Step 3: Select against the non-emitter scene interior**

Compute a reference point from the bounds of all triangles outside the emitter range. For closed emitters, consider rectangular candidates in descending area and accept exactly one largest candidate whose unmodified geometric normal satisfies:

```csharp
float3 faceCenter = candidate.Corner + (candidate.Edge1 + candidate.Edge2) * 0.5f;
bool facesSceneInterior = float3.Dot(candidate.Normal, sceneInteriorCenter - faceCenter) > GeometryTolerance;
```

Do not use the emitter's own center. Do not discard the emitter's other triangles. Do not flip every candidate normal toward the scene center, because that makes opposite faces ambiguous. A two-triangle plane emitter may use its sole valid rectangle with its normal oriented toward the scene interior, but the closed-cube path must require the uniquely wound inward-facing largest face.

- [ ] **Step 4: Close the rejected coverage gaps**

Add independent assertions for byte constants `88`, `24`, and `72`. Add cases for null submesh elements, overlapping submeshes, uncovered index ranges, empty positions, and a genuine post-validation flattening failure that still disposes the raw asset.

- [ ] **Step 5: Run Task 1 and Task 2 regression tests**

```powershell
rtk dotnet test user_settings\generated_code\editor-command\EditorFull\projects\gameplay.tests\gameplay.tests.csproj --no-restore --filter "FullyQualifiedName~SoftwareTraceSceneTests|FullyQualifiedName~SoftwareModelComponentTests" -v:minimal
rtk dotnet build user_settings\generated_code\editor-command\EditorFull\projects\gameplay\gameplay.csproj --no-restore -v:minimal
```

Expected: all selected tests PASS and the gameplay project builds with zero errors.

- [ ] **Step 6: Commit the DemoDisc remediation**

```powershell
rtk git add -- assets/codebase/gameplay.tests/SoftwareTraceSceneTests.cs assets/codebase/rendering/SoftwareTraceScene.cs
rtk git commit -m "Handle packaged cube area lights"
```

### Task 3: Cross-repository verification

**Files:**
- Verify only: the four production/test files modified above

- [ ] **Step 1: Verify the engine companion contract**

Run the focused engine test from Task 1 and deserialize a generated cube companion. Expected: 24 positions, 36 active indices, exactly one submesh covering indices 0 through 35.

- [ ] **Step 2: Verify the DemoDisc full-cube contract**

Run the focused DemoDisc tests from Task 2. Expected: the emitter contributes twelve intersectable triangles while exactly two form the sampling rectangle with a downward inward normal.

- [ ] **Step 3: Audit scope and forbidden dependencies**

```powershell
rtk rg -n "RuntimeModel|RenderManager3D|MeshComponent|File\.Write|OpenWrite|System\.Linq" assets/codebase/rendering/SoftwareTraceScene.cs
```

Expected: no matches. Confirm the engine diff does not touch `ModelUtils` or renderer code and the DemoDisc diff does not touch engine code.
