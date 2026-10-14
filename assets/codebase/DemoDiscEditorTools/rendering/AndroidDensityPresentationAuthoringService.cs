using helengine.editor;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Authors the Android landscape presentation on live generated entities: density-scaled viewports, scalable
    /// glyphs and physical edge anchors. Android sits in no platform group, so these rules use the <c>android</c>
    /// platform scope; every other platform keeps the common 1280 by 720 reference-canvas presentation.
    /// Generators call it before the entities are saved, so a plain regeneration writes the complete scene.
    /// </summary>
    public sealed class AndroidDensityPresentationAuthoringService {
        /// <summary>
        /// Platform id of the Android target in the project's platform registry.
        /// </summary>
        public const string AndroidPlatformId = "android";

        /// <summary>
        /// Approved Android overlay multiplier, producing the 650 by 348 instruction panel at 420 DPI.
        /// </summary>
        public const float AndroidUiScale = 0.5625f;

        /// <summary>
        /// Baseline Android density (160 DPI) expressed in pixels per centimeter.
        /// </summary>
        public const float AndroidPixelsPerCentimeter = (float)(160d / 2.54d);

        /// <summary>
        /// Stable component key of the Android-only density viewport added beside each FPS counter.
        /// </summary>
        public const string FpsDensityViewportComponentKey = "c0de0000000000000000000000000001";

        /// <summary>
        /// Stable component key of the Android-only layout component that pins a widget to its screen edges.
        /// </summary>
        public const string EdgeAnchorLayoutComponentKey = "c0de0000000000000000000000000002";

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
        /// Gets the one-step Android platform scope every rule of this service is authored at.
        /// </summary>
        public static EditorOverrideScope AndroidScope => EditorOverrideScope.ForPlatform(AndroidPlatformId);

        /// <summary>
        /// Switches every viewport, text and FPS counter in one generated subtree to density scaling on Android.
        /// Viewports scale by physical density with <see cref="AndroidUiScale"/>; text and FPS glyphs stop snapping to
        /// native font pixels so they scale with their viewport; each FPS counter gains an Android-only density viewport.
        /// </summary>
        /// <param name="root">Generated subtree root whose overlay components should be configured.</param>
        public void ConfigureDensitySubtree(Entity root) {
            if (root == null) {
                throw new ArgumentNullException(nameof(root));
            }

            EntitySaveComponent save = FindRequiredSaveComponent(root);
            Component[] components = root.Components.ToArray();
            for (int index = 0; index < components.Length; index++) {
                if (components[index] is ViewportComponent viewport) {
                    ViewportComponent android = (ViewportComponent)PlatformEditing.EnsureScopeOverrideComponent(viewport, save, AndroidScope);
                    android.ScalingMode = ViewportComponent.DensityScalingMode;
                    android.PixelsPerCentimeter = AndroidPixelsPerCentimeter;
                    android.UiScale = AndroidUiScale;
                    PersistOverride(viewport, android, save, nameof(ViewportComponent.ScalingMode), nameof(ViewportComponent.PixelsPerCentimeter), nameof(ViewportComponent.UiScale));
                } else if (components[index] is TextComponent text) {
                    TextComponent android = (TextComponent)PlatformEditing.EnsureScopeOverrideComponent(text, save, AndroidScope);
                    android.NativeFontPixels = TextNativePixelMode.Disabled;
                    PersistOverride(text, android, save, nameof(TextComponent.NativeFontPixels));
                } else if (components[index] is FPSComponent fps) {
                    FPSComponent android = (FPSComponent)PlatformEditing.EnsureScopeOverrideComponent(fps, save, AndroidScope);
                    android.NativeFontPixels = TextNativePixelMode.Disabled;
                    PersistOverride(fps, android, save, nameof(FPSComponent.NativeFontPixels));
                    AddAndroidComponent(save, new ViewportComponent {
                        ScalingMode = ViewportComponent.DensityScalingMode,
                        UiScale = AndroidUiScale
                    }, FpsDensityViewportComponentKey);
                }
            }

            for (int index = 0; index < root.Children.Count; index++) {
                ConfigureDensitySubtree(root.Children[index]);
            }
        }

        /// <summary>
        /// Pins one widget to the left and bottom edges of the Android camera viewport, keeping the margins it has
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
        /// Pins one widget to the right and top edges of the Android camera viewport, keeping the margins it has
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
        /// Records the current value of one component property as an explicit Android override, so the Android
        /// package binds it independently of any inherited value.
        /// </summary>
        /// <param name="owner">Entity that owns the component and its editor save metadata.</param>
        /// <param name="component">Common component whose property value is pinned for Android.</param>
        /// <param name="propertyName">Public property name of the pinned value.</param>
        public void PinPropertyForAndroid(Entity owner, Component component, string propertyName) {
            if (owner == null) {
                throw new ArgumentNullException(nameof(owner));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            } else if (string.IsNullOrWhiteSpace(propertyName)) {
                throw new ArgumentException("Property name must be provided.", nameof(propertyName));
            }

            EntitySaveComponent save = FindRequiredSaveComponent(owner);
            Component android = PlatformEditing.EnsureScopeOverrideComponent(component, save, AndroidScope);
            PersistOverride(component, android, save, propertyName);
        }

        /// <summary>
        /// Marks the edited properties and serializes the Android override payload for one common component.
        /// </summary>
        /// <param name="common">Common live component attached to the entity.</param>
        /// <param name="android">Detached Android copy carrying the edited values.</param>
        /// <param name="save">Entity save metadata that stores the override.</param>
        /// <param name="propertyNames">Properties explicitly overridden on Android.</param>
        void PersistOverride(Component common, Component android, EntitySaveComponent save, params string[] propertyNames) {
            for (int index = 0; index < propertyNames.Length; index++) {
                PlatformEditing.MarkScopePropertyOverride(common, save, AndroidScope, propertyNames[index]);
            }

            PlatformEditing.PersistScopeOverride(common, android, save, AndroidScope);
        }

        /// <summary>
        /// Adds the Android-only edge-anchor layout component to one widget.
        /// </summary>
        /// <param name="widget">Widget that receives the layout component.</param>
        /// <param name="anchorFlags">Edges the widget is pinned to.</param>
        /// <param name="anchorDistances">Left, right, top and bottom margins in reference-canvas units.</param>
        void AddEdgeAnchor(EditorEntity widget, byte anchorFlags, float4 anchorDistances) {
            AddAndroidComponent(FindRequiredSaveComponent(widget), new LayoutComponent {
                LayoutSpace = LayoutComponent.CameraViewportLayoutSpace,
                AnchorFlags = anchorFlags,
                AnchorDistances = anchorDistances
            }, EdgeAnchorLayoutComponentKey);
        }

        /// <summary>
        /// Adds or replaces one detached Android-only component under a stable key.
        /// </summary>
        /// <param name="save">Entity save metadata that stores the component addition.</param>
        /// <param name="component">Detached component instance that exists only on Android.</param>
        /// <param name="componentKey">Stable key that identifies the addition across regenerations.</param>
        static void AddAndroidComponent(EntitySaveComponent save, Component component, string componentKey) {
            save.GetOrCreateComponentPlatformOverride(AndroidScope).SetAddedComponent(new EntityPlatformAddedComponentState {
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

            throw new InvalidOperationException("Edge-anchored Android widget has no authored bounds: " + widget.Name);
        }

        /// <summary>
        /// Resolves the hidden save component attached to every generated editor entity.
        /// </summary>
        /// <param name="entity">Generated entity whose save metadata is required.</param>
        /// <returns>The entity's save component.</returns>
        static EntitySaveComponent FindRequiredSaveComponent(Entity entity) {
            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is EntitySaveComponent save) {
                    return save;
                }
            }

            throw new InvalidOperationException("Generated entity requires editor save metadata: " + (entity is EditorEntity editorEntity ? editorEntity.Name : entity.GetType().Name));
        }
    }
}
