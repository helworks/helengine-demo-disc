# Helen of Code Splash Screen Design

## Goal

Show `helen_of_code_logo.png` on a black splash screen during initial startup, then reveal an already-loaded standard main menu. The splash applies to Windows, PS2, PSP, PS Vita, GameCube, Wii, and Wii U. Nintendo DS and Nintendo 3DS remain unchanged for a later handheld-specific pass.

## User-visible behavior

- The logo is centered and rendered as a square whose height is 90% of the authored screen height.
- The splash background is opaque black while the splash is visible.
- The splash fades in over 0.75 seconds.
- It remains fully visible for 3 seconds.
- It fades out over 0.75 seconds.
- The existing standard main-menu scene is loaded additively at splash startup, before the fade sequence completes.
- After the fade-out, the splash scene unloads itself; the already-loaded main menu remains active.
- Returning to the main menu from gameplay continues to load the menu directly and does not show the splash.

## Architecture

The project will contain a generated `HelenOfCodeSplash` scene with a camera and two 2D sprites: an opaque black screen-sized background and the supplied logo. A `HelenOfCodeSplashComponent` owns the startup transition. On its first update it requests the standard main menu using `SceneLoadMode.Additive`. It then advances a deterministic phase timer using the engine frame delta, updates the logo and black overlay alpha values, and requests its own scene unload after the fade-out.

The splash camera is drawn after the menu camera and does not clear the frame, allowing the splash sprites to obscure the additive menu until their alpha reaches zero. The menu scene remains responsible for all menu input and navigation.

The source PNG will be copied into `assets/images/splash/helen_of_code_logo.png` and referenced through normal texture asset persistence. Generated scene output will be refreshed through the existing editor build/generation pipeline rather than hand-edited.

## Build configuration

The standard non-handheld platform scene lists for Windows, PS2, PSP, PS Vita, GameCube, Wii, Switch, and Wii U will include `HelenOfCodeSplash` as order 1 and retain `DemoDiscMainMenu` and all existing scenes after it. DS and 3DS scene lists will not include the splash scene or change their startup behavior.

## Testing and verification

- Add component tests for the initial additive menu request, fade phase boundaries, alpha values, and one-time self-unload request.
- Add source/configuration tests proving the splash scene is first for the eight standard non-handheld platforms and absent from DS/3DS.
- Regenerate the splash scene and build the project for Windows, verifying the splash asset and scene are included in the package.
- Run the focused tests and the smallest relevant project build; leave unrelated pre-existing working-tree changes untouched.

## Alternatives considered

1. Put the splash overlay in the main-menu scene and guard it with process-global state. This would make return-to-menu behavior depend on global lifetime and complicate scene reloading.
2. Add the splash in the engine host before scene startup. This would broaden a project-specific visual feature into engine-wide startup behavior.
3. Use a dedicated additive bootstrap scene. This keeps startup orchestration, visual timing, and cleanup in one project-owned scene while allowing the menu to load behind the splash. This is the selected approach.
