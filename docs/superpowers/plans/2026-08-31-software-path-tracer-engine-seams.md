# Software Path Tracer Engine Seams Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add exactly two reusable HelenEngine primitives required by DemoDisc: selective CPU-readable model companions and rectangular RGBA8 updates to existing runtime textures.

**Architecture:** A reflected `[CpuReadableModelReference]` marker tells scene packaging to rewrite a `SceneAssetReference` into a separate generic `ModelAsset` companion path. The marker does not load geometry. `RenderManager2D` owns common argument validation for region uploads and delegates validated bytes to each backend. Both APIs remain ignorant of path tracing.

**Tech Stack:** C#/.NET 9, xUnit, HelenEngine editor packaging, cs2cpp-generated C++, Direct3D 11, Vulkan, Nintendo/Sony platform texture APIs.

**Spec:** `C:\dev\helprojs\demodisc\docs\superpowers\specs\2026-08-31-demodisc-software-path-tracer-design.md`

## Global Constraints

- Work in the canonical repositories under `C:\dev\helworks`; create an isolated worktree before edits if the active checkout is dirty.
- Do not add a model loader, ray type, BVH, software material, tracer, tone mapper, or DemoDisc type to the engine.
- CPU companions use distinct `cooked/cpu-models/...` paths so platform-owned model cooking cannot overwrite them.
- Only members marked `[CpuReadableModelReference]` produce companions. Ordinary `RuntimeModel` references keep current size and behavior.
- The marked member type is exactly `SceneAssetReference`; arrays and other member types fail packaging with a clear exception.
- `UpdateTextureRegion` accepts tightly or loosely pitched RGBA8 source rows, validates before dispatch, and never retains the source array.
- Each native backend must update the already-created texture and must not replace the `RuntimeTexture` object.

---

### Task 1: Declare and test selective CPU-readable model references

**Files:**
- Create: `C:\dev\helworks\helengine\engine\helengine.core\scene\CpuReadableModelReferenceAttribute.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\scene\runtime\AutomaticScriptComponentRuntimeDeserializer.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\serialization\scene\ScriptComponentReflectionMember.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\SceneComponentPackagingTransformService.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\ScriptComponentPlayerDeserializerGenerator.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\managers\project\SceneComponentPackagingTransformServiceTests.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\managers\project\ScriptComponentPlayerDeserializerGeneratorTests.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\serialization\scene\AutomaticScriptComponentPersistenceDescriptorTests.cs`

**Interfaces:**
- Produces: `[CpuReadableModelReference]` for a public `SceneAssetReference` field/property.
- Produces: packaged generic `ModelAsset` paths beneath `cooked/cpu-models/`.
- Produces: raw `SceneAssetReference` restoration in reflected and generated managed/native player deserializers without resolving a GPU/runtime asset.
- Consumed by: `SoftwareModelComponent.ModelReference` in the DemoDisc-core plan.

- [ ] **Step 1: Write failing packaging tests.** Add a test-only component with one marked model reference and assert all of the following:

```csharp
sealed class CpuModelReferenceProbeComponent : Component {
    [CpuReadableModelReference]
    public SceneAssetReference ModelReference { get; set; }
}

[Fact]
public void Transform_marked_generated_cube_reference_writes_generic_cpu_companion() {
    SceneAssetReference source = SceneAssetReferenceTestFactory.CreateEngineCubeModel();
    SceneComponentAssetRecord output = TransformCpuProbe(source, marked: true);
    SceneAssetReference packaged = ReadMemberReference<CpuModelReferenceProbeComponent>(output, "ModelReference");
    Assert.Equal("cooked/cpu-models/engine/cube.hasset", packaged.RelativePath.Replace('\\', '/'));
    ModelAsset model = Assert.IsType<ModelAsset>(ReadPackagedAsset(packaged.RelativePath));
    Assert.NotEmpty(model.Positions);
    Assert.True((model.Indices16.Length > 0) ^ (model.Indices32.Length > 0));
}

[Fact]
public void Transform_unmarked_scene_reference_does_not_write_cpu_model_companion() {
    TransformCpuProbe(SceneAssetReferenceTestFactory.CreateEngineCubeModel(), marked: false);
    Assert.False(File.Exists(Path.Combine(BuildRootPath, "cooked", "cpu-models", "engine", "cube.hasset")));
}
```

