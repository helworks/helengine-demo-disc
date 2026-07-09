# Shared Control Icon Resolution Design

## Goal

Add one shared authoring utility that resolves generated control-icon PNGs into imported texture asset IDs for a requested raw control ID and target platform family, so rendering and physics scene generators can reuse the same icon source of truth without hard-coded file paths.

## Problem

The project now has a generated control-icon pack under `assets/images/instructions/controls/generated`, with one folder per platform family and one PNG per control name. Rendering and physics scene generators do not currently have a reusable way to consume those assets.

Today:

- Material factories already know how to resolve imported texture asset IDs through the editor import pipeline.
- Scene factories still own their own overlay/entity layout.
- There is no shared utility for `platform id + raw control id -> generated PNG -> imported texture asset ID`.

Without a shared resolver, each scene generator would need to duplicate:

- platform-family mapping
- generated manifest lookup
- PNG path construction
- texture import resolution
- missing-icon validation

That duplication is the wrong boundary.

## Requirements

The utility must:

- use the generated control-pack manifest as the source of truth
- resolve raw control IDs, not semantic actions
- map authored/runtime platform IDs to exact icon families
- return imported texture asset IDs, not just file paths
- fail hard when a family or control is missing
- default `windows` to the `keyboard` family
- remain layout-agnostic so scene factories still author their own entities and overlays

The utility must not:

- build UI entities
- define prompt layout
- introduce fallback icon families beyond the explicit platform map
- duplicate the generated icon manifest in a second hand-authored catalog

## Proposed Architecture

Add three shared pieces in `assets/codebase/rendering.tools/`:

### 1. `GeneratedControlIconPlatformMap`

Owns the platform-to-family mapping rules.

Responsibilities:

- normalize platform IDs
- map a platform ID to one generated icon family
- throw when the platform cannot be mapped

Initial behavior:

- `windows` and `win32` map to `keyboard`
- all known authored native platforms map to their matching generated family
- unknown platforms fail

This is intentionally strict so prompt coverage issues surface during generation instead of silently shipping the wrong icon family.

### 2. `GeneratedControlIconCatalog`

Loads and validates the generated control-pack manifest.

Responsibilities:

- load `assets/images/instructions/controls/generated/manifest.json`
- validate that the manifest exists and contains the requested family
- validate that the requested raw control ID exists in that family
- construct the generated PNG relative path for the resolved family/control pair

This type should treat the generated icon pack as authoritative. No second copy of control coverage should exist in source.

### 3. `GeneratedControlIconAssetResolver`

Bridges catalog lookup to the editor import pipeline.

Responsibilities:

- accept `projectRootPath`, `platformId`, and `rawControlId`
- resolve the family through `GeneratedControlIconPlatformMap`
- resolve the PNG path through `GeneratedControlIconCatalog`
- load or create texture import settings for that PNG
- require a persisted imported texture asset ID
- return a resolved record containing both path and asset ID data

This should reuse the same editor import-manager pattern already used by existing textured material factories.

## Proposed API

Use a small data record plus one resolver entry point.

### `ResolvedControlIcon`

Fields:

- `PlatformId`
- `FamilyId`
- `ControlId`
- `SourcePngRelativePath`
- `ImportedTextureAssetId`

### `GeneratedControlIconCatalog`

Suggested methods:

- `Load(string projectRootPath)`
- `ResolveFamily(string platformId)`
- `RequireControlPath(string projectRootPath, string platformId, string rawControlId)`
- `RequireTextureAssetId(string projectRootPath, string platformId, string rawControlId)`

The public API can live either on the catalog or on a dedicated resolver wrapper, but the behavior must stay the same:

- input is raw control ID plus platform ID
- output includes imported texture asset ID
- failures are exceptions with concrete missing-family or missing-control messages

## Data Flow

For one requested control prompt:

1. Scene factory asks for a raw control ID such as `start`, `a`, `dpad_left`, or `left_stick`.
2. Platform map resolves the current authored/runtime platform to one icon family.
3. Catalog loads manifest data and validates the family/control pair.
4. Resolver builds the generated PNG relative path.
5. Resolver runs the PNG through the editor import pipeline.
6. Resolver returns the imported texture asset ID and supporting metadata.
7. Scene factory uses that asset ID while authoring its own overlay/image/material content.

