using city.menu;
using city.rendering;
using city.rendering.tools;
using helengine;
using helengine.editor;

namespace city.menu.tools {
    /// <summary>
    /// Authors the live demo-disc main menu scene hierarchy before it is persisted through the editor save pipeline.
    /// </summary>
    public sealed class DemoDiscStandardMainMenuSceneFactory {
        /// <summary>
        /// Stable scene id used by the generated top-level demo-disc menu scene.
        /// </summary>
        public const string SceneId = "Scenes/DemoDiscMainMenu.helen";

        /// <summary>
        /// Main-console menu panel width expressed as a fraction of the authored canvas width.
        /// </summary>
        const double MainMenuPanelWidthRatio = 0.4d;

        /// <summary>
        /// Runtime 2D layer mask used by baked menu visuals after authored scene layers are normalized during packaging.
        /// </summary>
        const byte RuntimeLayerMask = 0b00000001;

        /// <summary>
        /// Fixed top offset where panel item viewports begin beneath the menu header chrome.
        /// </summary>
        const float ItemsViewportTop = 90f;

        /// <summary>
        /// Fixed Nintendo DS screen width in authored pixels.
        /// </summary>
        const int NintendoDsScreenWidth = 256;

        /// <summary>
        /// Fixed Nintendo DS screen height in authored pixels.
        /// </summary>
        const int NintendoDsScreenHeight = 192;

        /// <summary>
        /// Fixed Nintendo DS button height in authored pixels.
        /// </summary>
        const int NintendoDsButtonHeight = 36;

        /// <summary>
        /// Fixed Nintendo DS button spacing in authored pixels.
        /// </summary>
        const int NintendoDsButtonSpacing = 4;

        /// <summary>
        /// Fixed Nintendo DS logo width in authored pixels.
        /// </summary>
        const int NintendoDsLogoWidth = 128;

        /// <summary>
        /// Stable relative animation-clip path used by the rotating demo-disc logo.
        /// </summary>
        const string DemoDiscLogoIdleAnimationRelativePath = "Animations/DemoDiscLogoIdle.hanim";

        /// <summary>
        /// Fixed top inset applied to the Nintendo DS rotating logo so affine OBJ tiles never need negative screen anchors.
        /// </summary>
        const int NintendoDsLogoTopInset = 36;

        /// <summary>
        /// Fixed left inset applied to visible Nintendo DS bottom-menu item labels.
        /// </summary>
        const int NintendoDsBottomMenuLabelLeft = 8;

        /// <summary>
        /// Fixed top inset applied to visible Nintendo DS bottom-menu item labels.
        /// </summary>
        const int NintendoDsBottomMenuLabelTop = 6;

        /// <summary>
        /// Fixed width applied to visible Nintendo DS bottom-menu item labels.
        /// </summary>
        const int NintendoDsBottomMenuLabelWidth = NintendoDsScreenWidth - 16;

        /// <summary>
        /// Fixed height applied to visible Nintendo DS bottom-menu item labels.
        /// </summary>
        const int NintendoDsBottomMenuLabelHeight = 24;

        /// <summary>
        /// Fixed font scale applied to visible Nintendo DS bottom-menu item labels.
        /// </summary>
        const float NintendoDsBottomMenuLabelFontScale = 1f;

        /// <summary>
        /// Fixed render order applied to visible Nintendo DS bottom-menu item labels.
        /// </summary>
        const byte NintendoDsBottomMenuLabelRenderOrder = 220;

        /// <summary>
        /// Dedicated authored scene layer used by the Nintendo DS menu metadata subtree so scene save preserves the runtime panel-binding structure.
        /// </summary>
        const ushort NintendoDsMenuMetadataLayerMask = EditorLayerMasks.SceneObjects;

        /// <summary>
        /// Stable save-state slot name used for serialized font references.
        /// </summary>
        const string FontReferenceName = "Font";

        /// <summary>
        /// Placeholder font assigned during live authoring before the real file-backed font references are serialized.
        /// </summary>
        readonly FontAsset PlaceholderFont;

        /// <summary>
        /// Initializes one demo-disc main menu scene factory.
        /// </summary>
        public DemoDiscStandardMainMenuSceneFactory() {
            PlaceholderFont = new FontAsset(
                new FontInfo("CityDemoDiscPlaceholder", 16, 4f),
                new ManagedRuntimeTexture {
                    Width = 1,
                    Height = 1
                },
                new Dictionary<char, FontChar>(),
                16f,
                1,
                1);
        }

