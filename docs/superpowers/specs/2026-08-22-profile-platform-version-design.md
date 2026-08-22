# Per-platform profile version

## Context

The runtime menu footer displays `Core.Instance.PlatformInfo.Version`. That value is currently stamped during platform cooking from the platform builder descriptor, which gives every platform the same builder-defined version. The editor's Profiles modal is already the user-facing place for platform-specific build configuration, so it should own the version shown by each platform build.

## Goals

- Add an editable version string to the editor Profiles modal.
- Store the value independently for every platform profile.
- Stamp the selected platform profile's value into the runtime platform manifest during a build.
- Preserve the existing footer path through `PlatformInfo.Version`.
- Keep existing profiles/builds compatible by using `1.0.0` when no profile version exists.

## Non-goals

- Do not append platform names or suffixes to versions.
- Do not replace the project-wide `project.heproj` version; that remains separate project metadata.
- Do not regenerate scenes or levels.

## User experience

Add a `Version` text field in the platform-wide header of the Profiles modal, next to the selected platform control and above the Build/Graphics/Codegen tabs. This makes the value clearly apply to the selected platform rather than to one tab's settings.

Each platform can contain an ordinary version string, for example:

- PS2: `1.0.0`
- PSP: `1.0.1`

Switching the selected platform saves the current draft and loads the other platform's version along with its other profile settings. Cancel discards all edits; Save persists all changed platform profiles using the existing profile settings service.

## Data model and persistence

Add a `Version` property to `EditorPlatformProfileSettingsDocument`. It is persisted in the existing per-platform profile files, such as `settings/platform.ps2.json` and `settings/platform.psp.json`.

Profile normalization must ensure the property has a usable value. When loading an older profile with a missing or blank version, use `1.0.0`. This avoids changing existing output unexpectedly and gives the Profiles modal a concrete editable value immediately.

## Build and runtime flow

The selected platform profile version must flow through the existing build graph into `EditorPlatformAssetCookService` and the `PlatformBuildManifest` as `PlatformVersion`. The generated native runtime manifest continues to expose that value through `he_get_runtime_platform_version()`.

At runtime, platform boot code continues constructing `PlatformInfo` from the generated manifest. The footer remains unchanged and therefore displays the version configured in the Profiles modal.

The profile value, not the builder descriptor's hardcoded version, is the source of truth for the platform build. The builder descriptor may remain as a fallback only at an integration boundary where an older caller does not supply profile settings; the normal editor build path must always pass the normalized profile version.

## Validation

Add tests for:

1. Loading and normalizing an older platform profile produces `1.0.0`.
2. Saving and reloading preserves distinct versions for two platforms.
3. The build/cook path places the selected profile version in `PlatformBuildManifest.PlatformVersion`.
4. The generated runtime manifest receives the same version string.

No platform build is part of this change unless explicitly requested after implementation.

## Alternatives considered

### Build-tab field

Putting the field under the Build tab would require less header layout work, but it would make platform identity metadata look like a build option and obscure that it applies across all profile tabs.

### Project-wide version

Using `project.heproj` would not support independent platform versions and would bypass the Profiles modal requirement.

