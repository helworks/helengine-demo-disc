using DemoDisc.EditorTools;

namespace DemoDisc.EditorTools.tests {
    public sealed class TiltTrialReferenceAuthoringTests {
        [Fact]
        public void Gameplay_scene_persists_stage_camera_and_hud_links() {
            SceneAsset scene = ReadScene("tilt_trial_level_01.helen");
            SceneEntityAsset stage = FindByName(scene.RootEntities, "StageRoot");
            SceneEntityAsset camera = FindByName(scene.RootEntities, "TiltTrialCamera");
            SceneEntityAsset player = FindByName(scene.RootEntities, "PlayerSphere");
            SceneEntityAsset speedText = FindByName(scene.RootEntities, "TiltTrialSpeedText");

            DemoDisc.TiltPlay.DemoTiltStageComponent stageComponent = ReadComponent<DemoDisc.TiltPlay.DemoTiltStageComponent>(stage);
            DemoDisc.TiltPlay.DemoTiltFollowCameraComponent cameraComponent = ReadComponent<DemoDisc.TiltPlay.DemoTiltFollowCameraComponent>(camera);
            DemoDisc.TiltPlay.DemoTiltSpeedTextComponent speedComponent = ReadComponent<DemoDisc.TiltPlay.DemoTiltSpeedTextComponent>(speedText);
            Assert.Equal(player.Id, stageComponent.PlayerSphereReference?.EntityId);
            Assert.Equal(camera.Id, stageComponent.OrbitCameraReference?.EntityId);
            Assert.Equal(player.Id, cameraComponent.TargetEntityReference?.EntityId);
            Assert.Equal(player.Id, speedComponent.TargetEntityReference?.EntityId);
        }

        [Fact]
        public void Title_scene_persists_menu_panel_and_selected_overlay_links() {
            SceneAsset scene = ReadScene("tilt_trial.helen");
            SceneEntityAsset shell = FindByName(scene.RootEntities, "TiltPlayShellUi");
            DemoDisc.TiltPlay.TiltPlayMenuComponent menu = ReadComponent<DemoDisc.TiltPlay.TiltPlayMenuComponent>(shell);
            Assert.Equal(FindByName(scene.RootEntities, "TiltPlayTitlePanel").Id, menu.TitlePanelReference?.EntityId);
            Assert.Equal(FindByName(scene.RootEntities, "TiltPlayOptionsPanel").Id, menu.OptionsPanelReference?.EntityId);
            Assert.Equal(FindByName(scene.RootEntities, "TiltTrialLevelSelectUi").Id, menu.LevelSelectPanelReference?.EntityId);
            Assert.Equal(FindByName(scene.RootEntities, "TiltPlayPlayButtonSelectedOverlay").Id, menu.PlaySelectedOverlayReference?.EntityId);
            Assert.Equal(FindByName(scene.RootEntities, "TiltPlayOptionsButtonSelectedOverlay").Id, menu.OptionsSelectedOverlayReference?.EntityId);
            Assert.Equal(FindByName(scene.RootEntities, "TiltPlayDemoDiscButtonSelectedOverlay").Id, menu.DemoDiscSelectedOverlayReference?.EntityId);
        }

        static SceneAsset ReadScene(string filename) {
            string path = DemoDisc.testing.DemoDiscTestProject.GetPath("assets", "scenes", "games", "tilt", filename);
            using FileStream stream = File.OpenRead(path);
            return (SceneAsset)global::helengine.AssetSerializer.Deserialize(stream);
        }

        static SceneEntityAsset FindByName(SceneEntityAsset[] roots, string name) {
            foreach (SceneEntityAsset root in roots ?? Array.Empty<SceneEntityAsset>()) {
                SceneEntityAsset match = FindByName(root, name);
                if (match != null) {
                    return match;
                }
            }
            throw new InvalidOperationException($"Authored scene is missing '{name}'.");
        }

        static SceneEntityAsset FindByName(SceneEntityAsset entity, string name) {
            if (entity.Name == name) {
                return entity;
            }
            foreach (SceneEntityAsset child in entity.Children ?? Array.Empty<SceneEntityAsset>()) {
                SceneEntityAsset match = FindByName(child, name);
                if (match != null) {
                    return match;
                }
            }
            return null;
        }

        static TComponent ReadComponent<TComponent>(SceneEntityAsset entity) where TComponent : Component {
            ComponentPersistenceRegistry registry = new ComponentPersistenceRegistry();
            foreach (SceneComponentAssetRecord record in entity.Components ?? Array.Empty<SceneComponentAssetRecord>()) {
                if (record.ComponentTypeId == AutomaticScriptComponentPersistenceDescriptor.BuildComponentTypeId(typeof(TComponent))) {
                    return (TComponent)registry.GetDescriptor(record.ComponentTypeId).DeserializeComponent(record, null, null);
                }
            }
            throw new InvalidOperationException($"Authored '{entity.Name}' is missing {typeof(TComponent).Name}.");
        }
    }
}
