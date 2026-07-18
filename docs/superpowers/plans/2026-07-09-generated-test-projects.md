# Generated Test Projects Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make helengine generate strict per-surface xUnit projects from raw `.tests` folders under `assets/codebase`, then migrate the tracked city tests off the legacy repo-owned `tests/gameplay.tests/gameplay.tests.csproj`.

**Architecture:** Extend the existing generated-project pipeline instead of adding a second test-project system. Production module discovery stays unchanged; after those projects exist, the builder infers additional test projects from raw `.tests` folder names, emits them into the same generated solution, and writes test-specific SDK/package/project-reference entries. One important correction from the earlier draft: the current city runtime fallback surface is the module id `gameplay`, so the migrated runtime test folder must be `assets/codebase/gameplay.tests`, not `assets/codebase/game.tests`.

**Tech Stack:** C# 13 / .NET 9 SDK-style projects, xUnit 2.9, Microsoft.NET.Test.Sdk 17.11.1, coverlet.collector 6.0.2, helengine editor project generator, `rtk` shell wrapper

---

## File Structure

- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\EditorGameSolutionServiceTests.cs`
  Purpose: lock in failing coverage for fallback-module test discovery, editor-surface test projects, solution membership, strict project references, and orphan `.tests` failure.
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorGeneratedCodeProjectKind.cs`
  Purpose: distinguish production and generated test projects without creating a parallel legacy path.
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorGeneratedCodeTestProjectDiscoveryService.cs`
  Purpose: scan `assets/codebase` for raw `.tests` folders, resolve them by module id, and fail hard on orphans.
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorGeneratedCodeModuleProject.cs`
  Purpose: carry project kind and one referenced production module id for generated test projects.
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorGeneratedCodeSolution.cs`
  Purpose: expose all generated projects while preserving the primary production project contract used by hot reload/build services.
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorGeneratedCodeSolutionBuilder.cs`
  Purpose: append inferred test projects after production projects are built.
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorGameSolutionService.cs`
  Purpose: emit test-specific `.csproj` contents, write all generated project files, and keep strict one-to-one production references.
- Create: `C:\dev\helprojs\demodisc\assets\codebase\gameplay.tests\*.cs`
  Purpose: runtime/component tests for the fallback `gameplay` surface.
- Create: `C:\dev\helprojs\demodisc\assets\codebase\game.tools.tests\*.cs`
  Purpose: source-authoring tests for the `game.tools` editor surface.
- Create: `C:\dev\helprojs\demodisc\assets\codebase\rendering.tools.tests\*.cs`
  Purpose: source-authoring and catalog tests for the `rendering.tools` editor surface.
- Delete: `C:\dev\helprojs\demodisc\tests\gameplay.tests\gameplay.tests.csproj`
  Purpose: remove the legacy repo-owned test project after generated test projects exist.

## Constraints To Preserve

- The test-folder convention is name-only, not manifest-driven.
- `assets/codebase/gameplay.tests` is valid even though the matching production surface is the fallback module `gameplay` rooted at `assets`.
- Generated test projects may reference only their matching generated production project; no “reference every module” fallback is allowed.
- Keep engine assembly references available in generated test projects so direct engine-type assertions still compile when they are already reachable in the current suite.
- Do not mass-edit historical files under `C:\dev\helprojs\demodisc\docs\superpowers\plans\`; they are historical artifacts, not active workflow configuration.
- Do not delete `C:\dev\helprojs\demodisc\tests\gameplay.tests\` wholesale if untracked local experiments still live there. Remove only tracked legacy files in this migration.

### Task 1: Lock Engine Behavior With Failing Tests

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.tests\EditorGameSolutionServiceTests.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj`

- [ ] **Step 1: Add failing tests for fallback runtime test discovery, editor-surface test discovery, and orphan failure**