The scene factory never hard-codes generated asset paths directly.

## Why Raw Control IDs

Raw control IDs are the correct boundary for this utility because the generated pack is already organized by platform family and raw control name. Introducing semantic-action translation here would add a second mapping problem that does not exist yet in rendering or physics generators.

If semantic prompts are needed later, they should be built on top of this resolver, not baked into it.

## Failure Behavior

Generation must fail when:

- the generated manifest is missing
- the requested platform cannot be mapped to a family
- the resolved family is not present in the manifest
- the requested raw control ID is not exported for that family
- the generated PNG file is missing
- the editor import pipeline cannot produce a persisted texture asset ID

Error messages should include the platform ID, resolved family, and raw control ID so broken prompt coverage is easy to diagnose.

## Integration Plan

The shared resolver should be adopted in stages.

### Stage 1: Shared Utility

Build the shared catalog, platform map, and asset resolver in `rendering.tools`.

Reasoning:

- `rendering.tools` already owns generated authored material and scene helper patterns
- it already contains the editor import-resolution code patterns this feature needs
- `physics.tools` already depends on `city.rendering.tools`

### Stage 2: First Rendering Consumer

Update one rendering showcase overlay to use the resolver for prompt icons while leaving entity layout local to that factory.

This proves:

- manifest lookup works
- import resolution works
- one shared scene can bind correct prompt textures without platform-specific scene duplication

### Stage 3: First Physics Consumer

Update one physics overlay using the same resolver.

This confirms the utility boundary is truly shared and not rendering-specific.

### Stage 4: Remaining Prompt Surfaces

Adopt the resolver in the rest of the rendering and physics prompt-bearing scenes once the pattern is stable.

## Testing Strategy

Add tests at three levels.

### Catalog Tests

Verify:

- manifest loads successfully
- known platform IDs resolve to expected families
- `windows` resolves to `keyboard`
- unknown platform IDs fail
- known family/control pairs resolve to expected PNG paths
- missing controls fail

These should be small, direct, and deterministic.

### Import Resolution Tests

Verify:

- a generated PNG can produce a non-empty imported texture asset ID through the editor import pipeline
- failures are explicit when import settings are unavailable or invalid

These tests should use the real editor-side importer path, because that is the behavior scene generators rely on.

### Integration/Source Tests

For early adopters, verify:

- the selected rendering factory calls into the shared resolver instead of hard-coding icon paths
- the selected physics factory does the same

These can start as source-level guardrail tests if broader authored-scene integration tests do not yet exist for the prompt overlays.

## File Plan

Expected new files:

- `assets/codebase/rendering.tools/GeneratedControlIconPlatformMap.cs`
- `assets/codebase/rendering.tools/GeneratedControlIconCatalog.cs`
- `assets/codebase/rendering.tools/GeneratedControlIconAssetResolver.cs`
- `assets/codebase/rendering.tools/ResolvedControlIcon.cs`

Expected modified files:

- one rendering scene factory that owns a prompt overlay
- one physics scene/overlay factory that owns a prompt overlay
- related tests in `tests/gameplay.tests/` or the relevant rendering/physics test project locations

## Risks

### Manifest Drift

If the icon pack is regenerated with renamed controls or families, scene generation will fail. That is acceptable and preferred over silent fallback, but the failures need to be explicit.

### Platform Naming Drift

If runtime/editor platform IDs are inconsistent, the platform map may reject valid targets. Keep mapping logic centralized and normalized to avoid scattered string matching.

### Import Pipeline Assumptions

If generated PNGs are not always importable the same way authored textures are, the resolver may need a small helper around import settings generation. This is still the correct place to add that behavior.

## Recommendation

Build the shared raw-control resolver now and keep it narrow:

- generated manifest is authoritative
- platform mapping is strict
- output is imported texture asset IDs
- scene factories keep ownership of layout

This gives rendering and physics one reusable prompt-icon foundation without forcing a larger prompt-system redesign.
