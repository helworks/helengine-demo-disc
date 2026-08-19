# Navigator Input Class Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let Navigator navigation requests choose `auto`, `keyboard`, or `gamepad` delivery while retaining semantic profile routes and existing client compatibility.

**Architecture:** The service navigation request owns the optional input class. `SessionNavigationService` passes it into `HelenProjectNavigationService`, which selects the keyboard emitter, the ViGEm emitter, or the existing composite. MCP forwards the same optional field. PSP coverage uses `gamepad` for each route and never sends raw recovery input.

**Tech Stack:** .NET 9/C#, MCP SDK, ViGEm Xbox 360 controller, PowerShell.

---

### Task 1: Add the navigation request contract

**Files:**
- Modify: `C:\dev\helenui\plugins\navigator-service\src\NavigatorService\Contracts\NavigatorApiModels.cs`
- Test: `C:\dev\helenui\plugins\navigator-service\tests\NavigatorService.Tests\NavigatorHttpPrimitivesTests.cs`

- [ ] **Step 1: Write the failing request tests**

Add tests that `new NavigateRequest("target", InputClass: "gamepad")` preserves `gamepad`, an omitted value is `auto`, and `"touch"` throws `ArgumentException` naming `InputClass`.

- [ ] **Step 2: Run RED**

```powershell
rtk dotnet test plugins\navigator-service\tests\NavigatorService.Tests\NavigatorService.Tests.csproj --no-restore --filter FullyQualifiedName~NavigatorHttpPrimitivesTests -v:minimal
```

Expected: compilation fails because `NavigateRequest` has no input-class parameter.

- [ ] **Step 3: Implement the field**

Extend `NavigateRequest` with `string InputClass = "auto"`. Normalize to lower case and accept only `auto`, `keyboard`, and `gamepad`; otherwise throw `ArgumentException("InputClass must be auto, keyboard, or gamepad.", nameof(InputClass))`.

- [ ] **Step 4: Run GREEN**

Run the Step 2 command. Expected: selected tests pass.

- [ ] **Step 5: Commit**

```powershell
git -C C:\dev\helenui add -- plugins/navigator-service/src/NavigatorService/Contracts/NavigatorApiModels.cs plugins/navigator-service/tests/NavigatorService.Tests/NavigatorHttpPrimitivesTests.cs
git -C C:\dev\helenui commit -m "Add Navigator navigation input class"
```

### Task 2: Select one physical input emitter

**Files:**
- Modify: `C:\dev\helenui\plugins\navigator-service\src\NavigatorService\Input\IKeyboardInputService.cs`
- Modify: `C:\dev\helenui\plugins\navigator-service\src\NavigatorService\Input\KeyboardInputService.cs`
- Modify: `C:\dev\helenui\plugins\navigator-service\src\NavigatorService\Application\NavigatorServiceRuntimeFactory.cs`
- Modify: `C:\dev\helenui\plugins\navigator-service\src\NavigatorService\Navigation\HelenProjectNavigationService.cs`
- Modify: `C:\dev\helenui\plugins\navigator-service\src\NavigatorService\Application\SessionNavigationService.cs`
- Test: `C:\dev\helenui\plugins\navigator-service\tests\NavigatorService.Tests\KeyboardInputServiceTests.cs`
- Test: `C:\dev\helenui\plugins\navigator-service\tests\NavigatorService.Tests\SessionNavigationServiceTests.cs`

- [ ] **Step 1: Write failing isolation tests**

Inject recording keyboard and ViGEm emitters. Assert `gamepad` delivers normalized `J` only to ViGEm, `keyboard` only to keyboard, and omitted input class delivers to both through `auto`. Add a session-navigation test proving the request input class reaches the input service.

- [ ] **Step 2: Run RED**

```powershell
rtk dotnet test plugins\navigator-service\tests\NavigatorService.Tests\NavigatorService.Tests.csproj --no-restore --filter "FullyQualifiedName~KeyboardInputServiceTests|FullyQualifiedName~SessionNavigationServiceTests" -v:minimal
```

Expected: test code cannot select an input class.

- [ ] **Step 3: Implement transport selection**

Change `IKeyboardInputService.SendAsync` to accept `string inputClass = "auto"`. Make `KeyboardInputService` own keyboard, controller, and composite emitters. After foreground verification, choose exactly one:

```csharp
IKeyboardInputEmitter emitter = inputClass switch
{
    "keyboard" => _keyboardEmitter,
    "gamepad" => _controllerEmitter,
    "auto" => _autoEmitter,
    _ => throw new ArgumentException("InputClass must be auto, keyboard, or gamepad.", nameof(inputClass)),
};
```

Pass `request.InputClass` through `SessionNavigationService` and `HelenProjectNavigationService`. Keep `SendKeysAsync` unchanged. Build the runtime service from one `SendInputKeyboardEmitter` and one `ViGEmXbox360InputEmitter`.

- [ ] **Step 4: Run GREEN**

Run Step 2. Expected: all tests pass and `gamepad` has zero keyboard-emitter calls.

- [ ] **Step 5: Commit**

