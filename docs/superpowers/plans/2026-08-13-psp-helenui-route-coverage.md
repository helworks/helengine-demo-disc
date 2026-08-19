# PSP HelenUI Route Coverage Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a deterministic PSP navigation runner that proves the Demo Disc's runtime-reachable HelenUI routes through PPSSPP's composed profile session.

**Architecture:** A PowerShell runner owns one Navigator Service session and derives its route plan directly from `helenui/demodisc.json`. A small contract test invokes the runner's `-PlanOnly` mode, so the profile-to-route filtering is regression-tested without a graphical runtime. At runtime, the runner uses `game` scope navigation and normalized controller controls only.

**Tech Stack:** PowerShell 7-compatible scripts, existing Navigator Service HTTP API, HelenUI PPSSPP/Demo Disc profiles, ViGEm virtual controller supplied by Navigator Service.

## Global Constraints

- Never start, stop, or restart Navigator Service or PPSSPP.
- Attach the PPSSPP root profile as `emulator` and the Demo Disc profile as child `game`.
- Send only `Up`, `Down`, `Left`, `Right`, `J`, and `K` to the `game` scope; never send `Escape`.
- Treat a route as passed only after the `game` scope recognizes its expected target surface.
- Use no OCR or image-processing path outside HelenUI.
- Write reports only under `output/psp/`.

---

### Task 1: Deterministic route-plan contract

**Files:**

- Create: `tests/helenui/psp-route-coverage-contract.ps1`
- Create: `tools/helenui/run-psp-route-coverage.ps1`

**Interfaces:**

- Produces: `run-psp-route-coverage.ps1 -PlanOnly`, which writes JSON route entries with `SourceSurfaceId`, `NodeId`, `InteractionId`, and `TargetSurfaceId`.
- Consumes: `helenui/demodisc.json`.

- [ ] **Step 1: Write the failing contract test**

```powershell
$planJson = & $runnerPath -PlanOnly
$plan = $planJson | ConvertFrom-Json
if ($plan.Count -eq 0) { throw 'Expected at least one planned route.' }
if ($plan.TargetSurfaceId -contains 'surface-tilt-trial-console-clear-results-level-1') { throw 'Optional result metadata must not be planned.' }
if ($plan.TargetSurfaceId -notcontains 'surface-demodisc-games-menu') { throw 'Games menu route is missing.' }
```

- [ ] **Step 2: Run the contract test to verify it fails**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File .\tests\helenui\psp-route-coverage-contract.ps1`

Expected: failure because `run-psp-route-coverage.ps1` does not exist.

- [ ] **Step 3: Implement `-PlanOnly`**

```powershell
$profile = Get-Content -LiteralPath $ProfilePath -Raw | ConvertFrom-Json
$routes = foreach ($surface in $profile.surfaces) {
    foreach ($node in $surface.uiNodes | Where-Object { -not $_.isOptional }) {
        foreach ($interaction in $node.interactions | Where-Object { -not [string]::IsNullOrWhiteSpace($_.targetSurfaceId) }) {
            [pscustomobject]@{ SourceSurfaceId=$surface.id; NodeId=$node.id; InteractionId=$interaction.id; TargetSurfaceId=$interaction.targetSurfaceId }
        }
    }
}
$routes | ConvertTo-Json -Depth 5
```

- [ ] **Step 4: Run the contract test to verify it passes**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File .\tests\helenui\psp-route-coverage-contract.ps1`

Expected: exit code 0.

### Task 2: Composed-session route execution and reporting

**Files:**

- Modify: `tools/helenui/run-psp-route-coverage.ps1`
- Modify: `tests/helenui/psp-route-coverage-contract.ps1`

**Interfaces:**

- Consumes: `-PlanOnly` route entries from Task 1, live Navigator Service on `http://localhost:38406`.
- Produces: `output/psp/helenui-route-coverage.json`, `output/psp/helenui-route-coverage.txt`, and an exit code.

- [ ] **Step 1: Extend the failing contract test for controller safety**

```powershell
$source = Get-Content -LiteralPath $runnerPath -Raw
if ($source -match "'Escape'|\"Escape\"") { throw 'PSP runner must not send Escape.' }
if ($source -notmatch "'K'|\"K\"") { throw 'PSP runner must declare the Circle/back gamepad control.' }
```

- [ ] **Step 2: Run the contract test to verify it fails**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File .\tests\helenui\psp-route-coverage-contract.ps1`

Expected: failure because the runner has no execution control declaration.

- [ ] **Step 3: Implement session lifecycle and verified navigation**

```powershell
$attachBody = @{ targetId=$target.TargetId; ocrConfigPath=$OcrConfigPath; profiles=@(@{scopeId='emulator';profilePath=$PpssppProfilePath;parentScopeId=$null},@{scopeId='game';profilePath=$GameProfilePath;parentScopeId='emulator'}) } | ConvertTo-Json -Depth 6
$session = Invoke-RestMethod -Method Post -Uri "$ServiceUrl/sessions" -ContentType 'application/json' -Body $attachBody
$gamepadBackControl = 'K'
Invoke-RestMethod -Method Post -Uri "$ServiceUrl/sessions/$($session.SessionId)/navigate" -ContentType 'application/json' -Body (@{scopeId='game';targetScreen=$route.TargetSurfaceId;timeoutMs=60000;retryLimit=2}|ConvertTo-Json)
```

After each navigation, call `/recognize`, inspect only the `game` profile scope, and compare its recognized screen name with the profile surface name. Record each result before continuing. Always delete the created session in `finally`.

- [ ] **Step 4: Run the contract test to verify it passes**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File .\tests\helenui\psp-route-coverage-contract.ps1`

Expected: exit code 0.

- [ ] **Step 5: Run the PSP integration test**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\helenui\run-psp-route-coverage.ps1`

Expected: JSON and text reports under `output/psp/`; process exits 0 only when every required planned route is recognized.

### Task 3: Profile validation and final verification

**Files:**

- Modify: `tools/helenui/validate-demodisc-profile.ps1` only if the new route-plan invariants reveal an invalid profile relationship.

- [ ] **Step 1: Run existing profile validation**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\helenui\validate-demodisc-profile.ps1`

Expected: validation succeeds with all surfaces, nodes, and target routes resolved.

- [ ] **Step 2: Run both script contract and integration commands**

Run: `powershell -NoProfile -ExecutionPolicy Bypass -File .\tests\helenui\psp-route-coverage-contract.ps1; powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\helenui\run-psp-route-coverage.ps1`

Expected: both commands exit 0 and the report contains a passed entry for every planned required route.

- [ ] **Step 3: Review worktree and report evidence**

Run: `git status --short; Get-Content .\output\psp\helenui-route-coverage.txt`

Expected: only intended runner/test/documentation changes are introduced; report summary agrees with the JSON route results.
