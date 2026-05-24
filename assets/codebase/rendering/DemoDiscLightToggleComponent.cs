namespace city.rendering {
    /// <summary>
    /// Toggles authored directional lights on or off and publishes the matching controls text through the existing FPS or debug overlay.
    /// </summary>
    public sealed class DemoDiscLightToggleComponent : UpdateComponent {
        /// <summary>
        /// Stable debug-line id used for the light toggle status row.
        /// </summary>
        const string DebugLightStatusLineId = "demo-disc-light-toggle-status";

        /// <summary>
        /// Stable debug-line id used for the camera controls row.
        /// </summary>
        const string DebugCameraControlsLineId = "demo-disc-camera-controls";

        /// <summary>
        /// Shared camera-controls label rendered by every supported scene overlay.
        /// </summary>
        const string CameraControlsText = "Camera: WASD / DPad / Stick";

        /// <summary>
        /// Cached authored directional lights controlled by this component.
        /// </summary>
        readonly List<DemoDiscDirectionalLightToggleState> LightStates;

        /// <summary>
        /// FPS overlay hosted on the same entity when the scene uses the FPS diagnostics path.
        /// </summary>
        FPSComponent FpsComponentValue;

        /// <summary>
        /// Debug overlay hosted on the same entity when the scene uses the debug diagnostics path.
        /// </summary>
        DebugComponent DebugComponentValue;

        /// <summary>
        /// Tracks whether the authored directional lights are currently enabled.
        /// </summary>
        bool LightsEnabled;

        /// <summary>
        /// Initializes one light-toggle component.
        /// </summary>
        public DemoDiscLightToggleComponent() {
            LightStates = new List<DemoDiscDirectionalLightToggleState>();
            LightsEnabled = true;
        }

        /// <summary>
        /// Captures the supported overlay and authored directional lights after hierarchy initialization.
        /// </summary>
        /// <param name="entity">Initialized overlay owner entity.</param>
        public override void ComponentInitialized(Entity entity) {
            base.ComponentInitialized(entity);
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            BindOverlayOwner(entity);
            CaptureDirectionalLightStates();
            ApplyOverlayText();
        }

        /// <summary>
        /// Toggles the authored directional lights when the current frame requests it.
        /// </summary>
        public override void Update() {
            if (!WasToggleRequested()) {
                return;
            }

            LightsEnabled = !LightsEnabled;
            ApplyDirectionalLightState();
            ApplyOverlayText();
        }

        /// <summary>
        /// Clears any shared debug-overlay rows when the component leaves the scene.
        /// </summary>
        /// <param name="entity">Owning entity.</param>
        public override void ComponentRemoved(Entity entity) {
            ClearDebugOverlayText();
            base.ComponentRemoved(entity);
        }

        /// <summary>
        /// Clears any shared debug-overlay rows when the component is disposed directly during teardown.
        /// </summary>
        public override void Dispose() {
            ClearDebugOverlayText();
            base.Dispose();
        }

        /// <summary>
        /// Resolves the supported overlay component hosted on the same entity.
        /// </summary>
        /// <param name="entity">Overlay owner entity.</param>
        void BindOverlayOwner(Entity entity) {
            if (entity.Components == null) {
                throw new InvalidOperationException("Light toggle component requires an initialized overlay owner.");
            }

            for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                Component component = entity.Components[componentIndex];
                if (component is FPSComponent fpsComponent) {
                    FpsComponentValue = fpsComponent;
                } else if (component is DebugComponent debugComponent) {
                    DebugComponentValue = debugComponent;
                }
            }

            if (FpsComponentValue == null && DebugComponentValue == null) {
                throw new InvalidOperationException("Light toggle component requires either an FPSComponent or DebugComponent on the same entity.");
            }
        }

        /// <summary>
        /// Captures the authored directional-light states from the active object manager.
        /// </summary>
        void CaptureDirectionalLightStates() {
            LightStates.Clear();

            List<DirectionalLightComponent> directionalLights = Core.Instance.ObjectManager.DirectionalLights;
            for (int lightIndex = 0; lightIndex < directionalLights.Count; lightIndex++) {
                DirectionalLightComponent light = directionalLights[lightIndex];
                if (light == null) {
                    continue;
                }

                LightStates.Add(new DemoDiscDirectionalLightToggleState {
                    Light = light,
                    Intensity = light.Intensity,
                    ShadowsEnabled = light.ShadowsEnabled
                });
            }

            if (LightStates.Count == 0) {
                throw new InvalidOperationException("Light toggle component requires at least one directional light in the scene.");
            }
        }

        /// <summary>
        /// Applies the current enabled state to all captured directional lights.
        /// </summary>
        void ApplyDirectionalLightState() {
            for (int lightIndex = 0; lightIndex < LightStates.Count; lightIndex++) {
                DemoDiscDirectionalLightToggleState lightState = LightStates[lightIndex];
                if (lightState.Light == null) {
                    continue;
                }

                if (LightsEnabled) {
                    lightState.Light.Intensity = lightState.Intensity;
                    lightState.Light.ShadowsEnabled = lightState.ShadowsEnabled;
                } else {
                    lightState.Light.Intensity = 0f;
                    lightState.Light.ShadowsEnabled = false;
                }
            }
        }

        /// <summary>
        /// Applies the current controls text through the supported overlay path.
        /// </summary>
        void ApplyOverlayText() {
            if (FpsComponentValue != null) {
                FpsComponentValue.AdditionalText = BuildOverlayText();
            }

            if (DebugComponentValue != null) {
                DebugComponent.SetAdditionalLine(DebugLightStatusLineId, BuildLightStatusText());
                DebugComponent.SetAdditionalLine(DebugCameraControlsLineId, CameraControlsText);
            }
        }

        /// <summary>
        /// Clears the shared debug overlay rows owned by this component.
        /// </summary>
        void ClearDebugOverlayText() {
            if (DebugComponentValue == null) {
                return;
            }

            DebugComponent.ClearAdditionalLine(DebugLightStatusLineId);
            DebugComponent.ClearAdditionalLine(DebugCameraControlsLineId);
        }

        /// <summary>
        /// Returns the full authored FPS overlay text block.
        /// </summary>
        /// <returns>Two-line controls text block.</returns>
        string BuildOverlayText() {
            return BuildLightStatusText() + "\n" + CameraControlsText;
        }

        /// <summary>
        /// Returns the current light-toggle status row.
        /// </summary>
        /// <returns>Status text that includes the current light state and toggle binding.</returns>
        string BuildLightStatusText() {
            return LightsEnabled ? "Light: On (L / South Toggle)" : "Light: Off (L / South Toggle)";
        }

        /// <summary>
        /// Returns whether the current frame requested a light toggle from keyboard or gamepad input.
        /// </summary>
        /// <returns>True when the current frame should toggle the directional lights.</returns>
        bool WasToggleRequested() {
            InputSystem inputSystem = Core.Instance.Input;
            if (inputSystem == null) {
                throw new InvalidOperationException("Light toggle component requires an initialized input system.");
            }

            return inputSystem.WasKeyPressed(Keys.L)
                || inputSystem.WasGamepadButtonPressed(0, InputGamepadButton.South);
        }
    }
}
