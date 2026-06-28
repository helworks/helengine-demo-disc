using city.menu;
using city.rendering.tools;
using helengine.editor;

namespace city.game.tools {
    /// <summary>
    /// Builds the generated authored gameplay scenes contributed by the city demo-disc project.
    /// </summary>
    public sealed class GameSceneFactory {
        /// <summary>
        /// Shared generated cube model used by the authored Tilt Trial course geometry.
        /// </summary>
        readonly RuntimeModel GeneratedCubeModel;

        /// <summary>
        /// Shared generated sphere model used by the playable ball.
        /// </summary>
        readonly RuntimeModel GeneratedSphereModel;

        /// <summary>
        /// Shared generated standard material used by the authored gameplay geometry.
        /// </summary>
        readonly RuntimeMaterial GeneratedStandardMaterial;

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
            } else if (assets.GeneratedStandardMaterial == null) {
                throw new ArgumentException("Game scene generation requires the generated standard runtime material.", nameof(assets));
            }

            GeneratedCubeModel = assets.GeneratedCubeModel;
            GeneratedSphereModel = assets.GeneratedSphereModel;
            GeneratedStandardMaterial = assets.GeneratedStandardMaterial;
        }

        /// <summary>
        /// Creates the generated authored Tilt Trial gameplay scene.
        /// </summary>
        /// <returns>Generated authored scene definition for Tilt Trial.</returns>
        public GeneratedAuthoringSceneDefinition CreateTiltTrialScene() {
            EditorEntity cameraEntity = CreateCameraEntity();
            EditorEntity stageRootEntity = CreateStageRootEntity();
            EditorEntity playerSphereEntity = CreatePlayerSphereEntity();
            Entity uiEntity = CreateUiEntity();
            Entity[] roots = new Entity[] {
                cameraEntity,
                CreateDirectionalLightEntity(),
                uiEntity,
                stageRootEntity,
                CreateCatchFloorEntity(),
                playerSphereEntity
            };

            ConfigureTiltTrialCameraTarget(cameraEntity, playerSphereEntity);
            return new GeneratedAuthoringSceneDefinition {
                SceneId = GameSceneCatalog.TiltTrialSceneId,
                SceneSettings = new SceneSettingsAsset(),
                RootEntities = roots
            };
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
            entity.LocalPosition = new float3(0f, 5.5f, 13.5f);
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
                Intensity = 1f,
                ShadowsEnabled = true,
                ShadowMapMode = ShadowMapMode.Forced,
                ShadowStrength = 0.95f,
                ShadowDistance = 72f
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored UI root that owns the shared return-to-menu input behavior while Tilt Trial is active.
        /// </summary>
        /// <returns>Generated UI root entity.</returns>
        Entity CreateUiEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("TiltTrialUi");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.AddComponent(new DemoDiscReturnToMenuComponent());
            return entity;
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
            entity.AddComponent(new city.game.DemoTiltStageComponent());
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
            entity.AddComponent(new MeshComponent {
                Model = GeneratedSphereModel,
                Materials = new[] { GeneratedStandardMaterial },
                RenderOrder3D = 0
            });
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
        /// Creates the visual-only floor that catches the eye while still allowing the playable sphere to fall through and reset.
        /// </summary>
        /// <returns>Generated floor entity.</returns>
        Entity CreateCatchFloorEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("CatchFloor");
            entity.LayerMask = EditorLayerMasks.SceneObjects;
            entity.LocalPosition = new float3(0f, -14f, 0f);
            entity.LocalScale = new float3(24f, 1f, 24f);
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new MeshComponent {
                Model = GeneratedCubeModel,
                Materials = new[] { GeneratedStandardMaterial },
                RenderOrder3D = 0
            });
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
            return CreateKinematicCourseBoxEntity("GoalPad", new float3(0f, -2.2f, 10.5f), new float3(7f, 1f, 6f), float4.Identity);
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
            entity.AddComponent(new MeshComponent {
                Model = GeneratedCubeModel,
                Materials = new[] { GeneratedStandardMaterial },
                RenderOrder3D = 0
            });
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
        /// Resolves the hidden save component attached to one generated editor entity after ids have been assigned.
        /// </summary>
        /// <param name="entity">Generated editor entity whose save component should be returned.</param>
        /// <returns>Attached entity save component with a non-zero scene id.</returns>
        EntitySaveComponent FindRequiredEntitySaveComponent(EditorEntity entity) {
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
