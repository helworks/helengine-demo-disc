namespace city.menu {
    /// <summary>
    /// Produces one city-owned menu definition that can be materialized by the city demo-disc runtime menu host.
    /// </summary>
    public interface IMenuDefinitionProvider {
        /// <summary>
        /// Builds the menu definition consumed by the runtime menu host.
        /// </summary>
        /// <returns>Menu definition describing panels, items, and theme assets.</returns>
        MenuDefinition CreateMenuDefinition();
    }
}