Add a separate schema-validation test whose marked member is `string`; assert the thrown `InvalidOperationException` contains both `SceneAssetReference` and the member name.

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~Cpu_readable_model_reference"
```

Expected: FAIL because the attribute and rewrite path do not exist.

- [ ] **Step 2: Add the marker and reflection query.** Implement the attribute as metadata only:

```csharp
namespace helengine {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class CpuReadableModelReferenceAttribute : Attribute { }
}
```

Add this to `ScriptComponentReflectionMember`:

```csharp
public bool HasAttribute<TAttribute>() where TAttribute : Attribute {
    return MemberInfoValue != null && MemberInfoValue.IsDefined(typeof(TAttribute), true);
}
```

- [ ] **Step 3: Rewrite only marked members before runtime payload serialization.** In `BuildAutomaticRuntimeComponentRecord`, call a new method before `WriteSupportedMemberValue`:

```csharp
void RewriteCpuReadableModelReferences(
    ScriptComponentReflectionSchema schema,
    Component component,
    string buildRootPath) {
    for (int index = 0; index < schema.Members.Count; index++) {
        ScriptComponentReflectionMember member = schema.Members[index];
        if (!member.HasAttribute<CpuReadableModelReferenceAttribute>()) {
            continue;
        }
        if (member.ValueType != typeof(SceneAssetReference)) {
            throw new InvalidOperationException(
                $"CPU-readable model member '{member.Name}' must use SceneAssetReference.");
        }

        SceneAssetReference source = (SceneAssetReference)member.GetValue(component);
        member.SetValue(component, RewriteCpuReadableModelReference(source, buildRootPath));
    }
}
```

Implement generated cube/plane/sphere and filesystem-backed model branches by writing the existing `ModelAsset` serializer output beneath `cooked/cpu-models/`. Use stable asset-id/hash-derived names for filesystem inputs so two same-named source files cannot collide. Return a packaged reference pointing at the companion; do not schedule a platform model-cook work item for it.

- [ ] **Step 3a: Preserve raw references in player deserializers.** Add direct `SceneAssetReferenceFactory.ReadOptionalReference` cases to the reflection runtime deserializer and generated managed/native deserializers. The raw reference must be assigned unchanged and must not call `RuntimeSceneAssetReferenceResolver` or load a renderer asset. Add null/non-null runtime round-trip tests plus managed/native generator-source tests, including native type and include coverage.

- [ ] **Step 4: Add rejection, cache, and collision coverage.** Assert null marked references remain null, repeated identical references write one companion, a deleted cached companion is recreated when the same service/build root is reused, same-asset-id references with different content hashes cannot overwrite one another, malformed/unsupported generated references fail, and the ordinary `MeshComponent.Model` packaged path remains unchanged.

- [ ] **Step 5: Run the focused and neighboring tests.** Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~SceneComponentPackagingTransformServiceTests|FullyQualifiedName~RuntimeSceneAssetReferenceResolver"
```

Expected: PASS.

- [ ] **Step 6: Commit.**

```powershell
rtk git add -- engine/helengine.core/scene/CpuReadableModelReferenceAttribute.cs engine/helengine.core/scene/runtime/AutomaticScriptComponentRuntimeDeserializer.cs engine/helengine.editor/serialization/scene/ScriptComponentReflectionMember.cs engine/helengine.editor/managers/project/SceneComponentPackagingTransformService.cs engine/helengine.editor/managers/project/ScriptComponentPlayerDeserializerGenerator.cs engine/helengine.editor.tests/managers/project/SceneComponentPackagingTransformServiceTests.cs engine/helengine.editor.tests/managers/project/ScriptComponentPlayerDeserializerGeneratorTests.cs engine/helengine.editor.tests/serialization/scene/AutomaticScriptComponentPersistenceDescriptorTests.cs
rtk git commit -m "Add selective CPU model companions"
```

---

