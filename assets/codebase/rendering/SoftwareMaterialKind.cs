namespace city.rendering {
    /// <summary>Selects the scattering law used by a software-traced surface.</summary>
    public enum SoftwareMaterialKind {
        /// <summary>Cosine-weighted diffuse scattering, preserving existing authored scenes.</summary>
        Diffuse = 0,
        /// <summary>Ideal specular reflection tinted by the material's reflection color.</summary>
        Mirror = 1,
        /// <summary>A smooth, clear dielectric boundary between air and a closed glass object.</summary>
        Glass = 2
    }
}
