using System;
using System.Collections.Generic;
using System.IO;
using city.menu;
using city.rendering;
using helengine;

namespace city.tests {
    /// <summary>
    /// Verifies the DemoDisc-owned progressive software path tracing session and component contract.
    /// </summary>
    public sealed class SoftwarePathTracerComponentTests {
        static readonly SoftwareTraceCamera Camera = new SoftwareTraceCamera(
            new float3(0f, 0f, 3f),
            new float3(0f, 0f, -1f),
            new float3(1f, 0f, 0f),
            new float3(0f, 1f, 0f),
            45f);

        /// <summary>
        /// Proves the new public session surface exists before implementation is supplied.
        /// </summary>
        [Fact]
        public void Session_surface_exposes_progressive_lifecycle() {
            Assert.Equal(typeof(IDisposable), typeof(SoftwarePathTraceSession).GetInterfaces()[0]);
            Assert.Equal(SoftwarePathTraceSessionState.Uninitialized, new SoftwarePathTraceSessionState());
        }

        /// <summary>
        /// Ensures the presentation texture is built at the exact requested dimensions and is opaque.
        /// </summary>
        [Fact]
        public void Initialize_builds_exact_opaque_presentation_texture() {
            using TestFixture fixture = CreateFixture(new SoftwareTraceResolution(9, 9));
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(fixture.RenderManager);

            session.Initialize(fixture.Roots, fixture.Source, new SoftwareTraceResolution(9, 9), Camera, 1f);

            Assert.Equal(SoftwarePathTraceSessionState.Tracing, session.State);
            Assert.Single(fixture.RenderManager.BuildCalls);
            Assert.Equal(9, fixture.RenderManager.BuildCalls[0].Width);
            Assert.Equal(9, fixture.RenderManager.BuildCalls[0].Height);
            Assert.NotNull(session.PresentationTexture);
            Assert.Null(fixture.RenderManager.BuildCalls[0].Colors);
            Assert.Equal(255, fixture.RenderManager.LastBuiltAlpha);
        }

        /// <summary>
        /// Ensures initialization disposes each raw model before progressive allocation callbacks execute.
        /// </summary>
        [Fact]
        public void Initialize_releases_raw_models_before_progressive_allocation() {
            using TestFixture fixture = CreateFixture(new SoftwareTraceResolution(8, 8));
            fixture.Allocator.RequireModelsDisposed = fixture.Source;
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(fixture.RenderManager);

            session.Initialize(fixture.Roots, fixture.Source, new SoftwareTraceResolution(8, 8), Camera, 1f, fixture.Allocator);

            Assert.Equal(SoftwarePathTraceSessionState.Tracing, session.State);
            Assert.True(fixture.Source.DisposedCount > 0);
            Assert.True(fixture.Allocator.ObservedAllModelsDisposed);
            Assert.NotNull(session.Scene);
            Assert.NotNull(session.Bvh);
            Assert.NotNull(session.Tracer);
        }

        /// <summary>
        /// Ensures memory diagnostics include the display, compact scene, BVH, stack, and progressive buffers.
        /// </summary>
        [Fact]
        public void Initialize_reports_exact_steady_and_peak_owned_bytes() {
            using TestFixture fixture = CreateFixture(new SoftwareTraceResolution(8, 8));
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(fixture.RenderManager);

            SoftwareTraceResolution resolution = new SoftwareTraceResolution(8, 8);
            session.Initialize(fixture.Roots, fixture.Source, resolution, Camera, 1f, fixture.Allocator);

            long displayBytes = (long)resolution.Width * resolution.Height * 4L;
            long sceneBytes = session.Scene.SteadyStateOwnedBytes;
            long bvhBytes = ((long)session.Bvh.Nodes.Length * 32L) + ((long)session.Bvh.TriangleOrder.Length * 4L);
            long stackBytes = (long)SoftwareBvh.TraversalStackCapacity * 4L;
            long expectedSteady = displayBytes + sceneBytes + bvhBytes + stackBytes + session.Tracer.ProgressiveOwnedBytes;
            Assert.Equal(expectedSteady, session.SteadyStateOwnedBytes);
            long presentationPeak = displayBytes + displayBytes;
            long modelPeak = displayBytes + session.Scene.InitializationPeakOwnedBytes;
            long bvhPeak = displayBytes + sceneBytes + bvhBytes + stackBytes;
            long expectedPeak = Math.Max(Math.Max(presentationPeak, modelPeak), Math.Max(bvhPeak, expectedSteady));
            Assert.Equal(expectedPeak, session.InitializationPeakOwnedBytes);
            Assert.Equal(SoftwarePathTracer.TileRgba8Bytes, session.Tracer.TileRgba8.Length);
        }

        /// <summary>
        /// Ensures a second initialization is rejected without replacing live resources.
        /// </summary>
        [Fact]
        public void Initialize_rejects_double_initialization() {
            using TestFixture fixture = CreateFixture(new SoftwareTraceResolution(8, 8));
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(fixture.RenderManager);
            session.Initialize(fixture.Roots, fixture.Source, new SoftwareTraceResolution(8, 8), Camera, 1f);

            Assert.Throws<InvalidOperationException>(() => session.Initialize(fixture.Roots, fixture.Source, new SoftwareTraceResolution(8, 8), Camera, 1f));
            Assert.Single(fixture.RenderManager.BuildCalls);
        }

