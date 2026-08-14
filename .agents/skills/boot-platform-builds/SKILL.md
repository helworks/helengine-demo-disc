---
name: boot-platform-builds
description: Use when building, booting, launching, or validating Demo Disc artifacts on Windows, PSP, PS2, PS Vita, GameCube, DS, 3DS, Wii, Wii U, Switch, or another emulator or hardware target.
---

# Boot Platform Builds

Boot every artifact through its platform-owned launcher. Never invoke an emulator executable directly.

## Resolve

1. Identify the target platform and verify the exact artifact path and timestamp.
2. Use `<platform-repo>\scripts\launch_in_emulator.ps1` when that standard launcher exists.
3. From Demodisc, use this owned launcher:

| Platform | Launcher | Artifact parameter |
| --- | --- | --- |
| Windows | `C:\dev\helworks\helengine-windows\scripts\launch_in_emulator.ps1` | `-ArtifactPath` |
| PSP | `C:\dev\helworks\helengine-psp\scripts\launch_in_emulator.ps1` | `-ArtifactPath` |
| PS2 | `C:\dev\helworks\helengine-ps2\scripts\launch_in_emulator.ps1` | `-ArtifactPath` |
| PS Vita | `C:\dev\helworks\helengine-psvita\tools\launch-vita3k.ps1` | `-VpkPath` |
| GameCube | `C:\dev\helworks\helengine-gc\scripts\launch_in_emulator.ps1` | `-ArtifactPath` |
| DS | `C:\dev\helworks\helengine-ds\scripts\launch_in_emulator.ps1` | `-ArtifactPath` |
| 3DS | `C:\dev\helworks\helengine-3ds\scripts\launch_in_emulator.ps1` | `-ArtifactPath` |
| Wii | `C:\dev\helworks\helengine-wii\scripts\launch_in_emulator.ps1` | `-ArtifactPath` |
| Wii U | `C:\dev\helworks\helengine-wiiu\scripts\launch_in_emulator.ps1` | `-ArtifactPath` |
| Switch | `C:\dev\helworks\helengine-switch\scripts\launch_in_emulator.ps1` | `-ArtifactPath` |

4. If a platform repository or its owned launcher is missing, stop and report it. Do not replace it with a direct emulator command.

## Boot

Use the selected launcher's required artifact parameter:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File <launcher> -ArtifactPath <artifact>
# PS Vita only:
powershell -NoProfile -ExecutionPolicy Bypass -File <launcher> -VpkPath <artifact>
```

Let the launcher perform installation, boot arguments, and emulator configuration. Do not call `Start-Process` on PPSSPP, PCSX2, Dolphin, Azahar, melonDS, Vita3K, or another emulator binary.

## UI Validation

- Never OCR or manually interpret pages/screenshots.
- Use the HelenUI Navigator MCP and matching project profile for UI state.
- Never restart HelenUI.
- Never manually navigate an emulator. Use the project route runner or Navigator MCP gamepad only when automated navigation is authorized.
- Never send `Escape`, save-state inputs, or desktop-key shortcuts to a console emulator.
- When the user is navigating manually, boot the correct artifact and wait for their observation.

## Red Flags

- "Launching the emulator directly is faster."
- "The artifact is already installed."
- "The launcher is unnecessary for this one test."

All mean: stop and use the platform-owned launcher.