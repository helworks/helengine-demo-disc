using city.menu;
using city.rendering.tools;
using helengine.editor;

namespace city.game.tools {
    /// <summary>
    /// Builds the generated authored gameplay scenes contributed by the city demo-disc project.
    /// </summary>
    public sealed class GameSceneFactory {
        /// <summary>
        /// Stable authored material asset id required by the Tilt Trial player sphere.
        /// </summary>
        const string TiltTrialPlayerSphereMarbleMaterialAssetId = "Materials.rendering.tilt_trial.PlayerSphereMarble";

        /// <summary>
        /// Stable authored material settings path required by the Tilt Trial player sphere.
        /// </summary>
        const string TiltTrialPlayerSphereMarbleMaterialRelativePath = "materials/rendering/tilt_trial/PlayerSphereMarble.hasset";

        /// <summary>
        /// Stable authored material asset id required by the Tilt Trial course geometry.
        /// </summary>
        const string TiltTrialCourseMaterialAssetId = "Materials.rendering.tilt_trial.Course";

        /// <summary>
        /// Stable authored material settings path required by the Tilt Trial course geometry.
        /// </summary>
        const string TiltTrialCourseMaterialRelativePath = "materials/rendering/tilt_trial/Course.hasset";

        /// <summary>
        /// Stable authored source-font path used by the Tilt Trial speed HUD.
        /// </summary>
        const string TiltTrialSpeedHudFontRelativePath = "Fonts/Fredoka.ttf";

        /// <summary>
        /// Stable mesh save-state slot used by the generated player sphere material reference.
        /// </summary>
        const string PlayerSphereMaterialReferenceName = "Materials[0]";

        /// <summary>
        /// Shared generated cube model used by the authored Tilt Trial course geometry.
        /// </summary>
        readonly RuntimeModel GeneratedCubeModel;

        /// <summary>
        /// Shared generated sphere model used by the playable ball.
        /// </summary>
        readonly RuntimeModel GeneratedSphereModel;

        /// <summary>
        /// Stable mesh save-state slot used by the generated course material reference.
        /// </summary>
        const string CourseMaterialReferenceName = "Materials[0]";

        /// <summary>
        /// Dedicated authored marble material used only by the Tilt Trial player sphere.
        /// </summary>
        readonly RuntimeMaterial TiltTrialPlayerSphereMarbleMaterial;

        /// <summary>
        /// Dedicated authored course material used by the Tilt Trial stage pieces and catch floor.
        /// </summary>
        readonly RuntimeMaterial TiltTrialCourseMaterial;

        /// <summary>
        /// Initializes one game-scene factory backed by the prepared generated runtime assets required by the authored gameplay scenes.
        /// </summary>
        /// <param name="assets">Prepared runtime assets consumed by the generated game scenes.</param>
        public GameSceneFactory(RenderingSceneGenerationAssets assets) {
            if (assets == null) {
                throw new ArgumentNullException(nameof(assets));
            } else if (assets.GeneratedCubeModel == null) {
                throw new ArgumentException("Game scene generation requires the generated cube runtime model.", nameof(assets));
            } else if (assets.GeneratedSphereModel == null) {
                throw new ArgumentException("Game scene generation requires the generated sphere runtime model.", nameof(assets));
            } else if (assets.TiltTrialPlayerSphereMarbleMaterial == null) {
                throw new ArgumentException($"Game scene generation requires authored runtime material '{TiltTrialPlayerSphereMarbleMaterialAssetId}'.", nameof(assets));
            } else if (assets.TiltTrialCourseMaterial == null) {
                throw new ArgumentException($"Game scene generation requires authored runtime material '{TiltTrialCourseMaterialAssetId}'.", nameof(assets));
            }

            GeneratedCubeModel = assets.GeneratedCubeModel;
            GeneratedSphereModel = assets.GeneratedSphereModel;
            TiltTrialPlayerSphereMarbleMaterial = assets.TiltTrialPlayerSphereMarbleMaterial;
            TiltTrialCourseMaterial = assets.TiltTrialCourseMaterial;
        }

        /// <summary>
        /// Creates the generated authored Tilt Trial front-door scene.
        /// </summary>
        /// <returns>Generated authored scene definition for Tilt Trial.</returns>
        public GeneratedAuthoringSceneDefinition CreateTiltTrialScene() {
            return CreateTiltTrialLevelSelectScene();
        }

