# Engine Audio Foundation Design

## Goal

Add a first-class Helengine audio foundation that supports:

- buffered 2D sound effects
- streamed looping music
- shared engine-side buses, volume, mute, pause, and resume
- canonical source assets with per-platform overrides
- per-platform cooked audio formats behind one runtime-facing API

The first validated target is `windows`, but the design must fit the project's supported platforms in [project.heproj](/abs/path/C:/dev/helprojs/city/project.heproj:1):

- `windows`
- `ps2`
- `psp`
- `ds`
- `3ds`
- `wii`
- `switch`
- `wiiu`

This foundation is intentionally 2D-first. It must leave room for later 3D/spatial audio without forcing 3D concerns into the first implementation.

## Problem

The current engine has partial editor/build hints for audio, but not a real end-to-end audio system:

- the editor classifies audio file extensions as `Audio`
- platform cook capabilities expose an `AudioEncodingFamily`
- the menu scene authoring path has no audio component or playback integration
- the runtime content processor registry has no audio processor id
- there is no discovered audio asset type, importer, packaged runtime loader, audio manager, backend, or scene audio component

This means "play music on the main menu" is blocked on missing engine infrastructure. A loose-file or menu-only workaround would cut across the existing asset, scene, packaging, and build architecture.

## Requirements

The first engine audio foundation must:

- define one first-class `AudioAsset`
- support one canonical project source with optional per-platform source overrides
- support buffered clip playback for short SFX
- support streamed playback for music and long clips
- support multiple simultaneous SFX voices
- support engine-owned buses with per-bus volume, mute, pause, and resume
- support scene-authored playback through a reusable component
- use the existing build/cook pipeline instead of loose runtime file lookups
- fail hard for missing required assets, unsupported cook requests, and missing runtime backends
- keep the public API neutral enough to extend toward future 3D audio

The first engine audio foundation must not:

- ship as a menu-only or game-only subsystem
- require gameplay code to understand platform-specific formats
- silently fall back to no-op playback when the backend or cooked asset is missing
- hardwire Windows-specific backend types into shared engine code
- force 3D/spatial authoring concepts into the initial 2D release

## Non-Goals

The first implementation does not need to include:

- 3D positional playback
- occlusion, reverb zones, or room simulation
- doppler or distance attenuation authoring
- audio recording or microphone input
- editor waveform tooling
- crossfading or snapshot-based mix automation

These should remain possible future extensions, but they are out of scope for the first audio foundation.

## Architecture

The recommended architecture is one shared engine audio model with platform-specific cook families and runtime backends.

### Shared Engine Surface

Add shared engine-facing concepts:

- `AudioAsset`
- `AudioAssetImportSettings`
- `AudioAssetProcessorSettings`
- `AudioAssetPlatformSettingsSectionDefinition`
- `RuntimeContentProcessorIds.AudioAsset`
- `AudioManager`
- `IAudioBackend`
- `AudioBus`
- `AudioPlaybackRequest`
- `AudioSourceComponent`

The shared engine surface owns:

- asset identity
- playback mode (`buffered` vs `streamed`)
- looping behavior
- base gain
- bus routing
- platform override metadata
- runtime voice lifetime and pause state

The shared engine surface must not own platform codec details. Codec and container details belong to cook-time settings and backend implementations.

### Runtime Layers

The runtime stack should be split into three layers.

#### 1. Asset and Content Layer

This layer loads cooked audio assets through the existing content manager and runtime processor id system, just like textures, fonts, models, and animation clips.

Responsibilities:

- deserialize packaged audio asset payloads
- expose the cooked payload and metadata needed by playback backends
- preserve stable asset ids and runtime asset ids

#### 2. Engine Audio Layer

This layer is platform-neutral and lives in shared engine code.

Responsibilities:

- manage buses
- allocate and reclaim playback voices
- track active playback handles
- route play/stop/pause/resume calls into the backend
- expose master and per-bus gain state
- coordinate update-time cleanup of finished voices

This is the layer gameplay and scene components talk to.

#### 3. Platform Backend Layer

This layer is implemented one platform at a time.

Responsibilities:

- create native playback resources
- submit buffered sample data for short clips
- stream long-form clip data in chunks
- apply gain and pause state
- report completion state back to the engine layer

Only this layer should know the actual platform API, sample format, and native buffering constraints.

## Asset Model

`AudioAsset` should be a first-class serialized asset, not a loose helper object.

Recommended fields:

- `Id`
- `RuntimeAssetId`
- `PlaybackMode`
- `DefaultLoop`
- `DefaultBusId`
- `Channels`
- `SampleRate`
- `DurationSeconds`
- cooked payload metadata
- optional platform override descriptors

The source-asset authoring model should match existing Helengine patterns:

- one canonical source file is the default
- platform overrides are optional and exceptional
- overrides replace source content only when a target genuinely needs a special asset

This is especially important for `ds`, where music may need a dedicated authored cut if automatic downsampling cannot preserve acceptable results within memory and streaming limits.

## Import and Platform Settings

Audio should follow the same editor-side pattern used by textures, models, fonts, and materials.

### Import Settings

Add one audio import settings document sidecar for source files. It should include:

- importer id
- stable audio asset id
- source checksum
- shared playback defaults
- per-platform settings sections

### Per-Platform Settings Section

Register one built-in `audio` platform settings section in `AssetPlatformSettingsSectionRegistry`.

The section payload should define cook-facing values such as:

- target encoding family
- target sample rate
- target channel count
- target bit depth or compression preset
- streaming chunk size
- preload behavior
- maximum buffered size threshold

The section should not duplicate runtime-only state such as current volume or paused state.

## Cook Model

The current build graph uses a placeholder audio encoding family of `"raw"`. That should be replaced by real audio cook families.

