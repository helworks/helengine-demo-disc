namespace city.rendering {
    /// <summary>
    /// Cycles authored directional lights through the fixed demo-disc color palette and mirrors the active state into the shared light-indicator swatch.
    /// </summary>
    public sealed class DemoDiscLightToggleComponent : UpdateComponent {
        /// <summary>
        /// Stable cycle index used by the off state.
        /// </summary>
        const int OffLightStateIndex = 5;

        /// <summary>
        /// White light color used by the first cycle state and startup normalization.
        /// </summary>
        static readonly float4 WhiteLightColor = new float4(1f, 1f, 1f, 1f);

        /// <summary>
        /// Yellow light color used by the second cycle state.
        /// </summary>
        static readonly float4 YellowLightColor = new float4(1f, 1f, 0f, 1f);

        /// <summary>
        /// Red light color used by the third cycle state.
        /// </summary>
        static readonly float4 RedLightColor = new float4(1f, 0f, 0f, 1f);

        /// <summary>
        /// Blue light color used by the fourth cycle state.
        /// </summary>
        static readonly float4 BlueLightColor = new float4(0f, 0f, 1f, 1f);

        /// <summary>
        /// Green light color used by the fifth cycle state.
        /// </summary>
        static readonly float4 GreenLightColor = new float4(0f, 1f, 0f, 1f);

        /// <summary>
        /// White swatch color used by the indicator for the white light state.
        /// </summary>
        static readonly byte4 WhiteSwatchColor = new byte4(255, 255, 255, 255);

        /// <summary>
        /// Yellow swatch color used by the indicator for the yellow light state.
        /// </summary>
        static readonly byte4 YellowSwatchColor = new byte4(255, 230, 0, 255);

        /// <summary>
        /// Red swatch color used by the indicator for the red light state.
        /// </summary>
        static readonly byte4 RedSwatchColor = new byte4(255, 0, 0, 255);

        /// <summary>
        /// Blue swatch color used by the indicator for the blue light state.
        /// </summary>
        static readonly byte4 BlueSwatchColor = new byte4(0, 120, 255, 255);

        /// <summary>
        /// Green swatch color used by the indicator for the green light state.
        /// </summary>
        static readonly byte4 GreenSwatchColor = new byte4(0, 220, 80, 255);

        /// <summary>
        /// Dark swatch color used by the indicator for the off state.
        /// </summary>
        static readonly byte4 OffSwatchColor = new byte4(0, 0, 0, 255);

        /// <summary>
        /// Cached authored directional lights controlled by this component.
        /// </summary>
        readonly List<DemoDiscDirectionalLightToggleState> LightStates;

        /// <summary>
        /// Cached preview square component whose fill color mirrors the current light state.
        /// </summary>
        RoundedRectComponent IndicatorSwatch;

        /// <summary>
        /// Tracks the current fixed light-cycle state.
        /// </summary>
        int CurrentLightStateIndex;

        /// <summary>
        /// Initializes one light-toggle component.
        /// </summary>
        public DemoDiscLightToggleComponent() {
            LightStates = new List<DemoDiscDirectionalLightToggleState>();
            CurrentLightStateIndex = 0;
        }

        /// <summary>
        /// Advances the authored directional lights to the next fixed color-cycle state when the current frame requests it.
        /// </summary>
        public override void Update() {
            if (!WasToggleRequested()) {
                return;
            }

            CurrentLightStateIndex++;
            if (CurrentLightStateIndex > OffLightStateIndex) {
                CurrentLightStateIndex = 0;
            }

            ApplyCurrentLightState();
        }

        /// <summary>
        /// Captures authored directional lights and the shared indicator swatch after the full scene hierarchy has initialized.
        /// </summary>
        /// <param name="entity">Initialized component owner entity.</param>
        public override void ComponentInitialized(Entity entity) {
            base.ComponentInitialized(entity);
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            CaptureDirectionalLightStates();
            CaptureIndicatorComponents();
            ApplyCurrentLightState();
        }

        /// <summary>
        /// Clears cached runtime state when the component leaves the scene.
        /// </summary>
        /// <param name="entity">Owning entity.</param>
        public override void ComponentRemoved(Entity entity) {
            base.ComponentRemoved(entity);
            IndicatorSwatch = null;
            LightStates.Clear();
        }

        /// <summary>
        /// Releases cached light-state and indicator references owned by the component.
        /// </summary>
        public override void Dispose() {
            base.Dispose();
            IndicatorSwatch = null;
            LightStates.Clear();
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
                            AuthoredIntensity = directionalLightComponent.Intensity,
                            AuthoredShadowsEnabled = directionalLightComponent.ShadowsEnabled
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Captures the shared indicator components required by the demo-disc light cycle.
        /// </summary>
        void CaptureIndicatorComponents() {
            if (Parent == null) {
                throw new InvalidOperationException("Light toggle component must be attached to an initialized scene UI root.");
            }

            IndicatorSwatch = FindRequiredIndicatorSwatch(Parent);
        }

        /// <summary>
        /// Applies the current fixed light-cycle state to all captured directional lights and to the shared indicator swatch.
        /// </summary>
        void ApplyCurrentLightState() {
            float4 activeLightColor = ResolveActiveLightColor();
            bool lightsEnabled = CurrentLightStateIndex != OffLightStateIndex;
            for (int lightIndex = 0; lightIndex < LightStates.Count; lightIndex++) {
                DemoDiscDirectionalLightToggleState lightState = LightStates[lightIndex];
                if (lightState == null || lightState.Light == null) {
                    continue;
                }

                DirectionalLightComponent directionalLightComponent = lightState.Light;
                directionalLightComponent.Color = activeLightColor;
                if (lightsEnabled) {
                    directionalLightComponent.Intensity = lightState.AuthoredIntensity;
                    directionalLightComponent.ShadowsEnabled = lightState.AuthoredShadowsEnabled;
                } else {
                    directionalLightComponent.Intensity = 0f;
                    directionalLightComponent.ShadowsEnabled = false;
                }
            }

            if (IndicatorSwatch == null) {
                throw new InvalidOperationException("Light toggle component requires an authored light indicator swatch.");
            }

            IndicatorSwatch.FillColor = ResolveIndicatorColor();
        }

        /// <summary>
        /// Resolves the active directional-light color for the current fixed cycle state.
        /// </summary>
        /// <returns>Directional-light color that should be applied to all captured lights.</returns>
        float4 ResolveActiveLightColor() {
            if (CurrentLightStateIndex == 0) {
                return WhiteLightColor;
            } else if (CurrentLightStateIndex == 1) {
                return YellowLightColor;
            } else if (CurrentLightStateIndex == 2) {
                return RedLightColor;
            } else if (CurrentLightStateIndex == 3) {
                return BlueLightColor;
            } else if (CurrentLightStateIndex == 4) {
                return GreenLightColor;
            }

            return WhiteLightColor;
        }

        /// <summary>
        /// Resolves the indicator swatch color for the current fixed cycle state.
        /// </summary>
        /// <returns>Indicator swatch fill color that should mirror the active light state.</returns>
        byte4 ResolveIndicatorColor() {
            if (CurrentLightStateIndex == 0) {
                return WhiteSwatchColor;
            } else if (CurrentLightStateIndex == 1) {
                return YellowSwatchColor;
            } else if (CurrentLightStateIndex == 2) {
                return RedSwatchColor;
            } else if (CurrentLightStateIndex == 3) {
                return BlueSwatchColor;
            } else if (CurrentLightStateIndex == 4) {
                return GreenSwatchColor;
            }

            return OffSwatchColor;
        }

        /// <summary>
        /// Finds the authored preview square created by the shared light-indicator overlay factory.
        /// </summary>
        /// <param name="rootEntity">Scene UI root that owns the indicator subtree.</param>
        /// <returns>Rounded-rectangle preview square component.</returns>
        RoundedRectComponent FindRequiredIndicatorSwatch(Entity rootEntity) {
            if (rootEntity == null) {
                throw new ArgumentNullException(nameof(rootEntity));
            }

            RoundedRectComponent indicatorSwatch = FindIndicatorSwatchRecursive(rootEntity);
            if (indicatorSwatch != null) {
                return indicatorSwatch;
            }

            throw new InvalidOperationException("Light toggle component requires an authored light indicator swatch.");
        }

        /// <summary>
        /// Walks the scene UI subtree until it finds the authored indicator swatch component.
        /// </summary>
        /// <param name="entity">Current scene UI entity being inspected.</param>
        /// <returns>Resolved rounded-rectangle indicator swatch, or null when the current branch does not contain it.</returns>
        RoundedRectComponent FindIndicatorSwatchRecursive(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity.Components != null) {
                for (int componentIndex = 0; componentIndex < entity.Components.Count; componentIndex++) {
                    if (entity.Components[componentIndex] is RoundedRectComponent roundedRectComponent) {
                        return roundedRectComponent;
                    }
                }
            }

            if (entity.Children == null) {
                return null;
            }

            for (int childIndex = 0; childIndex < entity.Children.Count; childIndex++) {
                RoundedRectComponent resolvedComponent = FindIndicatorSwatchRecursive(entity.Children[childIndex]);
                if (resolvedComponent != null) {
                    return resolvedComponent;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns whether the current frame requested a light-cycle advance from keyboard or gamepad input.
        /// </summary>
        /// <returns>True when the current frame should advance the directional-light cycle.</returns>
        bool WasToggleRequested() {
            InputSystem inputSystem = Core.Instance.Input;
            if (inputSystem == null) {
                throw new InvalidOperationException("Light toggle component requires an initialized input system.");
            }

            return inputSystem.WasKeyPressed(Keys.L)
                || inputSystem.WasGamepadButtonPressed(0, InputGamepadButton.North);
        }
    }
}
