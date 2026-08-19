# Main Menu Footer Marquee Exit Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the main-menu footer restart only after its platform-specific runtime text has fully scrolled off-screen.

**Architecture:** `FooterIdentityMarqueeComponent` will cache the runtime text's tight glyph width in reference-canvas pixels after its font becomes available. Its existing canvas scale and speed calculations will use that cached width for the exit test. The standard-menu factory will stop serializing a stale fixed text width.

**Tech Stack:** C#, helengine scene components, xUnit source-contract tests.

## Global Constraints

- Preserve the footer's existing start coordinate and `70f` pixels-per-second speed.
- Use `FooterTextComponent.Font.MeasureTight(FooterTextComponent.Text).Width` multiplied by `FooterTextComponent.FontScale`.
- Do not advance before the runtime text font has been resolved and measured.
- Retain the existing `ReferenceCanvasFitComponent` horizontal scaling.

---

### Task 1: Pin the runtime-width marquee contract

**Files:**
- Modify: `assets/codebase/menu.tools.tests/DemoDiscMenuButtonTextStyleSourceTests.cs:Footer_marquee_scales_its_runtime_geometry_to_the_viewport`

**Interfaces:**
- Consumes: `FooterIdentityMarqueeComponent.Update()`.
- Produces: A source-contract test that fails until the component uses measured runtime text width and the factory stops assigning `TextWidth`.

- [ ] **Step 1: Write the failing test**

Replace the stale fixed-width assertions in `Footer_marquee_scales_its_runtime_geometry_to_the_viewport` with:

```csharp
Assert.Contains("FooterTextComponent.Font.MeasureTight(FooterTextComponent.Text).Width", marqueeComponentSource, StringComparison.Ordinal);
Assert.Contains("FooterTextComponent.FontScale", marqueeComponentSource, StringComparison.Ordinal);
Assert.Contains("MeasuredTextWidth", marqueeComponentSource, StringComparison.Ordinal);
Assert.Contains("float textWidth = MeasuredTextWidth * canvasScale.X;", marqueeComponentSource, StringComparison.Ordinal);
Assert.DoesNotContain("public float TextWidth", marqueeComponentSource, StringComparison.Ordinal);
Assert.DoesNotContain("TextWidth = 420f", standardFactorySource, StringComparison.Ordinal);
```

- [ ] **Step 2: Run test to verify it fails**

Run `rtk dotnet test user_settings/generated_code/projects/menu.tools.tests/menu.tools.tests.csproj --no-restore --filter FullyQualifiedName~Footer_marquee_scales_its_runtime_geometry_to_the_viewport`.

Expected: FAIL because the current implementation still declares and uses `TextWidth`.

- [ ] **Step 3: Write minimal implementation**

In `FooterIdentityMarqueeComponent.cs`, replace the `TextWidth` property with a private `MeasuredTextWidth` field initialized to `0f`. After setting `FooterTextComponent.Text` in `ResolveTextEntityWhenNeeded`, leave it at zero. In `Update`, return until `EnsureMeasuredTextWidth()` succeeds. That helper must return false while `FooterTextComponent.Font` is null, otherwise set:

```csharp
MeasuredTextWidth = FooterTextComponent.Font.MeasureTight(FooterTextComponent.Text).Width
    * FooterTextComponent.FontScale;
```

Use `MeasuredTextWidth * canvasScale.X` for the existing complete-line exit condition. Remove `TextWidth = 420f` from `DemoDiscStandardMainMenuSceneFactory.cs`.

- [ ] **Step 4: Run test to verify it passes**

Run the Step 2 command again. Expected: PASS.

- [ ] **Step 5: Run the menu test project**

Run `rtk dotnet test user_settings/generated_code/projects/menu.tools.tests/menu.tools.tests.csproj --no-restore`.

Expected: PASS without introducing new failures.

- [ ] **Step 6: Commit**

Stage only the component, factory, source-contract test, and this plan, then commit with message `Fix footer marquee exit timing`.
