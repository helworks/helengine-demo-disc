# Realistic Textured Cube Grid Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the current placeholder textured cube-grid textures with sixteen distinct realistic 64x64 authored-looking surface textures while preserving the existing single-cube, colored-grid, and textured-grid scene structure.

**Architecture:** Keep the current `TexturedCubeGridSceneFactory` as the single source of truth for generated textures, imported texture cache entries, material assets, and the authored scene. Replace only the texture-generation logic and the source-level tests that describe the scene’s texture intent; do not change the PS2 runtime path or the packaging architecture that was just stabilized.

**Tech Stack:** C#/.NET 9, city scene generators, helengine asset serialization, xUnit, headless editor export, PS2 ISO export pipeline

---

### Task 1: Lock the new realistic texture requirements in source tests

**Files:**
- Modify: `C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\CityCubeTestSceneSourceTests.cs`
- Test: `C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\CityCubeTestSceneSourceTests.cs`

- [ ] **Step 1: Write the failing source test for the realistic texture set**

```csharp
/// <summary>
/// Ensures the textured cube-grid generator targets realistic 64x64 surface textures instead of the earlier flat-color diagnostic placeholders.
/// </summary>
[Fact]
public void Textured_cube_grid_scene_factory_uses_realistic_sixty_four_pixel_surface_textures() {
    string factorySource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\TexturedCubeGridSceneFactory.cs");

    Assert.Contains("const int TextureWidth = 64;", factorySource, StringComparison.Ordinal);
    Assert.Contains("const int TextureHeight = 64;", factorySource, StringComparison.Ordinal);
    Assert.Contains("static readonly RealisticTextureDefinition[] TextureDefinitions = {", factorySource, StringComparison.Ordinal);
    Assert.Contains("TextureSurfaceKind.Brick", factorySource, StringComparison.Ordinal);
    Assert.Contains("TextureSurfaceKind.StoneBlock", factorySource, StringComparison.Ordinal);
    Assert.Contains("TextureSurfaceKind.Concrete", factorySource, StringComparison.Ordinal);
    Assert.Contains("TextureSurfaceKind.Tile", factorySource, StringComparison.Ordinal);
    Assert.DoesNotContain("TextureBaseColors", factorySource, StringComparison.Ordinal);
}
```

- [ ] **Step 2: Run the focused source test to verify it fails**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\helengine.editor.tests.csproj -c Debug --filter FullyQualifiedName~Textured_cube_grid_scene_factory_uses_realistic_sixty_four_pixel_surface_textures
```

Expected: `FAIL` because `TexturedCubeGridSceneFactory.cs` still contains `TextureWidth = 32`, `TextureHeight = 32`, and `TextureBaseColors`.

- [ ] **Step 3: Keep the existing startup/export intent covered**

Do not change this existing test shape:

```csharp
AssertBuildConfigPlatformContainsOnlyTexturedCubeGridScene(platforms, "windows");
AssertBuildConfigPlatformContainsOnlyTexturedCubeGridScene(platforms, "ps2");
```

The realistic-texture pass must keep `textured_cube_grid.helen` as the only startup/export scene for now.

- [ ] **Step 4: Re-run the focused source test after implementation and expect it to pass**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\helengine.editor.tests.csproj -c Debug --filter FullyQualifiedName~Textured_cube_grid_scene_factory_uses_realistic_sixty_four_pixel_surface_textures
```

Expected: `PASS`

- [ ] **Step 5: Commit**

```bash
git -C C:\dev\helprojs\demodisc add assets/codebase/rendering.tools/TexturedCubeGridSceneFactory.cs
git -C C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core add engine/helengine.editor.tests/CityCubeTestSceneSourceTests.cs
git -C C:\dev\helprojs\demodisc commit -m "test: lock realistic textured cube grid requirements"
```

### Task 2: Replace the placeholder texture generator with realistic 64x64 surface generation

