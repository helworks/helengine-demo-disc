namespace city.tests {
    /// <summary>
    /// Verifies the runtime Tilt Trial scene ids stay aligned with the editor/build pipeline's scene-id derivation.
    /// </summary>
    public sealed class TiltTrialSceneIdsTests {
        [Fact]
        public void Tilt_trial_scene_ids_match_scene_id_utility_derivation() {
            Assert.Equal(city.game.TiltTrialSceneIds.LevelSelectSceneId, helengine.SceneIdUtility.FromPath("scenes/games/tilt_trial.helen"));
            Assert.Equal(city.game.TiltTrialSceneIds.Level01SceneId, helengine.SceneIdUtility.FromPath("scenes/games/tilt_trial_level_01.helen"));
            Assert.Equal(city.game.TiltTrialSceneIds.Level02SceneId, helengine.SceneIdUtility.FromPath("scenes/games/tilt_trial_level_02.helen"));
            Assert.Equal(city.game.TiltTrialSceneIds.Level03SceneId, helengine.SceneIdUtility.FromPath("scenes/games/tilt_trial_level_03.helen"));
            Assert.Equal(city.game.TiltTrialSceneIds.Level04SceneId, helengine.SceneIdUtility.FromPath("scenes/games/tilt_trial_level_04.helen"));
            Assert.Equal(city.game.TiltTrialSceneIds.Level05SceneId, helengine.SceneIdUtility.FromPath("scenes/games/tilt_trial_level_05.helen"));
        }
    }
}
