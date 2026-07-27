# PSP FPS Overlay Scale

## Goal

Render and physics scenes keep their authored FPS overlay scale of `2f` on every platform except PSP, where the overlay renders at `1f`.

## Design

The rendering and physics scene generators will preserve the shared `FPSComponent.FontScale = 2f` authoring value. Each generated FPS component will receive a PSP-only platform override for `FPSComponent.FontScale` with the value `1f`.

This keeps scene authoring portable, prevents platform conditions from leaking into runtime UI code, and applies the same PSP treatment to every generated render and physics scene.

## Validation

Focused generator source tests will verify both the unchanged shared scale and the PSP override. The PSP package build verifies generated scenes and native packaging.