```csharp
[Fact]
public void GenerateSolutionFiles_WhenFallbackGameplayAndSiblingTestFoldersExist_WritesGeneratedTestProjects() {
    File.Delete(Path.Combine(TempProjectRootPath, "assets", "Scripts", "Player.cs"));
    Directory.CreateDirectory(Path.Combine(TempProjectRootPath, "assets", "codebase", "gameplay.tests"));
    Directory.CreateDirectory(Path.Combine(TempProjectRootPath, "assets", "codebase", "rendering.tools"));
    Directory.CreateDirectory(Path.Combine(TempProjectRootPath, "assets", "codebase", "rendering.tools.tests"));
    File.WriteAllText(Path.Combine(TempProjectRootPath, "assets", "RuntimePlayer.cs"), "public sealed class RuntimePlayer { }");
    File.WriteAllText(Path.Combine(TempProjectRootPath, "assets", "codebase", "gameplay.tests", "RuntimePlayerTests.cs"), "public sealed class RuntimePlayerTests { }");
    File.WriteAllText(Path.Combine(TempProjectRootPath, "assets", "codebase", "rendering.tools", "code.module.json"), """
{
  "moduleId": "rendering.tools",
  "dependencyModuleIds": [ "gameplay" ],
  "loadScopes": [ "always-loaded" ],
  "moduleKind": "editor"
}
""");
    File.WriteAllText(Path.Combine(TempProjectRootPath, "assets", "codebase", "rendering.tools", "Factory.cs"), "public sealed class Factory { }");
    File.WriteAllText(Path.Combine(TempProjectRootPath, "assets", "codebase", "rendering.tools.tests", "FactoryTests.cs"), "public sealed class FactoryTests { }");

    EditorGameSolutionService service = new EditorGameSolutionService(TempProjectRootPath, "SkyRider", new TestIdeLauncher());

    string solutionPath = service.GenerateSolutionFiles();
    string gameplayTestsProjectPath = Path.Combine(TempProjectRootPath, "user_settings", "generated_code", "projects", "gameplay.tests", "gameplay.tests.csproj");
    string renderingTestsProjectPath = Path.Combine(TempProjectRootPath, "user_settings", "generated_code", "projects", "rendering.tools.tests", "rendering.tools.tests.csproj");
    string gameplayTestsProjectContents = File.ReadAllText(gameplayTestsProjectPath);
    string renderingTestsProjectContents = File.ReadAllText(renderingTestsProjectPath);
    string solutionFileContents = File.ReadAllText(solutionPath);

    Assert.True(File.Exists(gameplayTestsProjectPath));
    Assert.True(File.Exists(renderingTestsProjectPath));
    Assert.Contains("<PackageReference Include=\"Microsoft.NET.Test.Sdk\" Version=\"17.11.1\" />", gameplayTestsProjectContents, StringComparison.Ordinal);
    Assert.Contains("<PackageReference Include=\"xunit\" Version=\"2.9.0\" />", gameplayTestsProjectContents, StringComparison.Ordinal);
    Assert.Contains("<ProjectReference Include=\"..\\gameplay\\gameplay.csproj\" />", gameplayTestsProjectContents, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("<ProjectReference Include=\"..\\rendering.tools\\rendering.tools.csproj\" />", renderingTestsProjectContents, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("user_settings/generated_code/projects/gameplay.tests/gameplay.tests.csproj", solutionFileContents, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj", solutionFileContents, StringComparison.OrdinalIgnoreCase);
}

[Fact]
public void GenerateSolutionFiles_WhenEditorSurfaceTestFolderExists_WritesEditorAwareGlobalUsings() {
    File.Delete(Path.Combine(TempProjectRootPath, "assets", "Scripts", "Player.cs"));
    Directory.CreateDirectory(Path.Combine(TempProjectRootPath, "assets", "codebase", "menu.tools"));
    Directory.CreateDirectory(Path.Combine(TempProjectRootPath, "assets", "codebase", "menu.tools.tests"));
    File.WriteAllText(Path.Combine(TempProjectRootPath, "assets", "RuntimePlayer.cs"), "public sealed class RuntimePlayer { }");
    File.WriteAllText(Path.Combine(TempProjectRootPath, "assets", "codebase", "menu.tools", "code.module.json"), """
{
  "moduleId": "menu.tools",
  "dependencyModuleIds": [ "gameplay" ],
  "loadScopes": [ "always-loaded" ],
  "moduleKind": "editor"
}
""");
    File.WriteAllText(Path.Combine(TempProjectRootPath, "assets", "codebase", "menu.tools", "Command.cs"), "public sealed class Command { }");
    File.WriteAllText(Path.Combine(TempProjectRootPath, "assets", "codebase", "menu.tools.tests", "CommandTests.cs"), "public sealed class CommandTests { }");

    EditorGameSolutionService service = new EditorGameSolutionService(TempProjectRootPath, "SkyRider", new TestIdeLauncher());
    service.GenerateSolutionFiles();

    string globalUsingsPath = Path.Combine(TempProjectRootPath, "user_settings", "generated_code", "projects", "menu.tools.tests", "GlobalUsings.g.cs");
    Assert.Contains("global using helengine.editor;", File.ReadAllText(globalUsingsPath), StringComparison.Ordinal);
}

[Fact]
public void GenerateSolutionFiles_WhenOrphanTestFolderExists_ThrowsWithFolderAndExpectedSurface() {
    File.Delete(Path.Combine(TempProjectRootPath, "assets", "Scripts", "Player.cs"));
    Directory.CreateDirectory(Path.Combine(TempProjectRootPath, "assets", "codebase", "audio.tools.tests"));
    File.WriteAllText(Path.Combine(TempProjectRootPath, "assets", "RuntimePlayer.cs"), "public sealed class RuntimePlayer { }");
    File.WriteAllText(Path.Combine(TempProjectRootPath, "assets", "codebase", "audio.tools.tests", "AudioTests.cs"), "public sealed class AudioTests { }");

    EditorGameSolutionService service = new EditorGameSolutionService(TempProjectRootPath, "SkyRider", new TestIdeLauncher());

    InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => service.GenerateSolutionFiles());
    Assert.Contains("audio.tools.tests", exception.Message, StringComparison.OrdinalIgnoreCase);
    Assert.Contains("audio.tools", exception.Message, StringComparison.OrdinalIgnoreCase);
}
```

