# Main Menu Selected Edge-Light Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Give the selected standard Demo Disc menu button a teal border while idle buttons retain purple borders.

**Architecture:** `DemoDiscStandardMainMenuSceneFactory` already assigns colors consumed by `MenuItemComponent`. Change its selected-border value from `MenuDefinition.AccentColor` to the existing `MenuDefinition.AccentSecondaryColor`; leave the purple selected fill unchanged.

### Task 1: Test and author the edge-light

- [ ] Add `Assert.Contains("byte4 selectedBorderColor = definition.AccentSecondaryColor;", source, StringComparison.Ordinal);` to `assets/codebase/menu.tools.tests/DemoDiscMenuButtonTextStyleSourceTests.cs`.
- [ ] Run `dotnet test C:\dev\helprojs\demodisc\user_settings\generated_code\projects\menu.tools.tests\menu.tools.tests.csproj --filter FullyQualifiedName~DemoDiscMenuButtonTextStyleSourceTests --no-restore`; it should fail while the border uses `AccentColor`.
- [ ] In `assets/codebase/menu.tools/DemoDiscStandardMainMenuSceneFactory.cs`, set `selectedBorderColor` to `definition.AccentSecondaryColor`.
- [ ] Re-run the focused test; it should have zero failures.
- [ ] Rebuild Windows with the editor CLI and verify `output\windows\helengine_windows.exe` is refreshed. Do not change the handheld factory, selection logic, assets, or build outputs.
