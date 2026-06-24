using city.menu;
using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Builds the shared Nintendo DS dual-screen scaffold used by generated city rendering showcase companion scenes.
    /// </summary>
    public sealed class NintendoDsRenderingSceneScaffoldFactory {
        /// <summary>
        /// Fixed font scale used by the Nintendo DS default bottom overlay so debug text and the back button match the physics showcase sizing.
        /// </summary>
        const float NintendoDsBottomOverlayFontScale = 1f;

        /// <summary>
        /// Runtime layer mask used by packaged 2D overlay drawables.
        /// </summary>
        const byte RuntimeLayerMask = 0b00000001;

        /// <summary>
        /// Fixed Nintendo DS screen width used by the default bottom overlay.
        /// </summary>
        const int ScreenWidth = 256;

        /// <summary>
        /// Fixed Nintendo DS screen height used by the default bottom overlay.
        /// </summary>
        const int ScreenHeight = 192;
        /// <summary>
        /// Creates one dual-screen Nintendo DS root set from top-screen scene content and optional bottom-screen content.
        /// </summary>
        /// <param name="topScreenRoots">Scene roots that should remain on the top screen.</param>
        /// <param name="useDefaultBottomOverlay">True when the standard bottom debug and back overlay should be emitted.</param>
        /// <param name="bottomScreenRoots">Optional custom bottom-screen roots supplied by the generator.</param>
        /// <returns>Combined DS companion-scene roots.</returns>
        public Entity[] CreateSceneRoots(Entity[] topScreenRoots, bool useDefaultBottomOverlay, Entity[] bottomScreenRoots, FontAsset bottomOverlayFont) {
            if (topScreenRoots == null) {
                throw new ArgumentNullException(nameof(topScreenRoots));
            } else if (bottomScreenRoots == null) {
                throw new ArgumentNullException(nameof(bottomScreenRoots));
            } else if (bottomOverlayFont == null) {
                throw new ArgumentNullException(nameof(bottomOverlayFont));
            }

            ConfigureTopScreenRoots(topScreenRoots);
            Entity bottomScreenCameraEntity = CreateBottomScreenCameraEntity();
            Entity bottomScreenViewportRoot = Core.Instance.EntityFactory.CreateChild(bottomScreenCameraEntity, "DemoDiscBottomScreenRoot");
            bottomScreenViewportRoot.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.AncestorCameraBindingMode,
                FixedSize = new int2(ScreenWidth, ScreenHeight),
                ScalingMode = ViewportComponent.ReferenceCanvasScalingMode,
                ReferenceWidth = ScreenWidth,
                ReferenceHeight = ScreenHeight
            });

            if (useDefaultBottomOverlay) {
                CreateDefaultBottomOverlay(bottomScreenViewportRoot, bottomOverlayFont);
            }

            AttachBottomScreenRoots(bottomScreenViewportRoot, bottomScreenRoots);
            return CombineSceneRoots(topScreenRoots, bottomScreenCameraEntity);
        }

        /// <summary>
        /// Configures the top-screen roots for Nintendo DS presentation.
        /// </summary>
        /// <param name="topScreenRoots">Root entities that should target the top screen.</param>
        void ConfigureTopScreenRoots(Entity[] topScreenRoots) {
            if (topScreenRoots == null) {
                throw new ArgumentNullException(nameof(topScreenRoots));
            }

            bool assignedPrimaryCamera = false;
            for (int index = 0; index < topScreenRoots.Length; index++) {
                Entity rootEntity = topScreenRoots[index];
                if (rootEntity == null) {
                    continue;
                }

                ConfigureTopScreenRootRecursive(rootEntity, ref assignedPrimaryCamera);
            }
        }

        /// <summary>
        /// Applies Nintendo DS top-screen camera settings recursively throughout one scene subtree.
        /// </summary>
        /// <param name="entity">Current subtree entity.</param>
        /// <param name="assignedPrimaryCamera">Tracks whether the stable top-screen camera name was already assigned.</param>
        void ConfigureTopScreenRootRecursive(Entity entity, ref bool assignedPrimaryCamera) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            RemoveFpsComponents(entity);
            RemoveReturnToMenuComponents(entity);
            CameraComponent cameraComponent = FindFirstComponent<CameraComponent>(entity);
            if (cameraComponent != null) {
                cameraComponent.Viewport = new float4(0f, 0f, 1f, 1f);
                if (!assignedPrimaryCamera) {
                    if (entity is EditorEntity editorEntity) {
                        editorEntity.Name = "DemoDiscTopScreenCamera";
                    }

                    assignedPrimaryCamera = true;
                }
            }

            if (entity.Children == null) {
                return;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                ConfigureTopScreenRootRecursive(entity.Children[childIndex], ref assignedPrimaryCamera);
            }
        }

        /// <summary>
        /// Removes any FPS overlay components from one subtree so the DS companion scenes keep the top screen focused on 3D content.
        /// </summary>
        /// <param name="entity">Current subtree entity.</param>
        void RemoveFpsComponents(Entity entity) {
            if (entity == null || entity.Components == null) {
                return;
            }

            for (int componentIndex = entity.Components.Count - 1; componentIndex >= 0; componentIndex--) {
                if (entity.Components[componentIndex] is not FPSComponent fpsComponent) {
                    continue;
                }

                entity.RemoveComponent(fpsComponent);
                fpsComponent.Dispose();
            }
        }

        /// <summary>
        /// Removes inherited return-to-menu components from one subtree so the DS scaffold owns the only active return binding.
        /// </summary>
        /// <param name="entity">Current subtree entity.</param>
        void RemoveReturnToMenuComponents(Entity entity) {
            if (entity == null || entity.Components == null) {
                return;
            }

            for (int componentIndex = entity.Components.Count - 1; componentIndex >= 0; componentIndex--) {
                if (entity.Components[componentIndex] is not DemoDiscReturnToMenuComponent returnToMenuComponent) {
                    continue;
                }

                entity.RemoveComponent(returnToMenuComponent);
                returnToMenuComponent.Dispose();
            }
        }

        /// <summary>
        /// Creates the dedicated Nintendo DS bottom-screen camera entity.
        /// </summary>
        /// <returns>Bottom-screen camera entity.</returns>
        Entity CreateBottomScreenCameraEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("DemoDiscBottomScreenCamera");
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 1,
                LayerMask = RuntimeLayerMask,
                Viewport = new float4(0f, 1f, 1f, 1f),
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(0f, 0f, 0f, 1f),
                    true,
                    1f,
                    false,
                    0),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Disabled,
                    ShadowDistance = 0f,
                    PostProcessTier = PostProcessTier.Disabled
                }
            });
            return entity;
        }

        /// <summary>
        /// Creates the standard bottom-screen debug and back overlay beneath the supplied viewport root.
        /// </summary>
        /// <param name="bottomScreenViewportRoot">Bottom-screen viewport root that should own the default overlay.</param>
        void CreateDefaultBottomOverlay(Entity bottomScreenViewportRoot, FontAsset bottomOverlayFont) {
            if (bottomScreenViewportRoot == null) {
                throw new ArgumentNullException(nameof(bottomScreenViewportRoot));
            } else if (bottomOverlayFont == null) {
                throw new ArgumentNullException(nameof(bottomOverlayFont));
            }

            Entity debugRootEntity = Core.Instance.EntityFactory.CreateChild(bottomScreenViewportRoot, "DemoDiscBottomScreenDebugRoot");
            DebugComponent debugComponent = new DebugComponent();
            debugComponent.Font = bottomOverlayFont;
            debugComponent.FontScale = NintendoDsBottomOverlayFontScale;
            debugComponent.Padding = new int2(8, 8);
            debugComponent.RenderOrder2D = 220;
            debugComponent.RefreshIntervalSeconds = 0.25d;
            debugRootEntity.AddComponent(debugComponent);
            ApplyFontReference(debugRootEntity, debugComponent, DemoDiscSceneComponentRecordFactory.CreateEditorFontReference());

            Entity buttonEntity = Core.Instance.EntityFactory.CreateChild(bottomScreenViewportRoot, "DemoDiscBottomScreenBackButton");
            buttonEntity.LocalPosition = new float3(16f, 144f, 0f);
            buttonEntity.AddComponent(new InteractableComponent {
                Size = new int2(224, 32)
            });
            buttonEntity.AddComponent(new NintendoDsReturnOverlayComponent());
            SpriteComponent spriteComponent = new SpriteComponent {
                Size = new int2(224, 32),
                RenderOrder2D = 230,
                LayerMask = RuntimeLayerMask
            };
            buttonEntity.AddComponent(spriteComponent);
            ApplyTextureReference(buttonEntity, spriteComponent, "Images/Menu/ds-back-button.png");

            Entity textEntity = Core.Instance.EntityFactory.CreateChild(buttonEntity, "DemoDiscBottomScreenBackButtonText");
            textEntity.LocalPosition = new float3(16f, 8f, 0.1f);
            TextComponent textComponent = new TextComponent {
                Text = "BACK",
                Font = bottomOverlayFont,
                FontScale = NintendoDsBottomOverlayFontScale,
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(192, 24),
                RenderOrder2D = 231,
                LayerMask = RuntimeLayerMask
            };
            textEntity.AddComponent(textComponent);
            ApplyFontReference(textEntity, textComponent, DemoDiscSceneComponentRecordFactory.CreateEditorFontReference());
        }

        /// <summary>
        /// Attaches any custom bottom-screen root entities beneath the resolved bottom viewport root.
        /// </summary>
        /// <param name="bottomScreenViewportRoot">Viewport root that should own custom bottom-screen content.</param>
        /// <param name="bottomScreenRoots">Custom bottom-screen roots supplied by the generator.</param>
        void AttachBottomScreenRoots(Entity bottomScreenViewportRoot, Entity[] bottomScreenRoots) {
            if (bottomScreenViewportRoot == null) {
                throw new ArgumentNullException(nameof(bottomScreenViewportRoot));
            } else if (bottomScreenRoots == null) {
                throw new ArgumentNullException(nameof(bottomScreenRoots));
            }

            for (int index = 0; index < bottomScreenRoots.Length; index++) {
                Entity rootEntity = bottomScreenRoots[index];
                if (rootEntity == null) {
                    continue;
                }

                if (rootEntity.Parent != null) {
                    rootEntity.Parent.RemoveChild(rootEntity);
                }

                bottomScreenViewportRoot.AddChild(rootEntity);
            }
        }

        /// <summary>
        /// Combines the original top-screen roots with the generated bottom-screen camera root.
        /// </summary>
        /// <param name="topScreenRoots">Top-screen scene roots.</param>
        /// <param name="bottomScreenCameraEntity">Generated bottom-screen camera entity.</param>
        /// <returns>Combined companion-scene roots.</returns>
        Entity[] CombineSceneRoots(Entity[] topScreenRoots, Entity bottomScreenCameraEntity) {
            if (topScreenRoots == null) {
                throw new ArgumentNullException(nameof(topScreenRoots));
            } else if (bottomScreenCameraEntity == null) {
                throw new ArgumentNullException(nameof(bottomScreenCameraEntity));
            }

            Entity[] combinedRoots = new Entity[topScreenRoots.Length + 1];
            for (int index = 0; index < topScreenRoots.Length; index++) {
                combinedRoots[index] = topScreenRoots[index];
            }

            combinedRoots[topScreenRoots.Length] = bottomScreenCameraEntity;
            return combinedRoots;
        }

        /// Finds the first component of the requested type on one entity.
        /// </summary>
        /// <typeparam name="TComponent">Component type to resolve.</typeparam>
        /// <param name="entity">Entity to inspect.</param>
        /// <returns>Resolved component when present; otherwise null.</returns>
        TComponent FindFirstComponent<TComponent>(Entity entity) where TComponent : Component {
            if (entity == null || entity.Components == null) {
                return null;
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is TComponent component) {
                    return component;
                }
            }

            return null;
        }

        /// <summary>
        /// Stores the supplied generated Nintendo DS debug-font reference on the generated scene save state for the given component.
        /// </summary>
        /// <param name="entity">Entity that owns the component.</param>
        /// <param name="component">Component whose font reference should be stored.</param>
        /// <param name="fontReference">Generated Nintendo DS debug-font reference.</param>
        void ApplyFontReference(Entity entity, Component component, SceneAssetReference fontReference) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            } else if (fontReference == null) {
                throw new ArgumentNullException(nameof(fontReference));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, "Font", fontReference);
        }

        /// <summary>
        /// Stores the supplied texture reference on the generated scene save state for the given sprite component.
        /// </summary>
        /// <param name="entity">Entity that owns the component.</param>
        /// <param name="component">Component whose texture reference should be stored.</param>
        /// <param name="texturePath">Project-relative texture path.</param>
        void ApplyTextureReference(Entity entity, Component component, string texturePath) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            } else if (string.IsNullOrWhiteSpace(texturePath)) {
                throw new ArgumentException("Texture path must be provided.", nameof(texturePath));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, TextureAssetScenePersistenceSupport.TextureReferenceName, BuildFileReference(texturePath));
        }

        /// <summary>
        /// Resolves the hidden entity save component attached by the editor entity factory.
        /// </summary>
        /// <param name="entity">Entity whose save component should be returned.</param>
        /// <returns>Attached entity save component.</returns>
        EntitySaveComponent FindRequiredEntitySaveComponent(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (entity.Components == null) {
                throw new InvalidOperationException("Generated entities must expose initialized component collections.");
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            throw new InvalidOperationException("Generated entities must include EntitySaveComponent.");
        }

        /// <summary>
        /// Builds one stable file-backed scene asset reference.
        /// </summary>
        /// <param name="relativePath">Project-relative asset path.</param>
        /// <returns>Stable file-backed scene asset reference.</returns>
        SceneAssetReference BuildFileReference(string relativePath) {
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Relative path must be provided.", nameof(relativePath));
            }

            return global::helengine.SceneAssetReferenceFactory.CreateFileSystemTexture(relativePath.Replace('\\', '/'));
        }
    }
}