- [ ] **Step 2: Run the focused engine tests and confirm they fail for the missing feature**

Run:

```bash
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~EditorGameSolutionServiceTests.GenerateSolutionFiles_WhenFallbackGameplayAndSiblingTestFoldersExist_WritesGeneratedTestProjects|FullyQualifiedName~EditorGameSolutionServiceTests.GenerateSolutionFiles_WhenEditorSurfaceTestFolderExists_WritesEditorAwareGlobalUsings|FullyQualifiedName~EditorGameSolutionServiceTests.GenerateSolutionFiles_WhenOrphanTestFolderExists_ThrowsWithFolderAndExpectedSurface" -v minimal
```

Expected: FAIL because `gameplay.tests.csproj` and `rendering.tools.tests.csproj` are not generated yet and orphan `.tests` folders are not validated.

- [ ] **Step 3: Commit the red tests in the helengine repo**

```bash
rtk git -C C:\dev\helworks\helengine add engine/helengine.editor.tests/EditorGameSolutionServiceTests.cs
rtk git -C C:\dev\helworks\helengine commit -m "test: cover generated test project discovery"
```

### Task 2: Add Test-Project Discovery And Shared Generated-Project Metadata

**Files:**
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorGeneratedCodeProjectKind.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorGeneratedCodeTestProjectDiscoveryService.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorGeneratedCodeModuleProject.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorGeneratedCodeSolution.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorGeneratedCodeSolutionBuilder.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj`

- [ ] **Step 1: Add the minimal project-kind enum**

```csharp
namespace helengine.editor {
    /// <summary>
    /// Distinguishes authored production module projects from inferred generated test projects.
    /// </summary>
    public enum EditorGeneratedCodeProjectKind {
        Production = 0,
        Test = 1
    }
}
```

- [ ] **Step 2: Extend the generated project model and solution container**

```csharp
public sealed class EditorGeneratedCodeModuleProject {
    public EditorGeneratedCodeModuleProject(
        string moduleId,
        string sourceFolderPath,
        IReadOnlyList<string> dependencyModuleIds,
        IReadOnlyList<string> nestedSourceFolderPaths,
        string projectFilePath,
        string generatedGlobalUsingsFilePath,
        string baseIntermediateOutputPath,
        string baseOutputPath,
        string targetFramework,
        string outputDirectoryPath,
        Guid projectGuid,
        EditorCodeModuleKind moduleKind,
        EditorGeneratedCodeProjectKind projectKind,
        string referencedProductionModuleId) {
        // existing guards...
        ProjectKind = projectKind;
        ReferencedProductionModuleId = referencedProductionModuleId ?? string.Empty;
    }

    public EditorGeneratedCodeProjectKind ProjectKind { get; }
    public string ReferencedProductionModuleId { get; }
}

public sealed class EditorGeneratedCodeSolution {
    public EditorGeneratedCodeSolution(
        IReadOnlyList<EditorGeneratedCodeModuleProject> moduleProjects,
        IReadOnlyList<EditorGeneratedCodeModuleProject> testProjects) {
        ModuleProjects = moduleProjects ?? throw new ArgumentNullException(nameof(moduleProjects));
        TestProjects = testProjects ?? throw new ArgumentNullException(nameof(testProjects));
        if (ModuleProjects.Count == 0) {
            throw new InvalidOperationException("Generated code solutions must include at least one production module project.");
        }

        Projects = [.. ModuleProjects, .. TestProjects];
    }

    public IReadOnlyList<EditorGeneratedCodeModuleProject> ModuleProjects { get; }
    public IReadOnlyList<EditorGeneratedCodeModuleProject> TestProjects { get; }
    public IReadOnlyList<EditorGeneratedCodeModuleProject> Projects { get; }
    public EditorGeneratedCodeModuleProject PrimaryModuleProject => ModuleProjects[0];
}
```

- [ ] **Step 3: Implement raw `.tests` folder discovery by module id**

```csharp
namespace helengine.editor {
    /// <summary>
    /// Discovers raw generated test surfaces from top-level folders beneath assets/codebase.
    /// </summary>
    public sealed class EditorGeneratedCodeTestProjectDiscoveryService {
        const string TestsSuffix = ".tests";

