using helengine;
using helengine.editor;
using DemoDisc.EditorTools;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Verifies the generated demo-disc logo clip preserves its complete idle transform animation.
    /// </summary>
    public sealed class DemoDiscLogoIdleAnimationGeneratorTests {
        /// <summary>
        /// Ensures generation writes the historical offset and scale channels alongside the current rotation channel.
        /// </summary>
        [Fact]
        public void Generate_writes_original_offset_scale_and_rotation_tracks() {
            string projectRootPath = Path.Combine(Path.GetTempPath(), "demodisc-logo-animation-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(projectRootPath);
            try {
                using RenderingTestGeneratedAssetGraph graph = new RenderingTestGeneratedAssetGraph(projectRootPath);
                IEditorProjectAuthoringSession authoringSession = graph.CreateAuthoringSession(projectRootPath);
                using EditorAuthoringTransaction transaction = authoringSession.BeginTransaction();
                new DemoDiscLogoIdleAnimationGenerator(authoringSession, transaction).Generate();
                transaction.Commit();

                string animationPath = Path.Combine(
                    projectRootPath,
                    "assets",
                    DemoDiscLogoIdleAnimationGenerator.AnimationRelativePath.Replace('/', Path.DirectorySeparatorChar));
                AnimationClipAsset animationClip;
                using (FileStream stream = File.OpenRead(animationPath)) {
                    animationClip = Assert.IsType<AnimationClipAsset>(global::helengine.editor.AssetSerializer.Deserialize(stream));
                }

                Assert.Equal(4f, animationClip.Duration);

                PositionOffsetKeyframeTrackAsset offsetTrack = Assert.Single(animationClip.PositionOffsetTracks);
                Assert.Equal(5, offsetTrack.Keyframes.Length);
                Assert.Equal(new float3(0f, 6f, 0f), offsetTrack.Keyframes[0].Value);
                Assert.Equal(new float3(6f, 0f, 0f), offsetTrack.Keyframes[1].Value);
                Assert.Equal(new float3(0f, 6f, 0f), offsetTrack.Keyframes[2].Value);
                Assert.Equal(new float3(-6f, 0f, 0f), offsetTrack.Keyframes[3].Value);
                Assert.Equal(new float3(0f, 6f, 0f), offsetTrack.Keyframes[4].Value);

                ScaleKeyframeTrackAsset scaleTrack = Assert.Single(animationClip.ScaleTracks);
                Assert.Equal(5, scaleTrack.Keyframes.Length);
                Assert.Equal(new float3(0.95f, 1.06f, 1f), scaleTrack.Keyframes[0].Value);
                Assert.Equal(new float3(1.08f, 0.93f, 1f), scaleTrack.Keyframes[1].Value);
                Assert.Equal(scaleTrack.Keyframes[0].Value, scaleTrack.Keyframes[2].Value);
                Assert.Equal(scaleTrack.Keyframes[1].Value, scaleTrack.Keyframes[3].Value);
                Assert.Equal(scaleTrack.Keyframes[0].Value, scaleTrack.Keyframes[4].Value);

                RotationKeyframeTrackAsset rotationTrack = Assert.Single(animationClip.RotationTracks);
                Assert.Equal(5, rotationTrack.Keyframes.Length);
                Assert.Equal(new float4(0f, 0f, 0f, 1f), rotationTrack.Keyframes[0].Value);
                Assert.Equal(new float4(0f, 0f, 0.06540313f, 0.9978589f), rotationTrack.Keyframes[1].Value);
                Assert.Equal(new float4(0f, 0f, 0f, 1f), rotationTrack.Keyframes[2].Value);
                Assert.Equal(new float4(0f, 0f, -0.06540313f, 0.9978589f), rotationTrack.Keyframes[3].Value);
                Assert.Equal(new float4(0f, 0f, 0f, 1f), rotationTrack.Keyframes[4].Value);
            } finally {
                if (Directory.Exists(projectRootPath)) {
                    Directory.Delete(projectRootPath, true);
                }
            }
        }
    }
}