**Files:**
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\rendering.tools\TexturedCubeGridSceneFactory.cs`
- Test: `C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\CityCubeTestSceneSourceTests.cs`

- [ ] **Step 1: Add explicit texture-surface definitions and remove the old flat-color palette**

Replace the old constants and palette section with a definition-driven structure:

```csharp
/// <summary>
/// Describes one generated realistic texture variant for the textured cube-grid scene.
/// </summary>
readonly record struct RealisticTextureDefinition(
    TextureSurfaceKind SurfaceKind,
    string PrimaryColor,
    string SecondaryColor,
    string AccentColor,
    int CellWidth,
    int CellHeight,
    int MortarThickness,
    double NoiseStrength,
    double WearStrength);

/// <summary>
/// Enumerates the generated realistic surface families used by the textured cube-grid scene.
/// </summary>
enum TextureSurfaceKind {
    Brick,
    StoneBlock,
    Concrete,
    Tile
}

const int TextureWidth = 64;
const int TextureHeight = 64;

static readonly RealisticTextureDefinition[] TextureDefinitions = {
    new(TextureSurfaceKind.Brick, "#7A3028FF", "#5A221CFF", "#A45642FF", 14, 8, 2, 0.10, 0.08),
    new(TextureSurfaceKind.Brick, "#8B3A2BFF", "#64281DFF", "#B5694CFF", 12, 7, 2, 0.11, 0.10),
    new(TextureSurfaceKind.Brick, "#6F2F24FF", "#4D2018FF", "#8E5140FF", 16, 8, 2, 0.09, 0.12),
    new(TextureSurfaceKind.Brick, "#9A4C34FF", "#733524FF", "#C07C56FF", 13, 8, 2, 0.10, 0.08),
    new(TextureSurfaceKind.Brick, "#7B4331FF", "#583022FF", "#A46C58FF", 15, 9, 2, 0.08, 0.11),
    new(TextureSurfaceKind.Brick, "#91553CFF", "#6B3E2BFF", "#B88563FF", 14, 8, 2, 0.08, 0.09),
    new(TextureSurfaceKind.StoneBlock, "#8A8177FF", "#6F685FFF", "#AAA196FF", 16, 12, 2, 0.07, 0.06),
    new(TextureSurfaceKind.StoneBlock, "#7E756BFF", "#635C54FF", "#9E948AFF", 18, 12, 2, 0.06, 0.08),
    new(TextureSurfaceKind.StoneBlock, "#958C81FF", "#766F66FF", "#B5ACA0FF", 14, 10, 2, 0.07, 0.07),
    new(TextureSurfaceKind.StoneBlock, "#A0978BFF", "#80786EFF", "#C0B7AAFF", 20, 12, 2, 0.05, 0.07),
    new(TextureSurfaceKind.Concrete, "#8A8A84FF", "#70706BFF", "#A4A49EFF", 32, 32, 0, 0.12, 0.10),
    new(TextureSurfaceKind.Concrete, "#7B7A74FF", "#61605BFF", "#94938DFF", 32, 32, 0, 0.10, 0.13),
    new(TextureSurfaceKind.Concrete, "#9B978FFF", "#7E7A73FF", "#B5B1A8FF", 32, 32, 0, 0.09, 0.12),
    new(TextureSurfaceKind.Tile, "#6C5E4BFF", "#544838FF", "#8B7A63FF", 10, 10, 1, 0.05, 0.05),
    new(TextureSurfaceKind.Tile, "#4F5F67FF", "#3E4B52FF", "#6D8089FF", 8, 8, 1, 0.04, 0.04),
    new(TextureSurfaceKind.Tile, "#837B70FF", "#675F56FF", "#A19A90FF", 12, 12, 1, 0.05, 0.06)
};
```

- [ ] **Step 2: Replace the old checker/stripe generator with surface-specific pixel generation**

Implement a definition-driven pixel path:

```csharp
static byte[] BuildTextureFileBytes(int cubeIndex) {
    RealisticTextureDefinition definition = TextureDefinitions[cubeIndex];
    int rowStride = ((TextureWidth * 3) + 3) & ~3;
    int pixelDataLength = rowStride * TextureHeight;
    int pixelDataOffset = 14 + 40;
    int fileLength = pixelDataOffset + pixelDataLength;
    byte[] fileBytes = new byte[fileLength];

    fileBytes[0] = (byte)'B';
    fileBytes[1] = (byte)'M';
    WriteInt32(fileBytes, 2, fileLength);
    WriteInt32(fileBytes, 10, pixelDataOffset);
    WriteInt32(fileBytes, 14, 40);
    WriteInt32(fileBytes, 18, TextureWidth);
    WriteInt32(fileBytes, 22, TextureHeight);
    WriteInt16(fileBytes, 26, 1);
    WriteInt16(fileBytes, 28, 24);
    WriteInt32(fileBytes, 34, pixelDataLength);

    for (int y = 0; y < TextureHeight; y++) {
        int rowOffset = pixelDataOffset + ((TextureHeight - 1 - y) * rowStride);
        for (int x = 0; x < TextureWidth; x++) {
            byte[] pixelColor = ResolveSurfacePixelColor(cubeIndex, x, y, definition);
            int pixelOffset = rowOffset + (x * 3);
            fileBytes[pixelOffset + 0] = pixelColor[2];
            fileBytes[pixelOffset + 1] = pixelColor[1];
            fileBytes[pixelOffset + 2] = pixelColor[0];
        }
    }

    return fileBytes;
}

