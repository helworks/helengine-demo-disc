# Demo Disc Scene Order Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reorder the demo-disc scene selector so Cube Test appears first, Colored Cube Grid second, and Textured Cube Grid third without changing any labels or scene ids.

**Architecture:** The change stays inside the authored scene catalog for the city menu. `DemoDiscSceneProvider` continues to build the same menu tree; only the item order returned by `DemoDiscSceneCatalog.CreateSceneItems()` changes. Verification is lightweight because this is a pure presentation-order change.

**Tech Stack:** C#, city gameplay/menu authored sources, .NET build for the generated `menu.tools` project.

---

### Task 1: Reorder the scene catalog entries

**Files:**
- Modify: `assets/codebase/menu/DemoDiscSceneCatalog.cs:10-16`
- Verify: `user_settings/generated_code/projects/menu.tools/menu.tools.csproj`

- [ ] **Step 1: Update the catalog order**

Replace the current `CreateSceneItems()` array with this sequence while keeping the same labels and scene ids:

```csharp
public MenuItemDefinition[] CreateSceneItems() {
    return new[] {
        new MenuItemDefinition("scene-cube-test", "Cube Test", "Minimal one-cube rendering validation scene.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "scenes/rendering/cube_test.helen")),
        new MenuItemDefinition("scene-colored-cube-grid", "Colored Cube Grid", "Sixteen rotating cubes with distinct lit material colors.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "scenes/rendering/colored_cube_grid.helen")),
        new MenuItemDefinition("scene-textured-cube-grid", "Textured Cube Grid", "Sixteen rotating cubes with distinct lit texture materials.", true, new MenuActionDefinition(MenuActionKind.LoadScene, "scenes/rendering/textured_cube_grid.helen")),
        new MenuItemDefinition("scene-back", "Back", "Returns to the main menu.", true, new MenuActionDefinition(MenuActionKind.Back, string.Empty))
    };
}
```

- [ ] **Step 2: Build the generated menu project**

Run:

```powershell
dotnet build user_settings/generated_code/projects/menu.tools/menu.tools.csproj
```

Expected: build succeeds with no errors.

- [ ] **Step 3: Confirm the authored order in the source file**

Run:

```powershell
Get-Content assets\codebase\menu\DemoDiscSceneCatalog.cs
```

Expected: the catalog lists the playable scenes in the new order:
1. `Cube Test`
2. `Colored Cube Grid`
3. `Textured Cube Grid`

- [ ] **Step 4: Commit the change**

```powershell
git add assets/codebase/menu/DemoDiscSceneCatalog.cs
git commit -m "Reorder demo disc scene catalog"
```