        public IReadOnlyList<EditorGeneratedCodeModuleProject> Discover(
            string projectRootPath,
            string generatedOutputRootPath,
            IReadOnlyList<EditorGeneratedCodeModuleProject> productionProjects) {
            string codebaseRootPath = Path.Combine(Path.GetFullPath(projectRootPath), "assets", "codebase");
            if (!Directory.Exists(codebaseRootPath)) {
                return [];
            }

            Dictionary<string, EditorGeneratedCodeModuleProject> productionProjectsById = productionProjects
                .Where(static project => project.ProjectKind == EditorGeneratedCodeProjectKind.Production)
                .ToDictionary(static project => project.ModuleId, StringComparer.OrdinalIgnoreCase);

            List<EditorGeneratedCodeModuleProject> discoveredProjects = [];
            foreach (string testFolderPath in Directory.EnumerateDirectories(codebaseRootPath, "*" + TestsSuffix, SearchOption.TopDirectoryOnly)) {
                string testSurfaceId = Path.GetFileName(testFolderPath);
                string productionSurfaceId = testSurfaceId[..^TestsSuffix.Length];
                if (!productionProjectsById.TryGetValue(productionSurfaceId, out EditorGeneratedCodeModuleProject productionProject)) {
                    throw new InvalidOperationException(
                        $"Generated test surface '{testSurfaceId}' expected production surface '{productionSurfaceId}', but no matching generated production project exists for '{testFolderPath}'.");
                }

                string relativeSourceFolderPath = Path.GetRelativePath(projectRootPath, testFolderPath).Replace('\\', '/');
                discoveredProjects.Add(CreateTestProject(projectRootPath, generatedOutputRootPath, productionProject, testSurfaceId, relativeSourceFolderPath));
            }

            discoveredProjects.Sort(static (left, right) => string.Compare(left.ModuleId, right.ModuleId, StringComparison.OrdinalIgnoreCase));
            return discoveredProjects;
        }
    }
}
```

- [ ] **Step 4: Append inferred test projects in the solution builder**

```csharp
public sealed class EditorGeneratedCodeSolutionBuilder {
    readonly EditorGeneratedCodeTestProjectDiscoveryService testProjectDiscoveryService = new EditorGeneratedCodeTestProjectDiscoveryService();

    public EditorGeneratedCodeSolution Build(string projectRootPath, EditorCodeModuleManifestDocument manifestDocument, string generatedOutputRootPath) {
        // existing production project loop...
        List<EditorGeneratedCodeModuleProject> moduleProjects = [];
        // populate moduleProjects exactly as before, but pass ProjectKind = Production and referencedProductionModuleId = string.Empty

        IReadOnlyList<EditorGeneratedCodeModuleProject> testProjects = testProjectDiscoveryService.Discover(
            fullProjectRootPath,
            fullGeneratedOutputRootPath,
            moduleProjects);

        return new EditorGeneratedCodeSolution(moduleProjects, testProjects);
    }
}
```

- [ ] **Step 5: Run the orphan test and metadata-sensitive tests again**

Run:

```bash
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~EditorGameSolutionServiceTests.GenerateSolutionFiles_WhenOrphanTestFolderExists_ThrowsWithFolderAndExpectedSurface|FullyQualifiedName~EditorGameSolutionServiceTests.GenerateSolutionFiles_WhenFallbackGameplayAndSiblingTestFoldersExist_WritesGeneratedTestProjects" -v minimal
```

Expected: the orphan failure should now pass, while the generated test project content assertions should still fail until the solution writer emits test-shaped `.csproj` files.

- [ ] **Step 6: Commit the discovery/model work in the helengine repo**

```bash
rtk git -C C:\dev\helworks\helengine add engine/helengine.editor/managers/project/EditorGeneratedCodeProjectKind.cs engine/helengine.editor/managers/project/EditorGeneratedCodeTestProjectDiscoveryService.cs engine/helengine.editor/managers/project/EditorGeneratedCodeModuleProject.cs engine/helengine.editor/managers/project/EditorGeneratedCodeSolution.cs engine/helengine.editor/managers/project/EditorGeneratedCodeSolutionBuilder.cs
rtk git -C C:\dev\helworks\helengine commit -m "feat: infer generated test projects"
```

### Task 3: Emit Test-Specific Project Files In The Shared Solution Writer

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorGameSolutionService.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj`

