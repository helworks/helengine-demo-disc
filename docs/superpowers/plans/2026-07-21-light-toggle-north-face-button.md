# Light Toggle North Face Button Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the light toggle respond to the controller North face button on all platforms while preserving keyboard and touch activation.

**Architecture:** Keep the existing light-cycle components and change only their abstract gamepad button query. Add source-level regression assertions beside the existing rendering scaffold tests because the behavior is authored directly in the two C# components and does not require generated asset changes.

**Tech Stack:** C#, .NET test project, xUnit, existing DemoDisc input abstractions.

---

### Task 1: Add regression coverage for the face-button binding

**Files:**
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\rendering.tools.tests\NintendoDsRenderingSceneScaffoldSourceTests.cs`
- Test: the two light-toggle component source files

- [ ] **Step 1: Add a test that reads both light-toggle sources and asserts the North binding.**

Add a test method that reads `DemoDiscLightToggleComponent.cs` and `NintendoDsLightToggleOverlayComponent.cs`, asserts each contains `InputGamepadButton.North`, and asserts neither contains `InputGamepadButton.RightShoulder`.

- [ ] **Step 2: Run the focused test project and verify the new test fails before implementation.**

Run the repository's existing test command for the project containing `NintendoDsRenderingSceneScaffoldSourceTests`; expect failure because both components currently query `RightShoulder`.

### Task 2: Change the shared and handheld light-toggle bindings

**Files:**
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\rendering\DemoDiscLightToggleComponent.cs:309`
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\rendering\NintendoDsLightToggleOverlayComponent.cs:260`

- [ ] **Step 1: Replace only the abstract gamepad button enum value.**

Change each existing expression from:

```csharp
InputGamepadButton.RightShoulder
```

to:

```csharp
InputGamepadButton.North
```

Leave `inputSystem.WasKeyPressed(Keys.L)` and the Nintendo DS pointer-event path unchanged.

- [ ] **Step 2: Run the focused test project and verify it passes.**

Run the same focused test command and confirm the new binding test and existing tests pass.

- [ ] **Step 3: Confirm the diff is limited to requested source and test files.**

Run `git diff --stat` and `git diff --check`; verify no generated scenes, blueprints, or unrelated existing modifications were changed by this task.

- [ ] **Step 4: Commit only the task files.**

Stage the two light-toggle components, the focused test file, and the design/plan documents; commit with message `Change light toggle to North face button`.
