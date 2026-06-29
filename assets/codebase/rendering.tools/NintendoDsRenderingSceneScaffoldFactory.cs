using city.menu;
using city.rendering;
using helengine.editor;
using System.Globalization;

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
        /// Vertical space reserved by the temporary scaffold-owned bottom text row.
        /// </summary>
        const int DefaultBottomOverlayReservedHeight = 24;

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

            Entity[] filteredTopScreenRoots = FilterTopScreenRoots(topScreenRoots);
            Entity bottomScreenCameraEntity = CreateBottomScreenCameraEntity();
            Entity bottomScreenViewportRoot = Core.Instance.EntityFactory.CreateChild(bottomScreenCameraEntity, "DemoDiscBottomScreenRoot");
            bottomScreenViewportRoot.LayerMask = RuntimeLayerMask;
            bottomScreenViewportRoot.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.AncestorCameraBindingMode,
                FixedSize = new int2(ScreenWidth, ScreenHeight),
                ScalingMode = ViewportComponent.ReferenceCanvasScalingMode,
                ReferenceWidth = ScreenWidth,
                ReferenceHeight = ScreenHeight
            });
            RelocateFpsComponentsToBottomScreen(filteredTopScreenRoots, bottomScreenViewportRoot, bottomOverlayFont, useDefaultBottomOverlay);
            ConfigureTopScreenRoots(filteredTopScreenRoots);

            if (useDefaultBottomOverlay) {
                CreateDefaultBottomOverlay(bottomScreenViewportRoot, bottomOverlayFont);
            }

            AttachBottomScreenRoots(bottomScreenViewportRoot, bottomScreenRoots);
            return CombineSceneRoots(filteredTopScreenRoots, bottomScreenCameraEntity);
        }

        /// <summary>
        /// Filters the authored top-screen roots so DS companion scenes do not keep desktop-only instruction panels.
        /// </summary>
        /// <param name="topScreenRoots">Authored scene roots that may contain desktop-only overlays.</param>
        /// <returns>Filtered top-screen roots that should remain visible in the DS companion scene.</returns>
        Entity[] FilterTopScreenRoots(Entity[] topScreenRoots) {
            if (topScreenRoots == null) {
                throw new ArgumentNullException(nameof(topScreenRoots));
            }

            List<Entity> filteredTopScreenRoots = new List<Entity>();
            for (int index = 0; index < topScreenRoots.Length; index++) {
                Entity rootEntity = topScreenRoots[index];
                if (rootEntity == null) {
                    continue;
                } else if (rootEntity is EditorEntity editorRoot
                    && string.Equals(editorRoot.Name, "DemoSceneInstructionViewport", StringComparison.Ordinal)) {
                    continue;
                }

                filteredTopScreenRoots.Add(rootEntity);
            }

            return filteredTopScreenRoots.ToArray();
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

            RemoveReturnToMenuComponents(entity);
            RemoveLightToggleComponents(entity);
            RemoveLightIndicatorOverlays(entity);
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
        /// Relocates authored FPS overlay components from the top-screen scene roots into scaffold-owned bottom-screen entities.
        /// </summary>
        /// <param name="topScreenRoots">Top-screen scene roots that may contain authored FPS overlay components.</param>
        /// <param name="bottomScreenViewportRoot">Bottom-screen viewport root that should own the relocated FPS entities.</param>
        /// <param name="bottomOverlayFont">Live font asset assigned while the generated DS scene is being saved.</param>
        /// <param name="useDefaultBottomOverlay">True when the temporary bottom text row is also emitted.</param>
        void RelocateFpsComponentsToBottomScreen(
            Entity[] topScreenRoots,
            Entity bottomScreenViewportRoot,
            FontAsset bottomOverlayFont,
            bool useDefaultBottomOverlay) {
            if (topScreenRoots == null) {
                throw new ArgumentNullException(nameof(topScreenRoots));
            } else if (bottomScreenViewportRoot == null) {
                throw new ArgumentNullException(nameof(bottomScreenViewportRoot));
            } else if (bottomOverlayFont == null) {
                throw new ArgumentNullException(nameof(bottomOverlayFont));
            }

            int createdBottomScreenFpsCount = 0;
            for (int index = 0; index < topScreenRoots.Length; index++) {
                Entity rootEntity = topScreenRoots[index];
                if (rootEntity == null) {
                    continue;
                }

                RelocateFpsComponentsToBottomScreenRecursive(
                    rootEntity,
                    bottomScreenViewportRoot,
                    bottomOverlayFont,
                    useDefaultBottomOverlay,
                    ref createdBottomScreenFpsCount);
            }
        }

        /// <summary>
        /// Relocates authored FPS overlay components from one subtree into scaffold-owned bottom-screen entities.
        /// </summary>
        /// <param name="entity">Current top-screen subtree entity being inspected.</param>
        /// <param name="bottomScreenViewportRoot">Bottom-screen viewport root that should own the relocated FPS entities.</param>
        /// <param name="bottomOverlayFont">Live font asset assigned while the generated DS scene is being saved.</param>
        /// <param name="useDefaultBottomOverlay">True when the temporary bottom text row is also emitted.</param>
        /// <param name="createdBottomScreenFpsCount">Running count used to keep scaffold-owned FPS entity names stable.</param>
        void RelocateFpsComponentsToBottomScreenRecursive(
            Entity entity,
            Entity bottomScreenViewportRoot,
            FontAsset bottomOverlayFont,
            bool useDefaultBottomOverlay,
            ref int createdBottomScreenFpsCount) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (bottomScreenViewportRoot == null) {
                throw new ArgumentNullException(nameof(bottomScreenViewportRoot));
            } else if (bottomOverlayFont == null) {
                throw new ArgumentNullException(nameof(bottomOverlayFont));
            }

            if (entity.Components != null) {
                for (int componentIndex = entity.Components.Count - 1; componentIndex >= 0; componentIndex--) {
                    if (entity.Components[componentIndex] is not FPSComponent fpsComponent) {
                        continue;
                    }

                    CreateBottomScreenFpsEntity(
                        bottomScreenViewportRoot,
                        bottomOverlayFont,
                        fpsComponent,
                        useDefaultBottomOverlay,
                        createdBottomScreenFpsCount);
                    createdBottomScreenFpsCount++;
                    entity.RemoveComponent(fpsComponent);
                    fpsComponent.Dispose();
                }
            }

            if (entity.Children == null) {
                return;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                RelocateFpsComponentsToBottomScreenRecursive(
                    entity.Children[childIndex],
                    bottomScreenViewportRoot,
                    bottomOverlayFont,
                    useDefaultBottomOverlay,
                    ref createdBottomScreenFpsCount);
            }
        }

        /// <summary>
        /// Creates one scaffold-owned bottom-screen FPS entity that preserves the authored FPS overlay behavior.
        /// </summary>
        /// <param name="bottomScreenViewportRoot">Bottom-screen viewport root that should own the FPS entity.</param>
        /// <param name="bottomOverlayFont">Live font asset assigned while the generated DS scene is being saved.</param>
        /// <param name="sourceComponent">Authored top-screen FPS component being relocated.</param>
        /// <param name="useDefaultBottomOverlay">True when the temporary bottom text row is also emitted.</param>
        /// <param name="fpsIndex">Zero-based scaffold-owned FPS entity index.</param>
        void CreateBottomScreenFpsEntity(
            Entity bottomScreenViewportRoot,
            FontAsset bottomOverlayFont,
            FPSComponent sourceComponent,
            bool useDefaultBottomOverlay,
            int fpsIndex) {
            if (bottomScreenViewportRoot == null) {
                throw new ArgumentNullException(nameof(bottomScreenViewportRoot));
            } else if (bottomOverlayFont == null) {
                throw new ArgumentNullException(nameof(bottomOverlayFont));
            } else if (sourceComponent == null) {
                throw new ArgumentNullException(nameof(sourceComponent));
            }

            Entity fpsEntity = fpsIndex == 0
                ? bottomScreenViewportRoot
                : Core.Instance.EntityFactory.CreateChild(bottomScreenViewportRoot, BuildBottomScreenFpsEntityName(fpsIndex));
            fpsEntity.LayerMask = RuntimeLayerMask;
            fpsEntity.LocalPosition = float3.Zero;
            fpsEntity.LocalScale = float3.One;
            fpsEntity.LocalOrientation = float4.Identity;

            FPSComponent bottomScreenFpsComponent = new FPSComponent {
                Font = bottomOverlayFont,
                FontScale = sourceComponent.FontScale,
                AdditionalText = sourceComponent.AdditionalText,
                RefreshIntervalSeconds = sourceComponent.RefreshIntervalSeconds,
                Padding = ResolveBottomScreenFpsPadding(sourceComponent, useDefaultBottomOverlay, fpsIndex),
                RenderOrder2D = sourceComponent.RenderOrder2D
            };
            fpsEntity.AddComponent(bottomScreenFpsComponent);
            ApplyFontReference(fpsEntity, bottomScreenFpsComponent, DemoDiscSceneComponentRecordFactory.CreateEditorUiFontReference());
        }

        /// <summary>
        /// Builds the stable scaffold-owned bottom-screen FPS entity name for the supplied index.
        /// </summary>
        /// <param name="fpsIndex">Zero-based FPS entity index.</param>
        /// <returns>Stable entity name used by the generated DS scenes.</returns>
        string BuildBottomScreenFpsEntityName(int fpsIndex) {
            if (fpsIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(fpsIndex), "FPS entity index must be non-negative.");
            } else if (fpsIndex == 0) {
                return "DemoDiscBottomScreenFps";
            }

            return "DemoDiscBottomScreenFps" + fpsIndex.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Resolves the bottom-screen padding applied to one relocated FPS overlay.
        /// </summary>
        /// <param name="sourceComponent">Authored top-screen FPS component being relocated.</param>
        /// <param name="useDefaultBottomOverlay">True when the temporary bottom text row is also emitted.</param>
        /// <param name="fpsIndex">Zero-based scaffold-owned FPS entity index.</param>
        /// <returns>Bottom-screen padding assigned to the relocated FPS overlay.</returns>
        int2 ResolveBottomScreenFpsPadding(FPSComponent sourceComponent, bool useDefaultBottomOverlay, int fpsIndex) {
            if (sourceComponent == null) {
                throw new ArgumentNullException(nameof(sourceComponent));
            } else if (fpsIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(fpsIndex), "FPS entity index must be non-negative.");
            }

            int overlayOffsetY = useDefaultBottomOverlay ? DefaultBottomOverlayReservedHeight : 0;
            int rowOffsetY = fpsIndex * 40;
            int2 padding = sourceComponent.Padding;
            return new int2(padding.X, padding.Y + overlayOffsetY + rowOffsetY);
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
        /// Removes desktop-only light-toggle components from one subtree so DS companion scenes do not require the removed top-screen indicator overlay.
        /// </summary>
        /// <param name="entity">Current subtree entity.</param>
        void RemoveLightToggleComponents(Entity entity) {
            if (entity == null || entity.Components == null) {
                return;
            }

            for (int componentIndex = entity.Components.Count - 1; componentIndex >= 0; componentIndex--) {
                if (entity.Components[componentIndex] is not DemoDiscLightToggleComponent lightToggleComponent) {
                    continue;
                }

                entity.RemoveComponent(lightToggleComponent);
                lightToggleComponent.Dispose();
            }
        }

        /// <summary>
        /// Removes the authored light-indicator viewport subtree from one DS top-screen branch.
        /// </summary>
        /// <param name="entity">Current subtree entity.</param>
        void RemoveLightIndicatorOverlays(Entity entity) {
            if (entity == null || entity.Children == null) {
                return;
            }

            for (int childIndex = entity.Children.Count - 1; childIndex >= 0; childIndex--) {
                Entity childEntity = entity.Children[childIndex];
                if (childEntity is EditorEntity editorChild
                    && string.Equals(editorChild.Name, DemoDiscLightIndicatorOverlayFactory.IndicatorViewportEntityName, StringComparison.Ordinal)) {
                    entity.RemoveChild(childEntity);
                    continue;
                }

                RemoveLightIndicatorOverlays(childEntity);
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
        /// Creates one temporary text-only bottom-screen overlay so DS text rendering can be isolated without extra authored sprites or debug widgets.
        /// </summary>
        /// <param name="bottomScreenViewportRoot">Bottom-screen viewport root that should own the default overlay.</param>
        /// <param name="bottomOverlayFont">Font used by the isolated bottom-screen test label.</param>
        void CreateDefaultBottomOverlay(Entity bottomScreenViewportRoot, FontAsset bottomOverlayFont) {
            if (bottomScreenViewportRoot == null) {
                throw new ArgumentNullException(nameof(bottomScreenViewportRoot));
            } else if (bottomOverlayFont == null) {
                throw new ArgumentNullException(nameof(bottomOverlayFont));
            }

            Entity textEntity = Core.Instance.EntityFactory.CreateChild(bottomScreenViewportRoot, "DemoDiscBottomScreenTestText");
            textEntity.LocalPosition = new float3(8f, 8f, 0f);
            TextComponent textComponent = new TextComponent {
                Text = "BOTTOM TEXT",
                Font = bottomOverlayFont,
                FontScale = NintendoDsBottomOverlayFontScale,
                Color = new byte4(255, 255, 255, 255),
                Size = new int2(ScreenWidth - 16, 24),
                RenderOrder2D = 220,
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

        /// <summary>
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
