using helengine.editor;

namespace city.rendering.tools {
    /// <summary>
    /// Writes serialized scene component records for generated city rendering showcase runtime components.
    /// </summary>
    public static class RenderingScriptComponentRecordFactory {
        /// <summary>
        /// Stable serialized component type id for the tower-spin runtime component.
        /// </summary>
        const string TowerSpinTypeId = "helengine.DirectionalShadowTowerSpinComponent";

        /// <summary>
        /// Stable serialized component type id for the axis-test Z-spin runtime component.
        /// </summary>
        const string AxisTestZSpinTypeId = "gameplay.rendering.AxisTestZSpinComponent, gameplay";

        /// <summary>
        /// Stable serialized component type id for the axis-test-2 camera-forward spin runtime component.
        /// </summary>
        const string AxisTestCameraForwardSpinTypeId = "gameplay.rendering.AxisTestCameraForwardSpinComponent, gameplay";

        /// <summary>
        /// Stable serialized component type id for the orbit runtime component.
        /// </summary>
        const string OrbitTypeId = "gameplay.rendering.DirectionalShadowOrbitComponent, gameplay";

        /// <summary>
        /// Stable serialized component type id for the sun-sweep runtime component.
        /// </summary>
        const string SunSweepTypeId = "gameplay.rendering.DirectionalShadowSunSweepComponent, gameplay";

        /// <summary>
        /// Stable serialized component type id for the camera-orbit runtime component.
        /// </summary>
        const string CameraOrbitTypeId = "gameplay.rendering.DirectionalShadowCameraOrbitComponent, gameplay";

        /// <summary>
        /// Creates one serialized tower-spin component record.
        /// </summary>
        /// <param name="componentIndex">Entity-local component index.</param>
        /// <param name="baseYawRadians">Base yaw offset in radians.</param>
        /// <param name="angularSpeedRadians">Angular speed in radians per second.</param>
        /// <returns>Serialized scene component record for the tower-spin component.</returns>
        public static SceneComponentAssetRecord CreateTowerSpinRecord(int componentIndex, float baseYawRadians, float angularSpeedRadians) {
            if (componentIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(componentIndex), "Component index must be non-negative.");
            }

            using MemoryStream stream = new MemoryStream();
            using EngineBinaryWriter writer = EngineBinaryWriter.Create(stream, EngineBinaryEndianness.LittleEndian);
            writer.WriteByte(DirectionalShadowMotionComponentScenePayloadSerializer.CurrentVersion);
            DirectionalShadowMotionComponentScenePayloadSerializer.WriteTowerSpin(writer, new DirectionalShadowTowerSpinComponent {
                BaseYawRadians = baseYawRadians,
                AngularSpeedRadians = angularSpeedRadians
            });
            return new SceneComponentAssetRecord {
                ComponentTypeId = TowerSpinTypeId,
                ComponentIndex = componentIndex,
                Payload = stream.ToArray()
            };
        }

        /// <summary>
        /// Creates one serialized axis-test Z-spin component record.
        /// </summary>
        /// <param name="componentIndex">Entity-local component index.</param>
        /// <param name="baseRollRadians">Base roll offset in radians.</param>
        /// <param name="angularSpeedRadians">Angular speed in radians per second.</param>
        /// <returns>Serialized scene component record for the axis-test Z-spin component.</returns>
        public static SceneComponentAssetRecord CreateAxisTestZSpinRecord(int componentIndex, float baseRollRadians, float angularSpeedRadians) {
            if (componentIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(componentIndex), "Component index must be non-negative.");
            }

