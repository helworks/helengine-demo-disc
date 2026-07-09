# Generated Test Projects Design

## Goal

Make the engine generate C# test projects alongside generated production projects, so test compile surfaces stay aligned with authored code surfaces and repo-owned legacy test `.csproj` files are no longer needed.

## Problem

The current project layout is split in a way that causes compile-surface drift:

- the engine generates `user_settings/generated_code/projects/gameplay/gameplay.csproj`
- the repository owns `tests/gameplay.tests/gameplay.tests.csproj`
- the generated `gameplay.csproj` intentionally excludes `assets/codebase/rendering.tools/**`, `physics.tools/**`, `game.tools/**`, and `scene.tools/**`

That means a hand-authored test project can easily point at a generated production project that does not expose the surface the tests actually need. The result is a mismatch between:

- what code exists in the repo
- what code the generated production project compiles
- what code the test project can see

This is the wrong boundary. If the engine owns generated production projects, it should also own generated test projects that mirror those production surfaces.

## Requirements

The engine-generated test project system must:

- generate multiple test projects, not one aggregate project
- discover test sources by sibling folder naming under `assets/codebase`
- use name-only discovery for test folders
- require no `code.module.json` file for test folders
- generate one test project per matching production surface
- make each generated test project reference its matching generated production project
- fail hard when a test folder exists without a matching production surface
- include generated test projects in the generated solution
- replace legacy hand-authored repository test `.csproj` files

The system must not:

- keep a legacy repo-owned test project path in parallel
- infer broad fallback references to unrelated production surfaces
- depend on a separate top-level `tests/` convention
- require test module manifests for simple sibling test folders

## Folder Convention

Production and test code should live under `assets/codebase` using sibling surface folders.

Examples:

- `assets/codebase/game/**/*.cs`
- `assets/codebase/game.tests/**/*.cs`
- `assets/codebase/rendering.tools/**/*.cs`
- `assets/codebase/rendering.tools.tests/**/*.cs`
- `assets/codebase/physics.tools/**/*.cs`
- `assets/codebase/physics.tools.tests/**/*.cs`

The engine should interpret `<surface>.tests` as the test surface for `<surface>`.

## Discovery Rules

### Production Surface Discovery

The existing generated project flow already discovers production code surfaces and emits generated projects beneath:

- `user_settings/generated_code/projects/<surface>/<surface>.csproj`

That production discovery remains the source of truth for what surfaces exist.

### Test Surface Discovery

After production discovery, the engine should scan `assets/codebase` for sibling folders whose names end with `.tests`.

For each discovered test folder:

1. strip the `.tests` suffix to compute the base production surface name
2. verify the matching production surface exists
3. generate a test project only when the production surface exists
4. fail generation immediately when the production surface does not exist

Examples:

- `rendering.tools.tests` maps to `rendering.tools`
- `game.tests` maps to `game`
- `physics.tools.tests` maps to `physics.tools`

If `assets/codebase/audio.tools.tests` exists but `assets/codebase/audio.tools` does not, generation should fail with a clear error naming both paths.

## Generated Project Shape

For each discovered test surface, the engine should emit:

- `user_settings/generated_code/projects/<surface>.tests/<surface>.tests.csproj`

Example:

- `user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj`

Each generated test project should:

- target `net9.0`
- set `IsPackable` to `false`
- enable implicit usings
- disable nullable if that matches the current generated project/test baseline
- include only test files from `assets/codebase/<surface>.tests/**/*.cs`
- reference the generated production project for the matching base surface
- include the standard test package set
- include `Using Include="Xunit"`
- emit build outputs under the generated-code output root, consistent with the existing generated project path conventions

## Reference Rules

Generated test projects should stay strict and local.

For a test surface `<surface>.tests`:

- always reference generated production project `<surface>`
- do not automatically reference unrelated generated production projects
- do not add “reference everything” fallbacks
- do not infer extra references from namespaces or source text

Examples:

- `rendering.tools.tests` references `rendering.tools.csproj`
- `game.tests` references `game.csproj`
- `menu.tools.tests` references `menu.tools.csproj`

