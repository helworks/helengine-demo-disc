using System;
using System.Collections.Generic;
using helengine;

namespace city.tests {
    /// <summary>
    /// Records accepted render-manager texture ownership and rectangular upload calls for session tests.
    /// </summary>
    public sealed class SoftwarePathTracerTestRenderManager2D : RenderManager2D {
        readonly HashSet<RuntimeTexture> ownedTextures = new HashSet<RuntimeTexture>();

        /// <summary>Raw build calls observed by this fake manager.</summary>
        public List<TextureAsset> BuildCalls { get; } = new List<TextureAsset>();

        /// <summary>Rectangle uploads observed by this fake manager.</summary>
        public List<Upload> Uploads { get; } = new List<Upload>();

        /// <summary>Runtime textures released through this exact creator instance.</summary>
        public List<RuntimeTexture> ReleaseCalls { get; } = new List<RuntimeTexture>();

        /// <summary>All release attempts, including attempts configured to fail.</summary>
        public List<RuntimeTexture> ReleaseAttempts { get; } = new List<RuntimeTexture>();

        /// <summary>Raw asset presented to the creator, including a failed build attempt.</summary>
        public TextureAsset LastAttemptedBuildAsset { get; private set; }

        /// <summary>Disables test-only upload recording after the hot path is warmed.</summary>
        public bool RecordUploads { get; set; } = true;

        /// <summary>Injects a build failure.</summary>
        public bool ThrowOnBuild { get; set; }

        /// <summary>Injects a region-update failure.</summary>
        public bool ThrowOnUpdate { get; set; }

        /// <summary>Injects an independent runtime-texture release failure.</summary>
        public bool ThrowOnRelease { get; set; }

        /// <summary>Optional callback invoked from the texture creator before it returns its runtime texture.</summary>
        public Action OnBuildTextureFromRaw { get; set; }

        /// <summary>Optional width used by the next fake runtime texture.</summary>
        public int BuildWidthOverride { get; set; }

        /// <summary>Optional height used by the next fake runtime texture.</summary>
        public int BuildHeightOverride { get; set; }

        /// <summary>Alpha byte observed while building the blank presentation texture.</summary>
        public byte LastBuiltAlpha { get; private set; }

        /// <summary>Most recent runtime texture returned by this creator.</summary>
        public RuntimeTexture LastBuiltTexture { get; private set; }

        /// <summary>Creates a fake runtime texture with the raw dimensions.</summary>
        public override RuntimeTexture BuildTextureFromRaw(TextureAsset data) {
            LastAttemptedBuildAsset = data;
            OnBuildTextureFromRaw?.Invoke();
            if (ThrowOnBuild) {
                throw new InvalidOperationException("Injected texture build failure.");
            }
            BuildCalls.Add(data);
            LastBuiltAlpha = data.Colors == null || data.Colors.Length < 4 ? (byte)0 : data.Colors[3];
            RuntimeTexture texture = new TestRuntimeTexture {
                Width = BuildWidthOverride > 0 ? BuildWidthOverride : data.Width,
                Height = BuildHeightOverride > 0 ? BuildHeightOverride : data.Height
            };
            ownedTextures.Add(texture);
            LastBuiltTexture = texture;
            return texture;
        }

        /// <summary>Records the validated core upload after checking renderer ownership.</summary>
        protected override void UpdateTextureRegionCore(RuntimeTexture texture, int x, int y, int width, int height, [NativeNoEscape] byte[] rgba8, int sourceRowPitch) {
            if (ThrowOnUpdate) {
                throw new InvalidOperationException("Injected texture region update failure.");
            }
            if (!ownedTextures.Contains(texture)) {
                throw new InvalidOperationException("Texture was not created by this fake renderer.");
            }
            if (!RecordUploads) {
                return;
            }
            int copiedRowPitch = checked(width * 4);
            byte[] copy = new byte[checked(copiedRowPitch * height)];
            for (int row = 0; row < height; row++) {
                Array.Copy(rgba8, row * sourceRowPitch, copy, row * copiedRowPitch, copiedRowPitch);
            }
            Uploads.Add(new Upload(texture, x, y, width, height, rgba8, sourceRowPitch, copiedRowPitch, copy));
        }

        /// <summary>Releases one texture through the manager that created it.</summary>
        public override void ReleaseTexture(RuntimeTexture texture) {
            ReleaseAttempts.Add(texture);
            if (ThrowOnRelease) {
                throw new InvalidOperationException("Injected texture release failure.");
            }
            if (!ownedTextures.Remove(texture)) {
                throw new InvalidOperationException("Texture was not owned by this fake renderer or was already released.");
            }
            ReleaseCalls.Add(texture);
            base.ReleaseTexture(texture);
        }

        public override void DrawSprite(ISpriteDrawable2D sprite) { }
        public override void DrawText(ITextDrawable2D text) { }
        public override void DrawRoundedRect(IRoundedRectDrawable2D shape) { }

        /// <summary>One recorded validated rectangular upload.</summary>
        public sealed class Upload {
            public readonly RuntimeTexture Texture;
            public readonly int X;
            public readonly int Y;
            public readonly int Width;
            public readonly int Height;
            public readonly byte[] Source;
            public readonly int SourceRowPitch;
            public readonly int CopiedRowPitch;
            public readonly byte[] CopiedBytes;

            public Upload(RuntimeTexture texture, int x, int y, int width, int height, byte[] source, int sourceRowPitch, int copiedRowPitch, byte[] copiedBytes) {
                Texture = texture;
                X = x;
                Y = y;
                Width = width;
                Height = height;
                Source = source;
                SourceRowPitch = sourceRowPitch;
                CopiedRowPitch = copiedRowPitch;
                CopiedBytes = copiedBytes;
            }
        }

        sealed class TestRuntimeTexture : RuntimeTexture { }
    }
}
