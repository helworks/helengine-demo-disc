using System.Globalization;
using helengine;

namespace DemoDisc.rendering {
    /// <summary>
    /// Adds one cube per second until a one-second frame-rate sample reaches the configured target.
    /// </summary>
    public sealed class MaxObjectsBenchmarkComponent : UpdateComponent {
        /// <summary>
        /// Number of cubes placed in each row before a new row begins.
        /// </summary>
        public const int CubesPerRow = 17;

        /// <summary>
        /// Distance between neighboring cube centers in world units.
        /// </summary>
        public const float CubeSpacing = 0.55f;

        const string InitialStatusText = "Objects: 1    Avg FPS: measuring    Target: 30\nAdding 1 cube/sec";

        /// <summary>
        /// Gets or sets the frame-rate threshold that stops cube creation.
        /// </summary>
        public float TargetFramesPerSecond { get; set; } = 30f;

        /// <summary>
        /// Gets or sets the interval between newly created cubes, in seconds.
        /// </summary>
        public double SpawnIntervalSeconds { get; set; } = 1d;

        /// <summary>
        /// Gets the number of cubes currently in the benchmark.
        /// </summary>
        public int ObjectCount { get; private set; }

        /// <summary>
        /// Gets the latest one-second average frame rate sample.
        /// </summary>
        public double MeasuredFramesPerSecond { get; private set; }

        /// <summary>
        /// Gets whether the benchmark is still adding cubes.
        /// </summary>
        public bool IsSpawning { get; private set; }

        RuntimeModel CubeModel;
        RuntimeMaterial[] CubeMaterials;
        CameraComponent BenchmarkCamera;
        TextComponent StatusText;
        double SampleElapsedSeconds;
        double SpawnElapsedSeconds;
        int SampleFrameCount;

        /// <summary>
        /// Captures the authored first cube and resets benchmark counters when the scene attaches this component.
        /// </summary>
        /// <param name="entity">The benchmark UI entity, which also renders the initial cube.</param>
        public override void ComponentInitialized(Entity entity) {
            base.ComponentInitialized(entity);
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            if (!IsFinite(TargetFramesPerSecond) || TargetFramesPerSecond <= 0f) {
                throw new InvalidOperationException("The Max Objects benchmark requires a positive finite FPS target.");
            } else if (double.IsNaN(SpawnIntervalSeconds) || double.IsInfinity(SpawnIntervalSeconds) || SpawnIntervalSeconds <= 0d) {
                throw new InvalidOperationException("The Max Objects benchmark requires a positive finite cube interval.");
            }

            MeshComponent initialCube = FindInitialCube(entity);
            CubeModel = initialCube.Model;
            CubeMaterials = initialCube.Materials;
            BenchmarkCamera = FindBenchmarkCamera(entity);
            StatusText = FindStatusText(entity);
            if (BenchmarkCamera == null) {
                throw new InvalidOperationException("The Max Objects benchmark requires its generated camera entity.");
            }

            ObjectCount = 1;
            MeasuredFramesPerSecond = 0d;
            IsSpawning = true;
            SampleElapsedSeconds = 0d;
            SpawnElapsedSeconds = 0d;
            SampleFrameCount = 0;
            UpdateCameraFraming();
            UpdateStatus();
        }

        /// <summary>
        /// Samples frame rate once per second and adds cubes on one-second intervals until the target is reached.
        /// </summary>
        public override void Update() {
            base.Update();
            if (!IsSpawning) {
                return;
            }

            Core core = Core.Instance;
            if (core == null) {
                return;
            }

            double frameDeltaSeconds = core.FrameDeltaSeconds;
            if (double.IsNaN(frameDeltaSeconds) || double.IsInfinity(frameDeltaSeconds) || frameDeltaSeconds <= 0d) {
                return;
            }

            SampleElapsedSeconds += frameDeltaSeconds;
            SampleFrameCount++;
            SpawnElapsedSeconds += frameDeltaSeconds;

            if (SampleElapsedSeconds >= 1d) {
                MeasuredFramesPerSecond = ResolveFramesPerSecond(SampleFrameCount, SampleElapsedSeconds);
                SampleElapsedSeconds = 0d;
                SampleFrameCount = 0;

                if (HasReachedTarget(MeasuredFramesPerSecond, TargetFramesPerSecond)) {
                    IsSpawning = false;
                    UpdateStatus();
                    return;
                }
            }

            if (SpawnElapsedSeconds >= SpawnIntervalSeconds) {
                SpawnElapsedSeconds -= SpawnIntervalSeconds;
                SpawnCube();
                ObjectCount++;
                UpdateCameraFraming();
                UpdateStatus();
            }
        }

        /// <summary>
        /// Calculates a frame-rate sample for a measured frame count and elapsed interval.
        /// </summary>
        /// <param name="frameCount">Number of frames in the sample.</param>
        /// <param name="elapsedSeconds">Elapsed sample duration in seconds.</param>
        /// <returns>Average frames per second in the sample.</returns>
        public static double ResolveFramesPerSecond(int frameCount, double elapsedSeconds) {
            if (frameCount < 0) {
                throw new ArgumentOutOfRangeException(nameof(frameCount), "Frame count cannot be negative.");
            } else if (double.IsNaN(elapsedSeconds) || double.IsInfinity(elapsedSeconds) || elapsedSeconds <= 0d) {
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds), "Sample duration must be positive and finite.");
            }

