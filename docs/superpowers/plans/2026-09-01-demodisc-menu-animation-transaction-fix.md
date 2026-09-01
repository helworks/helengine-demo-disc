# DemoDisc Menu Animation Transaction Fix Plan

> **Worker:** Implement with `superpowers:test-driven-development`; use `superpowers:systematic-debugging` evidence from Windows build `a419b239-af9a-436f-b58c-a1fdcfb82236`.

**Goal:** Let `menu.regenerate-demo-disc-main-menu` publish its generated logo animation and then reference it while generating menu scenes, without changing engine transaction semantics.

**Root cause:** `DemoDiscLogoIdleAnimationGenerator` stages `animations/DemoDiscLogoIdle.hanim`, after which the same active transaction handles `CreateFileReference(..., AssetEntryKind.File)`. `EditorAuthoringTransaction` deliberately supports staged references only for concrete asset kinds and rejects generic `File`, so the menu prebuild fails before packaging.

**Architecture:** Make the command explicitly two-phase. Commit the deterministic animation asset in one transaction, then begin a fresh transaction for `DemoDiscSceneGenerator`. The second transaction resolves the already-published `.hanim` through the normal file reference resolver. Do not add an engine asset kind, weaken staged-reference validation, or pass engine internals into DemoDisc factories.

**Files:**

- Modify: `assets/codebase/menu.tools/RegenerateDemoDiscMainMenuCommand.cs`
- Modify: `assets/codebase/menu.tools.tests/DemoDiscLogoIdleAnimationGenerationSourceTests.cs`

## Task 1: Add the failing command contract

- [ ] Make the existing source test worktree-correct with a `[CallerFilePath]` checkout-root resolver.
- [ ] Require two clearly named transaction scopes in command order: animation generation and commit first; scene generation and commit second.
- [ ] Require `DemoDiscLogoIdleAnimationGenerator` to receive only the animation transaction and `DemoDiscSceneGenerator` to receive only the scene transaction.
- [ ] Reject the former one-transaction sequence.
- [ ] Run the focused test and record a meaningful red result.

## Task 2: Split the command transactions minimally

- [ ] In `Execute`, open `animationTransaction`, generate the logo animation, and commit it inside its own `using` scope.
- [ ] After that scope disposes, open `sceneTransaction`, generate the DemoDisc scenes, and commit them.
- [ ] Preserve exception propagation: scene generation must not run if animation publication fails.
- [ ] Do not change the animation generator, standard/handheld factories, engine code, or menu contents.

## Task 3: Verify and commit

- [ ] Run the focused `DemoDiscLogoIdleAnimationGenerationSourceTests` filter.
- [ ] Run the menu tools build and `rtk git diff --check`.
- [ ] Stage only the two planned files and commit as `Fix DemoDisc menu animation publication`.
- [ ] Leave the local Windows platform manifest, package output, and unrelated importer churn uncommitted and untouched.

## Task 4: Resume the existing Windows plan

- [ ] Rerun the Windows build-waiter once after the focused fix. Require the menu prebuild to pass; on any new failure, capture the first failing stage before further changes.
