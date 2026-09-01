# Software Path Tracer Platform Presentation Implementation Plan

> **Worker:** Implement with `superpowers:test-driven-development`. Keep all feature-specific authoring in DemoDisc; do not add engine utilities.

**Goal:** Replace the Task 1 placeholder presentation with one generated graph that resolves to a full-resolution DS top image, a centered monoscopic 3DS top image, and a 320x240 image with a small overlay everywhere else.

**Architecture:** Keep one output sprite and one tracer controller. Author separate desktop and handheld HUD trees with mutually exclusive entity-existence overrides. Persist `SoftwarePathTracerComponent` property overrides for `ds` and `3ds` so its three HUD references resolve to the handheld text entities after the desktop HUD is removed. Bind top content to the presentation camera; bind handheld HUD content to a second bottom-screen camera using the existing Nintendo DS viewport convention.

**Files:**

- Modify: `assets/codebase/rendering.tools/SoftwarePathTracerSceneFactory.cs`
- Modify: `assets/codebase/rendering.tools.tests/SoftwarePathTracerSceneFactoryTests.cs`
- Modify only if a shared convention needs a source contract: `assets/codebase/rendering.tools.tests/NintendoDsRenderingSceneScaffoldSourceTests.cs`

## Pinned behavior

- DS trace texture and sprite: exactly `256x192`; top screen remains unobstructed.
- 3DS trace texture and sprite: exactly `320x240`; output X is `40` on the `400x240` top canvas; one output sprite and one camera only, with no stereo/eye component or duplicated image.
- Every other supported platform: exactly `320x240`; the presentation viewport scales/centers that reference canvas without changing trace resolution.
- Desktop/non-handheld HUD: one translucent `RoundedRectComponent` panel containing SPP, elapsed time, rays/s, and a pointer-enabled Return button.
- DS/3DS HUD: the same three statistics plus Return on the bottom screen. Use `CameraComponent.Viewport = new float4(0f, 1f, 1f, 1f)`, an ancestor-camera `ViewportComponent`, `InteractableComponent`, and `NintendoDsReturnOverlayComponent`.
- No light controls, camera controls, pause, reset, save, exposure, or resolution controls.
- The output sprite remains texture-null at authoring time; `SoftwarePathTracerComponent` assigns the runtime texture.
- `SoftwarePathTracerComponent` remains the sole keyboard/gamepad Return poller on non-handheld targets. The desktop pointer component must be `DemoDiscReturnToMenuComponent { AllowKeyboardReturn = false, AllowGamepadReturn = false, AllowPointerReturn = true }`.
- On DS/3DS, `SoftwarePathTracerComponent.ShouldPollControllerReturn` remains false and `NintendoDsReturnOverlayComponent` owns touch/handheld Return.

## Task 1: Prove the platform graph before implementation

- [ ] Extend `SoftwarePathTracerSceneFactoryTests` with helpers that inspect common state plus persisted platform existence, transform, and component overrides.
- [ ] Assert the common output sprite is `320x240`, its texture is null, and its presentation viewport uses a `320x240` reference canvas.
- [ ] Assert the DS sprite override is `256x192` and the DS top canvas is `256x192`.
- [ ] Assert the 3DS sprite remains `320x240`, its platform transform is X=`40`, and the top canvas is `400x240`.
- [ ] Assert exactly one output sprite exists in the authored graph and no stereo/left-eye/right-eye presentation entity or component is introduced.
- [ ] Assert desktop and handheld HUD roots are mutually exclusive for each of the ten supported platform IDs.
- [ ] Resolve the controller override for `ds` and `3ds` with `ComponentPlatformEditingService`; assert all three effective HUD references are nonzero, distinct, and point to the handheld text entities. Assert the common controller still points to the desktop text entities.
- [ ] Assert desktop Return allows pointer only, handheld Return uses `NintendoDsReturnOverlayComponent`, and the scene contains no light/camera-control component.
- [ ] Run the focused test filter and confirm the new tests fail for missing presentation behavior, not setup errors.

## Task 2: Author the common top presentation

- [ ] Add one presentation viewport root beneath the existing presentation camera. Use `ViewportComponent.AncestorCameraBindingMode`, `ReferenceCanvasScalingMode`, common `FixedSize/ReferenceWidth/ReferenceHeight = 320x240`.
- [ ] Reparent the single `SoftwarePathTracerOutput` sprite beneath that viewport. Keep common position `(0,0,0)`, size `320x240`, opaque white color, and no authored texture reference.
- [ ] Persist a DS `SpriteComponent.Size` override of `256x192` through `ComponentPlatformEditingService`.
- [ ] Persist DS viewport dimensions `256x192` and 3DS viewport dimensions/reference canvas `400x240` through `ComponentPlatformEditingService`.
- [ ] Persist a 3DS local-position override `(40,0,0)` on the output entity using the existing entity transform-override save state/API. Do not add a second camera, sprite, texture, or eye-specific code.

## Task 3: Author mutually exclusive HUD trees

- [ ] Replace the three placeholder root text entities with a `SoftwarePathTracerDesktopHudRoot` subtree: translucent panel, three text rows, and one Return button.
- [ ] Put `Exists=false` overrides for `ds` and `3ds` on the desktop HUD root.
- [ ] Add `SoftwarePathTracerBottomScreenCamera` with draw order `1`, bottom-screen viewport `(0,1,1,1)`, the established lilac clear color, disabled depth/shadows/post-processing, and a child ancestor-camera reference viewport using the DS `256x192` canvas convention.
- [ ] Under that bottom viewport, add `SoftwarePathTracerHandheldHudRoot` with three separate saved text entities and a visible Return button. Reuse the existing DS palette-safe rounded body/border approach locally in this factory; do not add a generic engine helper or light button.
- [ ] Restrict the bottom-camera subtree to `ds` and `3ds` by setting `Exists=false` for the other eight supported IDs: `windows`, `gamecube`, `ps2`, `psp`, `psvita`, `wii`, `wiiu`, `switch`.
- [ ] Persist the HUD font reference on all six text rows and the Return label.

## Task 4: Wire platform-effective controller references

- [ ] Keep the common controller references pointed at the desktop SPP/time/rays entities.
- [ ] Create `ds` and `3ds` `SoftwarePathTracerComponent` override snapshots with only `SppTextEntityReference`, `ElapsedTextEntityReference`, and `RaysPerSecondTextEntityReference` marked as overridden; keep the common output reference and camera/exposure values inherited.
- [ ] Point both handheld override payloads to the three handheld text IDs and persist them through `ComponentPlatformEditingService`.
- [ ] Verify the effective components with `ResolveEditableComponent` in tests. Do not weaken runtime reference validation or add runtime HUD lookup utilities.

## Task 5: Verify and commit

- [ ] Run:

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/rendering.tools.tests/rendering.tools.tests.csproj --no-restore --filter "FullyQualifiedName~SoftwarePathTracerSceneFactory|FullyQualifiedName~SoftwarePathTracerPresentation" -v:minimal
rtk dotnet build user_settings/generated_code/editor-command/EditorFull/projects/rendering.tools/rendering.tools.csproj --no-restore -v:minimal
rtk dotnet build user_settings/generated_code/editor-command/EditorFull/projects/gameplay/gameplay.csproj --no-restore -v:minimal
rtk git diff --check
```

- [ ] Inspect the diff for DemoDisc-only ownership, one output sprite, persisted font/reference overrides, and no unrelated scaffold changes.
- [ ] Commit as `Add software path tracer presentation layouts`.

