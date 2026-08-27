using helengine;
using helengine.editor;

namespace city.menu.tools {
    /// <summary>
    /// Authors the current native animation clip used by the demo-disc logo.
    /// </summary>
    public sealed class DemoDiscLogoIdleAnimationGenerator {
        /// <summary>
        /// Project-relative path of the native demo-disc logo animation clip.
        /// </summary>
        public const string AnimationRelativePath = "animations/DemoDiscLogoIdle.hanim";

        /// <summary>
        /// Writes one current-format demo-disc logo animation clip beneath the project assets folder.
        /// </summary>
        /// <param name="projectRootPath">Project root that owns the assets directory.</param>
        public void Generate(string projectRootPath) {
            if (string.IsNullOrWhiteSpace(projectRootPath)) {
                throw new ArgumentException("Project root path must be provided.", nameof(projectRootPath));
            }

            AnimationClipAsset animationClip = new AnimationClipAsset {
                Id = "Animations/DemoDiscLogoIdle.animation",
                Duration = 4f,
                RotationTracks = [
                    new RotationKeyframeTrackAsset {
                        Keyframes = [
                            new RotationKeyframeAsset(0f, new float4(0f, 0f, 0f, 1f), AnimationInterpolationMode.Linear),
                            new RotationKeyframeAsset(1f, new float4(0f, 0f, 0.06540313f, 0.9978589f), AnimationInterpolationMode.Linear),
                            new RotationKeyframeAsset(2f, new float4(0f, 0f, 0f, 1f), AnimationInterpolationMode.Linear),
                            new RotationKeyframeAsset(3f, new float4(0f, 0f, -0.06540313f, 0.9978589f), AnimationInterpolationMode.Linear),
                            new RotationKeyframeAsset(4f, new float4(0f, 0f, 0f, 1f), AnimationInterpolationMode.Linear)
                        ]
                    }
                ]
            };

            new GeneratedAssetWriteService().WriteAsset(projectRootPath, AnimationRelativePath, animationClip);
        }
    }
}