- [ ] **Step 1: Branch project-file emission for test projects**

```csharp
const string TestSdkPackageVersion = "17.11.1";
const string XunitPackageVersion = "2.9.0";
const string XunitRunnerPackageVersion = "2.8.2";
const string CoverletCollectorPackageVersion = "6.0.2";

string BuildProjectFileContents(EditorGeneratedCodeModuleProject moduleProject) {
    StringBuilder builder = new StringBuilder();
    builder.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
    builder.AppendLine("  <PropertyGroup>");
    builder.AppendLine("    <TargetFramework>" + moduleProject.TargetFramework + "</TargetFramework>");
    builder.AppendLine("    <OutputType>Library</OutputType>");
    builder.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");
    builder.AppendLine("    <Nullable>disable</Nullable>");
    if (moduleProject.ProjectKind == EditorGeneratedCodeProjectKind.Test) {
        builder.AppendLine("    <IsPackable>false</IsPackable>");
    }
    builder.AppendLine("    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>");
    builder.AppendLine("    <EnableDefaultNoneItems>false</EnableDefaultNoneItems>");
    builder.AppendLine("    <EnableDefaultContentItems>false</EnableDefaultContentItems>");
    builder.AppendLine("    <EnableDefaultEmbeddedResourceItems>false</EnableDefaultEmbeddedResourceItems>");
    builder.AppendLine("    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>");
    builder.AppendLine("    <GenerateTargetFrameworkAttribute>false</GenerateTargetFrameworkAttribute>");
    builder.AppendLine("    <BaseIntermediateOutputPath>" + EscapeXml(moduleProject.BaseIntermediateOutputPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar) + "</BaseIntermediateOutputPath>");
    builder.AppendLine("    <BaseOutputPath>" + EscapeXml(moduleProject.BaseOutputPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar) + "</BaseOutputPath>");
    builder.AppendLine("    <AssemblyName>" + EscapeXml(moduleProject.ModuleId) + "</AssemblyName>");
    builder.AppendLine("    <RootNamespace>" + EscapeXml(moduleProject.ModuleId) + "</RootNamespace>");
    builder.AppendLine("  </PropertyGroup>");
    AppendProjectReferences(builder, moduleProject);
    AppendAssemblyReferences(builder, moduleProject);
    AppendTestPackages(builder, moduleProject);
    AppendCompileItems(builder, moduleProject);
    builder.AppendLine("</Project>");
    return builder.ToString();
}
```

- [ ] **Step 2: Emit strict one-to-one project references and xUnit package items for test projects**

```csharp
void AppendProjectReferences(StringBuilder builder, EditorGeneratedCodeModuleProject moduleProject) {
    if (moduleProject.ProjectKind == EditorGeneratedCodeProjectKind.Test) {
        EditorGeneratedCodeModuleProject productionProject = FindGeneratedModuleProject(moduleProject.ReferencedProductionModuleId);
        string relativeProjectPath = Path.GetRelativePath(
            Path.GetDirectoryName(moduleProject.ProjectFilePath) ?? ProjectRootPath,
            productionProject.ProjectFilePath);
        builder.AppendLine("  <ItemGroup>");
        builder.AppendLine("    <ProjectReference Include=\"" + EscapeXml(relativeProjectPath) + "\" />");
        builder.AppendLine("  </ItemGroup>");
        return;
    }

    if (moduleProject.DependencyModuleIds.Count == 0) {
        return;
    }

    builder.AppendLine("  <ItemGroup>");
    for (int index = 0; index < moduleProject.DependencyModuleIds.Count; index++) {
        EditorGeneratedCodeModuleProject dependencyProject = FindGeneratedModuleProject(moduleProject.DependencyModuleIds[index]);
        string relativeProjectPath = Path.GetRelativePath(
            Path.GetDirectoryName(moduleProject.ProjectFilePath) ?? ProjectRootPath,
            dependencyProject.ProjectFilePath);
        builder.AppendLine("    <ProjectReference Include=\"" + EscapeXml(relativeProjectPath) + "\" />");
    }
    builder.AppendLine("  </ItemGroup>");
}

void AppendTestPackages(StringBuilder builder, EditorGeneratedCodeModuleProject moduleProject) {
    if (moduleProject.ProjectKind != EditorGeneratedCodeProjectKind.Test) {
        return;
    }

    builder.AppendLine("  <ItemGroup>");
    builder.AppendLine("    <PackageReference Include=\"coverlet.collector\" Version=\"" + CoverletCollectorPackageVersion + "\" />");
    builder.AppendLine("    <PackageReference Include=\"Microsoft.NET.Test.Sdk\" Version=\"" + TestSdkPackageVersion + "\" />");
    builder.AppendLine("    <PackageReference Include=\"xunit\" Version=\"" + XunitPackageVersion + "\" />");
    builder.AppendLine("    <PackageReference Include=\"xunit.runner.visualstudio\" Version=\"" + XunitRunnerPackageVersion + "\" />");
    builder.AppendLine("  </ItemGroup>");
    builder.AppendLine("  <ItemGroup>");
    builder.AppendLine("    <Using Include=\"Xunit\" />");
    builder.AppendLine("  </ItemGroup>");
}
```