### Task 2: Add the validated texture-region contract

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\managers\rendering\RenderManager2D.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core.tests\assets\font\FontAssetBinarySerializerTests.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\testing\TestRenderManager2D.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.core.tests\managers\rendering\RenderManager2DTextureRegionTests.cs`

**Interfaces:**
- Produces: `RenderManager2D.UpdateTextureRegion(RuntimeTexture, int, int, int, int, byte[], int)`.
- Produces: protected backend hook `UpdateTextureRegionCore(...)` receiving validated arguments.
- Consumed by: `SoftwarePathTracerComponent` after each completed tile.

- [ ] **Step 1: Write failing validation/dispatch tests.** The fake renderer records one call and copied scalar arguments. Cover null/disposed/foreign textures, zero or negative dimensions, negative origin, out-of-bounds rectangles, row pitch below `width * 4`, row pitch not divisible by four, short arrays, and a valid padded-row upload.

```csharp
renderer.UpdateTextureRegion(texture, 3, 5, 8, 4, pixels, 40);
Assert.Equal((3, 5, 8, 4, 40), renderer.LastRegion);
Assert.Same(pixels, renderer.LastPixels);
```

Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.core.tests\helengine.core.tests.csproj --filter FullyQualifiedName~RenderManager2DTextureRegionTests
```

Expected: FAIL because the method is absent.

- [ ] **Step 2: Implement common validation and the backend hook.** Add:

```csharp
public void UpdateTextureRegion(
    RuntimeTexture texture,
    int x,
    int y,
    int width,
    int height,
    [NativeNoEscape] byte[] rgba8,
    int sourceRowPitch) {
    if (texture == null) throw new ArgumentNullException(nameof(texture));
    if (texture.IsDisposed) throw new ObjectDisposedException(nameof(texture));
    if (rgba8 == null) throw new ArgumentNullException(nameof(rgba8));
    if (x < 0 || y < 0 || width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width));
    if (x + width > texture.Width || y + height > texture.Height) throw new ArgumentException("Texture update region exceeds the destination texture.");
    int requiredRowBytes = checked(width * 4);
    if (sourceRowPitch < requiredRowBytes) throw new ArgumentException("Source row pitch is smaller than one RGBA8 row.", nameof(sourceRowPitch));
    if ((sourceRowPitch & 3) != 0) throw new ArgumentException("RGBA8 source row pitch must be divisible by four.", nameof(sourceRowPitch));
    int requiredBytes = checked(sourceRowPitch * (height - 1) + requiredRowBytes);
    if (rgba8.Length < requiredBytes) throw new ArgumentException("Source pixel data is too short for the requested region.", nameof(rgba8));
    UpdateTextureRegionCore(texture, x, y, width, height, rgba8, sourceRowPitch);
}

protected abstract void UpdateTextureRegionCore(
    RuntimeTexture texture, int x, int y, int width, int height,
    [NativeNoEscape] byte[] rgba8, int sourceRowPitch);
```

Backend implementations additionally reject a `RuntimeTexture` created by another backend. Update every existing test renderer with a minimal override.

- [ ] **Step 3: Run core tests and code generation.** Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.core.tests\helengine.core.tests.csproj --filter FullyQualifiedName~RenderManager2DTextureRegionTests
rtk dotnet build C:\dev\helworks\helengine\engine\helengine.core\helengine.core.csproj
```

Expected: PASS/build succeeds.

- [ ] **Step 4: Commit.**

```powershell
rtk git add -- engine/helengine.core/managers/rendering/RenderManager2D.cs engine/helengine.core.tests/assets/font/FontAssetBinarySerializerTests.cs engine/helengine.core.tests/managers/rendering/RenderManager2DTextureRegionTests.cs engine/helengine.editor.tests/testing/TestRenderManager2D.cs
rtk git commit -m "Add validated runtime texture region updates"
```

---

### Task 3: Implement Direct3D 11 and Vulkan region uploads

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.directx11\DirectX11Renderer2D.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.vulkan\VulkanRenderer2D.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\rendering\RuntimeTextureRegionUploadTests.cs`
- Modify: `C:\dev\helworks\helengine-windows\src\platform\windows\win32\win32_render_bridge.hpp`
- Modify: `C:\dev\helworks\helengine-windows\src\platform\windows\win32\win32_render_bridge.cpp`
- Modify: `C:\dev\helworks\helengine-windows\builder.tests\Win32RenderBridgeSourceTests.cs`

**Interfaces:**
- Consumes: validated `UpdateTextureRegionCore` hook.
- Produces: byte-exact desktop region upload used by the Windows reference scene and editor-hosted tests.

- [ ] **Step 1: Write a failing byte-exact GPU readback test.** Create a 4x4 opaque-black `TextureAsset`, update a 2x2 region at `(1,1)` with four distinct RGBA colors and padded source rows, copy/read back the resource, and assert untouched texels stay black.

