# Splash Input Gate Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Prevent Demo Disc menu navigation until the Helen of Code splash finishes.

**Architecture:** A menu-local static gate is acquired by the splash and released immediately before its scene unload. The menu continues loading and rendering, but skips keyboard, mouse, and gamepad routing while the gate is held.

**Tech Stack:** C#, xUnit, Demo Disc runtime components.

---

### Task 1: Add the boot-input state boundary

**Files:**
- Create: `C:/dev/helprojs/demodisc/assets/codebase/menu/StartupInputGate.cs`
- Create: `C:/dev/helprojs/demodisc/assets/codebase/menu.tools.tests/StartupInputGateTests.cs`

- [ ] Write this failing test:

```csharp
[Fact]
public void Release_after_acquire_allows_menu_input() {
    StartupInputGate.Acquire();
    Assert.True(StartupInputGate.IsBlocked);
    StartupInputGate.Release();
    Assert.False(StartupInputGate.IsBlocked);
}
```

- [ ] Run `rtk dotnet test C:\dev\helprojs\demodisc\user_settings\generated_code\projects\menu.tools.tests\menu.tools.tests.csproj --no-restore --filter "FullyQualifiedName~StartupInputGateTests"` and observe the expected missing-type failure.
- [ ] Implement `StartupInputGate` with `public static bool IsBlocked { get; private set; }`, plus `Acquire()` setting it to `true` and `Release()` setting it to `false`. Give the class and all members substantive XML comments.
- [ ] Re-run the focused test and expect one passing test.

### Task 2: Wire splash ownership into menu routing

**Files:**
- Modify: `C:/dev/helprojs/demodisc/assets/codebase/menu/HelenOfCodeSplashComponent.cs`
- Modify: `C:/dev/helprojs/demodisc/assets/codebase/menu/MenuComponent.cs`
- Modify: `C:/dev/helprojs/demodisc/assets/codebase/menu.tools.tests/HelenOfCodeSplashComponentTests.cs`

- [ ] Add failing assertions that the splash source contains `StartupInputGate.Acquire()` and `StartupInputGate.Release()`; run `HelenOfCodeSplashComponentTests` and observe the assertions fail.
- [ ] Add `StartupInputGate.Acquire();` in `HelenOfCodeSplashComponent.ComponentAdded` after `base.ComponentAdded(entity);`.
- [ ] Add `StartupInputGate.Release();` directly before `Core.Instance.SceneManager.UnloadScene(SplashSceneId);`.
- [ ] In `MenuComponent.Update`, preserve initialization, then add this guard before retrieving `Core.Instance.Input`:

```csharp
if (StartupInputGate.IsBlocked) {
    return;
}
```

- [ ] Run `rtk dotnet test C:\dev\helprojs\demodisc\user_settings\generated_code\projects\menu.tools.tests\menu.tools.tests.csproj --no-restore --filter "FullyQualifiedName~StartupInputGateTests|FullyQualifiedName~HelenOfCodeSplashComponentTests"` and expect all selected tests to pass.

### Task 3: Regenerate and package

**Files:**
- Generated: `C:/dev/helprojs/demodisc/assets/scenes/HelenOfCodeSplash.helen`
- Generated: `C:/dev/helprojs/demodisc/output/windows/helengine_windows.exe`

- [ ] Run `rtk dotnet test C:\dev\helprojs\demodisc\user_settings\generated_code\projects\menu.tools.tests\menu.tools.tests.csproj --no-restore --filter "FullyQualifiedName~HelenOfCodeSplash"` and expect all splash tests to pass.
- [ ] Run `rtk powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine\scripts\build-platform.ps1 -Project C:\dev\helprojs\demodisc\project.heproj -Platform windows -Output C:\dev\helprojs\demodisc\output\windows` and confirm a fresh executable is written.
- [ ] Stage only these implementation/test files and commit with `feat: gate menu input during boot splash`; do not stage unrelated worktree changes.
