# PS2 Tilt Play Right-Stick Camera Control Implementation Plan

> **For agentic workers:** Execute this plan inline with test-first checkpoints. The PS2 ELF is intentionally not built.

**Goal:** Publish the PS2 DualShock right analog stick through the shared input state so Tilt Play's existing orbit camera can respond to it.

**Architecture:** Keep `DemoTiltFollowCameraComponent` unchanged because it already consumes shared right-stick X/Y values for yaw and pitch. Extend the PS2 pad snapshot and backend publication path to normalize `rjoy_h`/`rjoy_v` exactly like the left stick, and protect the mapping with the existing source-level PS2 native-input test suite.

**Tech Stack:** PS2 C++ runtime input backend, generated `InputGamepadState` API, C# xUnit source-contract tests.

---

### Task 1: Add the failing right-stick mapping contract

**Files:**
- Modify: `C:/dev/helworks/helengine-ps2/builder.tests/Ps2NativeBuildInputsTests.cs`

- [ ] **Step 1: Add one focused test** after `Ps2_input_backend_maps_dualshock_left_stick_axes` asserting these exact contracts:

```csharp
[Fact]
public void Ps2_input_backend_maps_dualshock_right_stick_axes() {
    string repositoryRootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    string mapperHeader = File.ReadAllText(Path.Combine(repositoryRootPath, "src", "platform", "ps2", "Ps2PadInputMapper.hpp"));
    string inputSource = File.ReadAllText(Path.Combine(repositoryRootPath, "src", "platform", "ps2", "Ps2InputBackend.cpp"));

    Assert.Contains("int16_t RightStickX = 0;", mapperHeader, StringComparison.Ordinal);
    Assert.Contains("int16_t RightStickY = 0;", mapperHeader, StringComparison.Ordinal);
    Assert.Contains("snapshot.RightStickX = NormalizeAnalogAxis(buttons.rjoy_h);", inputSource, StringComparison.Ordinal);
    Assert.Contains("snapshot.RightStickY = NormalizeAnalogAxis(buttons.rjoy_v);", inputSource, StringComparison.Ordinal);
    Assert.Contains("gamepad.set_RightStickX(CurrentButtons.RightStickX);", inputSource, StringComparison.Ordinal);
    Assert.Contains("gamepad.set_RightStickY(CurrentButtons.RightStickY);", inputSource, StringComparison.Ordinal);
}
```

- [ ] **Step 2: Run the focused test and verify RED**:

```powershell
rtk dotnet test C:\dev\helworks\helengine-ps2\builder.tests\helengine.ps2.builder.tests.csproj --no-restore --filter "FullyQualifiedName~Ps2NativeBuildInputsTests.Ps2_input_backend_maps_dualshock_right_stick_axes"
```

Expected result: the test fails because the new right-stick fields and assignments are absent.

### Task 2: Publish the PS2 right-stick values

**Files:**
- Modify: `C:/dev/helworks/helengine-ps2/src/platform/ps2/Ps2PadInputMapper.hpp`
- Modify: `C:/dev/helworks/helengine-ps2/src/platform/ps2/Ps2InputBackend.cpp`

- [ ] **Step 1: Extend `Ps2PadButtons`** immediately after `LeftStickY`:

```cpp
int16_t RightStickX = 0;
int16_t RightStickY = 0;
```

- [ ] **Step 2: Decode the DualShock right-stick axes** inside the existing `if (analogAvailable)` block:

```cpp
snapshot.RightStickX = NormalizeAnalogAxis(buttons.rjoy_h);
snapshot.RightStickY = NormalizeAnalogAxis(buttons.rjoy_v);
```

- [ ] **Step 3: Publish both axes** beside the existing left-stick assignments:

```cpp
gamepad.set_RightStickX(CurrentButtons.RightStickX);
gamepad.set_RightStickY(CurrentButtons.RightStickY);
```

### Task 3: Verify the implementation

- [ ] **Step 1: Rerun the focused test** with the same command from Task 1 and verify it passes.
- [ ] **Step 2: Run the existing PS2 native-input source tests** without building the PS2 ELF:

```powershell
rtk dotnet test C:\dev\helworks\helengine-ps2\builder.tests\helengine.ps2.builder.tests.csproj --no-restore --filter "FullyQualifiedName~Ps2NativeBuildInputsTests"
```

- [ ] **Step 3: Run `git diff --check`** on the touched PS2 source/test files and inspect the diff to confirm no unrelated changes were made.
