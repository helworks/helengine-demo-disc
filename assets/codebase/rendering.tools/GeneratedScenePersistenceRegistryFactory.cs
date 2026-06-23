using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Creates the persistence registry used by generated city scene save and load workflows.
    /// </summary>
    public static class GeneratedScenePersistenceRegistryFactory {
        /// <summary>
        /// Creates one persistence registry that uses reflected component persistence for generated city scenes.
        /// </summary>
        /// <param name="scriptTypeResolver">Optional shared script type resolver used for reflected component persistence.</param>
        /// <returns>Configured scene persistence registry.</returns>
        public static ComponentPersistenceRegistry Create(IScriptTypeResolver scriptTypeResolver = null) {
            return new ComponentPersistenceRegistry(scriptTypeResolver);
        }
    }
}