static byte[] BuildTextureAssetColors(int cubeIndex) {
    RealisticTextureDefinition definition = TextureDefinitions[cubeIndex];
    byte[] colors = new byte[TextureWidth * TextureHeight * 4];

    for (int y = 0; y < TextureHeight; y++) {
        for (int x = 0; x < TextureWidth; x++) {
            byte[] pixelColor = ResolveSurfacePixelColor(cubeIndex, x, y, definition);
            int pixelOffset = ((y * TextureWidth) + x) * 4;
            colors[pixelOffset] = pixelColor[0];
            colors[pixelOffset + 1] = pixelColor[1];
            colors[pixelOffset + 2] = pixelColor[2];
            colors[pixelOffset + 3] = pixelColor[3];
        }
    }

    return colors;
}
```

- [ ] **Step 3: Add explicit helpers for realistic surface generation**

Add focused helpers instead of local functions:

```csharp
static byte[] ResolveSurfacePixelColor(int cubeIndex, int x, int y, RealisticTextureDefinition definition) {
    if (definition.SurfaceKind == TextureSurfaceKind.Brick) {
        return ResolveBrickPixelColor(cubeIndex, x, y, definition);
    } else if (definition.SurfaceKind == TextureSurfaceKind.StoneBlock) {
        return ResolveStoneBlockPixelColor(cubeIndex, x, y, definition);
    } else if (definition.SurfaceKind == TextureSurfaceKind.Concrete) {
        return ResolveConcretePixelColor(cubeIndex, x, y, definition);
    }

    return ResolveTilePixelColor(cubeIndex, x, y, definition);
}

static byte[] ResolveBrickPixelColor(int cubeIndex, int x, int y, RealisticTextureDefinition definition) { /* ... */ }
static byte[] ResolveStoneBlockPixelColor(int cubeIndex, int x, int y, RealisticTextureDefinition definition) { /* ... */ }
static byte[] ResolveConcretePixelColor(int cubeIndex, int x, int y, RealisticTextureDefinition definition) { /* ... */ }
static byte[] ResolveTilePixelColor(int cubeIndex, int x, int y, RealisticTextureDefinition definition) { /* ... */ }
static byte[] ApplyWearAndNoise(byte[] sourceColor, int cubeIndex, int x, int y, RealisticTextureDefinition definition) { /* ... */ }
static double ComputeDeterministicNoise(int cubeIndex, int x, int y, int salt) { /* ... */ }
static byte[] LerpColor(byte[] left, byte[] right, double amount) { /* ... */ }
static byte[] LightenColor(byte[] sourceColor, double amount) { /* ... */ }
```

Implementation rules:
- keep everything deterministic from `cubeIndex`, `x`, `y`
- no random runtime state
- keep file output and cached `TextureAsset` output visually identical
- use `double` math for mixing/noise and cast back to `byte`

- [ ] **Step 4: Preserve the current material and scene wiring**

Do not change these relationships:

```csharp
DiffuseTextureAssetId = CubeTextureAssetIds[cubeIndex],
windowsSettings.Material.FieldValues[TextureIdFieldId] = CubeTextureAssetIds[cubeIndex];
ps2Settings.Material.FieldValues[TextureIdFieldId] = CubeTextureAssetIds[cubeIndex];
```

The realistic texture pass is content generation only. It must not reopen the packaging/runtime bugs that were already fixed.

- [ ] **Step 5: Run the focused city source test group**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\helengine.editor.tests.csproj -c Debug --filter FullyQualifiedName~CityCubeTestSceneSourceTests
```