        /// <summary>
        /// Creates the dedicated generated authored Tilt Trial level-select scene.
        /// </summary>
        /// <returns>Generated authored scene definition for the Tilt Trial selector.</returns>
        public GeneratedAuthoringSceneDefinition CreateTiltTrialLevelSelectScene() {
            return new GeneratedAuthoringSceneDefinition {
                SceneId = GameSceneCatalog.TiltTrialSceneId,
                SceneSettings = new SceneSettingsAsset(),
                RootEntities = [
                    CreateLevelSelectCameraEntity(),
                    CreateLevelSelectUiEntity()
                ]
            };
        }

        /// <summary>
        /// Creates the scaffolded generated authored Tilt Trial gameplay level scenes.
        /// </summary>
        /// <returns>Ordered scaffolded gameplay scenes for all current Tilt Trial levels.</returns>
        public IReadOnlyList<GeneratedAuthoringSceneDefinition> CreateTiltTrialLevelScenes() {
            IReadOnlyList<global::city.game.TiltTrialLevelCatalogEntry> levelEntries = global::city.game.TiltTrialLevelCatalog.CreateEntries();
            GeneratedAuthoringSceneDefinition[] scenes = new GeneratedAuthoringSceneDefinition[levelEntries.Count];
            for (int index = 0; index < levelEntries.Count; index++) {
                scenes[index] = CreateTiltTrialGameplayScene(levelEntries[index]);
            }

            return scenes;
        }

        /// <summary>
        /// Creates one generated authored Tilt Trial gameplay scene from the supplied shared level metadata entry.
        /// </summary>
        /// <param name="levelEntry">Shared level entry defining scene id and timer metadata.</param>
        /// <returns>Generated authored gameplay scene.</returns>
        GeneratedAuthoringSceneDefinition CreateTiltTrialGameplayScene(global::city.game.TiltTrialLevelCatalogEntry levelEntry) {
            if (levelEntry == null) {
                throw new ArgumentNullException(nameof(levelEntry));
            }

            EditorEntity cameraEntity = CreateCameraEntity();
            EditorEntity stageRootEntity = CreateStageRootEntity();
            EditorEntity playerSphereEntity = CreatePlayerSphereEntity();
            EditorEntity uiEntity = CreateGameplayUiEntity(levelEntry);
            Entity[] roots = new Entity[] {
                CreateLevelMetadataEntity(levelEntry),
                cameraEntity,
                CreateDirectionalLightEntity(),
                CreateDirectionalFillLightEntity(),
                uiEntity,
                stageRootEntity,
                CreateCatchFloorEntity(),
                playerSphereEntity
            };

            ConfigureTiltTrialCameraTarget(cameraEntity, playerSphereEntity);
            ConfigureTiltTrialSpeedTextTarget(uiEntity, playerSphereEntity);
            return new GeneratedAuthoringSceneDefinition {
                SceneId = levelEntry.SceneId,
                SceneSettings = new SceneSettingsAsset(),
                RootEntities = roots
            };
        }

        /// <summary>
        /// Creates the authored camera used by the dedicated Tilt Trial selector scene.
        /// </summary>
        /// <returns>Generated selector camera entity.</returns>
        Entity CreateLevelSelectCameraEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialLevelSelectCamera");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 20f,
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(18f / 255f, 27f / 255f, 43f / 255f, 1f),
                    true,
                    1f,
                    false,
                    0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Auto,
                    ShadowDistance = 0f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored third-person camera root used by Tilt Trial.
        /// </summary>
        /// <returns>Generated editor camera entity.</returns>
        EditorEntity CreateCameraEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, -0.42f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialCamera");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 2.74425f, -3.08f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 120f,
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(100f / 255f, 149f / 255f, 237f / 255f, 1f),
                    true,
                    1f,
                    false,
                    0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Auto,
                    ShadowDistance = 72f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            entity.AddComponent(new city.game.DemoTiltFollowCameraComponent {
                TargetOffset = new float3(0f, 0.65f, 0f)
            });

            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Tilt Trial camera generation requires editor-authored entities.");
        }