            EditorTaggedSceneComponentFieldWriter writer = new EditorTaggedSceneComponentFieldWriter();
            writer.WriteField("AngularSpeedRadians", fieldWriter => fieldWriter.WriteSingle(angularSpeedRadians));
            writer.WriteField("BaseRollRadians", fieldWriter => fieldWriter.WriteSingle(baseRollRadians));
            return new SceneComponentAssetRecord {
                ComponentTypeId = AxisTestZSpinTypeId,
                ComponentIndex = componentIndex,
                Payload = writer.BuildPayload()
            };
        }

        /// <summary>
        /// Creates one serialized axis-test-2 camera-forward spin component record.
        /// </summary>
        /// <param name="componentIndex">Entity-local component index.</param>
        /// <param name="baseAngleRadians">Base angle offset in radians.</param>
        /// <param name="angularSpeedRadians">Angular speed in radians per second.</param>
        /// <param name="cameraForwardAxisX">Camera-forward axis X component.</param>
        /// <param name="cameraForwardAxisY">Camera-forward axis Y component.</param>
        /// <param name="cameraForwardAxisZ">Camera-forward axis Z component.</param>
        /// <returns>Serialized scene component record for the axis-test-2 camera-forward spin component.</returns>
        public static SceneComponentAssetRecord CreateAxisTestCameraForwardSpinRecord(
            int componentIndex,
            float baseAngleRadians,
            float angularSpeedRadians,
            float cameraForwardAxisX,
            float cameraForwardAxisY,
            float cameraForwardAxisZ) {
            if (componentIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(componentIndex), "Component index must be non-negative.");
            }

            EditorTaggedSceneComponentFieldWriter writer = new EditorTaggedSceneComponentFieldWriter();
            writer.WriteField("AngularSpeedRadians", fieldWriter => fieldWriter.WriteSingle(angularSpeedRadians));
            writer.WriteField("BaseAngleRadians", fieldWriter => fieldWriter.WriteSingle(baseAngleRadians));
            writer.WriteField("CameraForwardAxisX", fieldWriter => fieldWriter.WriteSingle(cameraForwardAxisX));
            writer.WriteField("CameraForwardAxisY", fieldWriter => fieldWriter.WriteSingle(cameraForwardAxisY));
            writer.WriteField("CameraForwardAxisZ", fieldWriter => fieldWriter.WriteSingle(cameraForwardAxisZ));
            return new SceneComponentAssetRecord {
                ComponentTypeId = AxisTestCameraForwardSpinTypeId,
                ComponentIndex = componentIndex,
                Payload = writer.BuildPayload()
            };
        }

        /// <summary>
        /// Creates one serialized orbit component record.
        /// </summary>
        /// <param name="componentIndex">Entity-local component index.</param>
        /// <param name="orbitCenter">World-space orbit center.</param>
        /// <param name="orbitRadius">Orbit radius in world units.</param>
        /// <param name="orbitHeight">Vertical orbit offset relative to the center.</param>
        /// <param name="baseAngleRadians">Base orbit angle in radians.</param>
        /// <param name="angularSpeedRadians">Angular speed in radians per second.</param>
        /// <returns>Serialized scene component record for the orbit component.</returns>
        public static SceneComponentAssetRecord CreateOrbitRecord(
            int componentIndex,
            float3 orbitCenter,
            float orbitRadius,
            float orbitHeight,
            float baseAngleRadians,
            float angularSpeedRadians) {
            if (componentIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(componentIndex), "Component index must be non-negative.");
            } else if (orbitRadius <= 0f) {
                throw new ArgumentOutOfRangeException(nameof(orbitRadius), "Orbit radius must be greater than zero.");
            }

            EditorTaggedSceneComponentFieldWriter writer = new EditorTaggedSceneComponentFieldWriter();
            writer.WriteField("AngularSpeedRadians", fieldWriter => fieldWriter.WriteSingle(angularSpeedRadians));
            writer.WriteField("BaseAngleRadians", fieldWriter => fieldWriter.WriteSingle(baseAngleRadians));
            writer.WriteField("OrbitCenter", fieldWriter => fieldWriter.WriteFloat3(orbitCenter));
            writer.WriteField("OrbitHeight", fieldWriter => fieldWriter.WriteSingle(orbitHeight));
            writer.WriteField("OrbitRadius", fieldWriter => fieldWriter.WriteSingle(orbitRadius));
            return new SceneComponentAssetRecord {
                ComponentTypeId = OrbitTypeId,
                ComponentIndex = componentIndex,
                Payload = writer.BuildPayload()
            };
        }

        /// <summary>
        /// Creates one serialized sun-sweep component record.
        /// </summary>
        /// <param name="componentIndex">Entity-local component index.</param>
        /// <param name="minYawRadians">Minimum authored yaw in radians.</param>
        /// <param name="maxYawRadians">Maximum authored yaw in radians.</param>
        /// <param name="pitchRadians">Fixed pitch in radians.</param>
        /// <param name="sweepSpeedRadians">Sweep rate in radians per second.</param>
        /// <returns>Serialized scene component record for the sun-sweep component.</returns>
        public static SceneComponentAssetRecord CreateSunSweepRecord(
            int componentIndex,
            float minYawRadians,
            float maxYawRadians,
            float pitchRadians,
            float sweepSpeedRadians) {
            if (componentIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(componentIndex), "Component index must be non-negative.");
            }

            EditorTaggedSceneComponentFieldWriter writer = new EditorTaggedSceneComponentFieldWriter();
            writer.WriteField("MaxYawRadians", fieldWriter => fieldWriter.WriteSingle(maxYawRadians));
            writer.WriteField("MinYawRadians", fieldWriter => fieldWriter.WriteSingle(minYawRadians));
            writer.WriteField("PitchRadians", fieldWriter => fieldWriter.WriteSingle(pitchRadians));
            writer.WriteField("SweepSpeedRadians", fieldWriter => fieldWriter.WriteSingle(sweepSpeedRadians));
            return new SceneComponentAssetRecord {
                ComponentTypeId = SunSweepTypeId,
                ComponentIndex = componentIndex,
                Payload = writer.BuildPayload()
            };
        }

        /// <summary>
        /// Creates one serialized camera-orbit component record.
        /// </summary>
        /// <param name="componentIndex">Entity-local component index.</param>
        /// <param name="orbitCenter">World-space orbit center.</param>
        /// <param name="orbitRadius">Orbit radius in world units.</param>
        /// <param name="orbitHeight">Vertical orbit offset relative to the center.</param>
        /// <param name="baseAngleRadians">Base orbit angle in radians.</param>
        /// <param name="angularSpeedRadians">Angular speed in radians per second.</param>
        /// <param name="lookDownPitchRadians">Fixed downward pitch in radians.</param>
        /// <returns>Serialized scene component record for the camera-orbit component.</returns>
        public static SceneComponentAssetRecord CreateCameraOrbitRecord(
            int componentIndex,
            float3 orbitCenter,
            float orbitRadius,
            float orbitHeight,
            float baseAngleRadians,
            float angularSpeedRadians,
            float lookDownPitchRadians) {
            if (componentIndex < 0) {
                throw new ArgumentOutOfRangeException(nameof(componentIndex), "Component index must be non-negative.");
            } else if (orbitRadius <= 0f) {
                throw new ArgumentOutOfRangeException(nameof(orbitRadius), "Orbit radius must be greater than zero.");
            }

            EditorTaggedSceneComponentFieldWriter writer = new EditorTaggedSceneComponentFieldWriter();
            writer.WriteField("AngularSpeedRadians", fieldWriter => fieldWriter.WriteSingle(angularSpeedRadians));
            writer.WriteField("BaseAngleRadians", fieldWriter => fieldWriter.WriteSingle(baseAngleRadians));
            writer.WriteField("LookDownPitchRadians", fieldWriter => fieldWriter.WriteSingle(lookDownPitchRadians));
            writer.WriteField("OrbitCenter", fieldWriter => fieldWriter.WriteFloat3(orbitCenter));
            writer.WriteField("OrbitHeight", fieldWriter => fieldWriter.WriteSingle(orbitHeight));
            writer.WriteField("OrbitRadius", fieldWriter => fieldWriter.WriteSingle(orbitRadius));
            return new SceneComponentAssetRecord {
                ComponentTypeId = CameraOrbitTypeId,
                ComponentIndex = componentIndex,
                Payload = writer.BuildPayload()
            };
        }

    }
}
