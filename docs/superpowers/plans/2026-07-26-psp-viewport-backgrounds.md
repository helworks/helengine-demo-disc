# PSP Viewport Backgrounds Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Render splash and loading blackout rectangles across the full 480 by 272 PSP viewport without changing the fitted content canvas.

**Architecture:** The factories create each background directly under its overlay camera.  Their runtime components resize those backgrounds from `RenderManager3D.MainWindowSize`; the 16:9 reference-canvas root keeps only content that should letterbox proportionally.

**Tech Stack:** C# generated Demo Disc scenes, xUnit source-contract tests, PSP package builder.

---

### Task 1: Author camera-owned viewport backgrounds

**Files:**
- Modify: `assets/codebase/menu.tools/HelenOfCodeSplashSceneFactory.cs`
- Modify: `assets/codebase/menu.tools/SceneLoadingScreenFactory.cs`
- Modify: `assets/codebase/menu.tools.tests/HelenOfCodeSplashSceneSourceTests.cs`
- Modify: `assets/codebase/menu.tools.tests/SceneLoadingScreenComponentSourceTests.cs`

- [ ] **Step 1: Write failing source-contract assertions**

Assert each factory creates its background from the camera before creating the fitted root, and assert each runtime component assigns `Core.Instance.RenderManager3D.MainWindowSize` to its resolved background rectangle.

- [ ] **Step 2: Run the focused tests and verify they fail**

Run: `dotnet test assets/codebase/menu.tools.tests/menu.tools.tests.csproj --filter "FullyQualifiedName~HelenOfCodeSplashSceneSourceTests|FullyQualifiedName~SceneLoadingScreenComponentSourceTests"`

Expected: FAIL because both backgrounds are currently created under their fitted roots and neither runtime component resizes a background from the live window size.

- [ ] **Step 3: Move only the background factories to the camera hierarchy**

Create each background from the camera before building its reference-canvas root, and resize it from `RenderManager3D.MainWindowSize` after reference resolution.  Preserve stable entity references, black alpha control, logo placement, track placement, and fill behavior.

- [ ] **Step 4: Run focused tests and verify they pass**

Run: `dotnet test assets/codebase/menu.tools.tests/menu.tools.tests.csproj --filter "FullyQualifiedName~HelenOfCodeSplashSceneSourceTests|FullyQualifiedName~SceneLoadingScreenComponentSourceTests"`

Expected: PASS.

- [ ] **Step 5: Regenerate scenes and package PSP**

Run the existing project scene-generation command, then `scripts/build-platform.ps1` for `psp`, and verify that the output `PSP/GAME/HELENGINE/EBOOT.PBP` receives a new timestamp.