Expected: `PASS`

- [ ] **Step 6: Commit**

```bash
git -C C:\dev\helprojs\demodisc add assets/codebase/rendering.tools/TexturedCubeGridSceneFactory.cs
git -C C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core add engine/helengine.editor.tests/CityCubeTestSceneSourceTests.cs
git -C C:\dev\helprojs\demodisc commit -m "feat: generate realistic textured cube grid surfaces"
```

### Task 3: Regenerate the city textured scene assets from the updated generator

**Files:**
- Modify: `C:\dev\helprojs\demodisc\assets\scenes\rendering\textured_cube_grid.helen`
- Modify: `C:\dev\helprojs\demodisc\assets\materials\rendering\textured_cube_grid\*.helmat`
- Modify: `C:\dev\helprojs\demodisc\assets\textures\rendering\textured_cube_grid\*.bmp`
- Modify: `C:\dev\helprojs\demodisc\cache\*`
- Test: `C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\CityCubeTestSceneSourceTests.cs`

- [ ] **Step 1: Regenerate the rendering scenes from the city project command**

Run:

```powershell
rtk dotnet C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\helengine.ui\helengine.editor.app\bin\Debug\net9.0-windows\helengine.editor.app.dll --project C:\dev\helprojs\demodisc\project.heproj --editor-command menu.generate-rendering-scenes
```

Expected: successful command completion and rewritten `textured_cube_grid.helen`, generated materials, generated textures, and cache assets under the city project.

- [ ] **Step 2: Verify the authored textured-grid scene and materials were regenerated**

Run:

```powershell
rtk proxy powershell.exe -NoProfile -Command "Get-Item 'C:\dev\helprojs\demodisc\assets\scenes\rendering\textured_cube_grid.helen','C:\dev\helprojs\demodisc\assets\textures\rendering\textured_cube_grid\Cube00.bmp','C:\dev\helprojs\demodisc\assets\materials\rendering\textured_cube_grid\Cube00.helmat' | Select-Object FullName,LastWriteTime,Length"
```

Expected: fresh timestamps for the scene, texture, and material outputs.

- [ ] **Step 3: Re-run the focused city source tests after regeneration**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\helengine.editor.tests.csproj -c Debug --filter FullyQualifiedName~CityCubeTestSceneSourceTests
```

Expected: `PASS`

- [ ] **Step 4: Commit**

```bash
git -C C:\dev\helprojs\demodisc add assets/scenes/rendering/textured_cube_grid.helen assets/materials/rendering/textured_cube_grid assets/textures/rendering/textured_cube_grid cache
git -C C:\dev\helprojs\demodisc commit -m "chore: regenerate realistic textured cube grid assets"
```

### Task 4: Rebuild and export the PS2 textured grid to verify runtime behavior

**Files:**
- Modify: `C:\dev\helprojs\output\ps2-textured-cube-grid-realistic\game.iso`
- Test: `C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\BinarySerializationTests.cs`
- Test: `C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\managers\project\EditorWindowsBuildScenePackagerTests.cs`
- Test: `C:\dev\helworks\helengine-ps2\.worktrees\normalize-camera-viewport-core\builder.tests\Ps2PlatformAssetBuilderTests.cs`

- [ ] **Step 1: Run the focused regression trio before export**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\engine\helengine.editor.tests\helengine.editor.tests.csproj -c Debug --filter "FullyQualifiedName~EngineBinaryReader_WhenStringLengthIsMinusOne_ReturnsEmptyString|FullyQualifiedName~Package_WhenSceneReferencesFileBackedPs2MaterialWithImportedTexture_RewritesTextureFieldToCookedRuntimePath"
```

