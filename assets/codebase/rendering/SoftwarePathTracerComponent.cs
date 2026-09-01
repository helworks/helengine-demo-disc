using System;
using System.Collections.Generic;
using city.menu;
using helengine;

namespace city.rendering {
    /// <summary>
    /// Describes the owned-resource stages of one progressive software trace.
    /// </summary>
    public enum SoftwarePathTraceSessionState {
        Uninitialized,
        CreatingPresentation,
        LoadingModels,
        BuildingBvh,
        AllocatingProgressiveBuffers,
        Tracing,
        Failed,
        Disposed
    }

    /// <summary>
    /// Owns one software trace presentation texture and the compact/progressive resources that feed it.
    /// </summary>
    public sealed class SoftwarePathTraceSession : IDisposable {
        readonly RenderManager2D renderManager2D;
        RuntimeTexture presentationTexture;
        SoftwareTraceScene scene;
        SoftwareBvh bvh;
        SoftwarePathTracer tracer;
        int[] traversalStack;
        bool presentationReleaseAttempted;
        SoftwarePathTraceSessionState state;
        string failureMessage;
        long initializationPeakOwnedBytes;
        long steadyStateOwnedBytes;

        /// <summary>
        /// Initializes one uninitialized session against the renderer that will create and release its texture.
        /// </summary>
        /// <param name="renderManager2D">Exact renderer instance that owns the presentation texture.</param>
        public SoftwarePathTraceSession(RenderManager2D renderManager2D) {
            this.renderManager2D = renderManager2D ?? throw new ArgumentNullException(nameof(renderManager2D));
            state = SoftwarePathTraceSessionState.Uninitialized;
        }

        /// <summary>Gets the current ownership stage.</summary>
        public SoftwarePathTraceSessionState State => state;

        /// <summary>Gets the renderer-owned presentation texture while tracing.</summary>
        public RuntimeTexture PresentationTexture => presentationTexture;

        /// <summary>Gets the compact scene while tracing.</summary>
        public SoftwareTraceScene Scene => scene;

        /// <summary>Gets the BVH while tracing.</summary>
        public SoftwareBvh Bvh => bvh;

        /// <summary>Gets the progressive tracer while tracing.</summary>
        public SoftwarePathTracer Tracer => tracer;

        /// <summary>Gets the stable failure message after a failed initialization or upload.</summary>
        public string FailureMessage => failureMessage;

        /// <summary>Gets the peak explicit owned bytes observed during initialization.</summary>
        public long InitializationPeakOwnedBytes => initializationPeakOwnedBytes;

        /// <summary>Gets the explicit owned bytes retained during tracing.</summary>
        public long SteadyStateOwnedBytes => steadyStateOwnedBytes;

