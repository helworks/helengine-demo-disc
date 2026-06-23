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
        /// Stable project-relative body-font path used by the demo-disc showcase overlays and labels.
        /// </summary>
        const string BodyFontRelativePath = "Fonts/DemoDiscBody.ttf";

        /// <summary>
        /// Generated provider id reserved for the editor-authored UI font asset.
        /// </summary>
        const string EditorGeneratedProviderId = "editor";

        /// <summary>
        /// Stable asset id used for the generated editor UI font asset.
        /// </summary>
        const string EditorFontAssetId = "ui-font";

        /// <summary>
        /// Stable relative path used for the generated editor UI font asset.
        /// </summary>
        const string EditorFontRelativePath = "generated/editor/fonts/ui.hefont";

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
            saveState.SetAssetReference(FontReferenceName, CreateEditorUiFontReference());
            return AutomaticDescriptor.SerializeComponent(fpsComponent, componentIndex, saveState);
        }

        /// <summary>
        /// Creates the stable file-backed demo-disc body-font reference used by non-FPS showcase overlays and labels.
        /// </summary>
        /// <returns>Stable file-backed demo-disc body-font reference.</returns>
        public static SceneAssetReference CreateEditorFontReference() {
            return new SceneAssetReference {
                SourceKind = SceneAssetReferenceSourceKind.FileSystem,
                RelativePath = BodyFontRelativePath
            };
        }

        /// <summary>
        /// Creates the stable generated editor UI-font reference used by showcase FPS overlays.
        /// </summary>
        /// <returns>Stable generated editor UI-font reference.</returns>
        public static SceneAssetReference CreateEditorUiFontReference() {
            return new SceneAssetReference {
                SourceKind = SceneAssetReferenceSourceKind.Generated,
                RelativePath = EditorFontRelativePath,
                ProviderId = EditorGeneratedProviderId,
                AssetId = EditorFontAssetId
            };
        }
    }
}