Run:

```powershell
$env:HELENGINE_ROOT='C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core'; rtk dotnet test C:\dev\helworks\helengine-ps2\.worktrees\normalize-camera-viewport-core\builder.tests\helengine.ps2.builder.tests.csproj -c Debug --filter FullyQualifiedName~BuildAsync_WhenPackagedEngineMatMaterialUsesImportedTexture_RewritesTexturePathToPhysicalDiscPath
```

Expected: all three focused regressions `PASS`

- [ ] **Step 2: Rebuild the worktree editor app**

Run:

```powershell
rtk dotnet build C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -c Debug
```

Expected: `0 errors`

- [ ] **Step 3: Export the fresh PS2 ISO**

Run:

```powershell
$env:HELENGINE_ROOT='C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core'; & 'C:\Program Files\dotnet\dotnet.exe' 'C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core\helengine.ui\helengine.editor.app\bin\Debug\net9.0-windows\helengine.editor.app.dll' --project 'C:\dev\helprojs\demodisc\project.heproj' --build ps2 --output 'C:\dev\helprojs\output\ps2-textured-cube-grid-realistic'
```

Expected: `Build completed for platform 'ps2'`

- [ ] **Step 4: Verify the exported disc contains `.HAS` imported textures**

Run:

```powershell
rtk proxy powershell.exe -NoProfile -Command "Get-ChildItem 'C:\dev\helprojs\output\ps2-textured-cube-grid-realistic\disc\IMPORTED' -Recurse | Select-Object FullName,Name,Length,PSIsContainer"
```

Expected: imported texture leaf files end in `.HAS`

- [ ] **Step 5: Launch PCSX2, capture the result, and close PCSX2 after reading it**

Run:

```powershell
rtk proxy powershell.exe -NoProfile -Command "Start-Process 'C:\Program Files\PCSX2\pcsx2-qt.exe' -ArgumentList 'C:\dev\helprojs\output\ps2-textured-cube-grid-realistic\game.iso'"
```

Then capture:

```powershell
rtk proxy powershell.exe -NoProfile -Command "& 'C:\dev\helenui\plugins\screenshot-cli\src\ScreenshotCli\bin\Debug\net8.0-windows10.0.19041.0\ScreenshotCli.exe' capture --title 'HELENGIN.ELF [?]' --output 'C:\tmp\pcsx2-ocr\ps2-textured-cube-grid-realistic.png'"
```

Then read the screenshot and close `PCSX2`.

Expected: the textured cube grid renders without runtime exceptions and shows sixteen distinct realistic surface textures under scene lighting.

- [ ] **Step 6: Commit**

```bash
git -C C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core add engine/helengine.core/serialization/EngineBinaryReader.cs engine/helengine.editor.tests/BinarySerializationTests.cs engine/helengine.editor/managers/project/EditorWindowsBuildScenePackager.cs engine/helengine.editor/managers/project/SceneComponentPackagingTransformService.cs engine/helengine.editor.tests/managers/project/EditorWindowsBuildScenePackagerTests.cs
git -C C:\dev\helworks\helengine-ps2\.worktrees\normalize-camera-viewport-core add builder.tests/Ps2PlatformAssetBuilderTests.cs
git -C C:\dev\helworks\helengine\.worktrees\normalize-camera-viewport-core commit -m "fix: stabilize textured ps2 asset loading"
git -C C:\dev\helworks\helengine-ps2\.worktrees\normalize-camera-viewport-core commit -m "test: cover textured ps2 asset paths"
```