        /// <summary>
        /// Creates the authored directional light used by Tilt Trial.
        /// </summary>
        /// <returns>Generated light entity.</returns>
        Entity CreateDirectionalLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(-0.6f, -0.95f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialSun");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 8f, 0f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(1f, 0.97f, 0.92f, 1f),
                Intensity = 1.15f,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 0.95f,
                ShadowDistance = 72f
            });
            return entity;
        }

        /// <summary>
        /// Creates one weaker directional fill light that lifts the sphere's unlit hemisphere without adding a second shadow pass.
        /// </summary>
        /// <returns>Generated fill-light entity.</returns>
        Entity CreateDirectionalFillLightEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(2.45f, -0.32f, 0f, out orientation);

            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialFill");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 6f, 0f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = orientation;
            entity.AddComponent(new DirectionalLightComponent {
                Color = new float4(0.78f, 0.84f, 1f, 1f),
                Intensity = 0.7f,
                ShadowsEnabled = false,
                ShadowMapMode = ShadowMapMode.Disabled,
                ShadowStrength = 0f,
                ShadowDistance = 0f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored UI root that drives the dedicated Tilt Trial level-select scene.
        /// </summary>
        /// <returns>Generated selector UI root entity.</returns>
        EditorEntity CreateLevelSelectUiEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialLevelSelectUi");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.AddComponent(new DemoDiscReturnToMenuComponent());
            entity.AddComponent(new city.game.TiltTrialLevelSelectComponent());
            entity.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.ScreenBindingMode,
                FixedSize = new int2(1280, 720)
            });
            entity.AddComponent(new ReferenceCanvasFitComponent {
                ReferenceWidth = 1280,
                ReferenceHeight = 720
            });

            Entity listPanelEntity = CreateRoundedPanelEntity(entity, "TiltTrialLevelSelectListPanel", new float3(40f, 52f, 0f), new int2(420, 616), 28f, 3f, new byte4(26, 40, 61, 255), new byte4(96, 128, 168, 255), 1);
            Entity detailsPanelEntity = CreateRoundedPanelEntity(entity, "TiltTrialLevelSelectDetailsPanel", new float3(500f, 52f, 0f), new int2(740, 616), 28f, 3f, new byte4(26, 40, 61, 255), new byte4(96, 128, 168, 255), 1);

            CreateUiTextEntity(entity, "TiltTrialLevelSelectTitle", new float3(52f, 18f, 0.1f), "Tilt Trial", new int2(420, 48), 2.5f, 2, new byte4(247, 248, 252, 255), TextAlignment.Left);
            CreateUiTextEntity(entity, "TiltTrialLevelSelectHint", new float3(500f, 18f, 0.1f), "Enter Play   Esc Menu", new int2(460, 40), 1.25f, 2, new byte4(196, 210, 226, 255), TextAlignment.Left);
            CreateUiTextEntity(detailsPanelEntity, "TiltTrialLevelSelectName", new float3(28f, 24f, 0.1f), "Level 1", new int2(420, 56), 2.2f, 3, new byte4(247, 248, 252, 255), TextAlignment.Left);
            CreateUiTextEntity(detailsPanelEntity, "TiltTrialLevelSelectTimer", new float3(28f, 86f, 0.1f), "Start 99.00", new int2(220, 36), 1.4f, 3, new byte4(255, 214, 138, 255), TextAlignment.Left);
            CreateUiTextEntity(detailsPanelEntity, "TiltTrialLevelSelectMedals", new float3(28f, 132f, 0.1f), "Gold  18.00\nSilver 28.00\nBronze 40.00", new int2(260, 120), 1.2f, 3, new byte4(223, 230, 239, 255), TextAlignment.Left);

            Entity previewPanelEntity = CreateRoundedPanelEntity(detailsPanelEntity, "TiltTrialLevelSelectPreviewPanel", new float3(390f, 24f, 0f), new int2(300, 300), 24f, 2f, new byte4(39, 57, 84, 255), new byte4(122, 147, 182, 255), 2);
            CreateUiTextEntity(previewPanelEntity, "TiltTrialLevelSelectPreviewPlaceholder", new float3(28f, 118f, 0.1f), "Preview Coming Soon", new int2(244, 64), 1.25f, 3, new byte4(223, 230, 239, 255), TextAlignment.Center);

            IReadOnlyList<global::city.game.TiltTrialLevelCatalogEntry> levelEntries = global::city.game.TiltTrialLevelCatalog.CreateEntries();
            for (int index = 0; index < levelEntries.Count; index++) {
                float top = 28f + (index * 108f);
                int oneBasedIndex = index + 1;
                Entity rowEntity = CreateRoundedPanelEntity(listPanelEntity, $"TiltTrialLevelRow{oneBasedIndex:00}", new float3(24f, top, 0f), new int2(372, 88), 18f, 2f, new byte4(40, 58, 87, 255), new byte4(109, 138, 170, 255), 2);
                CreateUiTextEntity(rowEntity, $"TiltTrialLevelRow{oneBasedIndex:00}Label", new float3(20f, 24f, 0.1f), levelEntries[index].DisplayName, new int2(320, 40), 1.55f, 3, new byte4(247, 248, 252, 255), TextAlignment.Left);
            }

            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Tilt Trial selector generation requires editor-authored entities.");
        }

        /// <summary>
        /// Creates one hidden metadata root entity that stores the shared authored level settings for a generated gameplay scene.
        /// </summary>
        /// <param name="levelEntry">Shared level entry defining scene id and timer metadata.</param>
        /// <returns>Generated metadata root entity.</returns>
        Entity CreateLevelMetadataEntity(global::city.game.TiltTrialLevelCatalogEntry levelEntry) {
            if (levelEntry == null) {
                throw new ArgumentNullException(nameof(levelEntry));
            }

            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialLevelMetadata");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.AddComponent(new city.game.TiltTrialLevelSettingsComponent {
                LevelId = levelEntry.LevelId,
                DisplayName = levelEntry.DisplayName,
                SceneId = levelEntry.SceneId,
                StartTimeSeconds = levelEntry.StartTimeSeconds,
                GoldTimeSeconds = levelEntry.GoldTimeSeconds,
                SilverTimeSeconds = levelEntry.SilverTimeSeconds,
                BronzeTimeSeconds = levelEntry.BronzeTimeSeconds,
                PreviewTexturePath = levelEntry.PreviewTexturePath
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored UI root that owns gameplay HUD and session overlay behavior while a Tilt Trial level is active.
        /// </summary>
        /// <returns>Generated UI root entity.</returns>
        EditorEntity CreateGameplayUiEntity(global::city.game.TiltTrialLevelCatalogEntry levelEntry) {
            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialUi");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.AddComponent(new city.game.TiltTrialSessionComponent());
            entity.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.ScreenBindingMode,
                FixedSize = new int2(1280, 720)
            });
            entity.AddComponent(new ReferenceCanvasFitComponent {
                ReferenceWidth = 1280,
                ReferenceHeight = 720
            });

            CreateUiTextEntity(entity, "TiltTrialTimerText", new float3(530f, 16f, 0f), global::city.game.TiltTrialLevelSelectComponent.FormatTimerSeconds(levelEntry.StartTimeSeconds), new int2(220, 56), 2.2f, 1, new byte4(255, 246, 223, 255), TextAlignment.Center);

            Entity speedTextEntity = Core.Instance.EntityFactory.CreateChild(entity, "TiltTrialSpeedText");
            speedTextEntity.LocalPosition = new float3(16f, 600f, 0f);
            speedTextEntity.Static = false;
            TextComponent speedTextComponent = new TextComponent {
                Text = "0\nkm/h",
                Font = ResolveRequiredEditorFont(),
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(320, 224),
                FontScale = 2.2f,
                Alignment = TextAlignment.Center,
                RenderOrder2D = 1,
                LayerMask = 1
            };
            speedTextEntity.AddComponent(speedTextComponent);
            LayoutComponent speedTextAnchorComponent = new LayoutComponent();
            speedTextAnchorComponent.LayoutSpace = LayoutComponent.CameraViewportLayoutSpace;
            speedTextAnchorComponent.SetAnchorDistances(left: 16f, bottom: 16f);
            speedTextEntity.AddComponent(speedTextAnchorComponent);
            ApplyFontReference(speedTextEntity, speedTextComponent, TiltTrialSpeedHudFontRelativePath);
            speedTextEntity.AddComponent(new city.game.DemoTiltSpeedTextComponent());

            Entity resultsOverlayEntity = CreateRoundedPanelEntity(entity, "TiltTrialResultsOverlay", new float3(320f, 170f, 0f), new int2(640, 280), 28f, 3f, new byte4(18, 27, 43, 238), new byte4(255, 214, 138, 255), 4);
            resultsOverlayEntity.Enabled = false;
            CreateUiTextEntity(resultsOverlayEntity, "TiltTrialResultsTitleText", new float3(36f, 28f, 0.1f), "Clear", new int2(360, 42), 2f, 5, new byte4(255, 236, 196, 255), TextAlignment.Left);
            CreateUiTextEntity(resultsOverlayEntity, "TiltTrialResultsBodyText", new float3(36f, 86f, 0.1f), "Time 00.00", new int2(420, 152), 1.35f, 5, new byte4(247, 248, 252, 255), TextAlignment.Left);

            Entity failOverlayEntity = CreateRoundedPanelEntity(entity, "TiltTrialFailOverlay", new float3(360f, 210f, 0f), new int2(560, 220), 28f, 3f, new byte4(43, 23, 28, 238), new byte4(214, 112, 112, 255), 4);
            failOverlayEntity.Enabled = false;
            CreateUiTextEntity(failOverlayEntity, "TiltTrialFailTitleText", new float3(36f, 28f, 0.1f), "Time Up", new int2(280, 42), 2f, 5, new byte4(255, 223, 223, 255), TextAlignment.Left);
            CreateUiTextEntity(failOverlayEntity, "TiltTrialFailBodyText", new float3(36f, 86f, 0.1f), "Retry", new int2(320, 96), 1.35f, 5, new byte4(247, 248, 252, 255), TextAlignment.Left);

            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Tilt Trial UI generation requires editor-authored entities.");
        }

        /// <summary>
        /// Creates the authored stage root that owns the runtime tilt controller and the kinematic support geometry it manipulates.
        /// </summary>
        /// <returns>Generated editor stage root.</returns>
        EditorEntity CreateStageRootEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("StageRoot");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = float3.Zero;
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new city.game.DemoTiltStageComponent {
                MaximumPlanarSpeed = 11.25f,
                PlanarAccelerationUnitsPerSecond = 4.25f
            });
            entity.AddChild(CreateStartPadEntity());
            entity.AddChild(CreateRampEntity());
            entity.AddChild(CreateGoalPadEntity());
            entity.AddChild(CreateLeftWallEntity());
            entity.AddChild(CreateRightWallEntity());

            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Tilt Trial stage generation requires editor-authored entities.");
        }

        /// <summary>
        /// Creates the authored playable ball entity.
        /// </summary>
        /// <returns>Generated editor sphere entity.</returns>
        EditorEntity CreatePlayerSphereEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("PlayerSphere");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, 1.2f, -7f);
            entity.LocalScale = float3.One;
            entity.LocalOrientation = float4.Identity;
            MeshComponent meshComponent = new MeshComponent {
                Model = GeneratedSphereModel,
                Materials = new[] { TiltTrialPlayerSphereMarbleMaterial },
                RenderOrder3D = 0
            };
            entity.AddComponent(meshComponent);
            ApplyTiltTrialPlayerSphereMaterialReference(entity, meshComponent);
            entity.AddComponent(new RigidBody3DComponent {
                BodyKind = BodyKind3D.Dynamic,
                UseGravity = true,
                Mass = 1d
            });
            entity.AddComponent(new SphereCollider3DComponent {
                Radius = 0.5f
            });
            entity.AddComponent(new city.game.DemoTiltBallResetComponent {
                SpawnPosition = new float3(0f, 1.2f, -7f),
                SpawnOrientation = float4.Identity,
                ResetHeight = -12f
            });

            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Tilt Trial player generation requires editor-authored entities.");
        }

        /// <summary>
        /// Stores the stable authored walnut material reference required by scene serialization on the generated player sphere mesh.
        /// </summary>
        /// <param name="entity">Generated player sphere entity that owns the mesh component.</param>
        /// <param name="meshComponent">Mesh component assigned to the generated player sphere.</param>
        void ApplyTiltTrialPlayerSphereMaterialReference(Entity entity, MeshComponent meshComponent) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (meshComponent == null) {
                throw new ArgumentNullException(nameof(meshComponent));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(
                meshComponent,
                PlayerSphereMaterialReferenceName,
                global::helengine.SceneAssetReferenceFactory.CreateFileSystemMaterial(TiltTrialPlayerSphereMarbleMaterialRelativePath));
        }

        /// <summary>
        /// Creates the visual-only floor that catches the eye while still allowing the playable sphere to fall through and reset.
        /// </summary>
        /// <returns>Generated floor entity.</returns>
        Entity CreateCatchFloorEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("CatchFloor");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, -14f, 0f);
            entity.LocalScale = new float3(24f, 1f, 24f);
            entity.LocalOrientation = float4.Identity;
            MeshComponent meshComponent = new MeshComponent {
                Model = GeneratedCubeModel,
                Materials = new[] { TiltTrialCourseMaterial },
                RenderOrder3D = 0
            };
            entity.AddComponent(meshComponent);
            ApplyTiltTrialCourseMaterialReference(entity, meshComponent);
            return entity;
        }

        /// <summary>
        /// Creates the starting support platform at the beginning of the Tilt Trial course.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateStartPadEntity() {
            return CreateKinematicCourseBoxEntity("StartPad", new float3(0f, 0f, -7f), new float3(6f, 1f, 8f), float4.Identity);
        }

        /// <summary>
        /// Creates the central ramp that accelerates the player sphere through the middle of the course.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateRampEntity() {
            float4 orientation;
            float4.CreateFromYawPitchRoll(0f, -0.22f, 0f, out orientation);
            return CreateKinematicCourseBoxEntity("Ramp", new float3(0f, -0.2f, 1f), new float3(6f, 1f, 12f), orientation);
        }

        /// <summary>
        /// Creates the finish platform at the far end of the Tilt Trial course.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateGoalPadEntity() {
            Entity entity = CreateKinematicCourseBoxEntity("GoalPad", new float3(0f, -2.2f, 10.5f), new float3(7f, 1f, 6f), float4.Identity);
            entity.AddComponent(new global::city.game.TiltTrialGoalComponent());
            return entity;
        }

        /// <summary>
        /// Creates the left guide wall that keeps the player sphere on the authored course.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateLeftWallEntity() {
            return CreateKinematicCourseBoxEntity("LeftWall", new float3(-3.75f, 0.9f, 1.8f), new float3(1f, 2.5f, 24f), float4.Identity);
        }

        /// <summary>
        /// Creates the right guide wall that keeps the player sphere on the authored course.
        /// </summary>
        /// <returns>Generated kinematic stage piece.</returns>
        Entity CreateRightWallEntity() {
            return CreateKinematicCourseBoxEntity("RightWall", new float3(3.75f, 0.9f, 1.8f), new float3(1f, 2.5f, 24f), float4.Identity);
        }

        /// <summary>
        /// Creates one mesh-backed kinematic stage box used by the Tilt Trial course.
        /// </summary>
        /// <param name="name">Authored entity name.</param>
        /// <param name="position">Local stage position.</param>
        /// <param name="scale">Full box size.</param>
        /// <param name="orientation">Local stage orientation.</param>
        /// <returns>Generated kinematic course entity.</returns>
        Entity CreateKinematicCourseBoxEntity(string name, float3 position, float3 scale, float4 orientation) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Course box names must be provided.", nameof(name));
            }

            Entity entity = Core.Instance.EntityFactory.Create(name);
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = position;
            entity.LocalScale = scale;
            entity.LocalOrientation = orientation;
            MeshComponent meshComponent = new MeshComponent {
                Model = GeneratedCubeModel,
                Materials = new[] { TiltTrialCourseMaterial },
                RenderOrder3D = 0
            };
            entity.AddComponent(meshComponent);
            ApplyTiltTrialCourseMaterialReference(entity, meshComponent);
            entity.AddComponent(new RigidBody3DComponent {
                BodyKind = BodyKind3D.Kinematic,
                UseGravity = false,
                Mass = 1d
            });
            entity.AddComponent(new BoxCollider3DComponent {
                Size = scale
            });
            return entity;
        }

        /// <summary>
        /// Stores the stable authored Tilt Trial course material reference required by scene serialization on one generated course mesh.
        /// </summary>
        /// <param name="entity">Generated course entity that owns the mesh component.</param>
        /// <param name="meshComponent">Mesh component assigned to the generated course entity.</param>
        void ApplyTiltTrialCourseMaterialReference(Entity entity, MeshComponent meshComponent) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (meshComponent == null) {
                throw new ArgumentNullException(nameof(meshComponent));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(
                meshComponent,
                CourseMaterialReferenceName,
                global::helengine.SceneAssetReferenceFactory.CreateFileSystemMaterial(TiltTrialCourseMaterialRelativePath));
        }

        /// <summary>
        /// Wires the generated third-person camera to the generated player sphere after fresh scene ids have been assigned.
        /// </summary>
        /// <param name="cameraEntity">Generated camera entity that owns the follow-camera component.</param>
        /// <param name="playerSphereEntity">Generated player sphere entity being followed.</param>
        void ConfigureTiltTrialCameraTarget(EditorEntity cameraEntity, EditorEntity playerSphereEntity) {
            if (cameraEntity == null) {
                throw new ArgumentNullException(nameof(cameraEntity));
            } else if (playerSphereEntity == null) {
                throw new ArgumentNullException(nameof(playerSphereEntity));
            }

            city.game.DemoTiltFollowCameraComponent followCameraComponent = FindRequiredFollowCameraComponent(cameraEntity);
            EntitySaveComponent playerSaveComponent = FindRequiredEntitySaveComponent(playerSphereEntity);
            followCameraComponent.TargetEntityReference = new SceneEntityReference {
                EntityId = playerSaveComponent.EntityId
            };
        }

        /// <summary>
        /// Wires the generated speed HUD to the generated player sphere after fresh scene ids have been assigned.
        /// </summary>
        /// <param name="uiEntity">Generated UI root that owns the speed HUD text entity.</param>
        /// <param name="playerSphereEntity">Generated player sphere whose live speed should be displayed.</param>
        void ConfigureTiltTrialSpeedTextTarget(EditorEntity uiEntity, EditorEntity playerSphereEntity) {
            if (uiEntity == null) {
                throw new ArgumentNullException(nameof(uiEntity));
            } else if (playerSphereEntity == null) {
                throw new ArgumentNullException(nameof(playerSphereEntity));
            }

            city.game.DemoTiltSpeedTextComponent speedTextComponent = FindRequiredSpeedTextComponent(uiEntity);
            EntitySaveComponent playerSaveComponent = FindRequiredEntitySaveComponent(playerSphereEntity);
            speedTextComponent.TargetEntityReference = new SceneEntityReference {
                EntityId = playerSaveComponent.EntityId
            };
        }

        /// <summary>
        /// Resolves the generated Tilt Trial follow camera attached to the supplied camera root.
        /// </summary>
        /// <param name="cameraEntity">Generated camera entity whose follow-camera component should be returned.</param>
        /// <returns>Attached Tilt Trial follow camera component.</returns>
        city.game.DemoTiltFollowCameraComponent FindRequiredFollowCameraComponent(EditorEntity cameraEntity) {
            if (cameraEntity == null || cameraEntity.Components == null) {
                throw new ArgumentNullException(nameof(cameraEntity));
            }

            for (int componentIndex = 0; componentIndex < cameraEntity.Components.Count; componentIndex++) {
                if (cameraEntity.Components[componentIndex] is city.game.DemoTiltFollowCameraComponent followCameraComponent) {
                    return followCameraComponent;
                }
            }

            throw new InvalidOperationException("Tilt Trial camera generation requires a DemoTiltFollowCameraComponent.");
        }

        /// <summary>
        /// Resolves the generated Tilt Trial speed HUD updater nested beneath the supplied UI root.
        /// </summary>
        /// <param name="uiEntity">Generated UI root whose HUD updater should be returned.</param>
        /// <returns>Attached Tilt Trial speed HUD updater.</returns>
        city.game.DemoTiltSpeedTextComponent FindRequiredSpeedTextComponent(EditorEntity uiEntity) {
            if (uiEntity == null || uiEntity.Children == null) {
                throw new ArgumentNullException(nameof(uiEntity));
            }

            for (int childIndex = 0; childIndex < uiEntity.Children.Count; childIndex++) {
                Entity child = uiEntity.Children[childIndex];
                if (child.Components == null) {
                    continue;
                }

                for (int componentIndex = 0; componentIndex < child.Components.Count; componentIndex++) {
                    if (child.Components[componentIndex] is city.game.DemoTiltSpeedTextComponent speedTextComponent) {
                        return speedTextComponent;
                    }
                }
            }

            throw new InvalidOperationException("Tilt Trial UI generation requires a DemoTiltSpeedTextComponent.");
        }

        /// <summary>
        /// Creates one rounded 2D panel entity beneath the supplied parent.
        /// </summary>
        /// <param name="parent">Parent entity that should own the panel.</param>
        /// <param name="entityName">Stable entity name.</param>
        /// <param name="localPosition">Local entity position.</param>
        /// <param name="size">Panel size in authored pixels.</param>
        /// <param name="radius">Corner radius.</param>
        /// <param name="borderThickness">Border thickness in authored pixels.</param>
        /// <param name="fillColor">Panel fill color.</param>
        /// <param name="borderColor">Panel border color.</param>
        /// <param name="renderOrder2D">2D render order.</param>
        /// <returns>Created panel entity.</returns>
        Entity CreateRoundedPanelEntity(Entity parent, string entityName, float3 localPosition, int2 size, float radius, float borderThickness, byte4 fillColor, byte4 borderColor, byte renderOrder2D) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, entityName);
            entity.LocalPosition = localPosition;
            entity.Static = false;
            entity.AddComponent(new RoundedRectComponent {
                Size = size,
                Radius = radius,
                BorderThickness = borderThickness,
                FillColor = fillColor,
                BorderColor = borderColor,
                RenderOrder2D = renderOrder2D,
                LayerMask = 1
            });
            return entity;
        }

        /// <summary>
        /// Creates one reusable UI text entity using the shared Tilt Trial HUD font.
        /// </summary>
        /// <param name="parent">Parent entity that should own the text.</param>
        /// <param name="entityName">Stable entity name.</param>
        /// <param name="localPosition">Local entity position.</param>
        /// <param name="text">Initial text content.</param>
        /// <param name="size">Text layout size in authored pixels.</param>
        /// <param name="fontScale">Uniform glyph scale.</param>
        /// <param name="renderOrder2D">2D render order.</param>
        /// <param name="color">Text color.</param>
        /// <param name="alignment">Requested text alignment.</param>
        /// <returns>Created text entity.</returns>
        Entity CreateUiTextEntity(Entity parent, string entityName, float3 localPosition, string text, int2 size, float fontScale, byte renderOrder2D, byte4 color, TextAlignment alignment) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, entityName);
            entity.LocalPosition = localPosition;
            entity.Static = false;
            TextComponent textComponent = new TextComponent {
                Text = text ?? string.Empty,
                Font = ResolveRequiredEditorFont(),
                Color = color,
                Size = size,
                FontScale = fontScale,
                Alignment = alignment,
                RenderOrder2D = renderOrder2D,
                LayerMask = 1
            };
            entity.AddComponent(textComponent);
            ApplyFontReference(entity, textComponent, TiltTrialSpeedHudFontRelativePath);
            return entity;
        }

        /// <summary>
        /// Resolves the editor font used by the live Tilt Trial HUD entities.
        /// </summary>
        /// <returns>Loaded default editor font.</returns>
        FontAsset ResolveRequiredEditorFont() {
            if (Core.Instance is not EditorCore editorCore || editorCore.DefaultFontAssetForEditor == null) {
                throw new InvalidOperationException("A default editor font must be loaded before the Tilt Trial scene can be generated.");
            }

            return editorCore.DefaultFontAssetForEditor;
        }

        /// <summary>
        /// Stores the supplied file-backed font reference on the entity save state for the supplied text component.
        /// </summary>
        /// <param name="entity">Entity that owns the text component.</param>
        /// <param name="component">Text component whose font reference should be stored.</param>
        /// <param name="fontPath">Project-relative font path.</param>
        void ApplyFontReference(Entity entity, TextComponent component, string fontPath) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            } else if (string.IsNullOrWhiteSpace(fontPath)) {
                throw new ArgumentException("Font path must be provided.", nameof(fontPath));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, "Font", global::helengine.SceneAssetReferenceFactory.CreateFileSystemFont(fontPath));
        }

        /// <summary>
        /// Resolves the hidden save component attached to one generated editor entity after ids have been assigned.
        /// </summary>
        /// <param name="entity">Generated editor entity whose save component should be returned.</param>
        /// <returns>Attached entity save component with a non-zero scene id.</returns>
        EntitySaveComponent FindRequiredEntitySaveComponent(Entity entity) {
            if (entity == null || entity.Components == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is EntitySaveComponent saveComponent) {
                    if (saveComponent.EntityId == 0u) {
                        throw new InvalidOperationException("Generated editor entities must have a preassigned numeric scene entity id.");
                    }

                    return saveComponent;
                }
            }

            throw new InvalidOperationException("Generated editor entities must include EntitySaveComponent.");
        }
    }
}
