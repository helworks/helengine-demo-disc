namespace city.menu {
    /// <summary>
    /// Binds the main-menu platform labels to the runtime platform metadata exposed by the engine core.
    /// </summary>
    public sealed class PlatformInfoTextComponent : Component {
        /// <summary>
        /// Gets or sets the text component that renders the stable platform name.
        /// </summary>
        public SceneComponentReference NameTextReference { get; set; }

        /// <summary>
        /// Gets or sets the text component that renders the runtime build version.
        /// </summary>
        public SceneComponentReference VersionTextReference { get; set; }

        /// <summary>
        /// Resolves the bound text components once the component is attached and writes the current platform metadata into them.
        /// </summary>
        /// <param name="entity">Owning entity receiving the component.</param>
        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);

            Core core = Core.Instance;
            if (core == null) {
                throw new InvalidOperationException("A core instance must exist before platform info labels can be initialized.");
            } else if (core.SceneComponentReferenceResolver == null) {
                throw new InvalidOperationException("Scene component references cannot be resolved before the core resolver is initialized.");
            } else if (core.PlatformInfo == null) {
                throw new InvalidOperationException("Platform info must be initialized before the platform labels can be authored.");
            }

            TextComponent nameTextComponent = ResolveTextComponent(NameTextReference);
            TextComponent versionTextComponent = ResolveTextComponent(VersionTextReference);
            nameTextComponent.Text = core.PlatformInfo.Name;
            versionTextComponent.Text = core.PlatformInfo.Version;
        }

        /// <summary>
        /// Resolves one saved component reference to a text component and validates the expected runtime type.
        /// </summary>
        /// <param name="reference">Serialized component reference to resolve.</param>
        /// <returns>Resolved text component.</returns>
        TextComponent ResolveTextComponent(SceneComponentReference reference) {
            Component component = Core.Instance.SceneComponentReferenceResolver.Resolve(reference);
            if (component is not TextComponent textComponent) {
                throw new InvalidOperationException("Platform info labels must resolve to text components.");
            }

            return textComponent;
        }
    }
}
