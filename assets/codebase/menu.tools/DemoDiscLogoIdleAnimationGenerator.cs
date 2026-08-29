using helengine;
using helengine.editor;

namespace city.menu.tools {
    /// <summary>
    /// Authors the current native animation clip used by the demo-disc logo.
    /// </summary>
    public sealed class DemoDiscLogoIdleAnimationGenerator {
        /// <summary>
        /// Host-owned capability used to author the current native animation asset.
        /// </summary>
        readonly IEditorProjectAuthoringSession AssetAuthoringService;
        readonly EditorAuthoringTransaction Transaction;

        /// <summary>
        /// Project-relative path of the native demo-disc logo animation clip.
        /// </summary>
        public const string AnimationRelativePath = "animations/DemoDiscLogoIdle.hanim";

        /// <summary>
        /// Initializes one logo animation author.
        /// </summary>
        /// <param name="assetAuthoringService">Host-owned capability used to author the current native animation asset.</param>
        public DemoDiscLogoIdleAnimationGenerator(
            IEditorProjectAuthoringSession assetAuthoringService,
            EditorAuthoringTransaction transaction) {
            AssetAuthoringService = assetAuthoringService ?? throw new ArgumentNullException(nameof(assetAuthoringService));
            Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        /// <summary>
        /// Writes one current-format demo-disc logo animation clip through the public editor asset API.
        /// </summary>
        public void Generate() {

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

            animationClip.AuthoringAssetId = city.scene.tools.ProjectAuthoringAssetIdentityCatalog.GetNativeAssetIdentity(AnimationRelativePath);
            animationClip.FormerAuthoringAssetIds = Array.Empty<string>();
            Transaction.WriteAsset(AnimationRelativePath, animationClip);
        }
    }
}
