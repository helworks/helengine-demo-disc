# City Wii U Main Menu Scene Set Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Change the default city Wii U build config so `wiiu` packages `DemoDiscMainMenu` and every currently reachable playable scene from the main menu.

**Architecture:** Keep this as a config-only city project change guarded by one editor-side source test. The test will lock the required Wii U scene set and menu-first ordering, then `build_config.json` will be updated to satisfy that contract and verified through one real Wii U wrapper build.

**Tech Stack:** C#, xUnit, `System.Text.Json`, city `build_config.json`, Helengine editor CLI wrapper

---

### Task 1: Add A Wii U Build Config Source Contract Test

**Files:**
- Create: `C:\dev\helworks\helengine\engine\helengine.editor.tests\CityWiiUBuildConfigSourceTests.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj`

- [ ] **Step 1: Write the failing test**

```csharp
using System.Text.Json;

namespace helengine.editor.tests;

/// <summary>
/// Verifies the authored city Wii U build configuration packages the demo-disc main menu and every scene reachable from it.
/// </summary>
public sealed class CityWiiUBuildConfigSourceTests {
    /// <summary>
    /// Ensures the persisted city Wii U build configuration keeps <c>DemoDiscMainMenu</c> first and includes every currently selectable demo-disc scene.
    /// </summary>
    [Fact]
    public void City_wiiu_build_config_includes_demo_disc_main_menu_scene_set() {
        string sourcePath = @"C:\dev\helprojs\demodisc\user_settings\build_config.json";
        string source = File.ReadAllText(sourcePath);
        using JsonDocument document = JsonDocument.Parse(source);
        JsonElement platforms = document.RootElement.GetProperty("platforms");
        JsonElement wiiuPlatform = platforms.EnumerateArray().Single(platform => string.Equals(platform.GetProperty("platformId").GetString(), "wiiu", StringComparison.Ordinal));
        JsonElement selectedSceneIds = wiiuPlatform.GetProperty("selectedSceneIds");
        JsonElement sceneOrders = wiiuPlatform.GetProperty("sceneOrders");
        string[] requiredSceneIds = [
            "DemoDiscMainMenu",
            "cube_test",
            "colored_cube_grid",
            "textured_cube_grid",
            "axis_test",
            "axis_test2",
            "test_scene_matrix_render",
            "directional_shadow_plaza",
            "test_scene_dynamic_stack_boxes",
            "test_scene_dynamic_sphere_stack",
            "test_scene_dynamic_mixed_stack",
            "test_scene_static_mesh_showcase",
            "test_scene_static_mesh_minimal",
            "tilt_trial"
        ];

        Assert.Equal("DemoDiscMainMenu", selectedSceneIds[0].GetString());
        JsonElement firstSceneOrder = sceneOrders.EnumerateArray().Single(sceneOrder => sceneOrder.GetProperty("orderNumber").GetInt32() == 1);
        Assert.Equal("DemoDiscMainMenu", firstSceneOrder.GetProperty("sceneId").GetString());
        Assert.Equal(requiredSceneIds.Length, selectedSceneIds.GetArrayLength());
        Assert.Equal(requiredSceneIds.Length, sceneOrders.GetArrayLength());

        foreach (string requiredSceneId in requiredSceneIds) {
            Assert.Contains(selectedSceneIds.EnumerateArray(), sceneId => string.Equals(sceneId.GetString(), requiredSceneId, StringComparison.Ordinal));
            Assert.Contains(sceneOrders.EnumerateArray(), sceneOrder => string.Equals(sceneOrder.GetProperty("sceneId").GetString(), requiredSceneId, StringComparison.Ordinal));
        }
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj -nologo --filter "FullyQualifiedName~CityWiiUBuildConfigSourceTests"
```

Expected: FAIL because the current `wiiu` config only contains `cube_test` and does not include `DemoDiscMainMenu` or the rest of the menu scene set.

- [ ] **Step 3: Commit the failing test**

```bash
git -C C:\dev\helworks\helengine add -- engine/helengine.editor.tests/CityWiiUBuildConfigSourceTests.cs
git -C C:\dev\helworks\helengine commit -m "test: lock city wiiu demo-disc scene set"
```

### Task 2: Update The City Wii U Build Configuration

