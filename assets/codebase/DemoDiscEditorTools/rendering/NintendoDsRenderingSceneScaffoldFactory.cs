using DemoDisc.menu;
using DemoDisc.rendering;
using helengine.editor;
using System.Globalization;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Builds the shared Nintendo DS dual-screen scaffold used by generated city rendering showcase companion scenes.
    /// </summary>
public sealed class NintendoDsRenderingSceneScaffoldFactory {
        readonly IEditorProjectAuthoringSession AssetAuthoringService;

        /// <summary>
        /// Initializes the scaffold with the host-owned public asset authoring capability.
        /// </summary>
        public NintendoDsRenderingSceneScaffoldFactory(IEditorProjectAuthoringSession assetAuthoringService) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
        }
        /// <summary>
        /// Fixed font scale used by the Nintendo DS default bottom overlay so debug text and the back button match the physics showcase sizing.
        /// </summary>
        const float NintendoDsBottomOverlayFontScale = 1f;


        /// <summary>
        /// Fixed font scale used by Nintendo 3DS FPS diagnostics after the shared DS reference canvas is resolved to the 3DS screen.
        /// </summary>
        const float Nintendo3DsFpsFontScale = 1f;

        /// <summary>
        /// Stable platform identifier used for the generated Nintendo 3DS button-label component override.
        /// </summary>
        const string Nintendo3DsPlatformId = "3ds";

        /// <summary>
        /// Editor component override service used to persist platform-specific label presentation without changing the shared DS baseline.
        /// </summary>
        readonly ComponentPlatformEditingService PlatformEditingServiceValue = new ComponentPlatformEditingService();


        /// <summary>
        /// Runtime layer mask used by generated bottom-screen entities so the Nintendo DS cameras can render them.
        /// </summary>
        const ushort PersistedSceneLayerMask = EditorLayerMasks.SceneObjects;

        /// <summary>
        /// Fixed Nintendo DS screen width used by the default bottom overlay.
        /// </summary>
        const int ScreenWidth = 256;

        /// <summary>
        /// Fixed Nintendo DS screen height used by the default bottom overlay.
        /// </summary>
        const int ScreenHeight = 192;

        /// <summary>













        /// <summary>
        /// Exact demo-disc lilac clear color reused by the shared Nintendo DS bottom-screen scaffold camera.
        /// </summary>
        static readonly float4 NintendoDsBottomScreenClearColor = new float4(30f / 255f, 17f / 255f, 41f / 255f, 1f);




        /// <summary>
        /// Render order used by the scaffold-owned Nintendo DS light swatch. It must sit at 220 or above: the DS
        /// bottom-screen renderer only promotes orders >= 220 to the foreground OBJ priority, and at the base
        /// priority the earlier-drawn opaque button body wins the hardware tie and hides the swatch.
        /// </summary>





        /// <summary>
        /// Creates one dual-screen Nintendo DS root set from top-screen scene content and optional bottom-screen content.
        /// </summary>
        /// <param name="topScreenRoots">Scene roots that should remain on the top screen.</param>
        /// <param name="useDefaultBottomOverlay">True when the scaffold-owned FPS, light, and back controls should be emitted.</param>
        /// <param name="moveTopScreen2DRootsToBottomScreen">True when authored 2D roots should be moved beneath the bottom-screen viewport.</param>
        /// <param name="bottomScreenRoots">Optional custom bottom-screen roots supplied by the generator.</param>
        /// <returns>Combined DS companion-scene roots.</returns>
        public Entity[] CreateSceneRoots(
            Entity[] topScreenRoots,
            bool useDefaultBottomOverlay,
            bool moveTopScreen2DRootsToBottomScreen,
            Entity[] bottomScreenRoots,
            FontAsset bottomOverlayFont) {
            if (topScreenRoots == null) {
                throw new ArgumentNullException(nameof(topScreenRoots));
            } else if (bottomScreenRoots == null) {
                throw new ArgumentNullException(nameof(bottomScreenRoots));
            } else if (bottomOverlayFont == null) {
                throw new ArgumentNullException(nameof(bottomOverlayFont));
            }

            Entity[] filteredTopScreenRoots = FilterTopScreenRoots(topScreenRoots);
            Entity bottomScreenCameraEntity = CreateBottomScreenCameraEntity();
            Entity bottomScreenViewportRoot = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(bottomScreenCameraEntity, "DemoDiscBottomScreenRoot");
            bottomScreenViewportRoot.LayerMask = PersistedSceneLayerMask;
            bottomScreenViewportRoot.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.AncestorCameraBindingMode,
                FixedSize = new int2(ScreenWidth, ScreenHeight),
                ScalingMode = ViewportComponent.ReferenceCanvasScalingMode,
                ReferenceWidth = ScreenWidth,
                ReferenceHeight = ScreenHeight
            });
            if (useDefaultBottomOverlay) {
                RelocateFpsComponentsToBottomScreen(filteredTopScreenRoots, bottomScreenViewportRoot, bottomOverlayFont);
            }
            Entity topScreenCameraEntity = ConfigureTopScreenRoots(filteredTopScreenRoots);
            Entity[] adjustedTopScreenRoots = moveTopScreen2DRootsToBottomScreen
                ? Move2DRootsUnderBottomScreenViewport(filteredTopScreenRoots, bottomScreenViewportRoot)
                : filteredTopScreenRoots;

            if (useDefaultBottomOverlay) {
                AttachBottomScreenChrome(bottomScreenViewportRoot);
            }
            AttachBottomScreenRoots(bottomScreenViewportRoot, bottomScreenRoots);
            return CombineSceneRoots(adjustedTopScreenRoots, bottomScreenCameraEntity);
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
        Entity ConfigureTopScreenRoots(Entity[] topScreenRoots) {
            if (topScreenRoots == null) {
                throw new ArgumentNullException(nameof(topScreenRoots));
            }

            bool assignedPrimaryCamera = false;
            CameraComponent primaryTopScreenCamera = null;
            Entity primaryTopScreenCameraEntity = null;
            for (int index = 0; index < topScreenRoots.Length; index++) {
                Entity rootEntity = topScreenRoots[index];
                if (rootEntity == null) {
                    continue;
                }

                ConfigureTopScreenRootRecursive(rootEntity, ref assignedPrimaryCamera, ref primaryTopScreenCamera, ref primaryTopScreenCameraEntity);
            }

            if (primaryTopScreenCamera == null || primaryTopScreenCameraEntity == null) {
                throw new InvalidOperationException("Nintendo DS companion scenes require one top-screen camera root.");
            }

            return primaryTopScreenCameraEntity;
        }

        /// <summary>
        /// Applies Nintendo DS top-screen camera settings recursively throughout one scene subtree.
        /// </summary>
        /// <param name="entity">Current subtree entity.</param>
        /// <param name="assignedPrimaryCamera">Tracks whether the stable top-screen camera name was already assigned.</param>
        /// <param name="primaryTopScreenCamera">Receives the first configured top-screen camera component.</param>
        /// <param name="primaryTopScreenCameraEntity">Receives the entity that owns the first configured top-screen camera component.</param>
        void ConfigureTopScreenRootRecursive(
            Entity entity,
            ref bool assignedPrimaryCamera,
            ref CameraComponent primaryTopScreenCamera,
            ref Entity primaryTopScreenCameraEntity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            RemoveReturnToMenuComponents(entity);
            RemoveLightToggleComponents(entity);
            RemoveLightIndicatorOverlays(entity);
            CameraComponent cameraComponent = FindFirstComponent<CameraComponent>(entity);
            if (cameraComponent != null) {
                cameraComponent.Viewport = new float4(0f, 0f, 1f, 1f);
                if (primaryTopScreenCamera == null) {
                    primaryTopScreenCamera = cameraComponent;
                    primaryTopScreenCameraEntity = entity;
                }
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
                ConfigureTopScreenRootRecursive(
                    entity.Children[childIndex],
                    ref assignedPrimaryCamera,
                    ref primaryTopScreenCamera,
                    ref primaryTopScreenCameraEntity);
            }
        }

        /// <summary>
        /// Moves remaining authored 2D roots under the shared bottom-screen viewport so physics and rendering scenes use one handheld UI surface.
        /// </summary>
        /// <param name="topScreenRoots">Top-screen roots emitted by the DS scaffold before 2D reparenting.</param>
        /// <param name="bottomScreenViewportRoot">Bottom-screen viewport root that should own the moved 2D roots.</param>
        /// <returns>Adjusted top-screen roots that should remain as serialized scene roots.</returns>
        Entity[] Move2DRootsUnderBottomScreenViewport(Entity[] topScreenRoots, Entity bottomScreenViewportRoot) {
            if (topScreenRoots == null) {
                throw new ArgumentNullException(nameof(topScreenRoots));
            } else if (bottomScreenViewportRoot == null) {
                throw new ArgumentNullException(nameof(bottomScreenViewportRoot));
            }

            List<Entity> remainingRoots = new List<Entity>();
            List<Entity> movedRoots = new List<Entity>();
            for (int index = 0; index < topScreenRoots.Length; index++) {
                Entity rootEntity = topScreenRoots[index];
                if (rootEntity == null) {
                    continue;
                } else if (ReferenceEquals(rootEntity, bottomScreenViewportRoot)) {
                    remainingRoots.Add(rootEntity);
                    continue;
                }

                if (ContainsDrawable2DRecursive(rootEntity)) {
                    movedRoots.Add(rootEntity);
                    continue;
                }

                remainingRoots.Add(rootEntity);
            }

            if (movedRoots.Count < 1) {
                return topScreenRoots;
            }

            for (int index = 0; index < movedRoots.Count; index++) {
                Entity rootEntity = movedRoots[index];
                if (rootEntity.Parent != null) {
                    rootEntity.Parent.RemoveChild(rootEntity);
                }

                NormalizeBottomScreenCoordinatesRecursive(rootEntity);
                bottomScreenViewportRoot.AddChild(rootEntity);
            }

            return remainingRoots.ToArray();
        }

        /// <summary>
        /// Converts legacy stacked-dual-screen Y coordinates into the local 192px bottom-screen canvas.
        /// </summary>
        /// <param name="entity">Current moved 2D subtree entity.</param>
        void NormalizeBottomScreenCoordinatesRecursive(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity.LocalPosition.Y >= ScreenHeight * 2) {
                entity.LocalPosition = new float3(
                    entity.LocalPosition.X,
                    entity.LocalPosition.Y - ScreenHeight * 2,
                    entity.LocalPosition.Z);
            }

            if (entity.Children == null) {
                return;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                NormalizeBottomScreenCoordinatesRecursive(entity.Children[childIndex]);
            }
        }

        /// <summary>
        /// Relocates authored FPS overlay components from the top-screen scene roots into scaffold-owned bottom-screen entities.
        /// </summary>
        /// <param name="topScreenRoots">Top-screen scene roots that may contain authored FPS overlay components.</param>
        /// <param name="bottomScreenViewportRoot">Bottom-screen viewport root that should own the relocated FPS entities.</param>
        /// <param name="bottomOverlayFont">Live font asset assigned while the generated DS scene is being saved.</param>
        void RelocateFpsComponentsToBottomScreen(
            Entity[] topScreenRoots,
            Entity bottomScreenViewportRoot,
            FontAsset bottomOverlayFont) {
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
                    ref createdBottomScreenFpsCount);
            }

            if (createdBottomScreenFpsCount == 0) {
                CreateDefaultBottomScreenFpsEntity(bottomScreenViewportRoot, bottomOverlayFont);
            }
        }

        /// <summary>
        /// Relocates authored FPS overlay components from one subtree into scaffold-owned bottom-screen entities.
        /// </summary>
        /// <param name="entity">Current top-screen subtree entity being inspected.</param>
        /// <param name="bottomScreenViewportRoot">Bottom-screen viewport root that should own the relocated FPS entities.</param>
        /// <param name="bottomOverlayFont">Live font asset assigned while the generated DS scene is being saved.</param>
        /// <param name="createdBottomScreenFpsCount">Running count used to keep scaffold-owned FPS entity names stable.</param>
        void RelocateFpsComponentsToBottomScreenRecursive(
            Entity entity,
            Entity bottomScreenViewportRoot,
            FontAsset bottomOverlayFont,
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
                    ref createdBottomScreenFpsCount);
            }
        }

        /// <summary>
        /// Creates one scaffold-owned bottom-screen FPS entity that preserves the authored FPS overlay behavior.
        /// </summary>
        /// <param name="bottomScreenViewportRoot">Bottom-screen viewport root that should own the FPS entity.</param>
        /// <param name="bottomOverlayFont">Live font asset assigned while the generated DS scene is being saved.</param>
        /// <param name="sourceComponent">Authored top-screen FPS component being relocated.</param>
        /// <param name="fpsIndex">Zero-based scaffold-owned FPS entity index.</param>
        void CreateBottomScreenFpsEntity(
            Entity bottomScreenViewportRoot,
            FontAsset bottomOverlayFont,
            FPSComponent sourceComponent,
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
                : AssetAuthoringService.OwningCore.EntityFactory.CreateChild(bottomScreenViewportRoot, BuildBottomScreenFpsEntityName(fpsIndex));
            fpsEntity.LayerMask = PersistedSceneLayerMask;
            fpsEntity.LocalPosition = float3.Zero;
            fpsEntity.LocalScale = float3.One;
            fpsEntity.LocalOrientation = float4.Identity;

            FPSComponent bottomScreenFpsComponent = new FPSComponent {
                Font = bottomOverlayFont,
                FontScale = NintendoDsBottomOverlayFontScale,
                AdditionalText = sourceComponent.AdditionalText,
                RefreshIntervalSeconds = sourceComponent.RefreshIntervalSeconds,
                Padding = ResolveBottomScreenFpsPadding(sourceComponent, fpsIndex),
                RenderOrder2D = sourceComponent.RenderOrder2D
            };
            fpsEntity.AddComponent(bottomScreenFpsComponent);
            ApplyFontReference(fpsEntity, bottomScreenFpsComponent);
            ApplyNintendo3DsFpsOverride(fpsEntity, bottomScreenFpsComponent);
        }

        /// <summary>
        /// Creates one scaffold-owned bottom-screen FPS entity when the authored scene does not provide any FPS overlay to relocate.
        /// </summary>
        /// <param name="bottomScreenViewportRoot">Bottom-screen viewport root that should own the fallback FPS entity.</param>
        /// <param name="bottomOverlayFont">Live font asset assigned while the generated DS scene is being saved.</param>
        void CreateDefaultBottomScreenFpsEntity(Entity bottomScreenViewportRoot, FontAsset bottomOverlayFont) {
            if (bottomScreenViewportRoot == null) {
                throw new ArgumentNullException(nameof(bottomScreenViewportRoot));
            } else if (bottomOverlayFont == null) {
                throw new ArgumentNullException(nameof(bottomOverlayFont));
            }

            Entity fpsEntity = bottomScreenViewportRoot;
            fpsEntity.LayerMask = PersistedSceneLayerMask;
            fpsEntity.LocalPosition = float3.Zero;
            fpsEntity.LocalScale = float3.One;
            fpsEntity.LocalOrientation = float4.Identity;

            FPSComponent bottomScreenFpsComponent = new FPSComponent {
                Font = bottomOverlayFont,
                FontScale = NintendoDsBottomOverlayFontScale
            };
            fpsEntity.AddComponent(bottomScreenFpsComponent);
            ApplyFontReference(fpsEntity, bottomScreenFpsComponent);
            ApplyNintendo3DsFpsOverride(fpsEntity, bottomScreenFpsComponent);
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
        /// <param name="fpsIndex">Zero-based scaffold-owned FPS entity index.</param>
        /// <returns>Bottom-screen padding assigned to the relocated FPS overlay.</returns>
        int2 ResolveBottomScreenFpsPadding(FPSComponent sourceComponent, int fpsIndex) {
            if (sourceComponent == null) {
                throw new ArgumentNullException(nameof(sourceComponent));
            } else if (fpsIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(fpsIndex), "FPS entity index must be non-negative.");
            }

            int rowOffsetY = fpsIndex * 40;
            int2 padding = sourceComponent.Padding;
            return new int2(padding.X, padding.Y + rowOffsetY);
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
            Entity entity = AssetAuthoringService.OwningCore.EntityFactory.Create("DemoDiscBottomScreenCamera");
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 1,
                // The pointer hit resolver rejects interactables whose entity mask misses the camera mask,
                // so the bottom camera must render the same SceneObjects layer its buttons live on.
                LayerMask = PersistedSceneLayerMask,
                Viewport = new float4(0f, 1f, 1f, 1f),
                ClearSettings = new CameraClearSettings(
                    true,
                    NintendoDsBottomScreenClearColor,
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
        /// Persists the smaller Nintendo 3DS FPS scale while retaining the shared DS diagnostic definition.
        /// </summary>
        /// <param name="fpsEntity">Generated FPS entity receiving the platform override.</param>
        /// <param name="commonFpsComponent">Shared FPS component used as the DS baseline.</param>
        void ApplyNintendo3DsFpsOverride(Entity fpsEntity, FPSComponent commonFpsComponent) {
            if (fpsEntity == null) {
                throw new ArgumentNullException(nameof(fpsEntity));
            } else if (commonFpsComponent == null) {
                throw new ArgumentNullException(nameof(commonFpsComponent));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(fpsEntity);
            FPSComponent overrideComponent = (FPSComponent)PlatformEditingServiceValue.EnsurePlatformOverrideComponent(
                commonFpsComponent,
                saveComponent,
                Nintendo3DsPlatformId);
            overrideComponent.FontScale = Nintendo3DsFpsFontScale;
            PlatformEditingServiceValue.MarkPropertyOverride(
                commonFpsComponent,
                saveComponent,
                Nintendo3DsPlatformId,
                nameof(FPSComponent.FontScale));
            PlatformEditingServiceValue.PersistPlatformOverride(
                commonFpsComponent,
                overrideComponent,
                saveComponent,
                Nintendo3DsPlatformId);
        }

        /// <summary>
        /// Attaches the shared bottom-screen chrome Blueprint beneath the resolved bottom viewport root. The
        /// light and back buttons are identical in every handheld scene, so they arrive as one Blueprint
        /// instance instead of being rebuilt per scene.
        /// </summary>
        /// <param name="bottomScreenViewportRoot">Bottom-screen viewport root that should own the chrome.</param>
        void AttachBottomScreenChrome(Entity bottomScreenViewportRoot) {
            if (bottomScreenViewportRoot == null) {
                throw new ArgumentNullException(nameof(bottomScreenViewportRoot));
            }

            Entity chromeEntity = AssetAuthoringService.OwningCore.EntityFactory.CreateChild(
                bottomScreenViewportRoot,
                HandheldBottomScreenChromeBlueprintGenerator.ChromeRootEntityName);
            chromeEntity.LayerMask = PersistedSceneLayerMask;
            chromeEntity.LocalPosition = float3.Zero;
            chromeEntity.LocalScale = float3.One;
            chromeEntity.LocalOrientation = float4.Identity;
            chromeEntity.AddComponent(new BlueprintInstanceComponent {
                BlueprintAssetReference = AssetAuthoringService.CreateFileReference(
                    HandheldBottomScreenChromeBlueprintGenerator.BlueprintRelativePath,
                    AssetEntryKind.Blueprint)
            });
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
        /// Returns whether the supplied subtree contains any 2D drawable component that should be owned by one screen-specific viewport binding.
        /// </summary>
        /// <param name="entity">Subtree root to inspect.</param>
        /// <returns>True when the subtree contains one 2D drawable component; otherwise false.</returns>
        bool ContainsDrawable2DRecursive(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity.Components != null) {
                for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                    if (entity.Components[componentIndex] is IDrawable2D) {
                        return true;
                    }
                }
            }

            if (entity.Children == null) {
                return false;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                if (ContainsDrawable2DRecursive(entity.Children[childIndex])) {
                    return true;
                }
            }

            return false;
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
        /// Stores the shared authored body-font reference on the generated scene save state for the given component.
        /// </summary>
        /// <param name="entity">Entity that owns the component.</param>
        /// <param name="component">Component whose font reference should be stored.</param>
        void ApplyFontReference(Entity entity, Component component) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, "Font", DemoDiscSceneComponentRecordFactory.CreateEditorFontReference((IEditorProjectAuthoringSession)AssetAuthoringService));
        }

        /// <summary>
        /// Stores one explicit font reference on the generated scene save state for the given component.
        /// </summary>
        /// <param name="entity">Entity that owns the component.</param>
        /// <param name="component">Component whose font reference should be stored.</param>
        /// <param name="fontReference">Explicit font reference that should be persisted.</param>
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

            return ((IEditorProjectAuthoringSession)AssetAuthoringService).CreateFileReference(relativePath.Replace('\\', '/'), AssetEntryKind.Image);
        }
    }
}
