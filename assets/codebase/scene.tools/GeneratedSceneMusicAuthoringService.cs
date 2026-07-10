using helengine.editor;

namespace city.scene.tools {
    /// <summary>
    /// Authors the shared looping background-music root used by generated showcase scenes.
    /// </summary>
    public sealed class GeneratedSceneMusicAuthoringService {
        /// <summary>
        /// Stable project-relative music asset path used by the rendering and physics showcase scenes.
        /// </summary>
        public const string RenderingAndPhysicsMusicAudioPath = "audio/scenes/helen_of_code_compling_v2.wav";

        /// <summary>
        /// Stable authored gain used by the shared showcase background-music source.
        /// </summary>
        public const float RenderingAndPhysicsMusicGain = 1f;

        /// <summary>
        /// Creates the shared looping showcase-music root as one live editor entity with a file-backed audio reference.
        /// </summary>
        /// <returns>Live editor-authored music root.</returns>
        public EditorEntity CreateRenderingAndPhysicsMusicEntity() {
            Entity entity = Core.Instance.EntityFactory.Create("SceneMusic");
            AudioSourceComponent audioSource = new AudioSourceComponent {
                Clip = new AudioAsset(),
                PlayOnStart = true,
                Loop = true,
                BusId = "music",
                Gain = RenderingAndPhysicsMusicGain
            };
            entity.AddComponent(audioSource);
            ApplyAudioReference(entity, audioSource, RenderingAndPhysicsMusicAudioPath);
            if (entity is EditorEntity editorEntity) {
                return editorEntity;
            }

            throw new InvalidOperationException("Generated scene music must be authored through editor entities.");
        }

        /// <summary>
        /// Stores the supplied file-backed audio reference on the entity save state for the given audio source component.
        /// </summary>
        /// <param name="entity">Entity that owns the audio source component.</param>
        /// <param name="audioSourceComponent">Audio source component whose clip reference should be stored.</param>
        /// <param name="audioPath">Project-relative audio asset path.</param>
        void ApplyAudioReference(Entity entity, AudioSourceComponent audioSourceComponent, string audioPath) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (audioSourceComponent == null) {
                throw new ArgumentNullException(nameof(audioSourceComponent));
            } else if (string.IsNullOrWhiteSpace(audioPath)) {
                throw new ArgumentException("Audio path must be provided.", nameof(audioPath));
            }

            EntitySaveComponent saveComponent = FindRequiredEntitySaveComponent(entity);
            saveComponent.SetAssetReference(
                audioSourceComponent,
                AutomaticComponentAssetReferenceSupport.BuildReferenceName(nameof(AudioSourceComponent.Clip)),
                global::helengine.SceneAssetReferenceFactory.CreateFileSystemAudio(audioPath));
        }

        /// <summary>
        /// Resolves the hidden entity save component attached by the editor entity factory.
        /// </summary>
        /// <param name="entity">Entity whose save component should be returned.</param>
        /// <returns>Attached entity save component.</returns>
        static EntitySaveComponent FindRequiredEntitySaveComponent(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (entity.Components == null) {
                throw new InvalidOperationException("Generated entities must expose initialized component collections.");
            }

            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            throw new InvalidOperationException("Generated entities must include EntitySaveComponent before asset references can be authored.");
        }
    }
}