        /// <summary>
        /// Performs synchronous staged initialization and rolls back every resource on any failure.
        /// </summary>
        /// <param name="sceneRoots">Authored roots to recursively flatten.</param>
        /// <param name="modelSource">Owned CPU-readable model source.</param>
        /// <param name="resolution">Exact output resolution.</param>
        /// <param name="camera">Validated authored camera basis.</param>
        /// <param name="exposure">Positive tone-mapping exposure.</param>
        /// <param name="allocator">Optional progressive allocation seam.</param>
        public void Initialize(
            IReadOnlyList<Entity> sceneRoots,
            ISoftwareModelAssetSource modelSource,
            SoftwareTraceResolution resolution,
            SoftwareTraceCamera camera,
            float exposure,
            ISoftwareTraceBufferAllocator allocator = null) {
            if (state != SoftwarePathTraceSessionState.Uninitialized) {
                throw new InvalidOperationException("Software path trace session initialization is permitted only once.");
            }

            long displayBytes = 0L;
            try {
                ValidateInitializationArguments(sceneRoots, modelSource, resolution, camera, exposure);
                displayBytes = checked((long)resolution.Width * resolution.Height * 4L);
                state = SoftwarePathTraceSessionState.CreatingPresentation;
                CreatePresentationTexture(resolution);
                ObservePeak(checked(displayBytes + displayBytes));
                ThrowIfDisposedDuringInitialization();

                state = SoftwarePathTraceSessionState.LoadingModels;
                scene = SoftwareTraceScene.Build(sceneRoots, modelSource);
                ObservePeak(checked(displayBytes + scene.InitializationPeakOwnedBytes));
                ThrowIfDisposedDuringInitialization();

                state = SoftwarePathTraceSessionState.BuildingBvh;
                bvh = SoftwareBvh.Build(scene.Triangles);
                long bvhBytes = checked(((long)bvh.Nodes.Length * 32L) + ((long)bvh.TriangleOrder.Length * 4L));
                ObservePeak(checked(displayBytes + scene.SteadyStateOwnedBytes + bvhBytes));
                ThrowIfDisposedDuringInitialization();

                traversalStack = new int[SoftwareBvh.TraversalStackCapacity];
                ObservePeak(checked(displayBytes + scene.SteadyStateOwnedBytes + bvhBytes + ((long)traversalStack.Length * 4L)));
                ThrowIfDisposedDuringInitialization();

                tracer = new SoftwarePathTracer(scene.Triangles, scene.Materials, scene.AreaLight, bvh, traversalStack);
                state = SoftwarePathTraceSessionState.AllocatingProgressiveBuffers;
                SoftwarePathTracer initializingTracer = tracer;
                initializingTracer.InitializeProgressive(resolution, camera, exposure, allocator);
                if (state == SoftwarePathTraceSessionState.Disposed) {
                    // Dispose may clear the session field while an allocator callback is in flight;
                    // reset the local tracer once its late buffers become reachable.
                    initializingTracer.DisposeProgressive();
                }
                ThrowIfDisposedDuringInitialization();

                steadyStateOwnedBytes = checked(displayBytes
                    + scene.SteadyStateOwnedBytes
                    + bvhBytes
                    + ((long)traversalStack.Length * 4L)
                    + tracer.ProgressiveOwnedBytes);
                ObservePeak(steadyStateOwnedBytes);
                state = SoftwarePathTraceSessionState.Tracing;
            }
            catch (Exception exception) {
                if (state == SoftwarePathTraceSessionState.Disposed) {
                    // A synchronous seam may dispose the session before the current stage returns
                    // its newly-created resource. Run one final rollback pass after that resource
                    // becomes reachable, while preserving Disposed and the original exception.
                    CleanupOwnedResources();
                    throw;
                }

                failureMessage = CreateFailureMessage(exception);
                CleanupOwnedResources();
                state = SoftwarePathTraceSessionState.Failed;
                throw new InvalidOperationException(failureMessage, exception);
            }
        }

        /// <summary>
        /// Renders and uploads exactly one edge-clipped tile from the persistent tile buffer.
        /// </summary>
        /// <returns>The uploaded tile rectangle.</returns>
        public SoftwareTraceTile RenderAndUploadNextTile() {
            if (state != SoftwarePathTraceSessionState.Tracing || tracer == null || presentationTexture == null) {
                throw new InvalidOperationException("Software path tracing is not in the tracing state.");
            }

            try {
                SoftwareTraceTile tile = tracer.RenderNextTile();
                renderManager2D.UpdateTextureRegion(
                    presentationTexture,
                    tile.X,
                    tile.Y,
                    tile.Width,
                    tile.Height,
                    tracer.TileRgba8,
                    tracer.TileRowPitch);
                return tile;
            }
            catch (Exception exception) {
                failureMessage = CreateFailureMessage(exception);
                CleanupOwnedResources();
                state = SoftwarePathTraceSessionState.Failed;
                throw new InvalidOperationException(failureMessage, exception);
            }
        }

        /// <summary>
        /// Releases progressive state, BVH arrays, and the presentation texture through its creator exactly once.
        /// </summary>
        public void Dispose() {
            if (state == SoftwarePathTraceSessionState.Disposed) {
                return;
            }

            CleanupOwnedResources();
            state = SoftwarePathTraceSessionState.Disposed;
        }

