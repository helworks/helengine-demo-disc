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
    }
}