- [ ] **Step 3: Write all generated projects into the solution and keep compile globs raw-path-only**

```csharp
public IReadOnlyList<EditorGeneratedCodeModuleProject> GeneratedModuleProjects {
    get {
        if (GeneratedCodeSolutionValue == null) {
            GeneratedCodeSolutionValue = BuildGeneratedCodeSolution();
        }

        return GeneratedCodeSolutionValue.ModuleProjects;
    }
}

public IReadOnlyList<EditorGeneratedCodeModuleProject> GeneratedProjects {
    get {
        if (GeneratedCodeSolutionValue == null) {
            GeneratedCodeSolutionValue = BuildGeneratedCodeSolution();
        }

        return GeneratedCodeSolutionValue.Projects;
    }
}

public string GenerateSolutionFiles() {
    Directory.CreateDirectory(ProjectRootPath);
    GeneratedCodeSolutionValue = BuildGeneratedCodeSolution();
    for (int index = 0; index < GeneratedCodeSolutionValue.Projects.Count; index++) {
        EditorGeneratedCodeModuleProject moduleProject = GeneratedCodeSolutionValue.Projects[index];
        string projectDirectoryPath = Path.GetDirectoryName(moduleProject.ProjectFilePath);
        if (!string.IsNullOrWhiteSpace(projectDirectoryPath)) {
            Directory.CreateDirectory(projectDirectoryPath);
        }

        File.WriteAllText(moduleProject.GeneratedGlobalUsingsFilePath, BuildGlobalUsingsFileContents(moduleProject));
        File.WriteAllText(moduleProject.ProjectFilePath, BuildProjectFileContents(moduleProject));
    }

    File.WriteAllText(SolutionFilePath, BuildSolutionFileContents(GeneratedCodeSolutionValue));
    return SolutionFilePath;
}

void AppendCompileItems(StringBuilder builder, EditorGeneratedCodeModuleProject moduleProject) {
    builder.AppendLine("  <ItemGroup>");
    builder.AppendLine("    <Compile Include=\"" + EscapeXml(moduleProject.GeneratedGlobalUsingsFilePath) + "\" />");
    builder.AppendLine("    <Compile Include=\"" + EscapeXml(Path.Combine(ResolveProjectPath(moduleProject.SourceFolderPath), "**", "*.cs")) + "\" />");
    if (moduleProject.ProjectKind == EditorGeneratedCodeProjectKind.Production) {
        for (int index = 0; index < moduleProject.NestedSourceFolderPaths.Count; index++) {
            builder.AppendLine("    <Compile Remove=\"" + EscapeXml(Path.Combine(ResolveProjectPath(moduleProject.NestedSourceFolderPaths[index]), "**", "*.cs")) + "\" />");
        }
    }
    builder.AppendLine("  </ItemGroup>");
}
```

- [ ] **Step 4: Run the full focused engine suite and verify it passes**

Run:

```bash
rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter "FullyQualifiedName~EditorGameSolutionServiceTests" -v minimal
```

Expected: PASS, including the new generated test project assertions.

- [ ] **Step 5: Commit the solution-writer changes in the helengine repo**

```bash
rtk git -C C:\dev\helworks\helengine add engine/helengine.editor/managers/project/EditorGameSolutionService.cs
rtk git -C C:\dev\helworks\helengine commit -m "feat: emit generated xunit projects"
```

### Task 4: Migrate Tracked City Tests Into Asset-Owned Raw Test Folders