- [ ] **Step 2: Implement Direct3D 11.** Type-check `DirectX11TextureResource`, pin `rgba8`, create a `DataBox` using `sourceRowPitch`, and call `Device.ImmediateContext.UpdateSubresource` with a `ResourceRegion(x, y, 0, x + width, y + height, 1)`. Do not recreate the shader-resource view.

- [ ] **Step 3: Implement Vulkan.** Type-check `VulkanTextureResource`; allocate/reuse a host-visible staging allocation sized to the validated source bytes, copy rows, transition the destination subresource from shader-read to transfer-destination, issue `CmdCopyBufferToImage` with the region offset/extent and row length derived from `sourceRowPitch / 4`, then transition back before the next draw. Fence or reuse staging memory only after completion.

- [ ] **Step 4: Implement the packaged native Windows bridge.** Override the generated hook in `Win32RenderManager2D`, resolve the existing texture resource from its runtime-texture map, pin/read the generated `Array<uint8_t>`, and call `ID3D11DeviceContext::UpdateSubresource` with a `D3D11_BOX` covering only the requested rectangle and `sourceRowPitch`. Keep the existing `RuntimeTexture` and shader-resource view.

- [ ] **Step 5: Run desktop tests.** Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\helengine.editor.windows.tests.csproj --filter FullyQualifiedName~RuntimeTextureRegionUploadTests
rtk dotnet test C:\dev\helworks\helengine-windows\builder.tests\helengine.windows.builder.tests.csproj --filter "FullyQualifiedName~Win32RenderBridgeSourceTests|FullyQualifiedName~TextureRegion"
```

Expected: PASS for available Direct3D 11 and Vulkan theories; an unavailable Vulkan runtime may be skipped only by the repository's existing capability guard.

- [ ] **Step 6: Commit.** Commit the C# backends in `helengine`, then commit the bridge/tests separately in `helengine-windows`.

```powershell
rtk git add -- engine/helengine.directx11/DirectX11Renderer2D.cs engine/helengine.vulkan/VulkanRenderer2D.cs engine/helengine.editor.windows.tests/rendering/RuntimeTextureRegionUploadTests.cs
rtk git commit -m "Implement desktop texture region uploads"
```

In `C:\dev\helworks\helengine-windows`:

```powershell
rtk git add -- src/platform/windows/win32/win32_render_bridge.hpp src/platform/windows/win32/win32_render_bridge.cpp builder.tests/Win32RenderBridgeSourceTests.cs
rtk git commit -m "Implement packaged Windows texture region uploads"
```

---

### Task 4: Implement Nintendo handheld region uploads

**Files:**
- Modify: `C:\dev\helworks\helengine-ds\src\platform\ds\NintendoDsRenderManager2D.hpp`
- Modify: `C:\dev\helworks\helengine-ds\src\platform\ds\NintendoDsRenderManager2D.cpp`
- Modify: `C:\dev\helworks\helengine-ds\builder.tests\NintendoDsGeneratedCoreStagerTests.cs`
- Modify: `C:\dev\helworks\helengine-3ds\src\platform\3ds\Nintendo3DsStartupRenderManager2D.hpp`
- Modify: `C:\dev\helworks\helengine-3ds\src\platform\3ds\Nintendo3DsStartupRenderManager2D.cpp`
- Modify: `C:\dev\helworks\helengine-3ds\builder.tests\Nintendo3DsStartupRenderManager2DSourceAuditTests.cs`

**Interfaces:**
- Consumes: generated `UpdateTextureRegionCore` virtual.
- Produces: DS 256x192 and 3DS 320x240 progressive texture updates.

- [ ] **Step 1: Add failing source-audit tests.** Require each class to override the generated hook, validate its native runtime-texture type, walk `height` source rows using `sourceRowPitch`, and flush/invalidate the platform cache required before drawing.
- [ ] **Step 2: Implement DS updates.** Convert RGBA8 rows into the existing DS runtime texture's native color/index representation using the same packing path as `BuildTextureFromRaw`; update only destination texels inside the rectangle and synchronize VRAM at the backend's normal safe point. Do not allocate a full-screen temporary buffer.
- [ ] **Step 3: Implement 3DS updates.** Copy/convert the rectangle into the existing `Nintendo3DsRuntimeTexture` backing allocation, respecting its tiled/swizzled layout, then perform the required data-cache flush before Citro3D samples it. The upload extent is 320x240 even though the top screen is 400 pixels wide.
- [ ] **Step 4: Run tests.** Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine-ds\builder.tests\helengine.ds.builder.tests.csproj --filter "FullyQualifiedName~TextureRegion|FullyQualifiedName~GeneratedCoreStager"
rtk dotnet test C:\dev\helworks\helengine-3ds\builder.tests\helengine.3ds.builder.tests.csproj --filter "FullyQualifiedName~TextureRegion|FullyQualifiedName~StartupRenderManager2D"
```

