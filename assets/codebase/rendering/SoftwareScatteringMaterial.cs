using helengine;

namespace city.rendering {
    /// <summary>Defines optional scattering parameters for one matching entry in a model's existing material array.</summary>
    public sealed class SoftwareScatteringMaterial {
        /// <summary>Gets or sets the scattering law; diffuse preserves the original material behavior.</summary>
        public SoftwareMaterialKind Kind { get; set; } = SoftwareMaterialKind.Diffuse;

        /// <summary>Gets or sets ideal mirror reflectance per channel, from zero to one.</summary>
        public float3 ReflectionColor { get; set; } = float3.One;

        /// <summary>Gets or sets the clear glass refractive index relative to air.</summary>
        public float IndexOfRefraction { get; set; } = 1.5f;
    }
}