        /// <summary>
        /// Validates all non-engine initialization inputs before taking ownership of resources.
        /// </summary>
        static void ValidateInitializationArguments(
            IReadOnlyList<Entity> sceneRoots,
            ISoftwareModelAssetSource modelSource,
            SoftwareTraceResolution resolution,
            SoftwareTraceCamera camera,
            float exposure) {
            if (sceneRoots == null) {
                throw new ArgumentNullException(nameof(sceneRoots));
            }
            if (sceneRoots.Count == 0) {
                throw new ArgumentException("At least one scene root is required.", nameof(sceneRoots));
            }
            if (modelSource == null) {
                throw new ArgumentNullException(nameof(modelSource));
            }
            if (resolution.Width > ushort.MaxValue || resolution.Height > ushort.MaxValue) {
                throw new ArgumentOutOfRangeException(nameof(resolution), "Presentation texture dimensions exceed the raw texture limit.");
            }
            if (resolution.Width <= 0 || resolution.Height <= 0) {
                throw new ArgumentOutOfRangeException(nameof(resolution), "Trace dimensions must be positive.");
            }
            _ = new SoftwareTraceCamera(
                camera.Origin,
                camera.Forward,
                camera.Right,
                camera.Up,
                camera.VerticalFieldOfViewDegrees);
            if (!float.IsFinite(exposure) || exposure <= 0f) {
                throw new ArgumentOutOfRangeException(nameof(exposure), "Trace exposure must be finite and positive.");
            }
        }

        /// <summary>
        /// Creates the blank opaque RGBA8 presentation texture and immediately disposes its CPU asset.
        /// </summary>
        void CreatePresentationTexture(SoftwareTraceResolution resolution) {
            int byteCount = checked(resolution.Width * resolution.Height * 4);
            TextureAsset rawTexture = new TextureAsset {
                Width = (ushort)resolution.Width,
                Height = (ushort)resolution.Height,
                ColorFormat = TextureAssetColorFormat.Rgba32,
                AlphaPrecision = TextureAssetAlphaPrecision.A8,
                Colors = new byte[byteCount]
            };
            for (int pixel = 3; pixel < rawTexture.Colors.Length; pixel += 4) {
                rawTexture.Colors[pixel] = byte.MaxValue;
            }

            try {
                presentationTexture = renderManager2D.BuildTextureFromRaw(rawTexture);
                if (presentationTexture == null) {
                    throw new InvalidOperationException("The 2D renderer returned no presentation texture.");
                }
                if (presentationTexture.Width != resolution.Width || presentationTexture.Height != resolution.Height) {
                    throw new InvalidOperationException("The 2D renderer returned a presentation texture with unexpected dimensions.");
                }
            }
            finally {
                rawTexture.Dispose();
            }
        }

        /// <summary>
        /// Drops every owned field in the required reverse dependency order and suppresses duplicate release attempts.
        /// </summary>
        void CleanupOwnedResources() {
            steadyStateOwnedBytes = 0L;
            if (tracer != null) {
                tracer.DisposeProgressive();
            }
            if (bvh != null) {
                bvh.Dispose();
            }

            RuntimeTexture ownedTexture = presentationTexture;
            presentationTexture = null;
            if (ownedTexture != null && !presentationReleaseAttempted) {
                presentationReleaseAttempted = true;
                try {
                    renderManager2D.ReleaseTexture(ownedTexture);
                }
                catch {
                    // Cleanup is idempotent even when a backend release reports its own failure.
                }
            }

            tracer = null;
            traversalStack = null;
            bvh = null;
            scene = null;
            initializationPeakOwnedBytes = 0L;
        }

        /// <summary>Records the maximum explicit owned-byte estimate observed so far.</summary>
        void ObservePeak(long value) {
            if (value > initializationPeakOwnedBytes) {
                initializationPeakOwnedBytes = value;
            }
        }

        /// <summary>Rejects callbacks that dispose the session while staged initialization is still running.</summary>
        void ThrowIfDisposedDuringInitialization() {
            if (state == SoftwarePathTraceSessionState.Disposed) {
                throw new ObjectDisposedException(nameof(SoftwarePathTraceSession));
            }
        }

        /// <summary>Builds one stable non-empty diagnostic message from a stage failure.</summary>
        static string CreateFailureMessage(Exception exception) {
            string detail = exception == null ? string.Empty : exception.Message;
            return string.IsNullOrWhiteSpace(detail)
                ? "Software path tracing failed."
                : "Software path tracing failed: " + detail;
        }
    }

