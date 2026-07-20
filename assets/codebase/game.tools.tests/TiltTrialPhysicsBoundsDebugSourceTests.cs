namespace city.tests {
    /// <summary>
    /// Verifies Tilt Trial gameplay scenes wire the Windows-only physics bounds debug overlay and keep its F3 toggle contract stable.
    /// </summary>
    public sealed class TiltTrialPhysicsBoundsDebugSourceTests {
        [Fact]
        public void Game_scene_factory_mounts_the_tilt_trial_physics_bounds_debug_root() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("CreatePhysicsBoundsDebugEntity()", source, StringComparison.Ordinal);
            Assert.Contains("Create(\"TiltTrialPhysicsBoundsDebug\")", source, StringComparison.Ordinal);
            Assert.Contains("entity.AddComponent(new global::city.game.TiltTrialPhysicsBoundsDebugDrawComponent());", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Tilt_trial_physics_bounds_debug_component_keeps_the_windows_only_f3_toggle_contract() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\TiltTrialPhysicsBoundsDebugDrawComponent.cs");

            Assert.Contains("const Keys ToggleKey = Keys.F3;", source, StringComparison.Ordinal);
            Assert.Contains("const string WindowsPlatformId = \"windows\";", source, StringComparison.Ordinal);
            Assert.Contains("core.Input.WasKeyPressed(ToggleKey)", source, StringComparison.Ordinal);
            Assert.Contains("core.Input.IsKeyDown(ToggleKey)", source, StringComparison.Ordinal);
            Assert.Contains("string.Equals(core.PlatformInfo.Name, WindowsPlatformId, StringComparison.OrdinalIgnoreCase)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Tilt_trial_physics_bounds_debug_component_uses_authored_box_collider_size_directly() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game\TiltTrialPhysicsBoundsDebugDrawComponent.cs");

            Assert.Contains("float3 halfExtents = CreateBoxHalfExtents(boxBounds.Size);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("CreateBoxHalfExtents(boxBounds.Size, entity.LocalScale)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Game_scene_factory_adds_one_visible_bounds_status_hud_row() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("\"TiltTrialPhysicsBoundsStatusText\"", source, StringComparison.Ordinal);
            Assert.Contains("\"F3 Bounds Off\"", source, StringComparison.Ordinal);
            Assert.Contains("physicsBoundsStatusTextEntity.AddComponent(new city.game.TiltTrialPhysicsBoundsStatusTextComponent());", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the F3 status row is removed from every non-Windows cooked gameplay scene.
        /// </summary>
        [Fact]
        public void Game_scene_factory_cooks_f3_status_row_only_for_windows() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("EntitySaveComponent physicsBoundsStatusTextEntitySaveComponent = FindRequiredEntitySaveComponent(physicsBoundsStatusTextEntity);", source, StringComparison.Ordinal);
            Assert.Contains("string[] nonWindowsPlatformIds = [\"ps2\", \"psp\", \"psvita\", \"gamecube\", \"wii\", \"wiiu\", \"switch\", \"ds\", \"3ds\"]", source, StringComparison.Ordinal);
            Assert.Contains("GetOrCreateExistencePlatformOverride(nonWindowsPlatformIds[platformIndex]).Exists = false", source, StringComparison.Ordinal);
        }
    }
}
