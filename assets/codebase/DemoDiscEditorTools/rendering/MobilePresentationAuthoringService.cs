using helengine.editor;
using DemoDisc.rendering;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Authors the presentation of the <c>mobile</c> platform group on live generated entities: density-scaled
    /// viewports, scalable glyphs, physical edge anchors, and the choice between duplicated presentation entities.
    /// Every rule is scoped to <see cref="DemoDiscOverrideScopes.Mobile"/>, so no generator names a platform; every other
    /// group keeps the common 1280 by 720 reference-canvas presentation. Generators call it before the entities are saved,
    /// and after their final entity ids are assigned, so a plain regeneration writes the complete scene.
    /// </summary>
    public sealed class MobilePresentationAuthoringService {
        /// <summary>
        /// Approved mobile overlay multiplier, producing the 650 by 348 instruction panel at 420 DPI.
        /// </summary>
        public const float MobileUiScale = 0.5625f;

        /// <summary>
        /// Baseline mobile density (160 DPI) expressed in pixels per centimeter.
        /// </summary>
        public const float MobilePixelsPerCentimeter = (float)(160d / 2.54d);

        /// <summary>
        /// Stable component key of the mobile-only density viewport added beside each FPS counter.
        /// </summary>
        public const string FpsDensityViewportComponentKey = "c0de0000000000000000000000000001";

        /// <summary>
        /// Stable component key of the mobile-only layout component that pins a widget to its screen edges.
        /// </summary>
        public const string EdgeAnchorLayoutComponentKey = "c0de0000000000000000000000000002";

        /// <summary>
        /// Name of the legacy scene-local instruction viewport root that the shared Blueprint replaces on mobile.
        /// </summary>
        public const string LegacyInstructionRootEntityName = "DemoSceneInstructionViewport";

        /// <summary>
        /// Name of the scene root that instantiates the shared console camera and light instruction Blueprint.
        /// </summary>
        public const string InstructionBlueprintRootEntityName = "ConsoleCameraLightInstructions";

        /// <summary>
        /// Width of the reference canvas the generated overlays are authored against.
        /// </summary>
        const float ReferenceCanvasWidth = 1280f;

        /// <summary>
        /// Height of the reference canvas the generated overlays are authored against.
        /// </summary>
        const float ReferenceCanvasHeight = 720f;

        /// <summary>
        /// Editor service that clones, marks and serializes scoped component overrides on live entities.
        /// </summary>
        readonly ComponentPlatformEditingService PlatformEditing = new ComponentPlatformEditingService();

        /// <summary>
        /// Gets the group scope every rule of this service is authored at.
        /// </summary>
        public static EditorOverrideScope MobileScope => DemoDiscOverrideScopes.Mobile;

        /// <summary>
        /// Authors the mobile presentation of one scene's single-screen presentation roots. When the scene also mounts
        /// the shared instruction Blueprint, the legacy scene-local instruction panel is excluded on mobile and the
        /// Blueprint presents the controls instead. Every other root is density scaled, and the entity references of
        /// the light toggle and the scene label are pinned for mobile. The Blueprint root itself carries no rule: the
        /// Blueprint generator authors its mobile presentation.
        /// </summary>
        /// <param name="presentationRoots">Single-screen presentation roots of one generated scene.</param>
        public void ConfigureScenePresentation(IReadOnlyList<Entity> presentationRoots) {
            if (presentationRoots == null) {
                throw new ArgumentNullException(nameof(presentationRoots));
            }

            bool hasInstructionBlueprint = false;
            for (int index = 0; index < presentationRoots.Count; index++) {
                if (IsNamed(presentationRoots[index], InstructionBlueprintRootEntityName)) {
                    hasInstructionBlueprint = true;
                }
            }

            for (int index = 0; index < presentationRoots.Count; index++) {
                Entity root = presentationRoots[index];
                if (root == null || IsNamed(root, InstructionBlueprintRootEntityName)) {
                    continue;
                } else if (hasInstructionBlueprint && IsNamed(root, LegacyInstructionRootEntityName)) {
                    Exclude(root);
                } else {
                    ConfigureDensitySubtree(root);
                    PinSceneUiReferences(root);
                }
            }
        }

        /// <summary>
        /// Switches every viewport, text and FPS counter in one generated subtree to density scaling on mobile.
        /// Viewports scale by physical density with <see cref="MobileUiScale"/>; text and FPS glyphs stop snapping to
        /// native font pixels so they scale with their viewport; each FPS counter gains a mobile-only density viewport.
        /// </summary>
        /// <param name="root">Generated subtree root whose overlay components should be configured.</param>
        public void ConfigureDensitySubtree(Entity root) {
            if (root == null) {
                throw new ArgumentNullException(nameof(root));
            }

            Component[] components = root.Components.ToArray();
            for (int index = 0; index < components.Length; index++) {
                if (components[index] is ViewportComponent viewport) {
                    EntitySaveComponent save = FindRequiredGroupFirstSaveComponent(root);
                    ViewportComponent mobile = (ViewportComponent)PlatformEditing.EnsureScopeOverrideComponent(viewport, save, MobileScope);
                    mobile.ScalingMode = ViewportComponent.DensityScalingMode;
                    mobile.PixelsPerCentimeter = MobilePixelsPerCentimeter;
                    mobile.UiScale = MobileUiScale;
                    PersistOverride(viewport, mobile, save, nameof(ViewportComponent.ScalingMode), nameof(ViewportComponent.PixelsPerCentimeter), nameof(ViewportComponent.UiScale));
                } else if (components[index] is TextComponent text) {
                    EntitySaveComponent save = FindRequiredGroupFirstSaveComponent(root);
                    TextComponent mobile = (TextComponent)PlatformEditing.EnsureScopeOverrideComponent(text, save, MobileScope);
                    mobile.NativeFontPixels = TextNativePixelMode.Disabled;
                    PersistOverride(text, mobile, save, nameof(TextComponent.NativeFontPixels));
                } else if (components[index] is FPSComponent fps) {
                    EntitySaveComponent save = FindRequiredGroupFirstSaveComponent(root);
                    FPSComponent mobile = (FPSComponent)PlatformEditing.EnsureScopeOverrideComponent(fps, save, MobileScope);
                    mobile.NativeFontPixels = TextNativePixelMode.Disabled;
                    PersistOverride(fps, mobile, save, nameof(FPSComponent.NativeFontPixels));
                    AddMobileComponent(save, new ViewportComponent {
                        ScalingMode = ViewportComponent.DensityScalingMode,
                        UiScale = MobileUiScale
                    }, FpsDensityViewportComponentKey);
                }
            }

            for (int index = 0; index < root.Children.Count; index++) {
                ConfigureDensitySubtree(root.Children[index]);
            }
        }

        /// <summary>
        /// Removes one entity, and with it its subtree, from the mobile presentation.
        /// </summary>
        /// <param name="entity">Generated entity that another entity presents instead on mobile.</param>
        public void Exclude(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            FindRequiredGroupFirstSaveComponent(entity).GetOrCreateExistencePlatformOverride(MobileScope).Exists = false;
        }

        /// <summary>
        /// Pins one widget to the left and bottom edges of the mobile camera viewport, keeping the margins it has
        /// on the reference canvas. Its viewport root must sit at the canvas origin.
        /// </summary>
        /// <param name="widget">Widget whose authored position and rounded-rect or text bounds define the margins.</param>
        public void AnchorToLeftBottomEdges(EditorEntity widget) {
            if (widget == null) {
                throw new ArgumentNullException(nameof(widget));
            }

            int2 size = ReadWidgetSize(widget);
            AddEdgeAnchor(widget,
                (byte)(LayoutComponent.LeftAnchorFlag | LayoutComponent.BottomAnchorFlag),
                new float4(widget.LocalPosition.X, 0f, 0f, ReferenceCanvasHeight - widget.LocalPosition.Y - size.Y));
        }

        /// <summary>
        /// Pins one widget to the right and top edges of the mobile camera viewport, keeping the margins it has
        /// on the reference canvas. Its viewport root must sit at the canvas origin.
        /// </summary>
        /// <param name="widget">Widget whose authored position and rounded-rect or text bounds define the margins.</param>
        public void AnchorToRightTopEdges(EditorEntity widget) {
            if (widget == null) {
                throw new ArgumentNullException(nameof(widget));
            }

            int2 size = ReadWidgetSize(widget);
            AddEdgeAnchor(widget,
                (byte)(LayoutComponent.RightAnchorFlag | LayoutComponent.TopAnchorFlag),
                new float4(0f, ReferenceCanvasWidth - widget.LocalPosition.X - size.X, widget.LocalPosition.Y, 0f));
        }

        /// <summary>
        /// Records the current value of one component property as an explicit mobile override, so the mobile
        /// package binds it independently of any inherited value. The payload is serialized now, so callers pin a
        /// reference only after the referenced entity has its final id.
        /// </summary>
        /// <param name="owner">Entity that owns the component and its editor save metadata.</param>
        /// <param name="component">Common component whose property value is pinned for mobile.</param>
        /// <param name="propertyName">Public property name of the pinned value.</param>
        public void PinProperty(Entity owner, Component component, string propertyName) {
            if (owner == null) {
                throw new ArgumentNullException(nameof(owner));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            } else if (string.IsNullOrWhiteSpace(propertyName)) {
                throw new ArgumentException("Property name must be provided.", nameof(propertyName));
            }

            EntitySaveComponent save = FindRequiredGroupFirstSaveComponent(owner);
            Component mobile = PlatformEditing.EnsureScopeOverrideComponent(component, save, MobileScope);
            PersistOverride(component, mobile, save, propertyName);
        }

        /// <summary>
        /// Pins the light swatch and scene label references of the standard scene UI kit found in one subtree.
        /// </summary>
        /// <param name="entity">Subtree root to search for light toggle and scene label components.</param>
        void PinSceneUiReferences(Entity entity) {
            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is DemoDiscLightToggleComponent lightToggle) {
                    PinProperty(entity, lightToggle, nameof(DemoDiscLightToggleComponent.IndicatorSwatchEntityReference));
                } else if (entity.Components[index] is DemoDiscDebugSceneLabelComponent sceneLabel) {
                    PinProperty(entity, sceneLabel, nameof(DemoDiscDebugSceneLabelComponent.LabelEntityReference));
                }
            }

            for (int index = 0; index < entity.Children.Count; index++) {
                PinSceneUiReferences(entity.Children[index]);
            }
        }

        /// <summary>
        /// Marks the edited properties and serializes the mobile override payload for one common component.
        /// </summary>
        /// <param name="common">Common live component attached to the entity.</param>
        /// <param name="mobile">Detached mobile copy carrying the edited values.</param>
        /// <param name="save">Entity save metadata that stores the override.</param>
        /// <param name="propertyNames">Properties explicitly overridden on mobile.</param>
        void PersistOverride(Component common, Component mobile, EntitySaveComponent save, params string[] propertyNames) {
            for (int index = 0; index < propertyNames.Length; index++) {
                PlatformEditing.MarkScopePropertyOverride(common, save, MobileScope, propertyNames[index]);
            }

            PlatformEditing.PersistScopeOverride(common, mobile, save, MobileScope);
        }

        /// <summary>
        /// Adds the mobile-only edge-anchor layout component to one widget.
        /// </summary>
        /// <param name="widget">Widget that receives the layout component.</param>
        /// <param name="anchorFlags">Edges the widget is pinned to.</param>
        /// <param name="anchorDistances">Left, right, top and bottom margins in reference-canvas units.</param>
        void AddEdgeAnchor(EditorEntity widget, byte anchorFlags, float4 anchorDistances) {
            AddMobileComponent(FindRequiredGroupFirstSaveComponent(widget), new LayoutComponent {
                LayoutSpace = LayoutComponent.CameraViewportLayoutSpace,
                AnchorFlags = anchorFlags,
                AnchorDistances = anchorDistances
            }, EdgeAnchorLayoutComponentKey);
        }

        /// <summary>
        /// Adds or replaces one detached mobile-only component under a stable key.
        /// </summary>
        /// <param name="save">Entity save metadata that stores the component addition.</param>
        /// <param name="component">Detached component instance that exists only on mobile.</param>
        /// <param name="componentKey">Stable key that identifies the addition across regenerations.</param>
        static void AddMobileComponent(EntitySaveComponent save, Component component, string componentKey) {
            save.GetOrCreateComponentPlatformOverride(MobileScope).SetAddedComponent(new EntityPlatformAddedComponentState {
                ComponentKey = componentKey,
                Component = component,
                SaveState = new EntityComponentSaveState { ComponentKey = componentKey }
            });
        }

        /// <summary>
        /// Reads the authored bounds of one widget from its rounded rect or text component.
        /// </summary>
        /// <param name="widget">Widget whose bounds are needed to preserve its far-edge margin.</param>
        /// <returns>Authored widget size in reference-canvas units.</returns>
        static int2 ReadWidgetSize(EditorEntity widget) {
            for (int index = 0; index < widget.Components.Count; index++) {
                if (widget.Components[index] is RoundedRectComponent panel) {
                    return panel.Size;
                } else if (widget.Components[index] is TextComponent text) {
                    return text.Size;
                }
            }

            throw new InvalidOperationException("Edge-anchored mobile widget has no authored bounds: " + widget.Name);
        }

        /// <summary>
        /// Returns whether one entity is an editor entity with the supplied name.
        /// </summary>
        /// <param name="entity">Entity to test; may be null.</param>
        /// <param name="name">Exact editor entity name.</param>
        /// <returns>True when the entity carries the name.</returns>
        static bool IsNamed(Entity entity, string name) {
            return entity is EditorEntity editorEntity && editorEntity.Name == name;
        }

        /// <summary>
        /// Resolves the hidden save component of one generated entity and makes sure a group scope can match on it:
        /// a scope applies only when it is a prefix of the target's path, so an entity that carries a mobile rule needs
        /// the group-first level order. An unset order adopts it; any other recorded order is a generator error.
        /// </summary>
        /// <param name="entity">Generated entity whose save metadata is required.</param>
        /// <returns>The entity's save component, on the group-first level order.</returns>
        static EntitySaveComponent FindRequiredGroupFirstSaveComponent(Entity entity) {
            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is EntitySaveComponent save) {
                    if (save.OverrideLevelOrder == null) {
                        save.OverrideLevelOrder = DemoDiscOverrideScopes.CreateGroupFirstLevelOrder();
                    } else if (!save.OverrideLevelOrder.SequenceEqual(DemoDiscOverrideScopes.GroupFirstLevelOrder)) {
                        throw new InvalidOperationException("Mobile presentation rules need the group-first level order: " + DescribeEntity(entity));
                    }

                    return save;
                }
            }

            throw new InvalidOperationException("Generated entity requires editor save metadata: " + DescribeEntity(entity));
        }

        /// <summary>
        /// Names one entity for an error message.
        /// </summary>
        /// <param name="entity">Entity to describe.</param>
        /// <returns>The editor name, or the runtime type name for a non-editor entity.</returns>
        static string DescribeEntity(Entity entity) {
            return entity is EditorEntity editorEntity ? editorEntity.Name : entity.GetType().Name;
        }
    }
}
