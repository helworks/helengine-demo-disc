namespace city.tests {
    /// <summary>
    /// Verifies the first Tilt Trial gameplay level now uses a dedicated beginner layout with collectible coins and a finish flag blueprint.
    /// </summary>
    public sealed class TiltTrialLevel01SceneSourceTests {
        [Fact]
        public void Game_scene_factory_authors_dedicated_cube_layouts_for_levels_02_through_05() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("CreateTiltTrialLevel02StageRootEntity()", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel03StageRootEntity()", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel04StageRootEntity()", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel05StageRootEntity()", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel02StageRootEntity", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel03StageRootEntity", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel04StageRootEntity", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel05StageRootEntity", source, StringComparison.Ordinal);
            Assert.Contains("Level02StartPad", source, StringComparison.Ordinal);
            Assert.Contains("Level03Platform01", source, StringComparison.Ordinal);
            Assert.Contains("Level04Blocker03", source, StringComparison.Ordinal);
            Assert.Contains("Level05Platform04", source, StringComparison.Ordinal);
        }

        [Fact]
        public void Game_scene_factory_authors_dedicated_level_01_layout_with_beginner_collectibles_and_flag() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("CreateStageRootEntity(levelEntry)", source, StringComparison.Ordinal);
            Assert.Contains("CreateTiltTrialLevel01StageRootEntity()", source, StringComparison.Ordinal);
            Assert.Contains("CreateGoalFlagEntity()", source, StringComparison.Ordinal);
            Assert.DoesNotContain("CreateGoalPadEntity(", source, StringComparison.Ordinal);
            Assert.Contains("CreateCollectibleCoinEntity(", source, StringComparison.Ordinal);
            Assert.Contains("SplitPlayAssetCatalog.GoldenCoinBlueprintRelativePath", source, StringComparison.Ordinal);
            Assert.Contains("SplitPlayAssetCatalog.GoalFlagBlueprintRelativePath", source, StringComparison.Ordinal);
            Assert.Contains("new BlueprintInstanceComponent", source, StringComparison.Ordinal);
            Assert.Contains("entity.LocalScale = new float3(0.51f, 0.51f, 0.51f);", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the render-test scene uses one constrained-platform tessellated cube for near-camera clipping diagnostics.
        /// </summary>
        [Fact]
        public void Game_scene_factory_creates_one_tessellated_clipping_probe_cube() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("CreateLevel01RenderOnlyCourseBoxEntity(\"ClipProbeCube\", float3.Zero, new float3(5f, 1f, 5f), float4.Identity, true)", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the playable first Tilt Trial level applies constrained-platform tessellation to its scaled course surfaces and side walls.
        /// </summary>
        [Fact]
        public void Game_scene_factory_configures_playable_level_01_walls_and_ground_for_ps2_and_psp_tessellation() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\GameSceneFactory.cs");

            Assert.Contains("CreateKinematicCourseBoxEntity(\"StartPad\", new float3(0f, 0f, -6.6f), new float3(7f, 1f, 7f), float4.Identity, true)", source, StringComparison.Ordinal);
            Assert.Contains("CreateKinematicCourseBoxEntity(\"Ramp\", new float3(0f, -0.05f, -0.1f), new float3(6f, 0.9f, 8f), orientation, true)", source, StringComparison.Ordinal);
            Assert.Contains("CreateKinematicCourseBoxEntity(\"Bridge\", new float3(0f, 0.5f, 5.8f), new float3(2.5f, 1f, 11.5f), float4.Identity, true)", source, StringComparison.Ordinal);
            Assert.Contains("CreateKinematicCourseBoxEntity(\"FinalPlatform\", new float3(1.35f, 0.2f, 13.8f), new float3(8.4f, 1f, 8.8f), float4.Identity, true)", source, StringComparison.Ordinal);
            Assert.Contains("CreateKinematicCourseBoxEntity(\"LeftWall\", new float3(-3.1f, 1.25f, 2.8f), new float3(0.8f, 2.8f, 19.8f), float4.Identity, true)", source, StringComparison.Ordinal);
            Assert.Contains("CreateKinematicCourseBoxEntity(\"RightWall\", new float3(3.1f, 1.25f, 2.8f), new float3(0.8f, 2.8f, 19.8f), float4.Identity, true)", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the existing authored playable Level 01 scene can receive the same constrained-platform tessellation metadata without regenerating gameplay content.
        /// </summary>
        [Fact]
        public void Level_01_tessellation_authoring_command_targets_the_existing_playable_scene() {
            string commandSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\ApplyTiltTrialLevel01TessellationCommand.cs");
            string serviceSource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\game.tools\TiltTrialLevel01TessellationAuthoringService.cs");

            Assert.Contains("menu.apply-tilt-trial-level-01-tessellation", commandSource, StringComparison.Ordinal);
            Assert.Contains("ApplyToAuthoredLevel01Scene", commandSource, StringComparison.Ordinal);
            Assert.Contains("tilt_trial_level_01.helen", serviceSource, StringComparison.Ordinal);
            Assert.Contains("StartPad", serviceSource, StringComparison.Ordinal);
            Assert.Contains("Ramp", serviceSource, StringComparison.Ordinal);
            Assert.Contains("Bridge", serviceSource, StringComparison.Ordinal);
            Assert.Contains("FinalPlatform", serviceSource, StringComparison.Ordinal);
            Assert.Contains("LeftWall", serviceSource, StringComparison.Ordinal);
            Assert.Contains("RightWall", serviceSource, StringComparison.Ordinal);
            Assert.Contains("MeshBakeScaleMemberName = \"MeshBakeScale\"", serviceSource, StringComparison.Ordinal);
            Assert.Contains("pspOverride.SetMemberValue(MeshBakeScaleMemberName, true.ToString(CultureInfo.InvariantCulture))", serviceSource, StringComparison.Ordinal);
        }
    }
}
