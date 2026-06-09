# City Wii Platform Config Plan

## Task 1: Add Wii platform declarations

Update the City project declarations so Wii is a supported project platform.

- Modify `project.heproj`
- Modify `settings/platforms.json`

Expected result:

- both files include `wii`

## Task 2: Add shared Wii platform profile settings

Create `settings/platform.wii.json` using the same structure as the existing GameCube platform profile, but with `platformId` set to `wii`.

Set the standard actions to:

- `accept` -> `South`
- `return` -> `East`

Leave other profile ids empty so the installed Wii platform metadata can resolve defaults during build execution.

Expected result:

- the project has one valid shared Wii platform profile file

## Task 3: Add Wii local build configuration

Modify `user_settings/build_config.json` to add one `wii` platform entry that mirrors the current GameCube demo-disc scene list and ordering.

Set:

- `platformId`: `wii`
- `selectedSceneIds`: same list as `gamecube`
- `sceneOrders`: same ordered scene map as `gamecube`
- `outputDirectoryPath`: `C:\dev\helprojs\output\wii`
- `debugBuild`: `true`

Keep the existing Wii build/graphics/codegen option values aligned with the working console-style defaults used by GameCube unless the headless builder proves a different explicit value is required.

Expected result:

- the headless editor can find one persisted Wii build configuration entry

## Task 4: Run the real editor-owned Wii build

Build from the shared editor wrapper:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ..\helengine\artifacts\build-platform.ps1 `
  -Project ..\helprojs\city\project.heproj `
  -Platform wii `
  -Output ..\helprojs\city\wii-build
```

Expected result:

- the build succeeds through the editor build graph

## Task 5: Verify staged/package scene parity

Inspect the produced Wii build output and confirm the cooked scene set includes the same demo-disc scenes as GameCube instead of only the menu scene.

Expected result:

- menu-linked demo scenes are present in the built Wii export

## Task 6: Verify in Dolphin

If the build emits a packaged ISO, launch it and confirm selecting a menu item now targets a scene that exists in the build output.

Expected result:

- scene loading is no longer blocked by missing build content
