# Cube Test Rotation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the committed `city` cube-test scene rotate slowly at runtime on every platform, including PS2.

**Architecture:** Add one small runtime `UpdateComponent` in `city.rendering` that rotates its parent around local Y using absolute elapsed time. Serialize that component into the generated cube scene through the existing rendering scene factory path so the authored scene keeps using normal runtime update and transform propagation.

**Tech Stack:** C#, helengine scene serialization, `city` rendering scene generators, xUnit source-level scene tests

---

## File Map

- Create: `C:\dev\helprojs\city\assets\codebase\rendering\CubeTestSpinComponent.cs`
  - Runtime update component that computes cube yaw from absolute elapsed time.
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\RenderingScriptComponentRecordFactory.cs`
  - Add one helper for the cube spin script component record.
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\CubeTestSceneFactory.cs`
  - Attach the cube spin record to the cube entity with the selected defaults.
- Modify: `C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\CityCubeTestSceneSourceTests.cs`
  - Add source-level coverage that the cube scene emits the spin component with the intended defaults.

### Task 1: Add The Runtime Cube Spin Component

**Files:**
- Create: `C:\dev\helprojs\city\assets\codebase\rendering\CubeTestSpinComponent.cs`

- [ ] **Step 1: Write the component file**

```csharp
namespace city.rendering {
    /// <summary>
    /// Rotates the cube-test entity around its local Y axis using deterministic absolute time.
    /// </summary>
    public sealed class CubeTestSpinComponent : UpdateComponent {
        /// <summary>
        /// Base yaw offset applied before time-driven rotation.
        /// </summary>
        public float BaseYawRadians { get; set; }

        /// <summary>
        /// Angular speed in radians per second used to rotate the parent entity.
        /// </summary>
        public float AngularSpeedRadians { get; set; }

        /// <summary>
        /// Updates the parent entity orientation from total elapsed runtime time.
        /// </summary>
        public override void Update() {
            double yawRadians = BaseYawRadians + (AngularSpeedRadians * Core.Instance.TotalElapsedSeconds);
            float4 orientation;
            float4.CreateFromYawPitchRoll((float)yawRadians, 0f, 0f, out orientation);
            Parent.LocalOrientation = orientation;
        }
    }
}
```

- [ ] **Step 2: Run a syntax-only build for the city scripts through the existing project binary path**

Run:

```powershell
dotnet build C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -c Debug
```

Expected:

```text
Build succeeded.
```

- [ ] **Step 3: Commit**

```bash
git add assets/codebase/rendering/CubeTestSpinComponent.cs
git commit -m "feat: add cube test spin component"
```

### Task 2: Serialize The Spin Component Into The Cube Scene

**Files:**
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\RenderingScriptComponentRecordFactory.cs`
- Modify: `C:\dev\helprojs\city\assets\codebase\rendering.tools\CubeTestSceneFactory.cs`

- [ ] **Step 1: Extend the rendering script record factory with a cube-spin helper**

Add this pattern to `RenderingScriptComponentRecordFactory.cs` near the other rendering script record helpers:

```csharp
/// <summary>
/// Stable script type id used by the cube-test spin component.
/// </summary>
const string CubeTestSpinTypeId = "city.rendering.CubeTestSpinComponent, gameplay";

