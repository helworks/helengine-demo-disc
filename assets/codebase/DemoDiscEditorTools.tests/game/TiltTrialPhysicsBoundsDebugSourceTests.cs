using DemoDisc.EditorTools;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Verifies Tilt Trial gameplay scenes wire the Windows-only physics bounds debug overlay and keep its F3 toggle contract stable.
    /// </summary>
    public sealed class TiltTrialPhysicsBoundsDebugSourceTests {
        [Fact]
        public void Game_scene_factory_mounts_the_tilt_trial_physics_bounds_debug_root() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\DemoDiscEditorTools\game\GameSceneFactory.cs");

            Assert.Contains("CreatePhysicsBoundsDebugEntity()", source, StringComparison.Ordinal);
            Assert.Contains("Create(\"TiltTrialPhysicsBoundsDebug\")", source, StringComparison.Ordinal);
            Assert.Contains("entity.AddComponent(new global::DemoDisc.TiltPlay.TiltTrialPhysicsBoundsDebugDrawComponent());", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Tilt_trial_physics_bounds_debug_component_keeps_the_windows_only_f3_toggle_contract() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\TiltPlay\TiltTrialPhysicsBoundsDebugDrawComponent.cs");

            Assert.Contains("const Keys ToggleKey = Keys.F3;", source, StringComparison.Ordinal);
            Assert.Contains("const string WindowsPlatformId = \"windows\";", source, StringComparison.Ordinal);
            Assert.Contains("#if DESKTOP_PLATFORM", source, StringComparison.Ordinal);
            Assert.Contains("core.Input.WasKeyPressed(ToggleKey)", source, StringComparison.Ordinal);
            Assert.Contains("core.Input.IsKeyDown(ToggleKey)", source, StringComparison.Ordinal);
            Assert.Contains("string.Equals(core.PlatformInfo.Name, WindowsPlatformId, StringComparison.OrdinalIgnoreCase)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Tilt_trial_physics_bounds_debug_behavior_is_compiled_only_for_debug_environment() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\TiltPlay\TiltTrialPhysicsBoundsDebugDrawComponent.cs");

            Assert.Contains("#if HELENGINE_ENV_DEBUG && DESKTOP_PLATFORM", source, StringComparison.Ordinal);
            Assert.Contains("#if HELENGINE_ENV_DEBUG", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Tilt_trial_physics_bounds_debug_component_scales_authored_box_collider_size_by_entity_scale() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\TiltPlay\TiltTrialPhysicsBoundsDebugDrawComponent.cs");

            Assert.Contains("boxBounds.Size.X * entityScale.X", source, StringComparison.Ordinal);
            Assert.Contains("float3 halfExtents = CreateBoxHalfExtents(scaledSize);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Game_scene_factory_adds_one_visible_bounds_status_hud_row() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\DemoDiscEditorTools\game\GameSceneFactory.cs");

            Assert.Contains("\"TiltTrialPhysicsBoundsStatusText\"", source, StringComparison.Ordinal);
            Assert.Contains("\"F3 Bounds Off\"", source, StringComparison.Ordinal);
            Assert.Contains("physicsBoundsStatusTextEntity.AddComponent(new DemoDisc.TiltPlay.TiltTrialPhysicsBoundsStatusTextComponent());", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the F3 status row is debug-only on every device: build config first, absent in release, no platform named.
        /// </summary>
        [Fact]
        public void Game_scene_factory_cooks_f3_status_row_in_debug_builds_only() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\DemoDiscEditorTools\game\GameSceneFactory.cs");

            Assert.Contains("EntitySaveComponent physicsBoundsStatusTextEntitySaveComponent = FindRequiredEntitySaveComponent(physicsBoundsStatusTextEntity);", source, StringComparison.Ordinal);
            Assert.Contains("physicsBoundsStatusTextEntitySaveComponent.OverrideLevelOrder = DemoDiscOverrideScopes.CreateBuildConfigFirstLevelOrder();", source, StringComparison.Ordinal);
            Assert.Contains("physicsBoundsStatusTextEntitySaveComponent.GetOrCreateExistencePlatformOverride(DemoDiscOverrideScopes.Release).Exists = false;", source, StringComparison.Ordinal);
            Assert.DoesNotContain("nonWindowsPlatformIds", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Game_scene_factory_excludes_physics_bounds_debug_root_from_release_cooks() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\DemoDiscEditorTools\game\GameSceneFactory.cs");

            Assert.Contains("saveComponent.GetOrCreateExistencePlatformOverride(DemoDiscOverrideScopes.Release).Exists = false;", source, StringComparison.Ordinal);
            Assert.DoesNotContain("EditorOverrideScope(\"windows\", \"release\")", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Tilt_trial_presentation_attachment_excludes_physics_bounds_root_from_release_cooks() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\DemoDiscEditorTools\game\TiltTrialGameplayPresentationAttachmentService.cs");

            Assert.Contains("Scope = DemoDiscOverrideScopes.Release.ToSteps(), Exists = false", source, StringComparison.Ordinal);
            Assert.Contains("Scope = DemoDiscOverrideScopes.DebugDualScreen.ToSteps(), Exists = false", source, StringComparison.Ordinal);
            Assert.Contains("root.OverrideLevelOrder = DemoDiscOverrideScopes.CreateBuildConfigFirstLevelOrder();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Tilt_trial_console_presentation_keeps_windows_release_scope_available() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\DemoDiscEditorTools\game\TiltTrialGameplayPresentationAttachmentService.cs");

            Assert.Contains("CreateConsolePresentationPlatformOverrides()", source, StringComparison.Ordinal);
            Assert.Contains("root.PlatformExistenceOverrides = CreateWindowsOnlyDebugPlatformOverrides();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Tilt_trial_presentation_attachment_prunes_f3_status_row_from_release() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\DemoDiscEditorTools\game\TiltTrialGameplayPresentationAttachmentService.cs");

            Assert.Contains("ApplyWindowsOnlyDebugStatusOverrideToConsoleBlueprint(fullProjectRootPath);", source, StringComparison.Ordinal);
            Assert.Contains("statusText.PlatformExistenceOverrides = CreateWindowsOnlyDebugStatusOverrides();", source, StringComparison.Ordinal);
            Assert.Contains("statusText.OverrideLevelOrder = DemoDiscOverrideScopes.CreateBuildConfigFirstLevelOrder();", source, StringComparison.Ordinal);
        }
    }
}
