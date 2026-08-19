# PSP NPOT UI Texture Fix Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Render non-power-of-two PSP UI textures once, without GU wrap tiling.

**Architecture:** Keep `RuntimeTexture::Width` and `Height` as authored/logical dimensions for normalized source rectangles. Add PSP-only physical GU image dimensions, pad copied rows and bottom rows to that physical extent, and bind the physical extent while continuing to calculate UV texels from logical dimensions.

**Tech Stack:** C++20 PSP runtime, PSP GU, xUnit source-contract tests.

---

### Task 1: Lock down the physical texture-image contract

**Files:**
- Modify: `C:\dev\helworks\helengine-psp\builder.tests\PspPackagedRuntimeSourceTests.cs`

- [ ] **Step 1: Write the failing test**

Add `PspTextureCache_pads_runtime_textures_to_power_of_two_gu_images` that reads `PspRuntimeTexture.hpp`, `PspTextureCache.cpp`, `PspRenderManager2D.cpp`, and `PspRenderManager3D.cpp`. Assert the runtime exposes `GetTextureImageHeight`/`SetTextureImageHeight`, the cache pads to `GetTextureBufferWidth()` and image height, and both renderers bind `sceGuTexImage(0, texture->GetTextureBufferWidth(), texture->GetTextureImageHeight(), texture->GetTextureBufferWidth(), ...)`.

- [ ] **Step 2: Verify RED**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine-psp\builder.tests\helengine.psp.builder.tests.csproj --no-restore --filter FullyQualifiedName~PspTextureCache_pads_runtime_textures_to_power_of_two_gu_images
```

Expected: FAIL because `PspRuntimeTexture` has no image-height API and renderers bind authored dimensions.

### Task 2: Pad physical PSP texture images and bind them

**Files:**
- Modify: `C:\dev\helworks\helengine-psp\src\platform\psp\rendering\PspRuntimeTexture.hpp`
- Modify: `C:\dev\helworks\helengine-psp\src\platform\psp\rendering\PspRuntimeTexture.cpp`
- Modify: `C:\dev\helworks\helengine-psp\src\platform\psp\rendering\PspTextureCache.cpp`
- Modify: `C:\dev\helworks\helengine-psp\src\platform\psp\rendering\PspRenderManager2D.cpp`
- Modify: `C:\dev\helworks\helengine-psp\src\platform\psp\rendering\PspRenderManager3D.cpp`

- [ ] **Step 1: Add physical image-height state**

Add `std::uint16_t TextureImageHeight = 0u;` and matching getter/setter to `PspRuntimeTexture`. Keep inherited `Width`/`Height` unchanged as authored logical dimensions.

- [ ] **Step 2: Pad to a full GU image**

In `PspTextureCache`, retain the existing power-of-two width calculation, add matching power-of-two height calculation with the GU 512-pixel limit, and update the pixel-copy helper to allocate `textureBufferWidth * textureImageHeight`. Copy only authored rows and leave right/bottom padding transparent. Assign the new height in `CreateTexture`.

- [ ] **Step 3: Bind physical dimensions**

In both `PspRenderManager2D::BindTexture` and `PspRenderManager3D` texture binding, pass `GetTextureBufferWidth()` and `GetTextureImageHeight()` to `sceGuTexImage`. Continue using logical `get_Width()` / `get_Height()` for `ConvertSourceRectToTexturePixels`.

- [ ] **Step 4: Verify GREEN**

Run the focused source-contract test from Task 1. Expected: PASS.

- [ ] **Step 5: Verify integration**

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine-psp\builder.tests\helengine.psp.builder.tests.csproj --no-restore --filter FullyQualifiedName~PspTextureCache
```

Then use the canonical PSP build skill to rebuild `C:\dev\helprojs\output\psp-npot-ui-fix` and the PSP-owned launcher to boot its EBOOT. Use HelenUI only for any UI-state validation; do not manually navigate or OCR screenshots.