using helengine;

namespace city.rendering {
    /// <summary>
    /// Stores one authored software-path-tracer material description.
    /// </summary>
    public sealed class SoftwareMaterial {
        /// <summary>
        /// Gets or sets the diffuse color used by the software material.
        /// </summary>
        public float3 DiffuseColor { get; set; } = float3.One;

        /// <summary>
        /// Gets or sets the emissive color used by the software material.
        /// </summary>
        public float3 EmissionColor { get; set; } = float3.Zero;

        /// <summary>
        /// Gets or sets the scalar strength applied to the emissive color.
        /// </summary>
        public float EmissionStrength { get; set; }
    }

    /// <summary>
    /// Stores one authored software-path-tracer model reference and its material descriptions.
    /// </summary>
    public sealed class SoftwareModelComponent : Component {
        /// <summary>
        /// Gets or sets the authored model reference used by the software path tracer.
        /// </summary>
        [CpuReadableModelReference]
        public SceneAssetReference ModelReference { get; set; }

        /// <summary>
        /// Gets or sets the authored materials used by the software path tracer.
        /// </summary>
        public SoftwareMaterial[] Materials { get; set; } = Array.Empty<SoftwareMaterial>();
    }
}
