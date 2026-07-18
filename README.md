# DemoDisc

Platform builds should output to:

`demodisc\output\<platform>`

Examples:

- `demodisc\output\windows`
- `demodisc\output\psp`
- `demodisc\output\ds`

Use the project-local `output` folder for every platform build instead of ad-hoc root folders like `windows-build`, `psp-build`, or `3ds-build`.

Example:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:\dev\helworks\helengine\scripts\build-platform.ps1 -Project C:\dev\helprojs\demodisc\project.heproj -Platform windows -Output C:\dev\helprojs\demodisc\output\windows
```