Expected: PASS.

- [ ] **Step 5: Commit separately in each repository.** Use `Implement DS texture region updates` and `Implement 3DS texture region updates` commit messages.

---

### Task 5: Implement Nintendo home-console region uploads

**Files:**
- Modify: `C:\dev\helworks\helengine-gc\src\platform\gamecube\GameCubeRenderManager2D.hpp`
- Modify: `C:\dev\helworks\helengine-gc\src\platform\gamecube\GameCubeRenderManager2D.cpp`
- Modify: `C:\dev\helworks\helengine-gc\builder.tests\GameCubePackagedRuntimeSourceTests.cs`
- Modify: `C:\dev\helworks\helengine-wii\src\platform\wii\WiiRenderManager2D.hpp`
- Modify: `C:\dev\helworks\helengine-wii\src\platform\wii\WiiRenderManager2D.cpp`
- Modify: `C:\dev\helworks\helengine-wii\builder.tests\WiiRuntimeSourceTests.cs`
- Modify: `C:\dev\helworks\helengine-wiiu\src\platform\wiiu\WiiURenderManager2D.hpp`
- Modify: `C:\dev\helworks\helengine-wiiu\src\platform\wiiu\WiiURenderManager2D.cpp`
- Modify: `C:\dev\helworks\helengine-wiiu\builder.tests\WiiURuntimeSourceTests.cs`

**Interfaces:**
- Consumes: generated region hook.
- Produces: in-place tiled texture updates on GameCube, Wii, and Wii U.

