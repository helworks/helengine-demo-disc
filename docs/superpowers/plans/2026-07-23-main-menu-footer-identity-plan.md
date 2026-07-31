# Main Menu Footer Identity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add the static `MADE BY HELENA / HELEN OF CODE` signature inside a narrow bottom footer strip in the standard Demo Disc menu.

**Architecture:** The standard menu factory will create an anchored footer-strip entity containing a translucent rounded-rectangle surface and the body-font identity text. The strip uses existing menu colors and reference-canvas dimensions; no runtime state or assets are needed.

### Task 1: Test and author the footer

- [ ] Add a source assertion to `assets/codebase/menu.tools.tests/DemoDiscMenuButtonTextStyleSourceTests.cs` for `MADE BY HELENA / HELEN OF CODE` in `DemoDiscStandardMainMenuSceneFactory.cs`.
- [ ] Run the focused test and confirm the new assertion fails.
- [ ] Add one static footer-strip entity under `CreateMenuRootEntity`, anchored eight pixels above the bottom with a 36-pixel height, one-pixel top and bottom borders, and the identity text inset 32 pixels from the left.
- [ ] Re-run the focused test and confirm it passes.
- [ ] Regenerate the standard menu with `--editor-command menu.regenerate-demo-disc-main-menu`, build Windows, and commit only the factory, test, and regenerated `assets/scenes/DemoDiscMainMenu.helen`.