/// <summary>
/// Creates one serialized script component record for the cube-test spin behavior.
/// </summary>
/// <param name="componentIndex">Stable script component index within the owning entity.</param>
/// <param name="baseYawRadians">Base yaw offset applied before time-driven rotation.</param>
/// <param name="angularSpeedRadians">Angular speed in radians per second.</param>
/// <returns>Serialized scene component record for the cube spin behavior.</returns>
public static SceneComponentAssetRecord CreateCubeTestSpinRecord(int componentIndex, float baseYawRadians, float angularSpeedRadians) {
    EditorTaggedSceneComponentFieldWriter writer = new EditorTaggedSceneComponentFieldWriter();
    writer.WriteField("BaseYawRadians", fieldWriter => fieldWriter.WriteSingle(baseYawRadians));
    writer.WriteField("AngularSpeedRadians", fieldWriter => fieldWriter.WriteSingle(angularSpeedRadians));
    return new SceneComponentAssetRecord {
        ComponentTypeId = CubeTestSpinTypeId,
        ComponentIndex = componentIndex,
        Payload = writer.BuildPayload()
    };
}
```

- [ ] **Step 2: Attach the spin component to the cube entity in `CubeTestSceneFactory.cs`**

Change the cube entity component array from:

```csharp
Components = new[] {
    CreateMeshComponentRecord(modelReference, materialReference)
},
```

to:

```csharp
Components = new[] {
    CreateMeshComponentRecord(modelReference, materialReference),
    RenderingScriptComponentRecordFactory.CreateCubeTestSpinRecord(1, 0f, (float)(Math.PI / 2.0))
},
```

Keep the cube transform unchanged:

```csharp
LocalPosition = float3.Zero,
LocalScale = new float3(2f, 2f, 2f),
LocalOrientation = float4.Identity,
```

- [ ] **Step 3: Regenerate the authored cube scene**

Run:

```powershell
dotnet C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\helengine.ui\helengine.editor.app\bin\Debug\net9.0-windows\helengine.editor.app.dll --project C:\dev\helprojs\city\project.heproj --headless-command menu.generate-rendering-scenes
```

Expected:

```text
Headless command completed successfully
```

- [ ] **Step 4: Commit**

```bash
git add assets/codebase/rendering.tools/RenderingScriptComponentRecordFactory.cs assets/codebase/rendering.tools/CubeTestSceneFactory.cs assets/scenes/rendering/cube_test.helen
git commit -m "feat: attach spin behavior to cube test scene"
```

### Task 3: Prove The Cube Scene Exports The Spin Behavior

**Files:**
- Modify: `C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\CityCubeTestSceneSourceTests.cs`

- [ ] **Step 1: Add a failing source-level test for the spin component emission**

Add a test shaped like this:

```csharp
[Fact]
public void Cube_test_scene_factory_emits_spin_component_with_slow_rotation_defaults() {
    string source = File.ReadAllText(@"C:\dev\helprojs\city\assets\codebase\rendering.tools\CubeTestSceneFactory.cs");

    Assert.Contains("RenderingScriptComponentRecordFactory.CreateCubeTestSpinRecord(1, 0f, (float)(Math.PI / 2.0))", source, StringComparison.Ordinal);
}
```

If the existing test file already checks the cube scene source, keep the new assertion there instead of creating a new test file.

- [ ] **Step 2: Run the focused test to verify it fails before the factory change lands**

Run:

```powershell
dotnet test C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\helengine.editor.tests.csproj -c Debug --filter FullyQualifiedName~Cube_test_scene_factory_emits_spin_component_with_slow_rotation_defaults
```

Expected:

```text
Failed
```

- [ ] **Step 3: Run the focused test again after the factory change**

Run:

```powershell
dotnet test C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\helengine.editor.tests.csproj -c Debug --filter FullyQualifiedName~Cube_test_scene_factory_emits_spin_component_with_slow_rotation_defaults
```

Expected:

```text
Passed
```

- [ ] **Step 4: Run the existing cube-scene source coverage**

Run:

```powershell
dotnet test C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\helengine.editor.tests.csproj -c Debug --filter FullyQualifiedName~CityCubeTestSceneSourceTests
```

Expected:

```text
Passed
```

- [ ] **Step 5: Commit**

```bash
git add C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\CityCubeTestSceneSourceTests.cs
git commit -m "test: cover cube scene spin serialization"
```

### Task 4: Rebuild And Verify The Rotating Cube On PS2

**Files:**
- Verify runtime behavior through exported artifacts only

- [ ] **Step 1: Rebuild the normal PS2 cube ISO from the current worktree/editor path**

Run:

```powershell
dotnet C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\helengine.ui\helengine.editor.app\bin\Debug\net9.0-windows\helengine.editor.app.dll --build ps2 --project C:\dev\helprojs\city\project.heproj --output C:\dev\helprojs\output\ps2-cube-rotating
```

Expected:

```text
Build completed for platform 'ps2': C:\dev\helprojs\output\ps2-cube-rotating
```

- [ ] **Step 2: Verify the artifact exists**

Run:

```powershell
Get-Item C:\dev\helprojs\output\ps2-cube-rotating\game.iso | Select-Object FullName,Length,LastWriteTime
```

Expected:

```text
FullName ...\ps2-cube-rotating\game.iso
```

- [ ] **Step 3: Launch PCSX2 with the rebuilt cube ISO**

Run:

```powershell
Start-Process -FilePath 'C:\Program Files\PCSX2\pcsx2-qt.exe' -ArgumentList 'C:\dev\helprojs\output\ps2-cube-rotating\game.iso'
```

Expected:

```text
PCSX2 launches with the cube-test ISO
```

- [ ] **Step 4: Verify runtime behavior manually**

Expected visual result:

```text
A single red cube remains visible and rotates slowly around local Y, completing one full turn in about four seconds.
```

- [ ] **Step 5: Commit**

```bash
git add assets/codebase/rendering/CubeTestSpinComponent.cs assets/codebase/rendering.tools/RenderingScriptComponentRecordFactory.cs assets/codebase/rendering.tools/CubeTestSceneFactory.cs assets/scenes/rendering/cube_test.helen C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\CityCubeTestSceneSourceTests.cs
git commit -m "feat: rotate cube test scene"
```
