using helengine;
using helengine.editor;
using city.scene.tools;

namespace city.rendering.tools {
    /// <summary>Generates compact raw meshes for software tracing without imported source files.</summary>
    public static class SoftwareRayTracingMeshFactory {
        /// <summary>Stable identity-backed sphere asset path.</summary>
        public const string SpherePath = "models/rendering/ray_tracing/sphere.hasset";
        /// <summary>Stable identity-backed procedural teapot asset path.</summary>
        public const string TeapotPath = "models/rendering/ray_tracing/teapot.hasset";
        const int Segments = 32;

        /// <summary>Writes both models through the caller's authoring transaction.</summary>
        public static void WriteAssets(EditorAuthoringTransaction transaction) {
            transaction.WriteAsset(SpherePath, CreateSphere());
            transaction.WriteAsset(TeapotPath, CreateTeapot());
        }

        /// <summary>Creates a closed sphere with nondegenerate pole fans and outward winding.</summary>
        public static ModelAsset CreateSphere() {
            List<float3> vertices = new List<float3>();
            List<ushort> indices = new List<ushort>();
            float2[] profile = new float2[17];
            for (int i = 0; i <= 16; i++) {
                double angle = Math.PI * i / 16;
                profile[i] = new float2(i == 0 || i == 16 ? 0f : 0.5f * (float)Math.Sin(angle), -0.5f * (float)Math.Cos(angle));
            }
            Lathe(vertices, indices, profile);
            return Model(SpherePath, vertices, indices);
        }

        /// <summary>Creates a stylized teapot from turned body/lid profiles and swept spout/handle tubes.</summary>
        public static ModelAsset CreateTeapot() {
            List<float3> vertices = new List<float3>();
            List<ushort> indices = new List<ushort>();
            Lathe(vertices, indices, new[] { new float2(0f, 0f), new float2(0.42f, 0f), new float2(0.62f, 0.12f), new float2(0.8f, 0.3f), new float2(0.88f, 0.55f), new float2(0.82f, 0.8f), new float2(0.66f, 1f), new float2(0.5f, 1.1f), new float2(0f, 1.1f) });
            Lathe(vertices, indices, new[] { new float2(0f, 1.1f), new float2(0.56f, 1.1f), new float2(0.58f, 1.14f), new float2(0.46f, 1.22f), new float2(0.2f, 1.26f), new float2(0.1f, 1.3f), new float2(0.16f, 1.38f), new float2(0.12f, 1.46f), new float2(0f, 1.49f) });
            Sweep(vertices, indices, new[] { new float3(0.62f, 0.42f, 0f), new float3(0.9f, 0.55f, 0f), new float3(1.12f, 0.78f, 0f), new float3(1.3f, 1.04f, 0f), new float3(1.55f, 1.2f, 0f) }, new[] { 0.26f, 0.21f, 0.16f, 0.12f, 0.13f });
            float3[] handle = new float3[17];
            float[] radii = new float[17];
            for (int i = 0; i < handle.Length; i++) {
                double angle = Math.PI / 2 + Math.PI * i / (handle.Length - 1);
                handle[i] = new float3(-0.65f + 0.72f * (float)Math.Cos(angle), 0.61f + 0.45f * (float)Math.Sin(angle), 0f);
                radii[i] = 0.105f;
            }
            Sweep(vertices, indices, handle, radii);
            return Model(TeapotPath, vertices, indices);
        }

        /// <summary>Turns a bottom-to-top radial profile; zero-radius ends become single pole vertices.</summary>
        static void Lathe(List<float3> vertices, List<ushort> indices, float2[] profile) {
            int previousStart = -1;
            bool previousPole = false;
            for (int ring = 0; ring < profile.Length; ring++) {
                bool pole = profile[ring].X == 0f;
                int start = vertices.Count;
                for (int segment = 0; segment < (pole ? 1 : Segments); segment++) {
                    double angle = segment * Math.PI * 2 / Segments;
                    vertices.Add(new float3(profile[ring].X * (float)Math.Cos(angle), profile[ring].Y, profile[ring].X * (float)Math.Sin(angle)));
                }
                if (ring > 0) {
                    for (int j = 0; j < Segments; j++) {
                        int next = (j + 1) % Segments;
                        if (previousPole) Triangle(indices, previousStart, start + j, start + next);
                        else if (pole) Triangle(indices, previousStart + j, start, previousStart + next);
                        else {
                            Triangle(indices, previousStart + j, start + j, start + next);
                            Triangle(indices, previousStart + j, start + next, previousStart + next);
                        }
                    }
                }
                previousStart = start;
                previousPole = pole;
            }
        }

        /// <summary>Sweeps capped circular rings along a planar centerline with consistently outward winding.</summary>
        static void Sweep(List<float3> vertices, List<ushort> indices, float3[] centers, float[] radii) {
            int start = vertices.Count;
            const int sides = 12;
            for (int i = 0; i < centers.Length; i++) {
                float3 tangent = float3.Normalize(centers[Math.Min(i + 1, centers.Length - 1)] - centers[Math.Max(0, i - 1)]);
                float3 normal = new float3(-tangent.Y, tangent.X, 0f);
                for (int j = 0; j < sides; j++) {
                    double angle = j * Math.PI * 2 / sides;
                    vertices.Add(centers[i] + normal * (radii[i] * (float)Math.Cos(angle)) + new float3(0f, 0f, radii[i] * (float)Math.Sin(angle)));
                    if (i > 0) {
                        int a = start + (i - 1) * sides + j;
                        int b = start + (i - 1) * sides + (j + 1) % sides;
                        int c = start + i * sides + j;
                        int d = start + i * sides + (j + 1) % sides;
                        Triangle(indices, a, b, d);
                        Triangle(indices, a, d, c);
                    }
                }
            }
            int bottom = vertices.Count;
            vertices.Add(centers[0]);
            int top = vertices.Count;
            vertices.Add(centers[centers.Length - 1]);
            int last = start + (centers.Length - 1) * sides;
            for (int j = 0; j < sides; j++) {
                int next = (j + 1) % sides;
                Triangle(indices, bottom, start + next, start + j);
                Triangle(indices, top, last + j, last + next);
            }
        }

        /// <summary>Adds one checked 16-bit triangle.</summary>
        static void Triangle(List<ushort> indices, int a, int b, int c) {
            indices.Add(checked((ushort)a)); indices.Add(checked((ushort)b)); indices.Add(checked((ushort)c));
        }

        /// <summary>Creates an identity-bearing raw asset with a single material slot.</summary>
        static ModelAsset Model(string path, List<float3> vertices, List<ushort> indices) {
            return new ModelAsset {
                AuthoringAssetId = ProjectAuthoringAssetIdentityCatalog.GetNativeAssetIdentity(path),
                FormerAuthoringAssetIds = Array.Empty<string>(),
                Positions = vertices.ToArray(), Indices16 = indices.ToArray(),
                Submeshes = new[] { new ModelSubmeshAsset { MaterialSlotName = "Surface", IndexStart = 0, IndexCount = indices.Count } }
            };
        }
    }
}
