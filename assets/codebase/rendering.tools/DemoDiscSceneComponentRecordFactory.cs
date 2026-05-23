using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Creates shared scripted scene component records used across the rendering showcase scenes.
    /// </summary>
    public static class DemoDiscSceneComponentRecordFactory {
        /// <summary>
        /// Stable runtime component type id for the demo-disc return-to-menu behavior.
        /// </summary>
        const string ReturnToMainMenuComponentTypeId = "city.menu.DemoDiscReturnToMenuComponent, gameplay";

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

            using MemoryStream stream = new MemoryStream();
            using EngineBinaryWriter writer = EngineBinaryWriter.Create(stream, EngineBinaryEndianness.LittleEndian);
            writer.WriteByte(AutomaticScriptComponentRuntimeDeserializer.CurrentVersion);
            writer.WriteInt32(1);
            writer.WriteByte(0);

            return new SceneComponentAssetRecord {
                ComponentTypeId = ReturnToMainMenuComponentTypeId,
                ComponentIndex = componentIndex,
                Payload = stream.ToArray()
            };
        }

        /// <summary>
        /// Creates one serialized FPS overlay component record for a showcase camera.
        /// </summary>
        /// <param name="componentIndex">Entity-local component index.</param>
        /// <returns>Serialized FPS overlay component record.</returns>
        public static SceneComponentAssetRecord CreateFpsComponentRecord(int componentIndex) {
            if (componentIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(componentIndex), "Component index must be non-negative.");
            }

            FPSComponent fpsComponent = new FPSComponent {
                Font = new FontAsset(new FontInfo("CityRenderingFpsPlaceholder", 16, 4f), null, new Dictionary<char, FontChar>(), 16f, 1, 1),
                FontScale = 2f
            };
            EntityComponentSaveState saveState = new EntityComponentSaveState();
            saveState.SetAssetReference(FontReferenceName, CreateEditorFontReference());
            return AutomaticDescriptor.SerializeComponent(fpsComponent, componentIndex, saveState);
        }

        /// <summary>
        /// Creates the stable generated asset reference for the editor's built-in UI font.
        /// </summary>
        /// <returns>Stable generated editor-font reference.</returns>
        public static SceneAssetReference CreateEditorFontReference() {
            return new SceneAssetReference {
                SourceKind = SceneAssetReferenceSourceKind.Generated,
                RelativePath = "generated/editor/fonts/ui.hefont",
                ProviderId = "editor",
                AssetId = "ui-font"
            };
        }
    }
}