Recommended initial family model:

- `pcm-buffered`
- `pcm-streamed`
- `adpcm-buffered`
- `adpcm-streamed`

The actual family names can change, but the separation matters:

- buffered and streamed assets have different runtime expectations
- some targets will prefer or require compressed encodings
- the cook layer must make these choices explicit and testable

### Cook Responsibilities

The audio cook pipeline should:

1. load the canonical or platform-override source
2. validate the source against shared and platform settings
3. transcode or resample into the target family
4. package the cooked payload plus metadata into the serialized audio asset
5. fail hard when the target request cannot be satisfied

The gameplay and scene layers should never need to know which codec or family a specific platform uses.

## Runtime Playback Model

### Buses

The engine should own a small bus hierarchy from the start:

- `master`
- `music`
- `sfx`

Every playback request routes through one bus. Buses expose:

- gain
- mute
- pause

Later work can add snapshots or parent-child bus graphs, but the first implementation only needs stable named buses and master routing.

### Playback Modes

#### Buffered

Buffered playback is intended for:

- UI sounds
- short impact cues
- repeated gameplay SFX

The full decoded or cooked sample payload is loaded into backend-managed memory before playback begins.

#### Streamed

Streamed playback is intended for:

- main menu music
- level music
- longer ambient loops

The backend reads or stages data incrementally. The shared engine layer should treat streaming as a playback mode, not as a separate ad hoc system.

### Scene Authoring

Add `AudioSourceComponent` so scenes can author reusable playback behavior.

The first version should support:

- referenced `AudioAsset`
- play on start
- loop override
- bus id
- gain
- playback mode override only when explicitly needed

This supports menu music without special-case menu code and keeps playback authoring reusable for gameplay scenes later.

## Windows-First Rollout

The implementation order should be phased.

### Phase 1: Windows

Windows is the proving target for:

- `AudioAsset` serialization
- import settings and platform section registration
- cook pipeline integration
- runtime content loading
- one concrete `IAudioBackend`
- scene-authored `AudioSourceComponent`
- one streamed looping menu-music path
- multiple simultaneous buffered SFX voices
- bus gain/mute/pause behavior

This phase validates the abstractions without committing the engine to per-platform hacks too early.

### Phase 2+: One Platform at a Time

After Windows works end to end, other platforms should be added by implementing backend and cook-family details behind the same engine-facing API.

Recommended order:

1. `windows`
2. `wii` or `wiiu`
3. `switch`
4. `psp`
5. `ps2`
6. `3ds`
7. `ds`

This order is based on implementation risk, not user-visible priority. `ds` should come last because it is most likely to force the tightest constraints and the earliest need for authored platform overrides.

## DS and Other Constrained Targets

`ds` is the highest-risk target and should shape validation rules early, even though the first implementation is Windows-only.

The design should assume that `ds` may require:

- lower sample rates
- mono music by default
- smaller stream chunks
- tighter buffering limits
- stricter validation thresholds
- exceptional authored override files for some long-form music

The engine should not silently accept oversized or incompatible requests for constrained targets. The cook step should reject invalid requests with explicit asset id and platform id diagnostics.

## Data Flow

End-to-end audio flow should look like this:

1. the project stores one canonical source audio file under `assets`
2. the editor creates or loads audio import settings sidecars
3. the platform cook selects canonical or override source data
4. the cook transcodes/resamples into the target family
5. the cooked `AudioAsset` is packaged with metadata and runtime ids
6. runtime content loading resolves the packaged `AudioAsset`
7. `AudioSourceComponent` or gameplay code submits an `AudioPlaybackRequest`
8. `AudioManager` routes that request to the correct bus and backend
9. the backend plays buffered or streamed data
10. the engine update loop reclaims completed voices and preserves pause/mute state

## Failure Behavior

The audio system should fail hard in all cases where the engine cannot honor authored intent safely.

Generation, import, cook, or runtime should fail when:

- the source audio file is missing
- a referenced override source file is missing
- the platform settings request an unsupported family or invalid sample configuration
- a cooked audio asset cannot be deserialized
- a scene references a required audio asset that cannot be resolved
- no runtime backend is registered for the active platform
- streaming resources cannot be created for a required streamed asset

Failure messages should include:

- platform id
- asset id
- source path or cooked path when applicable
- the invalid setting or missing dependency

The system must not silently downgrade to no audio.

## Testing Strategy

Add tests at four layers.

### 1. Serialization and Content Tests

Verify:

- `AudioAsset` serialization and deserialization
- runtime content processor id registration
- platform override payload round-tripping

### 2. Import and Settings Tests

Verify:

- audio import settings sidecars are created and loaded correctly
- audio platform settings sections register and clone correctly
- invalid audio settings fail validation with explicit errors

### 3. Cook Tests

Verify:

- canonical source cooking for `windows`
- platform override selection when an override exists
- playback mode metadata survives cooking
- invalid constrained-target settings fail hard

### 4. Runtime and Scene Tests

Verify:

- `AudioSourceComponent` resolves referenced assets correctly
- play-on-start works for streamed music
- multiple buffered SFX voices can coexist
- bus gain, mute, pause, and resume propagate correctly
- finished voices are reclaimed cleanly
- missing backend registration fails explicitly

## Main Menu Music Follow-On

Once the Windows audio foundation is complete, adding menu music should be a small vertical use of the system rather than a special engine change.

The menu implementation should:

- import the chosen music source as one canonical audio asset
- cook it for `windows`
- reference it from the shared main menu scene through `AudioSourceComponent`
- set it to streamed looping playback on the `music` bus

Later platforms should reuse the same authored scene and asset id. Only the cooked output and, when truly required, override source content should vary by platform.
