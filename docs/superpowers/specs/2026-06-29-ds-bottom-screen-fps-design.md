# DS Bottom Screen FPS Design

## Goal

Move authored `FPSComponent` overlays from the top screen to the bottom screen for all generated Nintendo DS rendering companion scenes.

## Context

Generated DS rendering scenes currently strip `FPSComponent` from the authored top-screen roots, but the scaffold does not recreate that overlay anywhere else. The result is that the top screen stays clean, but the bottom screen loses the FPS overlay entirely.

## Decision

The shared DS rendering scaffold will own FPS relocation for generated rendering scenes.

- Scene factories will keep authoring `FPSComponent` exactly once on their normal UI entities.
- `NintendoDsRenderingSceneScaffoldFactory` will detect authored `FPSComponent` instances while preparing the top-screen roots.
- For each authored FPS overlay, the scaffold will create a scaffold-owned bottom-screen entity under `DemoDiscBottomScreenRoot`, copy the authored FPS settings that matter at runtime, and persist the font reference through the existing FPS font-reference path.
- The authored top-screen `FPSComponent` instance will still be removed so the top screen remains dedicated to 3D content.

## Why This Approach

This keeps DS policy centralized in the generic scaffold instead of duplicating DS-only FPS entities across every rendering scene factory. It also preserves the existing authoring workflow: scene authors continue to add FPS once, and DS generation decides where it belongs.

## Scope

In scope:

- shared DS rendering scaffold behavior
- generated rendering companion scenes such as `cube_test_ds.helen` and `scaled_cube_ds.helen`
- focused source and asset verification

Out of scope:

- changing authored rendering scene factories
- changing physics-scene DS generation
- replacing the temporary bottom-screen test text
