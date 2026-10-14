using helengine.editor;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Describes the single font shared by every piece of text in the demo-disc scenes (FPS counters, labels, instructions,
    /// light indicators and chrome labels). The font is cooked per platform at a pixel size that equals a
    /// <see cref="ReferencePixelSize"/> pixel font on the 1280x720 reference canvas, so scenes draw it at native pixel size
    /// with a font scale of one instead of carrying per-platform scale overrides.
    /// </summary>
    public static class DemoDiscSceneUiFont {
        /// <summary>
        /// Project-relative path of the shared scene UI font asset.
        /// </summary>
        public const string RelativePath = "Fonts/DemoDiscSceneUi.ttf";

        /// <summary>
        /// Native-pixel rounding mode applied to every scene text component. Setting any mode other than disabled pins the
        /// component font scale to one, so scenes must not assign a font scale. Rounding up keeps glyphs on whole pixels
        /// without ever shrinking below the cooked size.
        /// </summary>
        public const TextNativePixelMode Mode = TextNativePixelMode.RoundUp;

        /// <summary>
        /// Font pixel size on the 1280x720 reference canvas that every platform cook of the font matches. The cooked table is
        /// round(ReferencePixelSize * framebufferHeight / 720) per platform and is produced by
        /// C:\dev\helworks\builds\builder-coordination\menu-20261007\scene-ui-font\create-scene-ui-font.ps1.
        /// </summary>
        public const int ReferencePixelSize = 40;

        /// <summary>
        /// Height in reference-canvas pixels of one text row: the typeface line height is roughly 1.3 times the pixel size.
        /// </summary>
        public const int ReferenceRowHeight = 52;

        /// <summary>
        /// Distance in reference-canvas pixels from a label's box top to the middle of its capital letters. Measured with the
        /// cook rasterizer: the cap middle sits about 0.6 of the pixel size below the line top (15.5 of 25 pixels on the PS2,
        /// 24 of 40 at 720p, 9 of 13 at 240p), which on each framebuffer is 24 to 27 reference pixels. Centering a label on an
        /// icon places the box top this far above the icon's middle.
        /// </summary>
        public const int ReferenceCapCenter = 25;

        /// <summary>
        /// Creates the file-backed scene reference to the shared scene UI font.
        /// </summary>
        /// <param name="authoringSession">Authoring session that resolves project-relative file references.</param>
        /// <returns>File-backed font reference for the scene UI font.</returns>
        public static SceneAssetReference CreateReference(IEditorProjectAuthoringSession authoringSession) {
            if (authoringSession == null) {
                throw new ArgumentNullException(nameof(authoringSession));
            }

            return authoringSession.CreateFileReference(RelativePath, AssetEntryKind.Font);
        }

        /// <summary>
        /// Stores the shared scene UI font reference in the editor save state of the entity that owns a text-like component.
        /// </summary>
        /// <param name="authoringSession">Authoring session that resolves project-relative file references.</param>
        /// <param name="entity">Entity that owns the component and its editor save component.</param>
        /// <param name="component">Text or FPS component whose Font reference is being stored.</param>
        public static void StoreReference(IEditorProjectAuthoringSession authoringSession, Entity entity, Component component) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (component == null) {
                throw new ArgumentNullException(nameof(component));
            } else if (entity.Components == null) {
                throw new InvalidOperationException("Generated entities must expose initialized component collections.");
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is EntitySaveComponent saveComponent) {
                    saveComponent.SetAssetReference(component, "Font", CreateReference(authoringSession));
                    return;
                }
            }

            throw new InvalidOperationException("Generated entity is missing required save state.");
        }
    }
}