            return frameCount / elapsedSeconds;
        }

        /// <summary>
        /// Determines whether a measured frame rate has reached or fallen below the stop threshold.
        /// </summary>
        /// <param name="measuredFramesPerSecond">Measured average frame rate.</param>
        /// <param name="targetFramesPerSecond">Configured stop threshold.</param>
        /// <returns>True when spawning should stop.</returns>
        public static bool HasReachedTarget(double measuredFramesPerSecond, double targetFramesPerSecond) {
            if (double.IsNaN(measuredFramesPerSecond) || double.IsInfinity(measuredFramesPerSecond) || measuredFramesPerSecond < 0d) {
                throw new ArgumentOutOfRangeException(nameof(measuredFramesPerSecond), "Measured frame rate must be finite and non-negative.");
            } else if (double.IsNaN(targetFramesPerSecond) || double.IsInfinity(targetFramesPerSecond) || targetFramesPerSecond <= 0d) {
                throw new ArgumentOutOfRangeException(nameof(targetFramesPerSecond), "Target frame rate must be positive and finite.");
            }

            return measuredFramesPerSecond <= targetFramesPerSecond;
        }

        MeshComponent FindInitialCube(Entity entity) {
            List<Component> components = entity.Components;
            if (components == null) {
                throw new InvalidOperationException("The Max Objects benchmark entity must have initialized components.");
            }

            for (int index = 0; index < components.Count; index++) {
                if (components[index] is MeshComponent meshComponent) {
                    if (meshComponent.Model == null || meshComponent.Materials == null || meshComponent.Materials.Length == 0) {
                        throw new InvalidOperationException("The Max Objects benchmark requires an initial cube with a model and at least one material.");
                    }

                    return meshComponent;
                }
            }

            throw new InvalidOperationException("The Max Objects benchmark entity must render its initial cube with a MeshComponent.");
        }

        TextComponent FindStatusText(Entity entity) {
            List<Entity> children = entity.Children;
            if (children == null) {
                throw new InvalidOperationException("The Max Objects benchmark entity must have an initialized child collection.");
            }

            for (int childIndex = 0; childIndex < children.Count; childIndex++) {
                Entity child = children[childIndex];
                if (child.Components == null) {
                    continue;
                }

                for (int componentIndex = 0; componentIndex < child.Components.Count; componentIndex++) {
                    if (child.Components[componentIndex] is TextComponent textComponent
                        && string.Equals(textComponent.Text, InitialStatusText, StringComparison.Ordinal)) {
                        return textComponent;
                    }
                }
            }

            throw new InvalidOperationException("The Max Objects benchmark requires its generated status text entity.");
        }

        CameraComponent FindBenchmarkCamera(Entity entity) {
            List<Entity> children = entity.Children;
            if (children == null) {
                return null;
            }

            for (int childIndex = 0; childIndex < children.Count; childIndex++) {
                List<Component> components = children[childIndex].Components;
                if (components == null) {
                    continue;
                }

                for (int componentIndex = 0; componentIndex < components.Count; componentIndex++) {
                    if (components[componentIndex] is CameraComponent cameraComponent) {
                        return cameraComponent;
                    }
                }
            }

            return null;
        }

        void SpawnCube() {
            Core core = Core.Instance;
            if (core == null || core.EntityFactory == null || Parent == null) {
                return;
            }

            if (Parent.Children == null) {
                Parent.InitChildren();
            }

            int nextObjectIndex = ObjectCount;
            int row = nextObjectIndex / CubesPerRow;
            int rowPosition = nextObjectIndex % CubesPerRow;
            int column = ResolveCenteredColumn(rowPosition);
            Entity cube = core.EntityFactory.Create("MaxObjectsCube" + nextObjectIndex.ToString("D6", CultureInfo.InvariantCulture));
            cube.InitComponents();
            cube.LayerMask = Parent.LayerMask;
            cube.LocalPosition = new float3((column - (CubesPerRow / 2)) * CubeSpacing, row * CubeSpacing, 0f);
            cube.LocalScale = new float3(0.5f, 0.5f, 0.5f);
            cube.LocalOrientation = float4.Identity;
            cube.AddComponent(new MeshComponent {
                Model = CubeModel,
                Materials = CubeMaterials,
                RenderOrder3D = 0
            });
            Parent.AddChild(cube);
        }

        void UpdateStatus() {
            if (StatusText == null) {
                return;
            }

            string fpsText = MeasuredFramesPerSecond > 0d
                ? MeasuredFramesPerSecond.ToString("F1", CultureInfo.InvariantCulture)
                : "measuring";
            string stateText = IsSpawning ? "Adding 1 cube/sec" : "Stopped at target";
            StatusText.Text = "Objects: " + ObjectCount.ToString(CultureInfo.InvariantCulture)
                + "    Avg FPS: " + fpsText
                + "    Target: " + TargetFramesPerSecond.ToString("F0", CultureInfo.InvariantCulture)
                + "\n" + stateText;
        }

        void UpdateCameraFraming() {
            if (BenchmarkCamera == null) {
                return;
            }

            int rowCount = (ObjectCount + CubesPerRow - 1) / CubesPerRow;
            float gridHeight = Math.Max(0, rowCount - 1) * CubeSpacing;
            float halfHeight = (gridHeight * 0.5f) + 1.5f;
            float tangent = (float)Math.Tan(BenchmarkCamera.FieldOfView * 0.5f);
            float cameraDistance = tangent > 0f ? halfHeight / tangent : 18f;
            BenchmarkCamera.Parent.LocalPosition = new float3(0f, gridHeight * 0.5f, Math.Max(18f, cameraDistance));
        }

        static int ResolveCenteredColumn(int rowPosition) {
            if (rowPosition == 0) {
                return CubesPerRow / 2;
            }

            int offset = (rowPosition + 1) / 2;
            return rowPosition % 2 == 1
                ? (CubesPerRow / 2) - offset
                : (CubesPerRow / 2) + offset;
        }

        static bool IsFinite(float value) {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