**Files:**
- Create: `C:\dev\helprojs\demodisc\assets\codebase\gameplay.tests\DemoDiscReturnInputUtilsTests.cs`
- Create: `C:\dev\helprojs\demodisc\assets\codebase\gameplay.tests\DemoTiltStageComponentTests.cs`
- Create: `C:\dev\helprojs\demodisc\assets\codebase\gameplay.tests\TiltTrialLevelCatalogTests.cs`
- Create: `C:\dev\helprojs\demodisc\assets\codebase\gameplay.tests\TiltTrialLevelSettingsComponentTests.cs`
- Create: `C:\dev\helprojs\demodisc\assets\codebase\gameplay.tests\TiltTrialSessionComponentTests.cs`
- Create: `C:\dev\helprojs\demodisc\assets\codebase\game.tools.tests\TiltTrialCameraAuthoringTests.cs`
- Create: `C:\dev\helprojs\demodisc\assets\codebase\game.tools.tests\TiltTrialLightingAuthoringTests.cs`
- Create: `C:\dev\helprojs\demodisc\assets\codebase\game.tools.tests\TiltTrialSceneGenerationSourceTests.cs`
- Create: `C:\dev\helprojs\demodisc\assets\codebase\rendering.tools.tests\TiltTrialCourseMaterialAuthoringTests.cs`
- Create: `C:\dev\helprojs\demodisc\assets\codebase\rendering.tools.tests\TiltTrialMaterialColorAuthoringTests.cs`
- Delete: `C:\dev\helprojs\demodisc\tests\gameplay.tests\gameplay.tests.csproj`
- Verify against: `C:\dev\helprojs\demodisc\user_settings\generated_code\projects\gameplay.tests\gameplay.tests.csproj`
- Verify against: `C:\dev\helprojs\demodisc\user_settings\generated_code\projects\game.tools.tests\game.tools.tests.csproj`
- Verify against: `C:\dev\helprojs\demodisc\user_settings\generated_code\projects\rendering.tools.tests\rendering.tools.tests.csproj`

- [ ] **Step 1: Create the destination raw test folders**

```bash
rtk powershell -NoProfile -Command "New-Item -ItemType Directory -Force 'C:\dev\helprojs\demodisc\assets\codebase\gameplay.tests','C:\dev\helprojs\demodisc\assets\codebase\game.tools.tests','C:\dev\helprojs\demodisc\assets\codebase\rendering.tools.tests' | Out-Null"
```

- [ ] **Step 2: Move the tracked runtime and authoring tests into their generated surfaces**

```bash
rtk git -C C:\dev\helprojs\demodisc mv tests/gameplay.tests/DemoDiscReturnInputUtilsTests.cs assets/codebase/gameplay.tests/DemoDiscReturnInputUtilsTests.cs
rtk git -C C:\dev\helprojs\demodisc mv tests/gameplay.tests/DemoTiltStageComponentTests.cs assets/codebase/gameplay.tests/DemoTiltStageComponentTests.cs
rtk git -C C:\dev\helprojs\demodisc mv tests/gameplay.tests/TiltTrialLevelCatalogTests.cs assets/codebase/gameplay.tests/TiltTrialLevelCatalogTests.cs
rtk git -C C:\dev\helprojs\demodisc mv tests/gameplay.tests/TiltTrialLevelSettingsComponentTests.cs assets/codebase/gameplay.tests/TiltTrialLevelSettingsComponentTests.cs
rtk git -C C:\dev\helprojs\demodisc mv tests/gameplay.tests/TiltTrialSessionComponentTests.cs assets/codebase/gameplay.tests/TiltTrialSessionComponentTests.cs
rtk git -C C:\dev\helprojs\demodisc mv tests/gameplay.tests/TiltTrialCameraAuthoringTests.cs assets/codebase/game.tools.tests/TiltTrialCameraAuthoringTests.cs
rtk git -C C:\dev\helprojs\demodisc mv tests/gameplay.tests/TiltTrialLightingAuthoringTests.cs assets/codebase/game.tools.tests/TiltTrialLightingAuthoringTests.cs
rtk git -C C:\dev\helprojs\demodisc mv tests/gameplay.tests/TiltTrialSceneGenerationSourceTests.cs assets/codebase/game.tools.tests/TiltTrialSceneGenerationSourceTests.cs
rtk git -C C:\dev\helprojs\demodisc mv tests/gameplay.tests/TiltTrialCourseMaterialAuthoringTests.cs assets/codebase/rendering.tools.tests/TiltTrialCourseMaterialAuthoringTests.cs
rtk git -C C:\dev\helprojs\demodisc mv tests/gameplay.tests/TiltTrialMaterialColorAuthoringTests.cs assets/codebase/rendering.tools.tests/TiltTrialMaterialColorAuthoringTests.cs
rtk git -C C:\dev\helprojs\demodisc rm tests/gameplay.tests/gameplay.tests.csproj
```

