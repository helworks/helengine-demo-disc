---
name: build-platform-builds
description: Use when building or packaging a Demo Disc artifact for Windows, PSP, PS2, PS Vita, GameCube, DS, 3DS, Wii, Wii U, Switch, or another supported target.
---

# Build Platform Artifacts

Build through HelEngine's canonical platform script. It creates an isolated workspace and owns scene generation, native packaging, and output layout.

## Build

1. Verify `project.heproj`, the platform id, output directory, and configuration. Default to `Debug` unless the user requests otherwise.
2. Use one supported id: `windows`, `psp`, `ps2`, `psvita`, `gamecube`, `ds`, `3ds`, `wii`, `wiiu`, or `switch`.
3. Run only this build entry point:

```powershell
rtk powershell -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine\scripts\build-platform.ps1 -Project C:\dev\helprojs\demodisc\project.heproj -Platform <platform> -Output C:\dev\helprojs\output\<platform>-<purpose> -Configuration Debug
```

4. Confirm the command succeeded and inspect the output path and fresh artifact timestamp. Do not assume a shared artifact filename across platforms.
5. If the artifact must be launched, use `boot-platform-builds`; do not launch an emulator as part of building.

## Do Not

- Do not invoke `dotnet publish`, a generator, a native compiler, or a platform packager directly.
- Do not reuse an artifact without checking its output path and timestamp.
- Do not overwrite, install, move, or delete artifacts unless the user explicitly asks.
- Do not replace build verification with emulator UI inspection.