**Files:**
- Modify: `C:\dev\helprojs\demodisc\user_settings\build_config.json`

- [ ] **Step 1: Replace the `wiiu` scene list and order with the full menu-backed set**

Use this exact `wiiu` block content for `selectedSceneIds` and `sceneOrders`:

```json
{
  "platformId": "wiiu",
  "selectedSceneIds": [
    "DemoDiscMainMenu",
    "cube_test",
    "colored_cube_grid",
    "textured_cube_grid",
    "axis_test",
    "axis_test2",
    "test_scene_matrix_render",
    "directional_shadow_plaza",
    "test_scene_dynamic_stack_boxes",
    "test_scene_dynamic_sphere_stack",
    "test_scene_dynamic_mixed_stack",
    "test_scene_static_mesh_showcase",
    "test_scene_static_mesh_minimal",
    "tilt_trial"
  ],
  "sceneOrders": [
    { "sceneId": "DemoDiscMainMenu", "orderNumber": 1 },
    { "sceneId": "cube_test", "orderNumber": 2 },
    { "sceneId": "colored_cube_grid", "orderNumber": 3 },
    { "sceneId": "textured_cube_grid", "orderNumber": 4 },
    { "sceneId": "axis_test", "orderNumber": 5 },
    { "sceneId": "axis_test2", "orderNumber": 6 },
    { "sceneId": "test_scene_matrix_render", "orderNumber": 7 },
    { "sceneId": "directional_shadow_plaza", "orderNumber": 8 },
    { "sceneId": "test_scene_dynamic_stack_boxes", "orderNumber": 9 },
    { "sceneId": "test_scene_dynamic_sphere_stack", "orderNumber": 10 },
    { "sceneId": "test_scene_dynamic_mixed_stack", "orderNumber": 11 },
    { "sceneId": "test_scene_static_mesh_showcase", "orderNumber": 12 },
    { "sceneId": "test_scene_static_mesh_minimal", "orderNumber": 13 },
    { "sceneId": "tilt_trial", "orderNumber": 14 }
  ]
}
```

Leave the existing Wii U output path, build profile, graphics profile, build options, graphics options, and codegen options unchanged.

- [ ] **Step 2: Run the targeted source test to verify it passes**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj -nologo --filter "FullyQualifiedName~CityWiiUBuildConfigSourceTests"
```

Expected: PASS with `1` test passed and `0` failed.

- [ ] **Step 3: Commit the config change**

```bash
git -C C:\dev\helprojs\demodisc add -- user_settings/build_config.json
git -C C:\dev\helprojs\demodisc commit -m "feat: expand city wiiu menu scene set"
```

### Task 3: Verify The Wii U Build Output

**Files:**
- Verify: `C:\dev\helworks\helengine\artifacts\build-platform.ps1`
- Verify: `C:\dev\helprojs\demodisc\project.heproj`
- Verify: `C:\dev\helprojs\output\wiiu`

- [ ] **Step 1: Build the city Wii U package through the shared wrapper**

Run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine\artifacts\build-platform.ps1 `
  -Project C:\dev\helprojs\demodisc\project.heproj `
  -Platform wiiu `
  -Output C:\dev\helprojs\output\wiiu
```

Expected: exit code `0` and a completed Wii U platform build.

- [ ] **Step 2: Inspect the staged cooked scenes**

Run:

```powershell
rtk proxy powershell.exe -NoProfile -Command "Get-ChildItem 'C:\dev\helprojs\output\wiiu\cooked\scenes' -Recurse -File | Select-Object -ExpandProperty FullName | Out-String -Width 240"
```

Expected: output includes cooked scene files for:

```text
DemoDiscMainMenu
cube_test
colored_cube_grid
textured_cube_grid
axis_test
axis_test2
test_scene_matrix_render
directional_shadow_plaza
test_scene_dynamic_stack_boxes
test_scene_dynamic_sphere_stack
test_scene_dynamic_mixed_stack
test_scene_static_mesh_showcase
test_scene_static_mesh_minimal
tilt_trial
```

- [ ] **Step 3: Commit the verification checkpoint**

```bash
git -C C:\dev\helworks\helengine status --short
git -C C:\dev\helprojs\demodisc status --short
```