    /// <summary>
    /// Drives one authored DemoDisc software path tracing presentation.
    /// </summary>
    public sealed class SoftwarePathTracerComponent : UpdateComponent {
        /// <summary>Authored output sprite entity reference.</summary>
        public SceneEntityReference OutputSpriteEntityReference { get; set; }

        /// <summary>Authored SPP diagnostic text entity reference.</summary>
        public SceneEntityReference SppTextEntityReference { get; set; }

        /// <summary>Authored elapsed diagnostic text entity reference.</summary>
        public SceneEntityReference ElapsedTextEntityReference { get; set; }

        /// <summary>Authored rays-per-second diagnostic text entity reference.</summary>
        public SceneEntityReference RaysPerSecondTextEntityReference { get; set; }

        /// <summary>Authored world-space camera origin.</summary>
        public float3 TraceCameraOrigin { get; set; }

        /// <summary>Authored camera forward basis.</summary>
        public float3 TraceCameraForward { get; set; }

        /// <summary>Authored camera right basis.</summary>
        public float3 TraceCameraRight { get; set; }

        /// <summary>Authored camera up basis.</summary>
        public float3 TraceCameraUp { get; set; }

        /// <summary>Authored vertical camera field of view in degrees.</summary>
        public float VerticalFieldOfViewDegrees { get; set; }

        /// <summary>Authored positive CPU tone-mapping exposure.</summary>
        public float Exposure { get; set; }

        SpriteComponent outputSprite;
        TextComponent sppText;
        TextComponent elapsedText;
        TextComponent raysPerSecondText;
        SoftwarePathTraceSession session;
        string componentFailureMessage;
        double traceStartSeconds;
        double lastHudRefreshSeconds;
        int lastHudSpp = -1;
        SoftwarePathTraceSessionState lastHudState;
        bool lifecycleInitialized;
        bool returnRequested;
        bool componentDisposed;

        /// <summary>Current session state, or Failed when component initialization failed before a session was created.</summary>
        public SoftwarePathTraceSessionState SessionState => (componentDisposed || returnRequested)
            ? SoftwarePathTraceSessionState.Disposed
            : (componentFailureMessage != null
                ? SoftwarePathTraceSessionState.Failed
                : (session == null ? SoftwarePathTraceSessionState.Uninitialized : session.State));

        /// <summary>Completed progressive samples per pixel.</summary>
        public int CompletedSpp => session?.Tracer?.CompletedPasses ?? 0;

        /// <summary>Elapsed trace seconds measured from Core.TotalElapsedSeconds.</summary>
        public double ElapsedTraceSeconds {
            get {
                if (session == null || OwnerCore == null || SessionState == SoftwarePathTraceSessionState.Uninitialized) {
                    return 0d;
                }
                return ComputeElapsedTraceSeconds(OwnerCore.TotalElapsedSeconds, traceStartSeconds, SessionState);
            }
        }

        /// <summary>Total primary, bounce, and shadow rays launched.</summary>
        public long TotalRays => session?.Tracer?.RayCount ?? 0L;

        /// <summary>Total rays divided by finite positive elapsed trace seconds.</summary>
        public double RaysPerSecond => ElapsedTraceSeconds > 0d && double.IsFinite(ElapsedTraceSeconds)
            ? TotalRays / ElapsedTraceSeconds
            : 0d;

        /// <summary>Samples discarded because a non-finite intermediate value was observed.</summary>
        public long NonFiniteSampleCount => session?.Tracer?.NonFiniteSampleCount ?? 0L;

        /// <summary>Explicit initialization peak bytes, excluding runtime/backend allocator overhead.</summary>
        public long InitializationPeakOwnedBytes => session?.InitializationPeakOwnedBytes ?? 0L;

        /// <summary>Explicit steady-state bytes, excluding runtime/backend allocator overhead.</summary>
        public long SteadyStateOwnedBytes => session?.SteadyStateOwnedBytes ?? 0L;

        /// <summary>Whether Return has been consumed by this component lifetime.</summary>
        public bool ReturnRequested => returnRequested;