```powershell
git -C C:\dev\helenui add -- plugins/navigator-service/src/NavigatorService/Input/IKeyboardInputService.cs plugins/navigator-service/src/NavigatorService/Input/KeyboardInputService.cs plugins/navigator-service/src/NavigatorService/Application/NavigatorServiceRuntimeFactory.cs plugins/navigator-service/src/NavigatorService/Navigation/HelenProjectNavigationService.cs plugins/navigator-service/src/NavigatorService/Application/SessionNavigationService.cs plugins/navigator-service/tests/NavigatorService.Tests/KeyboardInputServiceTests.cs plugins/navigator-service/tests/NavigatorService.Tests/SessionNavigationServiceTests.cs
git -C C:\dev\helenui commit -m "Dispatch Navigator navigation by input class"
```

### Task 3: Forward input class through MCP

**Files:**
- Modify: `C:\dev\helenui\plugins\navigator-mcp\src\NavigatorMcp\Contracts\SessionCommandModels.cs`
- Modify: `C:\dev\helenui\plugins\navigator-mcp\src\NavigatorMcp\Tools\NavigatorTools.cs`
- Test: `C:\dev\helenui\plugins\navigator-mcp\tests\NavigatorMcp.Tests\NavigatorServiceClientTests.cs`

- [ ] **Step 1: Write the failing forwarding tests**

Assert `NavigateInput("game", "target", 10000, 5, "gamepad")` sends JSON containing `"InputClass":"gamepad"`; omitted input class sends `"InputClass":"auto"`.

- [ ] **Step 2: Run RED**

```powershell
rtk dotnet test plugins\navigator-mcp\tests\NavigatorMcp.Tests\NavigatorMcp.Tests.csproj --no-restore --filter FullyQualifiedName~NavigatorServiceClientTests -v:minimal
```

Expected: `NavigateInput` has no input-class parameter.

- [ ] **Step 3: Implement MCP forwarding**

Add `string InputClass = "auto"` to `NavigateInput`. Add a final optional `string inputClass = "auto"` to `NavigatorTools.NavigateSession`, document the three values, and construct `NavigateInput(scopeId, targetScreen, timeoutMs, retryLimit, inputClass)`.

- [ ] **Step 4: Run GREEN**

Run Step 2. Expected: all selected tests pass.

- [ ] **Step 5: Commit**

```powershell
git -C C:\dev\helenui add -- plugins/navigator-mcp/src/NavigatorMcp/Contracts/SessionCommandModels.cs plugins/navigator-mcp/src/NavigatorMcp/Tools/NavigatorTools.cs plugins/navigator-mcp/tests/NavigatorMcp.Tests/NavigatorServiceClientTests.cs
git -C C:\dev\helenui commit -m "Expose Navigator input class through MCP"
```

### Task 4: Use only gamepad navigation for PSP coverage

**Files:**
- Modify: `tools\helenui\run-psp-route-coverage.ps1`
- Modify: `tests\helenui\psp-route-coverage-contract.ps1`

- [ ] **Step 1: Write the failing runner contract**

Require `inputClass = 'gamepad'` in the `/navigate` body and reject `/keys`, `Send-GamepadBack`, and `$gamepadControls` in the runner source.

- [ ] **Step 2: Run RED**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tests\helenui\psp-route-coverage-contract.ps1
```

Expected: fails because the runner contains raw key recovery.

- [ ] **Step 3: Implement the PSP runner change**

Delete `$gamepadControls` and `Send-GamepadBack`. `Ensure-MainMenu` recognizes once then throws when the main menu is not recognized; it never sends recovery input. Add `inputClass = 'gamepad'` to every `/navigate` request. Retain session cleanup and surface-name-to-ID normalization.

- [ ] **Step 4: Run GREEN**

Run Step 2. Expected: `PASS: PSP route-plan contract validated 40 required routes.`

- [ ] **Step 5: Commit**

```powershell
git add -- tools/helenui/run-psp-route-coverage.ps1 tests/helenui/psp-route-coverage-contract.ps1
git commit -m "Use gamepad navigation for PSP route coverage"
```

### Task 5: Verify complete behavior

**Files:**
- Verify only: `output\psp\helenui-route-coverage.json`

- [ ] **Step 1: Run complete automated suites**

```powershell
rtk dotnet test plugins\navigator-service\tests\NavigatorService.Tests\NavigatorService.Tests.csproj --no-restore -v:minimal
rtk dotnet test plugins\navigator-mcp\tests\NavigatorMcp.Tests\NavigatorMcp.Tests.csproj --no-restore -v:minimal
```

Expected: both suites pass.

- [ ] **Step 2: Boot and validate PSP**

Build Navigator, start it on port 38406, boot `output\psp-debug\PSP\GAME\HELENGINE\EBOOT.PBP` only through `C:\dev\helworks\helengine-psp\scripts\launch_in_emulator.ps1`, then run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\helenui\run-psp-route-coverage.ps1 -ServiceUrl http://localhost:38406 -RequestTimeoutSeconds 120 -NavigationTimeoutMilliseconds 120000
```

Expected: route coverage contains no `/keys` recovery operation and uses `gamepad` delivery; any recognition failure blocks without sending input.

- [ ] **Step 3: Do not commit generated output**

Commit only source/test changes required by the tasks above.