namespace city.rendering {
    /// <summary>
    /// Toggles authored directional lights on or off for the demo-disc scenes.
    /// </summary>
    public sealed class DemoDiscLightToggleComponent : UpdateComponent {
        /// <summary>
        /// Cached authored directional lights controlled by this component.
        /// </summary>
        readonly List<DemoDiscDirectionalLightToggleState> LightStates;

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
        /// Captures authored directional lights after hierarchy initialization.
        /// </summary>
        /// <param name="entity">Initialized component owner entity.</param>
        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            CaptureDirectionalLightStates();
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
        }

        /// <summary>
        /// Removes the component from the scene.
        /// </summary>
        /// <param name="entity">Owning entity.</param>
        public override void ComponentRemoved(Entity entity) {
            base.ComponentRemoved(entity);
        }

        /// <summary>
        /// Disposes the component without additional teardown because the current implementation owns no transient overlay state.
        /// </summary>
        public void Dispose() {
        }

        /// <summary>
        /// Captures the authored directional-light states from the active object manager.
        /// </summary>
        void CaptureDirectionalLightStates() {
            LightStates.Clear();
            if (Core.Instance == null || Core.Instance.ObjectManager == null) {
                throw new InvalidOperationException("Light toggle component requires an initialized object manager.");
            }

            List<Entity> entities = Core.Instance.ObjectManager.Entities;
            for (int entityIndex = 0; entityIndex < entities.Count; entityIndex++) {
                Entity entity = entities[entityIndex];
                if (entity == null || entity.Components == null) {
                    continue;
                }

                for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                    if (entity.Components[componentIndex] is DirectionalLightComponent directionalLightComponent) {
                        LightStates.Add(new DemoDiscDirectionalLightToggleState {
                            Light = directionalLightComponent,
                            Intensity = directionalLightComponent.Intensity,
                            ShadowsEnabled = directionalLightComponent.ShadowsEnabled
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Applies the current enabled state to all captured directional lights.
        /// </summary>
        void ApplyDirectionalLightState() {
            for (int lightIndex = 0; lightIndex < LightStates.Count; lightIndex++) {
                DemoDiscDirectionalLightToggleState lightState = LightStates[lightIndex];
                if (lightState == null || lightState.Light == null) {
                    continue;
                }

                DirectionalLightComponent directionalLightComponent = lightState.Light;
                if (LightsEnabled) {
                    directionalLightComponent.Intensity = lightState.Intensity;
                    directionalLightComponent.ShadowsEnabled = lightState.ShadowsEnabled;
                    continue;
                }

                directionalLightComponent.Intensity = 0f;
                directionalLightComponent.ShadowsEnabled = false;
            }
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
                || inputSystem.WasGamepadButtonPressed(0, InputGamepadButton.RightShoulder);
        }
    }
}