This strictness is important because it keeps test compile surfaces honest. If a test needs another production surface, that dependency should be represented at the production project layer or through a deliberate future extension, not hidden by the test generator.

## Recommended Generated Test Project Template

The generated test project shape should be analogous to the current repo-owned test project, but surface-specific.

Expected elements:

- `Microsoft.NET.Test.Sdk`
- `xunit`
- `xunit.runner.visualstudio`
- `coverlet.collector`
- `Using Include="Xunit"`
- `ProjectReference` to the generated production project

The generator should emit the real absolute include paths for:

- the generated `GlobalUsings.g.cs` file for the test project
- the `assets/codebase/<surface>.tests/**/*.cs` glob

## Solution Generation

The generated solution should include:

- all generated production projects
- all generated test projects

This keeps IDE behavior aligned with the generated compile surface and removes the need for repo-owned hand-maintained test project entries.

If a solution previously contained:

- `user_settings/generated_code/projects/gameplay/gameplay.csproj`

it should now also contain entries such as:

- `user_settings/generated_code/projects/game.tests/game.tests.csproj`
- `user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj`
- `user_settings/generated_code/projects/physics.tools.tests/physics.tools.tests.csproj`

depending on which sibling test folders exist.

## Migration Plan

This feature is intended to replace the legacy approach, not coexist with it.

Migration target:

- remove repo-owned `tests/gameplay.tests/gameplay.tests.csproj`
- move existing test files into `assets/codebase/<surface>.tests/`
- update generated solution output to include the new generated test projects
- update local workflows, docs, and automation that still point at legacy test project paths

Example mapping:

- `tests/gameplay.tests/TiltTrialLevelCatalogTests.cs` moves to `assets/codebase/game.tests/TiltTrialLevelCatalogTests.cs`
- source audits for `rendering.tools` move to `assets/codebase/rendering.tools.tests/`
- physics tool tests move to `assets/codebase/physics.tools.tests/`

There should be no long-lived compatibility path where both generated and repo-owned test project systems remain active.

## Failure Behavior

Generation must fail when:

- a `.tests` folder exists without a matching production surface folder
- a generated test project cannot determine the matching generated production project path
- solution generation cannot add the generated test project entry

Failure messages should include:

- the discovered test folder name
- the expected production surface name
- the relevant generated project path when applicable

## Testing Strategy

Add engine tests at three levels.

### 1. Solution Generator Tests

Extend `EditorGameSolutionServiceTests` to verify:

- generated test projects are written when matching `.tests` folders exist
- generated solution contents include those test projects
- generated test projects reference the correct generated production projects
- orphan `.tests` folders fail generation

### 2. Generated Project Content Tests

Verify generated test `.csproj` contents include:

- test SDK package references
- `xunit` package references
- `Using Include="Xunit"`
- compile globs only for the matching `assets/codebase/<surface>.tests/**/*.cs`
- project reference to the matching generated production project

### 3. Migration/Integration Verification

Verify a representative city project can:

- regenerate solution files with generated test projects
- remove legacy repo-owned test project usage
- build and run tests through the generated test project path

## Proposed Architecture

Add generated test-project support as an extension of the existing solution/project generator rather than as a separate system.

Recommended responsibilities:

### Production Project Generator

No major conceptual change. It remains the source of truth for production surface discovery and generated production project output.

### Test Surface Discovery Helper

New helper that:

- scans `assets/codebase` for sibling folders ending in `.tests`
- computes the matching base production surface
- validates the production surface exists

### Generated Test Project Model

New model that describes:

- test surface id
- production surface id
- test source glob
- generated project output path
- generated production project reference path

### Solution Writer Updates

Extend the existing solution writer so it appends generated test projects to the same solution output flow as generated production projects.

## Recommendation

Implement generated test projects as inferred sibling test surfaces under `assets/codebase`.

Why this is the right fit:

- it matches your preference that code lives under `assets`
- it removes hand-maintained test project drift
- it keeps test compile surfaces aligned with production compile surfaces
- it avoids test manifests and other ceremony
- it replaces the legacy test project path cleanly instead of layering one more system on top
