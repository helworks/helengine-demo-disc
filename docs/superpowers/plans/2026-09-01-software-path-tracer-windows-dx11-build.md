# Software Path Tracer Windows DX11 Build and Smoke Plan

> **Worker:** Execute after the shared scene, catalog, and build configuration are accepted. Use `superpowers:systematic-debugging` for any failure and `superpowers:verification-before-completion` before claiming success.

**Goal:** Produce and smoke-test a fresh DemoDisc Windows artifact using the existing Windows host/builder at `C:\dev\helworks\helengine-windows` and the normal DirectX 11 profile.

**Boundaries:** Do not use Vulkan for this build. Do not create a new native bridge or Windows builder. Do not modify engine/Windows-host code unless a reproducible failing test proves a defect in that owner. DemoDisc remains the owner of tracer code.

## Task 1: Verify the actual builder and profile before building

- [ ] Assert `C:\dev\helworks\helengine\user_settings\platforms.json` resolves Windows `builderAssemblyPath` to `C:\dev\helworks\helengine-windows\builder\bin\Debug\net9.0\helengine.windows.builder.dll` and `playerSourceRootPath` to `C:\dev\helworks\helengine-windows`.
- [ ] Build the existing Windows builder project once for this local-source run, passing `-p:HelEngineRoot=C:\dev\helprojs\.worktrees\helengine-software-path-tracer-engine-seams`, and require a fresh DLL. Builder-source timestamps alone are insufficient because the `HelEngineRoot` dependency changed; do not substitute another repository.
- [ ] Compare DemoDisc `requiredEngineVersion` with the shared platform entry before building. When they differ, do not mutate the shared installation registry or weaken exact-version filtering. Create an uncommitted, Windows-only build manifest at `user_settings/windows-dx11-build-platforms/platforms.json` whose entry uses DemoDisc's existing required version and absolute paths to the verified Windows builder, player source, generated-core output, and codegen tool. This is isolated build input, not a project asset.
- [ ] Parse DemoDisc `user_settings/build_config.json` and require the Windows block's `selectedGraphicsProfileId` to be `directx11` and selected build profile to be `release`.
- [ ] Ensure `HELENGINE_RENDER_BACKEND` and `HELENGINE_ENABLE_EXPERIMENTAL_VULKAN` are not set to the explicit Vulkan opt-in pair for the build process. Preserve the gated Vulkan code; simply do not select it.
- [ ] Verify the accepted generated `software_path_tracer.helen` and regenerated DemoDisc main-menu asset exist before packaging.

## Task 2: Run a fresh Windows package build

- [ ] Use the existing engine wrapper and Windows builder with the isolated worktree project:

```powershell
$env:HELENGINE_ENGINE_USER_SETTINGS_ROOT = "C:\dev\helprojs\demodisc\.worktrees\software-path-tracer-core\user_settings\windows-dx11-build-platforms"
rtk dotnet run --project C:\dev\helprojs\.worktrees\helengine-software-path-tracer-engine-seams\tools\build-waiter\helengine.buildwaiter.csproj -- \
  --output C:\dev\helprojs\demodisc\.worktrees\software-path-tracer-core\windows-build \
  --require helengine_windows.exe \
  -- powershell -NoProfile -ExecutionPolicy Bypass -File C:\dev\helprojs\.worktrees\helengine-software-path-tracer-engine-seams\scripts\build-platform.ps1 \
  -Project C:\dev\helprojs\demodisc\.worktrees\software-path-tracer-core\project.heproj \
  -Platform windows \
  -Configuration Release \
  -BuildProfile release \
  -Output C:\dev\helprojs\demodisc\.worktrees\software-path-tracer-core\windows-build
```

The wrapper and editor must come from the accepted engine-seams worktree so `HELENGINE_SOURCE_ROOT` points native code generation at the CPU-readable model, runtime deserializer, and texture-region upload changes. The process-scoped `HELENGINE_ENGINE_USER_SETTINGS_ROOT` override points platform discovery at the isolated Windows-only manifest, whose absolute payload paths still select `C:\dev\helworks\helengine-windows` and the installed codegen tool. This preserves the shared installation registry and the project's existing engine pin while binding all executable build inputs to the accepted source worktree. Do not launch the wrapper from engine `main`, weaken version matching, or rewrite the shared platform manifest.

- [ ] Require wrapper exit code `0`, a fresh nonempty `windows-build\helengine_windows.exe`, and no `codegen.exe` application-error dialog. If codegen fails, capture the console/build-state evidence and diagnose the first failing stage instead of rerunning blindly.

## Task 3: Audit the package

- [ ] Confirm the package scene map contains logical `software_path_tracer` and the regenerated menu points its new item at that logical ID.
- [ ] Confirm the generated scene contains eight software-model components and no traced `MeshComponent`.
- [ ] Locate the selective CPU-readable engine-cube companion produced for the software references. Require exactly one companion payload for the shared cube and confirm it deserializes/identifies as generic CPU model data.
- [ ] Confirm the software scene did not duplicate an ordinary GPU/runtime model payload for each of the eight instances.
- [ ] Confirm no tracer-specific writable cache/checkpoint/image directory is created in the package.
- [ ] Retain the build terminal state/log paths and artifact timestamp as completion evidence.
- [ ] Do not commit `user_settings/windows-dx11-build-platforms`; retain it only as local build evidence alongside the uncommitted Windows output.

## Task 4: Launch and smoke the DX11 artifact

- [ ] Launch `windows-build\helengine_windows.exe` through `C:\dev\helworks\helengine-windows\scripts\launch_in_emulator.ps1` or directly with a bounded diagnostic session.
- [ ] Navigate Rendering -> Software Path Tracer.
- [ ] Verify progressive tiles appear, completed SPP eventually advances, elapsed time/rays-per-second update, and rendering continues without a completion stop.
- [ ] Verify the image is the Cornell enclosure with two boxes, a ceiling emitter, red left wall, green right wall, and visible indirect color bleeding after sufficient samples.
- [ ] Verify the presentation is one 320x240 CPU-generated texture displayed by DX11; no Vulkan initialization appears in logs.
- [ ] Activate Return, require the main menu to load, then re-enter and require a clean zero-SPP start with no stale output texture.
- [ ] Close the process cleanly and inspect diagnostics for exceptions, leaked tracer ownership, or writable-storage activity.

## Task 5: Final evidence

- [ ] Run the full focused software tracer tests plus the rendering/menu/game integration tests.
- [ ] Record: artifact path/size/timestamp, build exit code, selected `directx11` profile evidence, first-pass time, initialization peak bytes, steady-state bytes, post-Return tracer-owned bytes (zero), and smoke outcome.
- [ ] Commit only source/test fixes that were driven by a reproducible failure. Do not commit `windows-build` artifacts.
