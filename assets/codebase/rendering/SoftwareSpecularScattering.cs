using helengine;

namespace city.rendering {
    /// <summary>Provides allocation-free ideal reflection and clear dielectric scattering for CPU tracing.</summary>
    public static class SoftwareSpecularScattering {
        /// <summary>Reflects a unit incoming ray around a unit surface normal.</summary>
        /// <param name="incoming">Unit direction toward the surface.</param>
        /// <param name="normal">Unit geometric surface normal; either orientation is accepted.</param>
        /// <returns>The unit reflected direction.</returns>
        public static float3 Reflect(float3 incoming, float3 normal) {
            return incoming - normal * (2f * float3.Dot(incoming, normal));
        }

        /// <summary>
        /// Samples a smooth air/glass interface using exact unpolarized Fresnel probabilities and Snell's law.
        /// Outward normals select entering versus exiting; glass objects must be closed and consistently wound.
        /// Separate or nested media with different surrounding indices are not represented by this material.
        /// </summary>
        /// <param name="incoming">Validated unit direction toward the boundary.</param>
        /// <param name="outwardNormal">Validated unit geometric normal pointing outside the glass.</param>
        /// <param name="indexOfRefraction">Finite glass index relative to air, at least one.</param>
        /// <param name="sample">Deterministic Fresnel branch sample in [0, 1).</param>
        /// <param name="outgoing">Receives the reflected or transmitted unit direction.</param>
        /// <param name="radianceWeight">Receives the eta-squared radiance factor, with branch probability canceled.</param>
        /// <returns>True for transmission, false for reflection or total internal reflection.</returns>
        public static bool SampleDielectric(float3 incoming, float3 outwardNormal, float indexOfRefraction, float sample, out float3 outgoing, out float radianceWeight) {
            if (!float.IsFinite(indexOfRefraction) || indexOfRefraction < 1f) {
                throw new ArgumentOutOfRangeException(nameof(indexOfRefraction));
            }
            if (!float.IsFinite(sample) || sample < 0f || sample >= 1f) {
                throw new ArgumentOutOfRangeException(nameof(sample));
            }
            if (indexOfRefraction == 1f) {
                outgoing = incoming;
                radianceWeight = 1f;
                return true;
            }

            float signedCosine = float3.Dot(incoming, outwardNormal);
            bool entering = signedCosine < 0f;
            float3 normal = entering ? outwardNormal : -outwardNormal;
            float incidentIndex = entering ? 1f : indexOfRefraction;
            float transmittedIndex = entering ? indexOfRefraction : 1f;
            float eta = incidentIndex / transmittedIndex;
            float cosIncident = Math.Clamp(-float3.Dot(incoming, normal), 0f, 1f);
            double sinTransmittedSquared = (double)eta * eta * Math.Max(0f, 1f - cosIncident * cosIncident);
            if (sinTransmittedSquared >= 1f) {
                outgoing = Reflect(incoming, normal);
                radianceWeight = 1f;
                return false;
            }

            float cosTransmitted = (float)Math.Sqrt(Math.Max(0f, 1f - sinTransmittedSquared));
            float perpendicular = (incidentIndex * cosIncident - transmittedIndex * cosTransmitted) / (incidentIndex * cosIncident + transmittedIndex * cosTransmitted);
            float parallel = (transmittedIndex * cosIncident - incidentIndex * cosTransmitted) / (transmittedIndex * cosIncident + incidentIndex * cosTransmitted);
            float fresnel = Math.Clamp(0.5f * (perpendicular * perpendicular + parallel * parallel), 0f, 1f);
            if (sample < fresnel) {
                outgoing = Reflect(incoming, normal);
                radianceWeight = 1f;
                return false;
            }

            outgoing = float3.Normalize(incoming * eta + normal * (eta * cosIncident - cosTransmitted));
            radianceWeight = eta * eta;
            return true;
        }

        /// <summary>Rejects unsupported scattering modes and nonphysical specular parameters before tracing.</summary>
        /// <param name="material">Compact authored material to validate.</param>
        public static void ValidateMaterial(SoftwareMaterialData material) {
            if (material.Kind != SoftwareMaterialKind.Diffuse && material.Kind != SoftwareMaterialKind.Mirror && material.Kind != SoftwareMaterialKind.Glass) {
                throw new ArgumentOutOfRangeException(nameof(material), "Unsupported software material kind.");
            }
            if (!float.IsFinite(material.IndexOfRefraction) || material.IndexOfRefraction < 1f) {
                throw new ArgumentOutOfRangeException(nameof(material), "Refractive index must be finite and at least one.");
            }
            float3 color = material.ReflectionColor;
            if (!float.IsFinite(color.X) || !float.IsFinite(color.Y) || !float.IsFinite(color.Z) || color.X < 0f || color.Y < 0f || color.Z < 0f || color.X > 1f || color.Y > 1f || color.Z > 1f) {
                throw new ArgumentOutOfRangeException(nameof(material), "Reflection color channels must be finite and between zero and one.");
            }
        }
    }
}
