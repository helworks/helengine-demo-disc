# PSP HelenUI Route Coverage Design

## Goal

Provide a reusable, repo-owned PSP navigation runner that tests every runtime-reachable route authored by the Demo Disc HelenUI profile through PPSSPP and HelenUI, producing an evidence-backed report.

## Scope

The runner lives at `tools/helenui/run-psp-route-coverage.ps1`. It uses an already-running Navigator Service and never starts, stops, or restarts HelenUI. It discovers the live PPSSPP target, attaches a composed session with the PPSSPP root profile and Demo Disc game subprofile, and releases only the session it created.

The runner treats the game profile as the authority for application state. It sends only normalized game controls that the Navigator Service mirrors through its ViGEm virtual gamepad: `Up`, `Down`, `Left`, `Right`, `J` (A/Cross), and `K` (B/Circle). It must not send `Escape`, because PPSSPP owns that key for emulator overlays/save states.

## Route Selection

The runner parses `helenui/demodisc.json` and builds a deterministic route plan from UI-node interactions with a `targetSurfaceId`. It excludes optional metadata-only result overlays unless an explicit runtime-recognizable route reaches them. Every planned route records the source surface, node, interaction, and target surface.

The plan starts from the recognized main menu. For each target, it asks Navigator Service to navigate in the `game` scope, waits for a completed recognition pass, and requires the child scope's recognized surface to match the expected target surface name. After a route, it returns to the main menu using authored game navigation. Failure to recognize, route, or return is recorded and stops the run to prevent subsequent results from becoming misleading.

## Reporting

The runner writes `output/psp/helenui-route-coverage.json` and a concise text summary beside it. Each entry includes timestamps, route identity, the recognized child surface before/after, session history evidence, and a pass/fail/blocked status. A nonzero exit code means at least one required route was not proven.

## Testability

`-PlanOnly` returns the deterministic route plan without requiring PPSSPP or HelenUI. A PowerShell contract test proves that the plan contains only non-optional, target-bearing UI interactions, excludes result-overlay metadata routes, and includes the main application menus. Runtime execution remains a hardware/emulator integration test and is verified by the generated report.

## Constraints

- HelenUI is the only visual recognition and input authority; no local OCR or separate screenshot processing is permitted.
- The runner must not restart Navigator Service or PPSSPP.
- The runner must preserve unrelated worktree changes.
- The runner may use Navigator Service's HTTP API because Navigator MCP proxies that same API and its service-side input emitter supplies the virtual gamepad.