Expected: only the tracked files above move. Do not delete `tests/gameplay.tests\` itself if `GeneratedControlIconAssetResolverTests.cs` or `SceneEntityTriggerObserverComponentTests.cs` still exist locally as untracked experiments.

- [ ] **Step 3: Verify the moved test files still declare the same test namespace and content**

```csharp
namespace city.tests {
    public sealed class TiltTrialLevelCatalogTests {
        [Fact]
        public void Catalog_returns_exactly_five_ordered_levels() {
            IReadOnlyList<city.game.TiltTrialLevelCatalogEntry> entries = city.game.TiltTrialLevelCatalog.CreateEntries();
            Assert.Equal(5, entries.Count);
            Assert.Equal("tilt-trial-01", entries[0].LevelId);
            Assert.Equal("tilt-trial-05", entries[4].LevelId);
        }
    }
}
```

No namespace rewrite is required. The migration is file-placement only.

- [ ] **Step 4: Commit the city test-file migration**

```bash
rtk git -C C:\dev\helprojs\demodisc add assets/codebase/gameplay.tests assets/codebase/game.tools.tests assets/codebase/rendering.tools.tests tests/gameplay.tests/gameplay.tests.csproj
rtk git -C C:\dev\helprojs\demodisc commit -m "refactor: move city tests into generated surfaces"
```

### Task 5: Regenerate City Projects Through The Editor CLI And Run The New Generated Test Projects

**Files:**
- Verify: `C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj`
- Verify: `C:\dev\helprojs\demodisc\user_settings\generated_code\projects\gameplay.tests\gameplay.tests.csproj`
- Verify: `C:\dev\helprojs\demodisc\user_settings\generated_code\projects\game.tools.tests\game.tools.tests.csproj`
- Verify: `C:\dev\helprojs\demodisc\user_settings\generated_code\projects\rendering.tools.tests\rendering.tools.tests.csproj`

- [ ] **Step 1: Force regeneration through the headless editor build path**

Run:

```bash
rtk dotnet run --project C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\helengine.editor.app.csproj -- --project C:\dev\helprojs\demodisc --build windows --output C:\dev\helprojs\output\windows-generated-test-projects-smoke
```

Expected: PASS. The headless build path calls `EditorGameSolutionService` during script build/reload and rewrites generated projects under `C:\dev\helprojs\demodisc\user_settings\generated_code\projects\`.

- [ ] **Step 2: Run the generated runtime test project**

Run:

```bash
rtk dotnet test C:\dev\helprojs\demodisc\user_settings\generated_code\projects\gameplay.tests\gameplay.tests.csproj --filter "TiltTrialLevelCatalogTests|TiltTrialLevelSettingsComponentTests|TiltTrialSessionComponentTests|DemoTiltStageComponentTests|DemoDiscReturnInputUtilsTests" -v minimal
```

Expected: PASS.

- [ ] **Step 3: Run the generated editor-surface test projects**

Run:

```bash
rtk dotnet test C:\dev\helprojs\demodisc\user_settings\generated_code\projects\game.tools.tests\game.tools.tests.csproj --filter "TiltTrialCameraAuthoringTests|TiltTrialLightingAuthoringTests|TiltTrialSceneGenerationSourceTests" -v minimal
rtk dotnet test C:\dev\helprojs\demodisc\user_settings\generated_code\projects\rendering.tools.tests\rendering.tools.tests.csproj --filter "TiltTrialCourseMaterialAuthoringTests|TiltTrialMaterialColorAuthoringTests" -v minimal
```

Expected: PASS.

- [ ] **Step 4: Confirm the generated solution now lists all test projects**

Run:

```bash
rtk powershell -NoProfile -Command "Get-Content -Raw 'C:\dev\helprojs\demodisc\city.sln'"
```

Expected to contain:

```text
user_settings/generated_code/projects/gameplay.tests/gameplay.tests.csproj
user_settings/generated_code/projects/game.tools.tests/game.tools.tests.csproj
user_settings/generated_code/projects/rendering.tools.tests/rendering.tools.tests.csproj
```

- [ ] **Step 5: Commit the final verification state if the regeneration changed tracked generated outputs**

```bash
rtk git -C C:\dev\helprojs\demodisc status --short
rtk git -C C:\dev\helprojs\demodisc add city.sln user_settings/generated_code/projects
rtk git -C C:\dev\helprojs\demodisc commit -m "test: verify generated city test projects"
```

## Self-Review

- Spec coverage: Tasks 1-3 cover fail-hard orphan handling, one generated test project per surface, shared solution membership, strict production-project references, raw-folder discovery, and editor-surface global usings. Tasks 4-5 cover the city migration off the legacy repo-owned test csproj and run the generated test projects end-to-end.
- Intentional correction: this plan uses `gameplay.tests` instead of `game.tests` because the current city runtime surface is the fallback production module id `gameplay`. Changing fallback module naming is a separate problem and intentionally out of scope here.
- Placeholder scan: the plan contains no unresolved placeholder markers. Every code-edit step includes the concrete file paths, code shape, and verification commands needed to execute it.
