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

## Script Modules and Prebuild Commands

Runtime code and editor authoring code are declared with `assets/codebase/**/code.module.json`. Platform cooks compile runtime modules only; the editor compiles tools and their sibling `<module-id>.tests` projects.

`user_settings/build_config.json` can declare ordered editor prebuild commands per platform build profile with `editorPrebuildCommandIdsByBuildProfileId`. The PS2 full profiles regenerate scenes and presentation bindings before cooking, while `colored-cube-grid` intentionally declares no editor prebuild commands.