        /// <summary>Initializes deterministic safe camera and exposure defaults.</summary>
        public SoftwarePathTracerComponent() {
            TraceCameraOrigin = new float3(0f, 0f, 3f);
            TraceCameraForward = new float3(0f, 0f, -1f);
            TraceCameraRight = new float3(1f, 0f, 0f);
            TraceCameraUp = new float3(0f, 1f, 0f);
            VerticalFieldOfViewDegrees = 45f;
            Exposure = 1f;
            lastHudState = SoftwarePathTraceSessionState.Uninitialized;
        }

        /// <summary>Resets only local lifecycle flags while the authored hierarchy is still being attached.</summary>
        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);
            lifecycleInitialized = false;
            returnRequested = false;
            componentDisposed = false;
            componentFailureMessage = null;
            session = null;
            outputSprite = null;
            sppText = null;
            elapsedText = null;
            raysPerSecondText = null;
            lastHudSpp = -1;
            lastHudRefreshSeconds = 0d;
        }

        /// <summary>
        /// Resolves authored references and performs one synchronous staged initialization after hierarchy materialization.
        /// </summary>
        public override void ComponentInitialized(Entity entity) {
            if (lifecycleInitialized) {
                return;
            }

            lifecycleInitialized = true;
            try {
                ValidateComponentSettings();
                outputSprite = FindRequiredComponent<SpriteComponent>(OutputSpriteEntityReference, "output sprite", out _);
                sppText = FindRequiredComponent<TextComponent>(SppTextEntityReference, "SPP text", out _);
                elapsedText = FindRequiredComponent<TextComponent>(ElapsedTextEntityReference, "elapsed text", out _);
                raysPerSecondText = FindRequiredComponent<TextComponent>(RaysPerSecondTextEntityReference, "rays-per-second text", out _);
                session = new SoftwarePathTraceSession(OwnerCore.RenderManager2D);
                SoftwareTraceResolution resolution = SoftwareTraceResolution.ForPlatform(OwnerCore.PlatformInfo.Name);
                SoftwareTraceCamera camera = new SoftwareTraceCamera(
                    TraceCameraOrigin,
                    TraceCameraForward,
                    TraceCameraRight,
                    TraceCameraUp,
                    VerticalFieldOfViewDegrees);
                session.Initialize(
                    new[] { Parent },
                    new ContentSoftwareModelAssetSource(OwnerCore.ContentManager),
                    resolution,
                    camera,
                    Exposure);
                outputSprite.Texture = session.PresentationTexture;
                traceStartSeconds = OwnerCore.TotalElapsedSeconds;
                lastHudRefreshSeconds = 0d;
                lastHudSpp = -1;
                RefreshHud(true);
            }
            catch (Exception exception) {
                componentFailureMessage = string.IsNullOrWhiteSpace(session?.FailureMessage)
                    ? CreateComponentFailureMessage(exception)
                    : session.FailureMessage;
                if (outputSprite != null) {
                    outputSprite.Texture = null;
                }
                if (session != null) {
                    session.Dispose();
                    session = null;
                }
                if (sppText != null) {
                    sppText.Text = "Trace error: " + componentFailureMessage;
                }
            }

            base.ComponentInitialized(entity);
        }

        /// <summary>Renders at most one tile and upload per update, with platform-specific Return handling.</summary>
        public override void Update() {
            base.Update();
            if (SessionState != SoftwarePathTraceSessionState.Tracing || session == null) {
                RefreshHud(false);
                return;
            }

            if (ShouldPollControllerReturn(OwnerCore.PlatformInfo.Name) && DemoDiscReturnInputUtils.WasReturnPressed(OwnerCore.Input)) {
                RequestReturnToMainMenu();
                return;
            }

            try {
                session.RenderAndUploadNextTile();
            }
            catch (Exception exception) {
                componentFailureMessage = session.FailureMessage ?? CreateComponentFailureMessage(exception);
                if (outputSprite != null) {
                    outputSprite.Texture = null;
                }
            }
            RefreshHud(false);
        }

        /// <summary>Clears UI texture and disposes session without requesting a scene transition.</summary>
        public override void ComponentRemoved(Entity entity) {
            ClearOutputTextureAndDisposeSession();
            base.ComponentRemoved(entity);
        }

        /// <summary>Releases session resources and calls the base component disposer once.</summary>
        public override void Dispose() {
            if (componentDisposed) {
                return;
            }

            componentDisposed = true;
            try {
                ClearOutputTextureAndDisposeSession();
            }
            finally {
                base.Dispose();
            }
        }

        /// <summary>Formats one visible SPP HUD value.</summary>
        public static string FormatSpp(int completedSpp) {
            return "SPP: " + Math.Max(0, completedSpp);
        }

        /// <summary>Formats one visible elapsed-time HUD value.</summary>
        public static string FormatElapsed(double elapsedSeconds) {
            return "Time: " + FormatFixedNonnegative(elapsedSeconds, 2) + "s";
        }

        /// <summary>Formats one visible rays-per-second HUD value.</summary>
        public static string FormatRaysPerSecond(double raysPerSecond) {
            return "Rays/s: " + FormatFixedNonnegative(raysPerSecond, 1);
        }

        /// <summary>Formats one nonnegative fixed-point value without culture-sensitive composite formatting.</summary>
        static string FormatFixedNonnegative(double value, int decimalPlaces) {
            if (!double.IsFinite(value) || value <= 0d) {
                value = 0d;
            }

            double scale = decimalPlaces == 2 ? 100d : 10d;
            double maximum = int.MaxValue / scale;
            value = Math.Min(value, maximum);
            int scaled = (int)Math.Round(value * scale, MidpointRounding.AwayFromZero);
            int whole = scaled / (int)scale;
            int fractional = Math.Abs(scaled % (int)scale);
            if (decimalPlaces == 2) {
                return whole + "." + (fractional < 10 ? "0" : string.Empty) + fractional;
            }

            return whole + "." + fractional;
        }

        /// <summary>Computes relative trace time without treating an origin timestamp of zero as uninitialized.</summary>
        public static double ComputeElapsedTraceSeconds(double totalElapsedSeconds, double traceStartSeconds, SoftwarePathTraceSessionState state) {
            if (state == SoftwarePathTraceSessionState.Uninitialized || state == SoftwarePathTraceSessionState.Disposed) {
                return 0d;
            }
            if (!double.IsFinite(totalElapsedSeconds) || !double.IsFinite(traceStartSeconds)) {
                return 0d;
            }
            return Math.Max(0d, totalElapsedSeconds - traceStartSeconds);
        }

        /// <summary>Returns whether one HUD refresh is due under the four-refreshes-per-trace-second limit.</summary>
        public static bool ShouldRefreshHud(double elapsedSeconds, int completedSpp, SoftwarePathTraceSessionState state, double lastRefreshSeconds, int lastSpp, SoftwarePathTraceSessionState lastState) {
            return state != lastState || completedSpp != lastSpp || elapsedSeconds - lastRefreshSeconds >= 0.25d;
        }

        /// <summary>Returns whether the desktop controller return path should be polled.</summary>
        public static bool ShouldPollControllerReturn(string platformName) {
            return !string.Equals(platformName, "ds", StringComparison.Ordinal)
                && !string.Equals(platformName, "3ds", StringComparison.Ordinal);
        }

        /// <summary>Validates references and authored camera/exposure values before engine access.</summary>
        void ValidateComponentSettings() {
            if (Parent == null || OwnerCore == null || OwnerCore.ObjectManager == null || OwnerCore.RenderManager2D == null) {
                throw new InvalidOperationException("Software path tracer requires an attached initialized core hierarchy.");
            }
            ValidateReference(OutputSpriteEntityReference, "output sprite");
            ValidateReference(SppTextEntityReference, "SPP text");
            ValidateReference(ElapsedTextEntityReference, "elapsed text");
            ValidateReference(RaysPerSecondTextEntityReference, "rays-per-second text");
            if (!float.IsFinite(Exposure) || Exposure <= 0f) {
                throw new InvalidOperationException("Software path tracer exposure must be finite and positive.");
            }
        }

        /// <summary>Resolves exactly one entity/component matching a stable runtime scene id.</summary>
        T FindRequiredComponent<T>(SceneEntityReference entityReference, string description, out Entity resolvedEntity) where T : Component {
            resolvedEntity = null;
            int matches = 0;
            T resolvedComponent = null;
            List<Entity> entities = OwnerCore.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                Entity candidate = entities[entityIndex];
                if (FindSceneEntityRuntimeIdOrZero(candidate) != entityReference.EntityId) {
                    continue;
                }

                matches++;
                resolvedEntity = candidate;
                if (candidate.Components != null) {
                    for (int componentIndex = 0; componentIndex < candidate.Components.Count; componentIndex++) {
                        if (candidate.Components[componentIndex] is T component) {
                            if (resolvedComponent != null) {
                                throw new InvalidOperationException($"Software path tracer {description} entity contains duplicate required components.");
                            }
                            resolvedComponent = component;
                        }
                    }
                }
            }

            if (matches == 0) {
                throw new InvalidOperationException($"Software path tracer could not resolve {description} scene entity id {entityReference.EntityId}.");
            }
            if (matches > 1) {
                throw new InvalidOperationException($"Software path tracer {description} scene entity id {entityReference.EntityId} is duplicated.");
            }
            if (resolvedComponent == null) {
                throw new InvalidOperationException($"Software path tracer {description} entity must contain the required component.");
            }
            return resolvedComponent;
        }

        /// <summary>Validates a non-null, non-zero serialized entity reference.</summary>
        static void ValidateReference(SceneEntityReference entityReference, string description) {
            if (entityReference == null || entityReference.EntityId == 0u) {
                throw new InvalidOperationException($"Software path tracer requires a non-zero {description} entity reference.");
            }
        }

        /// <summary>Returns one candidate's stable runtime scene id, or zero when absent.</summary>
        static uint FindSceneEntityRuntimeIdOrZero(Entity entity) {
            if (entity == null || entity.Components == null) {
                return 0u;
            }
            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                if (entity.Components[componentIndex] is SceneEntityRuntimeIdComponent runtimeIdComponent) {
                    return runtimeIdComponent.SceneEntityId;
                }
            }
            return 0u;
        }

        /// <summary>Refreshes the three diagnostics only when a throttled or immediate refresh is due.</summary>
        void RefreshHud(bool force) {
            if (sppText == null || elapsedText == null || raysPerSecondText == null) {
                return;
            }

            double elapsed = ElapsedTraceSeconds;
            int spp = CompletedSpp;
            SoftwarePathTraceSessionState currentState = SessionState;
            if (!force && !ShouldRefreshHud(elapsed, spp, currentState, lastHudRefreshSeconds, lastHudSpp, lastHudState)) {
                return;
            }
            if (currentState == SoftwarePathTraceSessionState.Failed) {
                sppText.Text = "Trace error: " + (session?.FailureMessage ?? componentFailureMessage ?? "Software path tracing failed.");
            }
            else {
                sppText.Text = FormatSpp(spp);
            }
            elapsedText.Text = FormatElapsed(elapsed);
            raysPerSecondText.Text = FormatRaysPerSecond(RaysPerSecond);
            lastHudRefreshSeconds = elapsed;
            lastHudSpp = spp;
            lastHudState = currentState;
        }

        /// <summary>Clears the output sprite first, then releases all session-owned resources.</summary>
        void ClearOutputTextureAndDisposeSession() {
            if (outputSprite != null) {
                outputSprite.Texture = null;
            }
            if (session != null) {
                session.Dispose();
                session = null;
            }
            outputSprite = null;
            sppText = null;
            elapsedText = null;
            raysPerSecondText = null;
        }

        /// <summary>Consumes Return once, clears the output, and resolves the DemoDisc main menu transition once.</summary>
        void RequestReturnToMainMenu() {
            if (returnRequested) {
                return;
            }
            returnRequested = true;
            ClearOutputTextureAndDisposeSession();
            string sceneId = DemoDiscMainMenuSceneResolver.ResolveRuntimeSceneId();
            OwnerCore.SceneManager.RequestSceneTransition(sceneId);
        }

        /// <summary>Builds one stable component-side initialization error.</summary>
        static string CreateComponentFailureMessage(Exception exception) {
            string detail = exception == null ? string.Empty : exception.Message;
            return string.IsNullOrWhiteSpace(detail)
                ? "Software path tracing failed."
                : "Software path tracing failed: " + detail;
        }
    }
}
