# Split Play Golden Coin Blueprint Design

## Goal

Add one reusable `GoldenCoin` blueprint for Split Play that can be dropped into authored levels and collects all shared coin authoring in one place. The blueprint should use a more rounded generated cylinder mesh on most platforms and a lower-step generated cylinder mesh on Nintendo DS through a mesh-component platform override.

## Scope

This design covers:

- Generated model assets for one common coin mesh and one DS coin mesh
- One generated gold coin material asset
- One generated `GoldenCoin.hblueprint` asset
- A DS platform override on the blueprint mesh component that swaps only the model reference
- Tests that prove the generated asset paths and DS override wiring

This design does not cover:

- Coin gameplay logic changes
- Scene placement of coin instances
- Local blueprint overrides beyond transform changes
- Animated spinning, bobbing, or VFX

## Asset Layout

The generated assets will live under the city project `assets` tree:

- `assets/models/games/split_play/golden_coin.hasset`
- `assets/models/games/split_play/golden_coin_ds.hasset`
- `assets/materials/games/split_play/GoldenCoin.hasset`
- `assets/blueprints/games/split_play/GoldenCoin.hblueprint`

The blueprint will be the single reusable authored entry point. The model assets remain separate so the blueprint can reference a common mesh and a DS override mesh without duplicating geometry inside the blueprint payload.

## Geometry Strategy

The coin mesh will be authored procedurally as a thin cylinder:

- Common mesh: higher radial step count for a rounder silhouette on desktop and consoles
- DS mesh: reduced radial step count for a cheaper silhouette on Nintendo DS

Both meshes should share the same overall dimensions so the DS override only changes visual density, not gameplay scale or placement. The common and DS models should keep the same pivot and face orientation so swapping references does not require any transform override.

## Blueprint Structure

`GoldenCoin.hblueprint` will contain one editable root entity with one visible mesh child, unless the final serializer path prefers a single root mesh entity. The authored content should stay minimal:

- mesh component referencing `models/games/split_play/golden_coin.hasset`
- material reference pointing at `materials/games/split_play/GoldenCoin.hasset`
- optional gameplay-facing component hooks can be added later by scene instances or future revisions

The mesh component will carry a `ds` platform override whose only difference is:

- model reference changes to `models/games/split_play/golden_coin_ds.hasset`

Every other platform will use the common mesh through the shared component payload.

## Generation Path

City already contains generated asset writers for scenes and materials. Split Play coin support should follow that pattern with a dedicated generated asset helper in the city codebase.

The implementation should:

1. Build the two raw `ModelAsset` cylinder variants in code
2. Serialize both model assets into the project assets tree
3. Write one gold material asset using the existing generated material pipeline
4. Build the blueprint document in code and serialize it as one `.hblueprint`

The recommended implementation shape is a focused Split Play asset authoring service rather than embedding the full write logic in a scene factory. That keeps coin asset generation reusable when multiple levels start referencing the same blueprint.

## Material Direction

The coin material should be a shared gold material asset under `materials/games/split_play`. It should be authored once and reused by the blueprint across all supported platforms. Platform-specific material tuning can be added through normal material sidecars if needed later, but this first pass should keep one material path and avoid unnecessary platform splits unless the city material pipeline already emits them automatically.

## Data Flow

Authoring flow:

1. Split Play asset generator runs from city code
2. Generator writes coin material and both model assets
3. Generator constructs the `GoldenCoin` blueprint asset
4. Blueprint stores the shared asset references plus the DS model override
5. Level scenes can later embed the blueprint through `BlueprintInstanceComponent`

Runtime/editor flow:

1. Blueprint loads as one normal editable root
2. Default mesh uses the common rounded coin model
3. DS platform selection resolves the mesh-component override
4. DS builds package the low-step model through normal scene and asset reference expansion

## Error Handling

The generator should fail fast when:

- project root or assets root cannot be resolved
- generated model serialization fails
- material serialization fails
- blueprint serialization fails
- the blueprint cannot be validated as a single-root asset before save

Failures should leave no ambiguous partial authoring state when possible. Asset writes should prefer the same replace-or-overwrite pattern already used by the project's generated asset services.

## Testing

Tests should prove the authored outputs, not just helper internals.

Required coverage:

- source or authoring test verifying the Split Play asset generator writes the expected common model, DS model, material, and blueprint paths
- blueprint-focused test verifying `GoldenCoin.hblueprint` references the common coin mesh in shared state
- blueprint-focused test verifying the `ds` component override swaps the model reference to the DS mesh
- geometry test or source assertion proving the DS cylinder uses fewer radial steps than the common cylinder

Tests can be source-driven if that is the existing pattern for the city project, but at least one test should inspect the serialized authored asset result closely enough to catch a broken override path.

## Recommendation

Use one generated blueprint plus two generated model assets. This gives Split Play exactly one reusable coin blueprint while keeping the DS mesh swap explicit, cheap, and easy to validate. It also aligns with the engine's new blueprint support and the city project's existing generated-authoring pattern.