        /// <summary>
        /// Ensures a default or malformed resolution fails before any renderer texture is built.
        /// </summary>
        [Fact]
        public void Initialize_rejects_non_positive_resolution_as_failed() {
            using TestFixture fixture = CreateFixture(new SoftwareTraceResolution(8, 8));
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(fixture.RenderManager);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => session.Initialize(fixture.Roots, fixture.Source, default, Camera, 1f));

            Assert.Equal(SoftwarePathTraceSessionState.Failed, session.State);
            Assert.False(string.IsNullOrWhiteSpace(session.FailureMessage));
            Assert.Empty(fixture.RenderManager.BuildCalls);
            Assert.Contains("dimensions", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Ensures an unvalidated default camera fails before presentation ownership begins.
        /// </summary>
        [Fact]
        public void Initialize_rejects_default_camera_before_texture_creation() {
            using TestFixture fixture = CreateFixture(new SoftwareTraceResolution(8, 8));
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(fixture.RenderManager);

            Assert.Throws<InvalidOperationException>(() => session.Initialize(fixture.Roots, fixture.Source, new SoftwareTraceResolution(8, 8), default, 1f));

            Assert.Equal(SoftwarePathTraceSessionState.Failed, session.State);
            Assert.Empty(fixture.RenderManager.BuildCalls);
        }

        /// <summary>
        /// Ensures an empty root list fails before presentation ownership begins.
        /// </summary>
        [Fact]
        public void Initialize_rejects_empty_roots_before_texture_creation() {
            using TestFixture fixture = CreateFixture(new SoftwareTraceResolution(8, 8));
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(fixture.RenderManager);

            Assert.Throws<InvalidOperationException>(() => session.Initialize(Array.Empty<Entity>(), fixture.Source, new SoftwareTraceResolution(8, 8), Camera, 1f));

            Assert.Equal(SoftwarePathTraceSessionState.Failed, session.State);
            Assert.Empty(fixture.RenderManager.BuildCalls);
        }

        /// <summary>
        /// Ensures a renderer dimension mismatch is rolled back through the creator exactly once.
        /// </summary>
        [Fact]
        public void Initialize_rejects_renderer_dimension_mismatch_and_releases_once() {
            using TestFixture fixture = CreateFixture(new SoftwareTraceResolution(8, 8));
            fixture.RenderManager.BuildWidthOverride = 7;
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(fixture.RenderManager);

            Assert.Throws<InvalidOperationException>(() => session.Initialize(fixture.Roots, fixture.Source, new SoftwareTraceResolution(8, 8), Camera, 1f));

            Assert.Equal(SoftwarePathTraceSessionState.Failed, session.State);
            Assert.Null(session.PresentationTexture);
            Assert.Single(fixture.RenderManager.ReleaseCalls);
        }

        /// <summary>Ensures a presentation creator failure leaves no retained raw pixels or ownership.</summary>
        [Fact]
        public void Initialize_texture_build_failure_rolls_back_before_ownership() {
            using TestFixture fixture = CreateFixture(new SoftwareTraceResolution(8, 8));
            fixture.RenderManager.ThrowOnBuild = true;
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(fixture.RenderManager);

            Assert.Throws<InvalidOperationException>(() => session.Initialize(fixture.Roots, fixture.Source, new SoftwareTraceResolution(8, 8), Camera, 1f));

            Assert.Equal(SoftwarePathTraceSessionState.Failed, session.State);
            Assert.False(string.IsNullOrWhiteSpace(session.FailureMessage));
            Assert.Empty(fixture.RenderManager.ReleaseCalls);
            Assert.NotNull(fixture.RenderManager.LastAttemptedBuildAsset);
            Assert.Null(fixture.RenderManager.LastAttemptedBuildAsset.Colors);
            Assert.Equal(0L, session.InitializationPeakOwnedBytes);
            Assert.Equal(0L, session.SteadyStateOwnedBytes);
        }

        /// <summary>Ensures a raw model loading failure releases the presentation through its creator.</summary>
        [Fact]
        public void Initialize_raw_model_failure_rolls_back_texture_and_scene() {
            using TestFixture fixture = CreateFixture(new SoftwareTraceResolution(8, 8));
            fixture.Source.ThrowAfterLoad = true;
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(fixture.RenderManager);

            Assert.Throws<InvalidOperationException>(() => session.Initialize(fixture.Roots, fixture.Source, new SoftwareTraceResolution(8, 8), Camera, 1f));

            Assert.Equal(SoftwarePathTraceSessionState.Failed, session.State);
            Assert.Single(fixture.RenderManager.BuildCalls);
            Assert.Single(fixture.RenderManager.ReleaseCalls);
            Assert.Null(session.PresentationTexture);
            Assert.Null(session.Scene);
            Assert.Equal(0L, session.InitializationPeakOwnedBytes);
        }

        /// <summary>Ensures accumulator allocation failures release all earlier stages.</summary>
        [Fact]
        public void Initialize_accumulator_failure_rolls_back_all_owned_stages() {
            using TestFixture fixture = CreateFixture(new SoftwareTraceResolution(8, 8));
            fixture.Allocator.ThrowOnAccumulator = true;
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(fixture.RenderManager);

            Assert.Throws<InvalidOperationException>(() => session.Initialize(fixture.Roots, fixture.Source, new SoftwareTraceResolution(8, 8), Camera, 1f, fixture.Allocator));

            Assert.Equal(SoftwarePathTraceSessionState.Failed, session.State);
            Assert.Single(fixture.RenderManager.ReleaseCalls);
            Assert.Null(session.Scene);
            Assert.Null(session.Bvh);
            Assert.Null(session.Tracer);
            Assert.Equal(0L, session.SteadyStateOwnedBytes);
        }

        /// <summary>Ensures tile-buffer allocation failures release all earlier stages.</summary>
        [Fact]
        public void Initialize_tile_buffer_failure_rolls_back_all_owned_stages() {
            using TestFixture fixture = CreateFixture(new SoftwareTraceResolution(8, 8));
            fixture.Allocator.ThrowOnTile = true;
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(fixture.RenderManager);

            Assert.Throws<InvalidOperationException>(() => session.Initialize(fixture.Roots, fixture.Source, new SoftwareTraceResolution(8, 8), Camera, 1f, fixture.Allocator));

            Assert.Equal(SoftwarePathTraceSessionState.Failed, session.State);
            Assert.Single(fixture.RenderManager.ReleaseCalls);
            Assert.Null(session.Scene);
            Assert.Null(session.Bvh);
            Assert.Null(session.Tracer);
            Assert.Equal(0L, session.SteadyStateOwnedBytes);
        }

        /// <summary>Ensures platform presets use the exact documented output dimensions and byte diagnostics.</summary>
        [Theory]
        [InlineData("ds", 256, 192)]
        [InlineData("windows", 320, 240)]
        public void Initialize_platform_resolution_reports_exact_texture_and_memory(string platformName, int expectedWidth, int expectedHeight) {
            using TestFixture fixture = CreateFixture(new SoftwareTraceResolution(expectedWidth, expectedHeight));
            SoftwareTraceResolution resolution = SoftwareTraceResolution.ForPlatform(platformName);
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(fixture.RenderManager);

            session.Initialize(fixture.Roots, fixture.Source, resolution, Camera, 1f, fixture.Allocator);

            Assert.Single(fixture.RenderManager.BuildCalls);
            Assert.Equal(expectedWidth, fixture.RenderManager.BuildCalls[0].Width);
            Assert.Equal(expectedHeight, fixture.RenderManager.BuildCalls[0].Height);
            long displayBytes = (long)expectedWidth * expectedHeight * 4L;
            long sceneBytes = session.Scene.SteadyStateOwnedBytes;
            long bvhBytes = ((long)session.Bvh.Nodes.Length * 32L) + ((long)session.Bvh.TriangleOrder.Length * 4L);
            long stackBytes = (long)SoftwareBvh.TraversalStackCapacity * 4L;
            long expectedSteady = displayBytes + sceneBytes + bvhBytes + stackBytes + session.Tracer.ProgressiveOwnedBytes;
            Assert.Equal(expectedSteady, session.SteadyStateOwnedBytes);
            long presentationPeak = displayBytes + displayBytes;
            long modelPeak = displayBytes + session.Scene.InitializationPeakOwnedBytes;
            long bvhPeak = displayBytes + sceneBytes + bvhBytes + stackBytes;
            long expectedPeak = Math.Max(Math.Max(presentationPeak, modelPeak), Math.Max(bvhPeak, expectedSteady));
            Assert.Equal(expectedPeak, session.InitializationPeakOwnedBytes);
        }

        /// <summary>
        /// Ensures every session render call performs one rectangle upload with the tracer-owned tile buffer.
        /// </summary>
        [Fact]
        public void Render_and_upload_next_tile_uploads_one_exact_tile() {
            using TestFixture fixture = CreateFixture(new SoftwareTraceResolution(8, 8));
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(fixture.RenderManager);
            session.Initialize(fixture.Roots, fixture.Source, new SoftwareTraceResolution(8, 8), Camera, 1f);

            SoftwareTraceTile tile = session.RenderAndUploadNextTile();

            Assert.Single(fixture.RenderManager.Uploads);
            SoftwarePathTracerTestRenderManager2D.Upload upload = fixture.RenderManager.Uploads[0];
            Assert.Equal(tile.X, upload.X);
            Assert.Equal(tile.Y, upload.Y);
            Assert.Equal(tile.Width, upload.Width);
            Assert.Equal(tile.Height, upload.Height);
            Assert.Same(session.Tracer.TileRgba8, upload.Source);
            Assert.Equal(32, upload.SourceRowPitch);
            Assert.Same(session.PresentationTexture, upload.Texture);
        }

        /// <summary>
        /// Ensures a non-divisible fixture reaches both full and clipped edge uploads without stale bytes.
        /// </summary>
        [Fact]
        public void Render_and_upload_next_tile_clips_edges_and_reuses_resources() {
            using TestFixture fixture = CreateFixture(new SoftwareTraceResolution(9, 9));
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(fixture.RenderManager);
            session.Initialize(fixture.Roots, fixture.Source, new SoftwareTraceResolution(9, 9), Camera, 1f);
            RuntimeTexture texture = session.PresentationTexture;
            byte[] tileBuffer = session.Tracer.TileRgba8;

            for (int i = 0; i < 4; i++) {
                session.RenderAndUploadNextTile();
            }

            Assert.Equal(4, fixture.RenderManager.Uploads.Count);
            Assert.Contains(fixture.RenderManager.Uploads, upload => upload.Width == 1 || upload.Height == 1);
            Assert.All(fixture.RenderManager.Uploads, upload => {
                Assert.Same(texture, upload.Texture);
                Assert.Same(tileBuffer, upload.Source);
                Assert.Equal(32, upload.SourceRowPitch);
            });
        }

        /// <summary>Ensures a warmed tile/upload call performs no managed allocation when recording is disabled.</summary>
        [Fact]
        public void Render_and_upload_next_tile_hot_path_does_not_allocate_with_preallocated_recording() {
            using TestFixture fixture = CreateFixture(new SoftwareTraceResolution(8, 8));
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(fixture.RenderManager);
            session.Initialize(fixture.Roots, fixture.Source, new SoftwareTraceResolution(8, 8), Camera, 1f, fixture.Allocator);

            session.RenderAndUploadNextTile();
            fixture.RenderManager.Uploads.Capacity = 16;
            fixture.RenderManager.RecordUploads = false;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            long before = GC.GetAllocatedBytesForCurrentThread();

            session.RenderAndUploadNextTile();

            long after = GC.GetAllocatedBytesForCurrentThread();
            Assert.Equal(0L, after - before);
        }

        /// <summary>
        /// Ensures a region-upload failure transitions to Failed and cleanup releases the creator-owned texture once.
        /// </summary>
        [Fact]
        public void Upload_failure_rolls_back_once_and_blocks_future_work() {
            using TestFixture fixture = CreateFixture(new SoftwareTraceResolution(8, 8));
            using SoftwarePathTraceSession session = new SoftwarePathTraceSession(fixture.RenderManager);
            session.Initialize(fixture.Roots, fixture.Source, new SoftwareTraceResolution(8, 8), Camera, 1f);
            fixture.RenderManager.ThrowOnUpdate = true;

            Assert.Throws<InvalidOperationException>(() => session.RenderAndUploadNextTile());
            Assert.Equal(SoftwarePathTraceSessionState.Failed, session.State);
            Assert.False(string.IsNullOrWhiteSpace(session.FailureMessage));
            Assert.Null(session.PresentationTexture);
            Assert.Single(fixture.RenderManager.ReleaseCalls);
            Assert.Throws<InvalidOperationException>(() => session.RenderAndUploadNextTile());
            session.Dispose();
            Assert.Single(fixture.RenderManager.ReleaseCalls);
        }

        /// <summary>
        /// Ensures explicit disposal is idempotent and clears all diagnostic owned bytes.
        /// </summary>
        [Fact]
        public void Dispose_is_idempotent_and_clears_owned_state() {
            using TestFixture fixture = CreateFixture(new SoftwareTraceResolution(8, 8));
            SoftwarePathTraceSession session = new SoftwarePathTraceSession(fixture.RenderManager);
            session.Initialize(fixture.Roots, fixture.Source, new SoftwareTraceResolution(8, 8), Camera, 1f);

            session.Dispose();
            session.Dispose();

            Assert.Equal(SoftwarePathTraceSessionState.Disposed, session.State);
            Assert.Null(session.Scene);
            Assert.Null(session.Bvh);
            Assert.Null(session.Tracer);
            Assert.Null(session.PresentationTexture);
            Assert.Equal(0L, session.SteadyStateOwnedBytes);
            Assert.Equal(0L, session.InitializationPeakOwnedBytes);
            Assert.Single(fixture.RenderManager.ReleaseCalls);
        }

        /// <summary>
        /// Ensures authored component properties contain all required references and no forbidden runtime model properties.
        /// </summary>
        [Fact]
        public void Component_surface_uses_references_and_authored_camera_values() {
            Type componentType = typeof(SoftwarePathTracerComponent);
            Assert.NotNull(componentType.GetProperty(nameof(SoftwarePathTracerComponent.OutputSpriteEntityReference)));
            Assert.NotNull(componentType.GetProperty(nameof(SoftwarePathTracerComponent.SppTextEntityReference)));
            Assert.NotNull(componentType.GetProperty(nameof(SoftwarePathTracerComponent.ElapsedTextEntityReference)));
            Assert.NotNull(componentType.GetProperty(nameof(SoftwarePathTracerComponent.RaysPerSecondTextEntityReference)));
            Assert.NotNull(componentType.GetProperty(nameof(SoftwarePathTracerComponent.TraceCameraOrigin)));
            Assert.NotNull(componentType.GetProperty(nameof(SoftwarePathTracerComponent.TraceCameraForward)));
            Assert.NotNull(componentType.GetProperty(nameof(SoftwarePathTracerComponent.TraceCameraRight)));
            Assert.NotNull(componentType.GetProperty(nameof(SoftwarePathTracerComponent.TraceCameraUp)));
            Assert.NotNull(componentType.GetProperty(nameof(SoftwarePathTracerComponent.VerticalFieldOfViewDegrees)));
            Assert.NotNull(componentType.GetProperty(nameof(SoftwarePathTracerComponent.Exposure)));
            Assert.Null(componentType.GetProperty("RuntimeTexture"));
            Assert.Null(componentType.GetProperty("RuntimeModel"));
        }

        /// <summary>
        /// Ensures HUD formatting retains the required visible prefixes.
        /// </summary>
        [Fact]
        public void Hud_formatting_uses_required_prefixes() {
            Assert.StartsWith("SPP: ", SoftwarePathTracerComponent.FormatSpp(3));
            Assert.StartsWith("Time: ", SoftwarePathTracerComponent.FormatElapsed(1.25));
            Assert.StartsWith("Rays/s: ", SoftwarePathTracerComponent.FormatRaysPerSecond(120.5));
            Assert.Equal("SPP: 0", SoftwarePathTracerComponent.FormatSpp(-3));
            Assert.Equal("Time: 1.26s", SoftwarePathTracerComponent.FormatElapsed(1.256));
            Assert.Equal("Rays/s: 120.5", SoftwarePathTracerComponent.FormatRaysPerSecond(120.54));
            Assert.Equal("Time: 0.00s", SoftwarePathTracerComponent.FormatElapsed(double.NaN));
            Assert.Equal("Rays/s: 0.0", SoftwarePathTracerComponent.FormatRaysPerSecond(double.PositiveInfinity));
        }

        /// <summary>
        /// Ensures an elapsed trace that starts at Core time zero advances normally.
        /// </summary>
        [Fact]
        public void Elapsed_trace_time_accepts_zero_start_timestamp() {
            Assert.Equal(1.25d, SoftwarePathTracerComponent.ComputeElapsedTraceSeconds(1.25d, 0d, SoftwarePathTraceSessionState.Tracing));
            Assert.Equal(0d, SoftwarePathTracerComponent.ComputeElapsedTraceSeconds(1.25d, 0d, SoftwarePathTraceSessionState.Uninitialized));
        }

        /// <summary>
        /// Ensures HUD throttling compares relative elapsed time to a relative zero baseline.
        /// </summary>
        [Fact]
        public void Hud_refresh_is_due_at_relative_quarter_second() {
            Assert.False(SoftwarePathTracerComponent.ShouldRefreshHud(0.24d, 0, SoftwarePathTraceSessionState.Tracing, 0d, 0, SoftwarePathTraceSessionState.Tracing));
            Assert.True(SoftwarePathTracerComponent.ShouldRefreshHud(0.25d, 0, SoftwarePathTraceSessionState.Tracing, 0d, 0, SoftwarePathTraceSessionState.Tracing));
        }

        /// <summary>
        /// Ensures handheld platforms bypass controller polling while desktop platforms permit it.
        /// </summary>
        [Theory]
        [InlineData("ds", false)]
        [InlineData("3ds", false)]
        [InlineData("windows", true)]
        public void Return_policy_identifies_handheld_polling(string platform, bool expected) {
            Assert.Equal(expected, SoftwarePathTracerComponent.ShouldPollControllerReturn(platform));
        }

        /// <summary>Ensures attachment only resets state; staged work begins at entity hierarchy initialization.</summary>
        [Fact]
        public void ComponentAdded_does_not_initialize_until_ComponentInitialized() {
            using ComponentFixture fixture = new ComponentFixture("ds");

            fixture.Root.AddComponent(fixture.Component);

            Assert.Empty(fixture.RenderManager.BuildCalls);
            fixture.Root.InitializeHierarchy();

            Assert.Single(fixture.RenderManager.BuildCalls);
            Assert.True(ReferenceEquals(fixture.RenderManager.LastBuiltTexture, fixture.OutputSprite.Texture), fixture.SppText.Text);
        }

        /// <summary>Ensures initialized references receive the exact session texture and one update uploads one tile.</summary>
        [Fact]
        public void ComponentInitialized_assigns_sprite_and_Update_uploads_once() {
            using ComponentFixture fixture = new ComponentFixture("windows");
            fixture.Root.AddComponent(fixture.Component);
            fixture.Root.InitializeHierarchy();
            RuntimeTexture texture = fixture.OutputSprite.Texture;

            Assert.True(texture != null, fixture.SppText.Text);
            Assert.Equal(SoftwarePathTraceSessionState.Tracing, fixture.Component.SessionState);
            int uploadsBefore = fixture.RenderManager.Uploads.Count;
            fixture.Component.Update();

            Assert.Same(texture, fixture.OutputSprite.Texture);
            Assert.Equal(uploadsBefore + 1, fixture.RenderManager.Uploads.Count);
        }

        /// <summary>Ensures the component catch clears the sprite when its session upload fails.</summary>
        [Fact]
        public void Component_update_upload_failure_clears_sprite_and_releases_session() {
            using ComponentFixture fixture = new ComponentFixture("windows");
            fixture.Root.AddComponent(fixture.Component);
            fixture.Root.InitializeHierarchy();
            fixture.RenderManager.ThrowOnUpdate = true;

            fixture.Component.Update();

            Assert.Null(fixture.OutputSprite.Texture);
            Assert.Equal(SoftwarePathTraceSessionState.Failed, fixture.Component.SessionState);
            Assert.Single(fixture.RenderManager.ReleaseCalls);
            Assert.StartsWith("Trace error: ", fixture.SppText.Text);

            fixture.Component.Dispose();
            Assert.Equal(SoftwarePathTraceSessionState.Disposed, fixture.Component.SessionState);
            Assert.Single(fixture.RenderManager.ReleaseCalls);
        }

        /// <summary>Ensures non-handheld Return uses the shared input/resolver path and suppresses later tracing.</summary>
        [Fact]
        public void Component_update_return_disposes_before_requesting_main_menu() {
            using ComponentFixture fixture = new ComponentFixture("windows");
            fixture.Root.AddComponent(fixture.Component);
            fixture.Root.InitializeHierarchy();
            int uploadsBeforeReturn = fixture.RenderManager.Uploads.Count;
            InputGamepadState pressed = new InputGamepadState { Connected = true };
            pressed.SetButtonDown(InputGamepadButton.East, true);
            fixture.InputBackend.Enqueue(new InputFrameState {
                Gamepads = new[] { pressed },
                GamepadCount = 1
            });
            fixture.Core.Input.EarlyUpdate();

            fixture.Component.Update();
            fixture.Core.Input.Update();

            Assert.True(fixture.Component.ReturnRequested);
            Assert.Equal(SoftwarePathTraceSessionState.Disposed, fixture.Component.SessionState);
            Assert.Null(fixture.OutputSprite.Texture);
            Assert.Single(fixture.RenderManager.ReleaseCalls);
            Assert.Equal(uploadsBeforeReturn, fixture.RenderManager.Uploads.Count);
            Assert.True(fixture.Core.SceneManager.IsSceneTransitionActive);
            Assert.Equal(DemoDiscMainMenuSceneResolver.ResolveRuntimeSceneId(), fixture.Core.SceneManager.TransitionTargetSceneId);
        }

        /// <summary>Ensures component disposal clears presentation, releases once, reports Disposed, and blocks updates.</summary>
        [Fact]
        public void Component_dispose_is_idempotent_and_blocks_later_updates() {
            using ComponentFixture fixture = new ComponentFixture("windows");
            fixture.Root.AddComponent(fixture.Component);
            fixture.Root.InitializeHierarchy();

            fixture.Component.Dispose();
            fixture.Component.Dispose();
            int uploadsAfterDispose = fixture.RenderManager.Uploads.Count;
            fixture.Component.Update();

            Assert.Null(fixture.OutputSprite.Texture);
            Assert.Equal(SoftwarePathTraceSessionState.Disposed, fixture.Component.SessionState);
            Assert.Equal(0L, fixture.Component.InitializationPeakOwnedBytes);
            Assert.Equal(0L, fixture.Component.SteadyStateOwnedBytes);
            Assert.Single(fixture.RenderManager.ReleaseCalls);
            Assert.Equal(uploadsAfterDispose, fixture.RenderManager.Uploads.Count);
        }

        /// <summary>Ensures each serialized reference target rejects zero, missing, duplicate, and wrong component identities.</summary>
        [Theory]
        [InlineData(0u, 0)]
        [InlineData(999u, 0)]
        [InlineData(101u, 1)]
        [InlineData(102u, 0)]
        public void ComponentInitialized_rejects_invalid_reference_targets(uint outputId, int duplicateOutput) {
            using ComponentFixture fixture = new ComponentFixture("windows");
            if (duplicateOutput != 0) {
                fixture.AddDuplicateOutputEntity(outputId);
            }
            fixture.Component.OutputSpriteEntityReference = new SceneEntityReference { EntityId = outputId };
            fixture.Root.AddComponent(fixture.Component);
            fixture.Root.InitializeHierarchy();

            Assert.Equal(SoftwarePathTraceSessionState.Failed, fixture.Component.SessionState);
            Assert.Empty(fixture.RenderManager.BuildCalls);
            Assert.Null(fixture.OutputSprite.Texture);
        }

        static TestFixture CreateFixture(SoftwareTraceResolution resolution) {
            TestFixture fixture = new TestFixture(resolution);
            fixture.Source.Register(fixture.Reference, CreateCubeAsset);
            fixture.Source.Register(fixture.EmitterReference, CreateGeneratedCubeAsset);
            fixture.Roots.Add(fixture.AddModel(fixture.Reference, new SoftwareMaterial()));
            Entity emitter = fixture.AddModel(fixture.EmitterReference, new SoftwareMaterial { EmissionColor = float3.One, EmissionStrength = 1f });
            emitter.LocalPosition = new float3(0f, 1f, 0f);
            emitter.LocalScale = new float3(0.55f, 0.025f, 0.45f);
            fixture.Roots.Add(emitter);
            return fixture;
        }

        static ModelAsset CreateCubeAsset() {
            ModelAsset cube = ModelUtils.GenerateCubeMesh(float3.Zero, float3.One);
            cube.Submeshes = new[] { new ModelSubmeshAsset { MaterialSlotName = "DefaultMaterial", IndexStart = 0, IndexCount = cube.Indices16.Length } };
            return cube;
        }

        static ModelAsset CreateGeneratedCubeAsset() {
            ModelAsset cube = ModelUtils.GenerateCubeMesh(float3.Zero, float3.One);
            cube.Submeshes = new[] { new ModelSubmeshAsset { MaterialSlotName = "DefaultMaterial", IndexStart = 0, IndexCount = cube.Indices16.Length } };
            return cube;
        }

        sealed class ComponentFixture : IDisposable {
            const uint OutputId = 101u;
            const uint SppId = 102u;
            const uint ElapsedId = 103u;
            const uint RaysId = 104u;
            readonly List<Entity> auxiliaryEntities = new List<Entity>();
            readonly SceneAssetReference modelReference = SceneAssetReferenceFactory.CreateFileSystemModel("models/cube.hasset");

            public readonly Core Core;
            public readonly SoftwarePathTracerTestRenderManager2D RenderManager;
            public readonly DemoDiscGamepadInputTestBackend InputBackend;
            public readonly Entity Root;
            public readonly Entity OutputEntity;
            public readonly Entity SppEntity;
            public readonly Entity ElapsedEntity;
            public readonly Entity RaysEntity;
            public readonly SpriteComponent OutputSprite;
            public readonly TextComponent SppText;
            public readonly TextComponent ElapsedText;
            public readonly TextComponent RaysText;
            public readonly SoftwarePathTracerComponent Component;

            public ComponentFixture(string platformName) {
                byte[] modelPayload = CreatePackagedCubePayload();
                Core = new Core(new CoreInitializationOptions {
                    ContentStreamSource = new InMemoryContentStreamSource(modelPayload),
                    ScenePathResolver = new PassthroughSceneIdPathResolver()
                });
                RenderManager = new SoftwarePathTracerTestRenderManager2D();
                InputBackend = new DemoDiscGamepadInputTestBackend();
                Core.Initialize(null, RenderManager, InputBackend, new PlatformInfo(platformName, "test"));

                OutputEntity = CreateUiEntity(OutputId, new SpriteComponent(), out OutputSprite);
                SppEntity = CreateUiEntity(SppId, new TextComponent(), out SppText);
                ElapsedEntity = CreateUiEntity(ElapsedId, new TextComponent(), out ElapsedText);
                RaysEntity = CreateUiEntity(RaysId, new TextComponent(), out RaysText);

                Root = new Entity(Core);
                Root.InitComponents();
                Root.InitChildren();
                Root.AddComponent(new SceneEntityRuntimeIdComponent { SceneEntityId = 1u });
                Root.AddComponent(new SoftwareModelComponent {
                    ModelReference = modelReference,
                    Materials = new[] { new SoftwareMaterial() }
                });
                Entity emitter = new Entity(Core);
                emitter.InitComponents();
                emitter.AddComponent(new SoftwareModelComponent {
                    ModelReference = modelReference,
                    Materials = new[] { new SoftwareMaterial { EmissionColor = float3.One, EmissionStrength = 1f } }
                });
                emitter.LocalPosition = new float3(0f, 1f, 0f);
                emitter.LocalScale = new float3(0.55f, 0.025f, 0.45f);
                Root.AddChild(emitter);
                Component = new SoftwarePathTracerComponent {
                    OutputSpriteEntityReference = new SceneEntityReference { EntityId = OutputId },
                    SppTextEntityReference = new SceneEntityReference { EntityId = SppId },
                    ElapsedTextEntityReference = new SceneEntityReference { EntityId = ElapsedId },
                    RaysPerSecondTextEntityReference = new SceneEntityReference { EntityId = RaysId }
                };
            }

            public void AddDuplicateOutputEntity(uint sceneEntityId) {
                SpriteComponent duplicateSprite;
                Entity duplicate = CreateUiEntity(sceneEntityId, new SpriteComponent(), out duplicateSprite);
                auxiliaryEntities.Add(duplicate);
            }

            Entity CreateUiEntity<T>(uint sceneEntityId, T component, out T typedComponent) where T : Component {
                Entity entity = new Entity(Core);
                entity.InitComponents();
                entity.AddComponent(new SceneEntityRuntimeIdComponent { SceneEntityId = sceneEntityId });
                entity.AddComponent(component);
                typedComponent = component;
                auxiliaryEntities.Add(entity);
                return entity;
            }

            public void Dispose() {
                if (Root != null && !Root.IsDisposed) {
                    Root.Dispose();
                }
                for (int index = auxiliaryEntities.Count - 1; index >= 0; index--) {
                    Entity entity = auxiliaryEntities[index];
                    if (!entity.IsDisposed) {
                        entity.Dispose();
                    }
                }
                Core.Dispose();
            }
        }

        sealed class InMemoryContentStreamSource : IContentStreamSource {
            readonly byte[] payload;

            public InMemoryContentStreamSource(byte[] payload) {
                this.payload = payload ?? throw new ArgumentNullException(nameof(payload));
            }

            public Stream OpenRead(string assetPath) {
                return new MemoryStream(payload, false);
            }
        }

        sealed class PassthroughSceneIdPathResolver : ISceneIdPathResolver {
            public string ResolveScenePath(string sceneId) {
                return sceneId;
            }
        }

        static byte[] CreatePackagedCubePayload() {
            ModelAsset cube = CreateCubeAsset();
            using MemoryStream stream = new MemoryStream();
            EngineBinaryHeader header = new EngineBinaryHeader(
                EngineBinaryEndianness.LittleEndian,
                PackagedAssetBinarySerializer.CurrentVersion,
                PackagedAssetBinarySerializer.FormatId,
                (ushort)PackagedAssetBinarySerializer.RecordKind,
                (ushort)EditorAssetBinaryValueKind.ModelAsset);
            EngineBinaryHeaderSerializer.Write(stream, header);
            using (EngineBinaryWriter writer = EngineBinaryWriter.Create(stream, EngineBinaryEndianness.LittleEndian)) {
                writer.WriteString(string.Empty);
                writer.WriteInt64(1L);
                writer.WriteString(string.Empty);
                writer.WriteArray(Array.Empty<string>(), (itemWriter, value) => itemWriter.WriteString(value));
                writer.WriteArray(cube.Positions, (itemWriter, value) => itemWriter.WriteFloat3(value));
                writer.WriteArray(cube.Normals, (itemWriter, value) => itemWriter.WriteFloat3(value));
                writer.WriteArray(cube.TexCoords, (itemWriter, value) => itemWriter.WriteFloat2(value));
                writer.WriteArray(cube.Indices16, (itemWriter, value) => itemWriter.WriteUInt16(value));
                writer.WriteArray(cube.Indices32, (itemWriter, value) => itemWriter.WriteUInt32(value));
                writer.WriteArray(cube.Submeshes, (itemWriter, value) => {
                    itemWriter.WriteString(value.MaterialSlotName);
                    itemWriter.WriteInt32(value.IndexStart);
                    itemWriter.WriteInt32(value.IndexCount);
                });
            }
            return stream.ToArray();
        }

        sealed class TestFixture : IDisposable {
            public readonly Core Core;
            public readonly SoftwarePathTracerTestRenderManager2D RenderManager;
            public readonly FakeSoftwareModelAssetSource Source = new FakeSoftwareModelAssetSource();
            public readonly RecordingAllocator Allocator = new RecordingAllocator();
            public readonly SceneAssetReference Reference = SceneAssetReferenceFactory.CreateFileSystemModel("models/cube.hasset");
            public readonly SceneAssetReference EmitterReference = SceneAssetReferenceFactory.CreateFileSystemModel("models/emitter.hasset");
            public readonly List<Entity> Roots = new List<Entity>();

            public TestFixture(SoftwareTraceResolution resolution) {
                Core = new Core(new CoreInitializationOptions { ContentStreamSource = new HostFileSystemContentStreamSource(Environment.CurrentDirectory) });
                RenderManager = new SoftwarePathTracerTestRenderManager2D();
                Core.Initialize(null, RenderManager, null, new PlatformInfo("windows", "test"));
            }

            public Entity AddModel(SceneAssetReference reference, params SoftwareMaterial[] materials) {
                Entity entity = new Entity(Core);
                entity.InitComponents();
                entity.AddComponent(new SoftwareModelComponent { ModelReference = reference, Materials = materials });
                return entity;
            }

            public void Dispose() {
                for (int i = Roots.Count - 1; i >= 0; i--) {
                    Roots[i].Dispose();
                }
                Core.Dispose();
            }
        }

        sealed class RecordingAllocator : ISoftwareTraceBufferAllocator {
            public FakeSoftwareModelAssetSource RequireModelsDisposed;
            public bool ObservedAllModelsDisposed;
            public bool ThrowOnAccumulator;
            public bool ThrowOnTile;

            public float3[] AllocateAccumulator(int pixelCount) {
                ObservedAllModelsDisposed = RequireModelsDisposed == null || RequireModelsDisposed.DisposedCount > 0;
                if (ThrowOnAccumulator) {
                    throw new InvalidOperationException("Injected accumulator allocation failure.");
                }
                return new float3[pixelCount];
            }

            public byte[] AllocateTileRgba8(int byteCount) {
                if (ThrowOnTile) {
                    throw new InvalidOperationException("Injected tile-buffer allocation failure.");
                }
                return new byte[byteCount];
            }
        }
    }
}
