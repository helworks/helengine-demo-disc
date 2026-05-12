## Goal

Add a third committed rendering scene, `Textured Cube Grid`, that preserves the existing `Cube Test` and `Colored Cube Grid` scenes, introduces sixteen rotating cubes with distinct authored textured materials, and makes the new scene the temporary startup/export target for PS2 and desktop validation.

## Task 1: Add the textured grid scene and authored assets in `city`

### Step 1.1: Add failing source coverage for the new scene

- Extend the existing city rendering scene source tests so they assert:
  - the rendering scene generator emits `textured_cube_grid.helen`
  - the catalog includes `Textured Cube Grid`
  - startup/export selection points at `scenes/rendering/textured_cube_grid.helen`
  - the new scene contains sixteen cubes with distinct textured material references
- Run the focused source test and confirm it fails before implementation.

### Step 1.2: Create authored textured material and texture assets

- Add one new texture-asset set for the grid under `city/assets/materials/rendering/textured_cube_grid` or a nearby owned location.
- Create sixteen distinct diagnostic textures.
- Create sixteen real authored materials that:
  - reference the standard material path
  - bind one distinct texture each

### Step 1.3: Add the new scene factory and generation wiring

- Add a new `TexturedCubeGridSceneFactory`.
- Reuse the established `4x4` layout, camera, light, and runtime spin path from the colored grid baseline.
- Bind each cube to its distinct authored textured material.
- Update `RenderingSceneGenerator` to emit all three rendering scenes.
- Update `DemoDiscSceneCatalog` to list all three scenes.
- Update `build_config.json` to make `textured_cube_grid.helen` the temporary startup/export scene.

### Step 1.4: Generate the authored scene asset and rerun the focused tests

- Regenerate the rendering scenes through the editor command path.
- Rerun the focused city source tests and confirm they pass.

## Task 2: Verify runtime material/texture packaging in `helengine`

### Step 2.1: Prove the package path handles file-backed textured materials

- Add or extend focused packager tests so file-backed textured materials in the new city scene:
  - rewrite to cooked runtime paths
  - preserve the expected texture reference through the packaged asset path

### Step 2.2: Implement any required editor-side material packaging updates

- Adjust the scene packager only if the new tests show a missing texture-binding path.
- Keep the existing colored-material path intact.

### Step 2.3: Run focused editor tests

- Run the relevant editor tests and confirm they pass.

## Task 3: Verify the PS2 textured runtime path in `helengine-ps2`

### Step 3.1: Add focused PS2 coverage for textured material loading

- Extend the focused PS2 tests to assert the cooked textured material path and renderer inputs match the new authored layout.
- If necessary, add a targeted regression around per-material texture loading in the runtime path.

### Step 3.2: Implement the smallest PS2 fixes required

- Only change the PS2 material loader or renderer if the new scene reveals a missing texture-binding path.
- Keep lighting, colored-material behavior, and the current directory-fanout fix intact.

### Step 3.3: Run focused PS2 tests

- Run the PS2 builder/runtime tests and confirm they pass.

## Task 4: Export and verify the textured grid scene

### Step 4.1: Rebuild the worktree editor app

- Build the worktree editor app so the export uses fresh packaging code.

### Step 4.2: Export a fresh PS2 ISO

- Build a new PS2 ISO from the city project with `textured_cube_grid.helen` as startup.

### Step 4.3: Launch and inspect in PCSX2

- Launch the new ISO in `PCSX2`.
- Confirm:
  - all sixteen cubes render
  - textures are distinct
  - rotation works
  - lighting still works on the textured surfaces

### Step 4.4: Capture the result and decide next renderer step

- If the scene renders correctly, prepare the work for commit.
- If it fails, use the smallest next diagnostic that isolates texture sampling from the rest of the material path.
