using helengine;
using helengine.editor;
using city.rendering;

namespace city.rendering.tools {
    /// <summary>Authors three distinct compositions for the shared software tracer.</summary>
    public sealed class SoftwareRayTracingShowcaseFactory {
        /// <summary>Stable path for the procedural teapot composition.</summary>
        public const string TeapotSceneId = "scenes/rendering/ray_tracing_teapot.helen";
        /// <summary>Stable path for the twelve-material sphere gallery.</summary>
        public const string SpheresSceneId = "scenes/rendering/ray_tracing_spheres.helen";
        /// <summary>Stable path for the area-light shadow study.</summary>
        public const string ShadowsSceneId = "scenes/rendering/ray_tracing_soft_shadows.helen";
        readonly IEditorProjectAuthoringSession authoring;

        /// <summary>Uses the existing editor session to create persistable scene entities.</summary>
        public SoftwareRayTracingShowcaseFactory(IEditorProjectAuthoringSession authoring) {
            this.authoring = authoring ?? throw new ArgumentNullException(nameof(authoring));
        }

        /// <summary>Adds raw-model instances, materials, and a single area emitter beneath the shared tracer.</summary>
        public void Populate(Entity controller, string sceneId, SceneAssetReference sphere, SceneAssetReference teapot) {
            SoftwarePathTracerComponent tracer = controller.Components.OfType<SoftwarePathTracerComponent>().Single();
            tracer.TraceCameraOrigin = new float3(0f, 6f, 7.5f);
            float3 target = new float3(0f, 0.35f, 0f);
            if (sceneId == TeapotSceneId) {
                tracer.TraceCameraOrigin = new float3(0f, 3.2f, 7f);
                target = new float3(0f, 0.65f, 0f);
            }
            tracer.TraceCameraForward = float3.Normalize(target - tracer.TraceCameraOrigin);
            tracer.TraceCameraRight = float3.Normalize(float3.Cross(tracer.TraceCameraForward, new float3(0f, 1f, 0f)));
            tracer.TraceCameraUp = float3.Cross(tracer.TraceCameraRight, tracer.TraceCameraForward);
            tracer.VerticalFieldOfViewDegrees = 50f;
            SceneAssetReference cube = EngineSceneAssetReferenceFactory.CreateCubeModel();
            Add(controller, "Floor", cube, new float3(0f, -0.08f, 0f), new float3(8f, 0.1f, 7f), new float3(0.65f, 0.65f, 0.65f));
            if (sceneId != ShadowsSceneId) {
                for (int z = 0; z < 6; z++) {
                    for (int x = 0; x < 8; x++) {
                        float shade = (x + z) % 2 == 0 ? 0.65f : 0.22f;
                        Add(controller, "FloorTile" + x + "_" + z, cube, new float3(x - 3.5f, -0.02f, z - 2.5f), new float3(1f, 0.02f, 1f), new float3(shade, shade, shade));
                    }
                }
            }
            Add(controller, "Backdrop", cube, new float3(0f, 1.7f, -3.5f), new float3(8f, 3.5f, 0.1f), new float3(0.5f, 0.6f, 0.75f));
            Entity emitter = Add(controller, "ShowcaseAreaLight", cube, new float3(-0.8f, sceneId == TeapotSceneId ? 4.3f : 5.8f, 0.3f), new float3(3.5f, 0.03f, 2.5f), float3.Zero);
            SoftwareMaterial light = emitter.Components.OfType<SoftwareModelComponent>().Single().Materials[0];
            light.EmissionColor = float3.One;
            light.EmissionStrength = sceneId == TeapotSceneId ? 10f : 16f;
            if (sceneId == SpheresSceneId) {
                float3[] diffuse = { new float3(0.75f, 0.08f, 0.05f), new float3(0.08f, 0.55f, 0.15f), new float3(0.08f, 0.2f, 0.8f), new float3(0.8f, 0.7f, 0.15f) };
                float3[] mirrors = { float3.One, new float3(1f, 0.75f, 0.3f), new float3(0.95f, 0.5f, 0.3f), new float3(0.55f, 0.75f, 1f) };
                float[] indices = { 1.1f, 1.33f, 1.5f, 1.8f };
                for (int row = 0; row < 3; row++) {
                    for (int column = 0; column < 4; column++) {
                        Entity entity = Add(controller, "MaterialSphere" + row + "_" + column, sphere, new float3((column - 1.5f) * 1.45f, 0.54f, 1.65f - row * 1.65f), new float3(1.1f, 1.1f, 1.1f), diffuse[column]);
                        SetScattering(entity, (SoftwareMaterialKind)row, mirrors[column], indices[column]);
                    }
                }
            } else if (sceneId == TeapotSceneId) {
                Add(controller, "RedTeapot", teapot, new float3(-1.65f, 0f, 0f), new float3(0.9f, 0.9f, 0.9f), new float3(0.7f, 0.06f, 0.04f));
                Entity right = Add(controller, "SilverTeapot", teapot, new float3(1.65f, 0f, 0f), new float3(0.9f, 0.9f, 0.9f), float3.Zero);
                SetScattering(right, SoftwareMaterialKind.Mirror, new float3(0.94f, 0.96f, 1f), 1.5f);
            } else if (sceneId == ShadowsSceneId) {
                for (int i = 0; i < 4; i++) {
                    Add(controller, "ShadowSphere" + i, sphere, new float3((i - 1.5f) * 1.45f, 0.44f + i * 0.45f, 0f), new float3(0.9f, 0.9f, 0.9f), new float3(0.15f, 0.4f, 0.75f));
                }
                Add(controller, "ShadowPillar", cube, new float3(-1.4f, 0.8f, -1.8f), new float3(0.5f, 1.6f, 0.5f), new float3(0.7f, 0.18f, 0.07f));
            } else {
                throw new ArgumentException("Unknown showcase scene.", nameof(sceneId));
            }
        }

        /// <summary>Creates one software-only model instance with an ordinary diffuse material.</summary>
        Entity Add(Entity parent, string name, SceneAssetReference model, float3 position, float3 scale, float3 color) {
            Entity entity = authoring.OwningCore.EntityFactory.CreateChild(parent, name);
            entity.LocalPosition = position;
            entity.LocalScale = scale;
            entity.LocalOrientation = float4.Identity;
            entity.AddComponent(new SoftwareModelComponent { ModelReference = model, Materials = new[] { new SoftwareMaterial { DiffuseColor = color } } });
            return entity;
        }

        /// <summary>Overrides scattering without changing the existing diffuse/emission serialization contract.</summary>
        static void SetScattering(Entity entity, SoftwareMaterialKind kind, float3 tint, float ior) {
            entity.Components.OfType<SoftwareModelComponent>().Single().Scattering = new[] { new SoftwareScatteringMaterial { Kind = kind, ReflectionColor = tint, IndexOfRefraction = ior } };
        }
    }
}
