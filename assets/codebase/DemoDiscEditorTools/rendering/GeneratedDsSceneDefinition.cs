using DemoDisc.rendering;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Stores the dual-screen presentation of one generated scene. It is merged into the canonical authored
    /// scene and restricted to the dual-screen platform group, so the shared content is authored once and only
    /// the presentation varies by group.
    /// </summary>
    public sealed class GeneratedDsSceneDefinition {
        /// <summary>
        /// Gets or sets complete dual-screen roots authored by the generator. When present the shared
        /// bottom-screen scaffold is bypassed entirely and these roots are used as-is.
        /// </summary>
        public Entity[] RootEntities { get; set; }

        /// <summary>
        /// Gets or sets the bottom-screen content mounted beneath the shared bottom-screen viewport, above the
        /// chrome Blueprint the scaffold instances.
        /// </summary>
        public Entity[] BottomScreenRootEntities { get; set; }
    }
}
