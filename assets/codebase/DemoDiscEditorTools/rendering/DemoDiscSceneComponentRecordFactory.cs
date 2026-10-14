using DemoDisc.menu;
using helengine.editor;
using DemoDisc.rendering;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Creates shared scripted scene component records used across the rendering showcase scenes.
    /// </summary>
    public static class DemoDiscSceneComponentRecordFactory {
        /// <summary>
        /// Stable save-state slot name used for serialized font references.
        /// </summary>
        const string FontReferenceName = "Font";

        /// <summary>
        /// Automatic reflected descriptor used to serialize built-in engine components for authored rendering showcase scenes.
        /// </summary>
        static readonly AutomaticScriptComponentPersistenceDescriptor AutomaticDescriptor =
            new AutomaticScriptComponentPersistenceDescriptor(new ScriptComponentReflectionSchemaBuilder());

        /// <summary>
        /// Creates one scripted component record that returns a demo-disc scene to the main menu.
        /// </summary>
        /// <param name="componentIndex">Entity-local component index.</param>
        /// <returns>Serialized demo-disc return component record.</returns>
        public static SceneComponentAssetRecord CreateReturnToMainMenuRecord(int componentIndex) {
            if (componentIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(componentIndex), "Component index must be non-negative.");
            }
            
            DemoDiscReturnToMenuComponent returnToMainMenuComponent = new DemoDiscReturnToMenuComponent();
            return AutomaticDescriptor.SerializeComponent(returnToMainMenuComponent, componentIndex, null);
        }

        /// <summary>
        /// Creates one serialized FPS overlay component record for a showcase camera.
        /// </summary>
        /// <param name="authoringSession">Authoring session used to resolve the shared scene UI font reference.</param>
        /// <param name="componentIndex">Entity-local component index.</param>
        /// <returns>Serialized FPS overlay component record.</returns>
        public static SceneComponentAssetRecord CreateFpsComponentRecord(IEditorProjectAuthoringSession authoringSession, int componentIndex) {
            if (componentIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(componentIndex), "Component index must be non-negative.");
            }

            FPSComponent fpsComponent = new FPSComponent {
                Font = new FontAsset(new FontInfo("CityRenderingFpsPlaceholder", 16, 4f), null, new Dictionary<char, FontChar>(), 16f, 1, 1),
                NativeFontPixels = DemoDiscSceneUiFont.Mode
            };
            EntityComponentSaveState saveState = new EntityComponentSaveState();
            saveState.SetAssetReference(FontReferenceName, DemoDiscSceneUiFont.CreateReference(authoringSession));
            return AutomaticDescriptor.SerializeComponent(fpsComponent, componentIndex, saveState);
        }

        /// <summary>
        /// Creates the stable generated editor UI-font reference. Demo scenes use <see cref="DemoDiscSceneUiFont"/>; this remains
        /// for the game scenes, which keep their own font presentation.
        /// </summary>
        /// <returns>Stable generated editor UI-font reference.</returns>
        public static SceneAssetReference CreateEditorUiFontReference() {
            return EditorSceneAssetReferenceFactory.CreateEditorUiFont();
        }
    }
}