- [ ] **Step 1: Add failing source-contract tests.** Require bounds-assured row traversal, existing texture reuse, platform tiling conversion, and cache invalidation (`DCFlushRange`/GX2 invalidate equivalent).
- [ ] **Step 2: Implement GameCube and Wii.** Use the existing RGBA-to-GX texture packing routines from `BuildTextureFromRaw`, write the affected tiles in the runtime texture allocation, flush only the touched allocation range when safe, and keep the existing `GXTexObj` binding.
- [ ] **Step 3: Implement Wii U.** Write through the runtime texture's CPU-visible surface allocation, use the existing GX2 surface swizzle helpers for the rectangle, and invalidate the texture resource before sampling. Do not allocate a second 320x240 surface.
- [ ] **Step 4: Run tests.** Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine-gc\builder.tests\helengine.gamecube.builder.tests.csproj --filter "FullyQualifiedName~TextureRegion|FullyQualifiedName~PackagedRuntimeSource"
rtk dotnet test C:\dev\helworks\helengine-wii\builder.tests\helengine.wii.builder.tests.csproj --filter "FullyQualifiedName~TextureRegion|FullyQualifiedName~RuntimeSource"
rtk dotnet test C:\dev\helworks\helengine-wiiu\builder.tests\helengine.wiiu.builder.tests.csproj --filter "FullyQualifiedName~TextureRegion|FullyQualifiedName~RuntimeSource"
```

Expected: PASS.

- [ ] **Step 5: Commit separately in each platform repository.**

---

### Task 6: Implement Sony and Switch region uploads

**Files:**
- Modify: `C:\dev\helworks\helengine-ps2\src\platform\ps2\Ps2BootHost.cpp`
- Create: `C:\dev\helworks\helengine-ps2\builder.tests\Ps2RenderManager2DSourceAuditTests.cs`
- Modify: `C:\dev\helworks\helengine-psp\src\platform\psp\rendering\PspRenderManager2D.hpp`
- Modify: `C:\dev\helworks\helengine-psp\src\platform\psp\rendering\PspRenderManager2D.cpp`
- Modify: `C:\dev\helworks\helengine-psp\src\platform\psp\rendering\PspTextureCache.hpp`
- Modify: `C:\dev\helworks\helengine-psp\src\platform\psp\rendering\PspTextureCache.cpp`
- Modify: `C:\dev\helworks\helengine-psp\builder.tests\PspPackagedRuntimeSourceTests.cs`
- Modify: `C:\dev\helworks\helengine-psvita\src\platform\psvita\rendering\PsVitaRenderManager2D.hpp`
- Modify: `C:\dev\helworks\helengine-psvita\src\platform\psvita\rendering\PsVitaRenderManager2D.cpp`
- Modify: `C:\dev\helworks\helengine-psvita\src\platform\psvita\rendering\PsVitaTextureCache.hpp`
- Modify: `C:\dev\helworks\helengine-psvita\src\platform\psvita\rendering\PsVitaTextureCache.cpp`
- Modify: `C:\dev\helworks\helengine-psvita\builder.tests\PsVitaRenderManager2DSourceAuditTests.cs`
- Modify: `C:\dev\helworks\helengine-switch\src\platform\switch\SwitchRenderManager2D.hpp`
- Modify: `C:\dev\helworks\helengine-switch\src\platform\switch\SwitchRenderManager2D.cpp`
- Create: `C:\dev\helworks\helengine-switch\builder.tests\SwitchRenderManager2DSourceAuditTests.cs`

**Interfaces:**
- Consumes: generated region hook.
- Produces: existing-texture updates on PS2, PSP, PS Vita, and Switch.

- [ ] **Step 1: Add failing source-audit tests** for the override, runtime-type check, row-pitch handling, in-place storage mutation, and cache synchronization on each target.
- [ ] **Step 2: Implement PS2.** Update the host-owned CPU texture payload for the rectangle and enqueue/reupload the affected texture through the existing GS texture path at a safe render boundary. Reuse the runtime texture and its allocation.
- [ ] **Step 3: Implement PSP and PS Vita.** Route the renderer hook into each texture cache, pack RGBA8 using the existing build path, mutate only touched rows/tiles, and flush the platform data cache before the next draw.
- [ ] **Step 4: Implement Switch.** Replace the current metadata-only raw-texture stub as narrowly as necessary so a `SwitchRuntimeTexture2D` retains its sampled allocation and can accept the rectangle. Do not add any tracing logic or a software frame buffer to the backend.
- [ ] **Step 5: Run tests.** Run:

```powershell
rtk dotnet test C:\dev\helworks\helengine-ps2\builder.tests\helengine.ps2.builder.tests.csproj --filter "FullyQualifiedName~TextureRegion|FullyQualifiedName~RuntimeSource"
rtk dotnet test C:\dev\helworks\helengine-psp\builder.tests\helengine.psp.builder.tests.csproj --filter "FullyQualifiedName~TextureRegion|FullyQualifiedName~PackagedRuntimeSource"
rtk dotnet test C:\dev\helworks\helengine-psvita\builder.tests\helengine.psvita.builder.tests.csproj --filter "FullyQualifiedName~TextureRegion|FullyQualifiedName~RenderManager2D"
rtk dotnet test C:\dev\helworks\helengine-switch\builder.tests\helengine.switch.builder.tests.csproj --filter FullyQualifiedName~TextureRegion
```

Expected: PASS.

- [ ] **Step 6: Commit separately in each platform repository.**

---

### Task 7: Regenerate core outputs and verify the complete seam matrix

**Files:**
- Modify only generated-core snapshots explicitly tracked by the affected platform repositories.
- Do not commit build caches under `C:\dev\helworks\builds`.

**Interfaces:**
- Consumes: Tasks 1-6.
- Produces: matching C# and generated C++ method signatures for every target.

- [ ] **Step 1: Run the repository's generated-core regeneration command** for every external platform and inspect the diff. Confirm the generated base declares `UpdateTextureRegion` plus the core hook once and every native derived class compiles against it.
- [ ] **Step 2: Run the full focused matrix.** Run all commands from Tasks 1-6 without filters that would omit neighboring runtime-source tests.
- [ ] **Step 3: Compile one minimal artifact per target.** Use `artifacts/build-platform.ps1` with DemoDisc only after the DemoDisc plans are implemented; at this checkpoint, run each platform repository's builder/runtime compile target documented in its README.
- [ ] **Step 4: Verify selective companion output.** Package a probe scene containing one normal `MeshComponent` cube and one marked reference to the same cube. Assert the package contains one ordinary platform model payload plus one generic `cooked/cpu-models/engine/cube.hasset`, and deserializing the latter requires no renderer.
- [ ] **Step 5: Record commit hashes in the master plan execution notes.** Do not squash cross-repository commits until all downstream DemoDisc builds pass.