        /// <summary>
        /// Creates the canonical live-authored demo-disc main menu scene definition.
        /// </summary>
        /// <param name="providerTypeName">Assembly-qualified menu provider type name persisted on the menu root.</param>
        /// <param name="definition">Menu definition used to author the live hierarchy.</param>
        /// <returns>Live-authored demo-disc main menu scene definition.</returns>
        public GeneratedAuthoringSceneDefinition CreateSceneDefinition(string providerTypeName, MenuDefinition definition) {
            if (string.IsNullOrWhiteSpace(providerTypeName)) {
                throw new ArgumentException("Provider type name must be provided.", nameof(providerTypeName));
            } else if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            }

            return new GeneratedAuthoringSceneDefinition {
                SceneId = SceneId,
                SceneSettings = new SceneSettingsAsset {
                    CanvasProfile = new SceneCanvasProfile {
                        Width = DemoMenuLayout.CanvasWidth,
                        Height = DemoMenuLayout.CanvasHeight
                    }
                },
                RootEntities = new[] {
                    CreateCameraEntity(),
                    CreateMenuRootEntity(providerTypeName, definition)
                }
            };
        }

        /// <summary>
        /// Creates the authored camera entity used by the demo-disc menu scene.
        /// </summary>
        /// <returns>Live-authored camera entity.</returns>
        Entity CreateCameraEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("DemoDiscCamera");
            entity.AddComponent(new CameraComponent {
                CameraDrawOrder = 0,
                LayerMask = EditorLayerMasks.SceneObjects,
                Viewport = new float4(0f, 0f, 1f, 1f),
                ClearSettings = new CameraClearSettings(
                    true,
                    new float4(0.11764706f, 0.06666667f, 0.16078432f, 1f),
                    true,
                    1f,
                    true,
                    1),
                RenderSettings = new CameraRenderSettings {
                    DepthPrepassMode = DepthPrepassMode.Auto,
                    ShadowDistance = 50f,
                    PostProcessTier = PostProcessTier.High
                }
            });
            return entity;
        }

        /// <summary>
        /// Creates the authored menu root entity and its generated fitted child subtree.
        /// </summary>
        /// <param name="providerTypeName">Assembly-qualified menu provider type name persisted on the menu root.</param>
        /// <param name="definition">Menu definition used to author the live hierarchy.</param>
        /// <returns>Live-authored menu root entity.</returns>
        Entity CreateMenuRootEntity(string providerTypeName, MenuDefinition definition) {
            Entity entity = Core.Instance.EntityFactory.Create("DemoDiscMenuRoot");
            entity.AddComponent(new MenuComponent {
                ProviderTypeName = providerTypeName,
                InitialPanelId = definition.InitialPanelId
            });
            entity.AddComponent(new ViewportComponent {
                BindingMode = ViewportComponent.ScreenBindingMode,
                FixedSize = new int2(DemoMenuLayout.CanvasWidth, DemoMenuLayout.CanvasHeight)
            });
            entity.AddComponent(new ReferenceCanvasFitComponent {
                ReferenceWidth = DemoMenuLayout.CanvasWidth,
                ReferenceHeight = DemoMenuLayout.CanvasHeight
            });

            Entity generatedRootEntity = Core.Instance.EntityFactory.CreateChild(entity, DemoMenuLayout.GeneratedRootEntityName);

            if (definition.OverlayImage != null) {
                CreateOverlayImageEntity(generatedRootEntity, definition.OverlayImage);
            }
            if (definition.PlatformInfoOverlay != null) {
                CreatePlatformInfoOverlayEntity(generatedRootEntity, definition, definition.PlatformInfoOverlay);
            }

            for (int panelIndex = 0; panelIndex < definition.Panels.Length; panelIndex++) {
                CreatePanelEntity(generatedRootEntity, definition, definition.Panels[panelIndex]);
            }

            return entity;
        }

        /// <summary>
        /// Creates one authored menu panel subtree.
        /// </summary>
        /// <param name="generatedRootEntity">Generated menu subtree root that owns the panel.</param>
        /// <param name="definition">Menu definition that owns the panel.</param>
        /// <param name="panelDefinition">Panel definition that should be authored.</param>
        void CreatePanelEntity(Entity generatedRootEntity, MenuDefinition definition, MenuPanelDefinition panelDefinition) {
            if (generatedRootEntity == null) {
                throw new ArgumentNullException(nameof(generatedRootEntity));
            } else if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            } else if (panelDefinition == null) {
                throw new ArgumentNullException(nameof(panelDefinition));
            }

            Entity panelEntity = Core.Instance.EntityFactory.CreateChild(generatedRootEntity, $"Panel-{panelDefinition.PanelId}");
            panelEntity.LocalPosition = new float3(0f, 0f, 0f);
            panelEntity.Enabled = string.Equals(panelDefinition.PanelId, definition.InitialPanelId, StringComparison.Ordinal);

            MenuPanelComponent panelComponent = new MenuPanelComponent {
                PanelId = panelDefinition.PanelId
            };
            panelEntity.AddComponent(panelComponent);

            LayoutComponent anchorComponent = new LayoutComponent();
            anchorComponent.SetAnchorDistances(left: 0f, top: 0f);
            panelEntity.AddComponent(anchorComponent);

            Entity backgroundEntity = CreateBackgroundEntity(
                panelEntity,
                $"panel-{panelDefinition.PanelId}-surface",
                new float3(0f, 0f, 0f),
                ResolveMainMenuPanelSize(),
                18f,
                3f,
                definition.SurfaceColor,
                definition.SurfaceBorderColor,
                30);
            LayoutComponent backgroundLayoutComponent = new LayoutComponent {
                LayoutSpace = LayoutComponent.CameraViewportLayoutSpace
            };
            backgroundLayoutComponent.SetAnchorDistances(left: 0f, top: 0f, bottom: 0f);
            backgroundEntity.AddComponent(backgroundLayoutComponent);

            Entity itemsViewportEntity = Core.Instance.EntityFactory.CreateChild(panelEntity, $"Panel-{panelDefinition.PanelId}-ItemsViewport");
            itemsViewportEntity.LocalPosition = new float3(0f, ItemsViewportTop, 0f);
            itemsViewportEntity.AddComponent(new ClipRectComponent {
                Size = BuildItemsViewportSize(panelDefinition)
            });
            LayoutComponent itemsViewportLayoutComponent = new LayoutComponent {
                LayoutSpace = LayoutComponent.CameraViewportLayoutSpace
            };
            itemsViewportLayoutComponent.SetAnchorDistances(left: 0f, top: ItemsViewportTop, bottom: 0f);
            itemsViewportEntity.AddComponent(itemsViewportLayoutComponent);

            Entity itemsRootEntity = Core.Instance.EntityFactory.CreateChild(itemsViewportEntity, $"Panel-{panelDefinition.PanelId}-ItemsRoot");
            itemsRootEntity.AddComponent(new ScrollComponent {
                Size = BuildItemsViewportSize(panelDefinition),
                ItemCount = CountEnabledItems(panelDefinition),
                VisibleItemCount = ResolveVisibleItemCount(panelDefinition),
                ScrollStepCount = 1,
                WheelNotchSize = 120,
                RequiresPointerInside = true
            });

            int visibleIndex = 0;
            for (int itemIndex = 0; itemIndex < panelDefinition.Items.Length; itemIndex++) {
                MenuItemDefinition itemDefinition = panelDefinition.Items[itemIndex];
                if (!itemDefinition.Enabled) {
                    continue;
                }

                CreateItemEntity(itemsRootEntity, definition, panelDefinition, itemDefinition, visibleIndex);
                visibleIndex++;
            }
        }

        /// <summary>
        /// Resolves the main-console menu panel size from the authored canvas size.
        /// </summary>
        /// <returns>Panel size in authored scene pixels.</returns>
        int2 ResolveMainMenuPanelSize() {
            int panelWidth = (int)Math.Round((double)DemoMenuLayout.CanvasWidth * MainMenuPanelWidthRatio);
            return new int2(panelWidth, DemoMenuLayout.CanvasHeight);
        }

        /// <summary>
        /// Creates one authored item row entity inside the supplied items root.
        /// </summary>
        /// <param name="itemsRootEntity">Scrolling item root that owns the row.</param>
        /// <param name="definition">Menu definition that owns the row.</param>
        /// <param name="panelDefinition">Panel that owns the row.</param>
        /// <param name="itemDefinition">Item definition that should be authored.</param>
        /// <param name="visibleIndex">Zero-based visible item index within the panel.</param>
        void CreateItemEntity(Entity itemsRootEntity, MenuDefinition definition, MenuPanelDefinition panelDefinition, MenuItemDefinition itemDefinition, int visibleIndex) {
            if (itemsRootEntity == null) {
                throw new ArgumentNullException(nameof(itemsRootEntity));
            } else if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            } else if (panelDefinition == null) {
                throw new ArgumentNullException(nameof(panelDefinition));
            } else if (itemDefinition == null) {
                throw new ArgumentNullException(nameof(itemDefinition));
            }

            byte4 idleFillColor = definition.AccentSecondaryColor;
            byte4 idleBorderColor = definition.SurfaceBorderColor;
            byte4 selectedFillColor = definition.AccentColor;
            byte4 selectedBorderColor = definition.AccentColor;

            Entity itemEntity = Core.Instance.EntityFactory.CreateChild(itemsRootEntity, $"Item-{itemDefinition.ItemId}");
            itemEntity.LocalPosition = new float3(0f, visibleIndex * (DemoMenuLayout.ButtonHeight + DemoMenuLayout.ButtonSpacing), 0f);
            itemEntity.AddComponent(new MenuItemComponent {
                PanelId = panelDefinition.PanelId,
                ItemId = itemDefinition.ItemId,
                ActionKind = itemDefinition.Action.Kind,
                TargetId = itemDefinition.Action.TargetId,
                IdleFillColor = idleFillColor,
                IdleBorderColor = idleBorderColor,
                SelectedFillColor = selectedFillColor,
                SelectedBorderColor = selectedBorderColor
            });
            itemEntity.AddComponent(new RoundedRectComponent {
                Size = new int2(DemoMenuLayout.ButtonWidth, DemoMenuLayout.ButtonHeight),
                Radius = 7.2f,
                BorderThickness = 2f,
                FillColor = visibleIndex == 0 ? selectedFillColor : idleFillColor,
                BorderColor = visibleIndex == 0 ? selectedBorderColor : idleBorderColor,
                RenderOrder2D = 33,
                LayerMask = RuntimeLayerMask
            });

            CreateTextEntity(
                itemEntity,
                $"item-label-{itemDefinition.ItemId}",
                new float3(20f, 10f, 0.1f),
                itemDefinition.Label,
                definition.BodyFontPath,
                definition.TextColor,
                new int2(DemoMenuLayout.ButtonWidth - 40, 76),
                34,
                null,
                2f,
                true);
        }

        /// <summary>
        /// Creates one text entity beneath the supplied parent.
        /// </summary>
        /// <param name="parent">Parent entity that should own the text entity.</param>
        /// <param name="entityName">Stable entity name.</param>
        /// <param name="localPosition">Local position applied to the entity.</param>
        /// <param name="text">Authored text content.</param>
        /// <param name="fontPath">Project-relative font path assigned through save metadata.</param>
        /// <param name="color">Text color.</param>
        /// <param name="size">Text layout size.</param>
        /// <param name="renderOrder2D">2D render order.</param>
        /// <param name="anchorComponent">Optional anchor component attached to the entity.</param>
        /// <param name="fontScale">Uniform glyph scale applied to the authored text component.</param>
        /// <param name="isStatic">Whether the authored text entity should be marked static for runtime caching.</param>
        void CreateTextEntity(Entity parent, string entityName, float3 localPosition, string text, string fontPath, byte4 color, int2 size, byte renderOrder2D, helengine.LayoutComponent anchorComponent, float fontScale = 1f, bool isStatic = true) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            } else if (string.IsNullOrWhiteSpace(fontPath)) {
                throw new ArgumentException("Font path must be provided.", nameof(fontPath));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, entityName);
            entity.LocalPosition = localPosition;
            entity.Static = isStatic;

            TextComponent textComponent = new TextComponent {
                Text = text ?? string.Empty,
                Font = PlaceholderFont,
                Color = color,
                Size = size,
                FontScale = fontScale,
                RenderOrder2D = renderOrder2D,
                LayerMask = RuntimeLayerMask
            };
            entity.AddComponent(textComponent);
            ApplyFontReference(entity, textComponent, fontPath);

            if (anchorComponent != null) {
                entity.AddComponent(anchorComponent);
            }
        }

        /// <summary>
        /// Creates one Nintendo DS text entity that resolves its font through the generated DS debug font reference instead of the shared file-backed body font.
        /// </summary>
        /// <param name="parent">Parent entity that should own the text entity.</param>
        /// <param name="entityName">Stable entity name.</param>
        /// <param name="localPosition">Local position applied to the entity.</param>
        /// <param name="text">Authored text content.</param>
        /// <param name="color">Text color.</param>
        /// <param name="size">Text layout size.</param>
        /// <param name="renderOrder2D">2D render order.</param>
        /// <param name="anchorComponent">Optional anchor component attached to the entity.</param>
        /// <param name="fontScale">Uniform glyph scale applied to the authored text component.</param>
        /// <param name="isStatic">Whether the authored text entity should be marked static for runtime caching.</param>
        /// <param name="convertTextToSprite">Whether scene packaging should bake this authored text into a sprite-backed runtime component.</param>
        void CreateNintendoDsTextEntity(Entity parent, string entityName, float3 localPosition, string text, string fontPath, byte4 color, int2 size, byte renderOrder2D, helengine.LayoutComponent anchorComponent, float fontScale = 1f, bool isStatic = true, bool convertTextToSprite = false) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            } else if (string.IsNullOrWhiteSpace(fontPath)) {
                throw new ArgumentException("Font path must be provided.", nameof(fontPath));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, entityName);
            entity.LocalPosition = localPosition;
            entity.Static = isStatic;

            TextComponent textComponent = new TextComponent {
                Text = text ?? string.Empty,
                Font = PlaceholderFont,
                Color = color,
                Size = size,
                FontScale = fontScale,
                RenderOrder2D = renderOrder2D,
                LayerMask = RuntimeLayerMask,
                ConvertTextToSprite = convertTextToSprite
            };
            entity.AddComponent(textComponent);
            ApplyFontReference(entity, textComponent, fontPath);

            if (anchorComponent != null) {
                entity.AddComponent(anchorComponent);
            }
        }

        /// <summary>
        /// Creates one rounded-rectangle background entity beneath the supplied parent.
        /// </summary>
        /// <param name="parent">Parent entity that should own the background entity.</param>
        /// <param name="entityName">Stable entity name.</param>
        /// <param name="localPosition">Local position applied to the entity.</param>
        /// <param name="size">Rounded-rectangle size.</param>
        /// <param name="radius">Corner radius.</param>
        /// <param name="borderThickness">Border thickness.</param>
        /// <param name="fillColor">Fill color.</param>
        /// <param name="borderColor">Border color.</param>
        /// <param name="renderOrder2D">2D render order.</param>
        /// <returns>The authored background entity so callers can attach layout behavior when needed.</returns>
        Entity CreateBackgroundEntity(Entity parent, string entityName, float3 localPosition, int2 size, float radius, float borderThickness, byte4 fillColor, byte4 borderColor, byte renderOrder2D) {
            if (parent == null) {
                throw new ArgumentNullException(nameof(parent));
            } else if (string.IsNullOrWhiteSpace(entityName)) {
                throw new ArgumentException("Entity name must be provided.", nameof(entityName));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(parent, entityName);
            entity.LocalPosition = localPosition;
            entity.AddComponent(new RoundedRectComponent {
                Size = size,
                Radius = radius,
                BorderThickness = borderThickness,
                FillColor = fillColor,
                BorderColor = borderColor,
                RenderOrder2D = renderOrder2D,
                LayerMask = RuntimeLayerMask
            });

            return entity;
        }

        /// <summary>
        /// Creates the decorative overlay sprite entity pinned to the bottom-right of the live viewport.
        /// </summary>
        /// <param name="generatedRootEntity">Generated menu subtree root that owns the overlay.</param>
        /// <param name="overlayImage">Overlay image definition that should be authored.</param>
        void CreateOverlayImageEntity(Entity generatedRootEntity, MenuOverlayImageDefinition overlayImage) {
            if (generatedRootEntity == null) {
                throw new ArgumentNullException(nameof(generatedRootEntity));
            } else if (overlayImage == null) {
                throw new ArgumentNullException(nameof(overlayImage));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(generatedRootEntity, "DemoDiscOverlayImage");
            SpriteComponent spriteComponent = new SpriteComponent {
                Size = new int2(overlayImage.Width, overlayImage.Height),
                RenderOrder2D = 28,
                LayerMask = RuntimeLayerMask
            };
            entity.AddComponent(spriteComponent);
            ApplyTextureReference(entity, spriteComponent, overlayImage.TexturePath);

            AnimationPlayerComponent animationPlayerComponent = new AnimationPlayerComponent {
                Clip = LoadRequiredAnimationClipAsset(DemoDiscLogoIdleAnimationRelativePath),
                PlayAutomatically = true,
                ShouldLoop = true
            };
            entity.AddComponent(animationPlayerComponent);
            ApplyAnimationClipReference(entity, animationPlayerComponent, DemoDiscLogoIdleAnimationRelativePath);

            LayoutComponent anchorComponent = new LayoutComponent {
                LayoutSpace = LayoutComponent.CameraViewportLayoutSpace
            };
            anchorComponent.SetAnchorDistances(right: overlayImage.RightMargin, bottom: overlayImage.BottomMargin);
            entity.AddComponent(anchorComponent);
        }

        /// <summary>
        /// Creates the platform-info overlay entity and its two child text rows.
        /// </summary>
        /// <param name="generatedRootEntity">Generated menu subtree root that owns the overlay.</param>
        /// <param name="definition">Menu definition that provides the body font and colors.</param>
        /// <param name="platformInfoOverlay">Platform-info layout definition.</param>
        void CreatePlatformInfoOverlayEntity(Entity generatedRootEntity, MenuDefinition definition, MenuPlatformInfoDefinition platformInfoOverlay) {
            if (generatedRootEntity == null) {
                throw new ArgumentNullException(nameof(generatedRootEntity));
            } else if (definition == null) {
                throw new ArgumentNullException(nameof(definition));
            } else if (platformInfoOverlay == null) {
                throw new ArgumentNullException(nameof(platformInfoOverlay));
            }

            Entity entity = Core.Instance.EntityFactory.CreateChild(generatedRootEntity, "DemoDiscPlatformInfoOverlay");
            LayoutComponent anchorComponent = new LayoutComponent();
            anchorComponent.LayoutSpace = LayoutComponent.CameraViewportLayoutSpace;
            anchorComponent.SetAnchorDistances(right: platformInfoOverlay.RightMargin, top: platformInfoOverlay.TopMargin);
            entity.AddComponent(anchorComponent);
            entity.AddComponent(new PlatformInfoTextComponent());

            CreateTextEntity(entity, "DemoDiscPlatformInfoNameText", new float3(0f, 0f, 0.1f), string.Empty, definition.BodyFontPath, definition.TextColor, new int2(1, 1), 42, null, 2f, false);
            CreateTextEntity(entity, "DemoDiscPlatformInfoVersionText", new float3(0f, platformInfoOverlay.LineSpacing, 0.1f), string.Empty, definition.BodyFontPath, definition.MutedTextColor, new int2(1, 1), 42, null, 2f, false);
        }

        /// <summary>
        /// Stores the supplied file-backed font reference on the entity save state for the given component.
        /// </summary>
        /// <param name="entity">Entity that owns the component.</param>
        /// <param name="component">Component whose font reference should be stored.</param>
        /// <param name="fontPath">Project-relative font path.</param>
        void ApplyFontReference(Entity entity, Component component, string fontPath) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            } else if (string.IsNullOrWhiteSpace(fontPath)) {
                throw new ArgumentException("Font path must be provided.", nameof(fontPath));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(component, FontReferenceName, global::helengine.SceneAssetReferenceFactory.CreateFileSystemFont(fontPath));
        }

        /// <summary>
        /// Stores the supplied file-backed texture reference on the entity save state for the given component.
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
            saveComponent.SetAssetReference(component, TextureAssetScenePersistenceSupport.TextureReferenceName, global::helengine.SceneAssetReferenceFactory.CreateFileSystemTexture(texturePath));
        }

        /// <summary>
        /// Stores the supplied file-backed animation clip reference on the entity save state for the given component.
        /// </summary>
        /// <param name="entity">Entity that owns the component.</param>
        /// <param name="component">Component whose animation clip reference should be stored.</param>
        /// <param name="animationClipPath">Project-relative animation clip path.</param>
        void ApplyAnimationClipReference(Entity entity, Component component, string animationClipPath) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            } else if (string.IsNullOrWhiteSpace(animationClipPath)) {
                throw new ArgumentException("Animation clip path must be provided.", nameof(animationClipPath));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(
                component,
                AutomaticComponentAssetReferenceSupport.BuildReferenceName(nameof(AnimationPlayerComponent.Clip)),
                global::helengine.SceneAssetReferenceFactory.CreateFileSystemAnimationClip(animationClipPath));
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

            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            throw new InvalidOperationException("Generated entities must include EntitySaveComponent.");
        }

        /// <summary>
        /// Loads one required authored animation clip from the current editor project assets folder so the generated scene preview plays the same data that gets serialized.
        /// </summary>
        /// <param name="relativePath">Project-relative animation clip path.</param>
        /// <returns>Deserialized animation clip asset.</returns>
        AnimationClipAsset LoadRequiredAnimationClipAsset(string relativePath) {
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentException("Animation clip path must be provided.", nameof(relativePath));
            } else if (string.IsNullOrWhiteSpace(EditorProjectPaths.AssetsRoot)) {
                throw new InvalidOperationException("Editor project paths must be initialized before generated animation clips can be loaded.");
            }

            string fullPath = Path.GetFullPath(Path.Combine(EditorProjectPaths.AssetsRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
            string assetsRootPrefix = EnsureTrailingDirectorySeparator(EditorProjectPaths.AssetsRoot);
            if (!fullPath.StartsWith(assetsRootPrefix, StringComparison.OrdinalIgnoreCase)) {
                throw new InvalidOperationException("Generated animation clip paths must stay inside the project assets folder.");
            }
            if (!File.Exists(fullPath)) {
                throw new FileNotFoundException("Generated animation clip asset could not be found.", fullPath);
            }

            using FileStream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (global::helengine.AssetSerializer.Deserialize(stream) is not AnimationClipAsset animationClipAsset) {
                throw new InvalidOperationException($"Asset '{relativePath}' is not an animation clip.");
            }

            return animationClipAsset;
        }

        /// <summary>
        /// Ensures one directory path ends with a trailing separator before prefix comparisons occur.
        /// </summary>
        /// <param name="path">Directory path that should end with a separator.</param>
        /// <returns>Directory path with a trailing separator.</returns>
        string EnsureTrailingDirectorySeparator(string path) {
            if (string.IsNullOrWhiteSpace(path)) {
                throw new ArgumentException("Directory path must be provided.", nameof(path));
            }

            if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                || path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal)) {
                return path;
            }

            return string.Concat(path, Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Counts the number of enabled items in the supplied panel.
        /// </summary>
        /// <param name="panelDefinition">Panel whose enabled items should be counted.</param>
        /// <returns>Enabled item count.</returns>
        int CountEnabledItems(MenuPanelDefinition panelDefinition) {
            if (panelDefinition == null) {
                throw new ArgumentNullException(nameof(panelDefinition));
            }

            int count = 0;
            for (int itemIndex = 0; itemIndex < panelDefinition.Items.Length; itemIndex++) {
                if (panelDefinition.Items[itemIndex].Enabled) {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Resolves the validated authored visible-row count for one panel.
        /// </summary>
        /// <param name="panelDefinition">Panel whose visible-row count should be validated.</param>
        /// <returns>Validated visible-row count.</returns>
        int ResolveVisibleItemCount(MenuPanelDefinition panelDefinition) {
            if (panelDefinition == null) {
                throw new ArgumentNullException(nameof(panelDefinition));
            } else if (panelDefinition.VisibleItemCount < 1) {
                throw new InvalidOperationException($"Menu panel '{panelDefinition.PanelId}' must expose at least one visible row.");
            }

            return panelDefinition.VisibleItemCount;
        }

        /// <summary>
        /// Builds the fixed viewport size used for one panel item list.
        /// </summary>
        /// <param name="panelDefinition">Panel whose visible-row count determines the viewport height.</param>
        /// <returns>Viewport size in authored scene pixels.</returns>
        int2 BuildItemsViewportSize(MenuPanelDefinition panelDefinition) {
            if (panelDefinition == null) {
                throw new ArgumentNullException(nameof(panelDefinition));
            }

            int visibleItemCount = ResolveVisibleItemCount(panelDefinition);
            int viewportHeight = (visibleItemCount * DemoMenuLayout.ButtonHeight)
                + ((visibleItemCount - 1) * DemoMenuLayout.ButtonSpacing);
            if (string.Equals(panelDefinition.PanelId, "scene-select", StringComparison.Ordinal)) {
                viewportHeight = DemoMenuLayout.CanvasHeight - (int)Math.Round((double)ItemsViewportTop);
            }

            return new int2(DemoMenuLayout.ButtonWidth, viewportHeight);
        }
    }
}
