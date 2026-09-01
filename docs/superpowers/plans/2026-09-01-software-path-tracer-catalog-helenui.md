# Software Path Tracer Catalog and HelenUI Plan

> **Worker:** Implement with `superpowers:test-driven-development` after the generated scene is accepted.

**Goal:** Make the shared `software_path_tracer` scene selectable in DemoDisc's Rendering catalog and recognizable/navigable through HelenUI without changing unrelated menu behavior.

**Files:**

- Modify: `assets/codebase/menu.authoring/DemoDiscSceneCatalog.cs`
- Create: `assets/codebase/menu.tools.tests/DemoDiscSoftwarePathTracerCatalogTests.cs`
- Create: `assets/codebase/menu.tools.tests/DemoDiscSoftwarePathTracerCatalogTests.cs.hmeta`
- Modify: `helenui/demodisc.json`

## Task 1: Write the failing catalog and metadata tests

- [ ] Add a direct catalog test that calls `CreateDemoSceneItems()` and requires exactly one item with:

```csharp
new MenuItemDefinition(
    "scene-software-path-tracer",
    "Software Path Tracer",
    true,
    new MenuActionDefinition(MenuActionKind.LoadScene, "software_path_tracer"))
```

- [ ] Assert it is immediately after `scene-pbr-shadow-theater` and immediately before `scene-back`; keep every existing item and relative order.
- [ ] Parse `helenui/demodisc.json` with `System.Text.Json` in the test. Do not use brittle whole-file string ordering as the only proof.
- [ ] Assert the `surface-demodisc-demo-scenes-menu` recognition text list includes `Software Path Tracer` once.
- [ ] Assert that surface contains one `node-demodisc-rendering-software-path-tracer` node with text/name `Software Path Tracer`, order `10`, normal previous/next interactions, and an activate interaction targeting `surface-demodisc-showcase-scene`.
- [ ] Assert the node has one selected-state `highlighted_text` clue whose candidate is `Software Path Tracer` and whose highlight settings match neighboring rendering nodes.
- [ ] Assert `node-demodisc-rendering-back` moves to order `11` and remains the last rendering-menu node.
- [ ] Assert the broad `surface-demodisc-showcase-scene` recognition candidate list also includes `Software Path Tracer` once; this is the list near the existing PBR/physics scene names, not just the Rendering-menu recognition clue.
- [ ] Run the focused menu.tools test and confirm a meaningful red result.

## Task 2: Add the catalog entry

- [ ] Insert the item after `PBR Shadow Theater` and before `Back` in `CreateDemoSceneItems()`.
- [ ] Do not add a separate handheld item, duplicate scene ID, or feature-specific menu component.

## Task 3: Update HelenUI precisely

- [ ] Add `Software Path Tracer` once to the Rendering catalog surface's recognition `texts`.
- [ ] Clone the neighboring PBR node shape into `node-demodisc-rendering-software-path-tracer`, replacing every node/clue/interactions ID consistently and using order `10`.
- [ ] Shift only the Rendering Back node from order `10` to `11`.
- [ ] Add `Software Path Tracer` once to the shared showcase-scene recognition text list so automation recognizes the launched scene.
- [ ] Preserve schema version, unrelated surfaces, all existing nodes, and JSON formatting style.

## Task 4: Verify and commit

- [ ] Run:

```powershell
Get-Content helenui/demodisc.json -Raw | ConvertFrom-Json | Out-Null
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/menu.tools.tests/menu.tools.tests.csproj --no-restore --filter "FullyQualifiedName~DemoDiscSoftwarePathTracerCatalogTests" -v:minimal
rtk dotnet build user_settings/generated_code/editor-command/EditorFull/projects/menu.authoring/menu.authoring.csproj --no-restore -v:minimal
rtk git diff --check
```

- [ ] Inspect the JSON diff to ensure only the two recognition arrays, the new node, and Back order changed.
- [ ] Commit as `Expose software path tracer in DemoDisc`.

