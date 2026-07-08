namespace city.rendering {
    /// <summary>
    /// Owns Nintendo DS companion-scene light-cycle behavior from the scaffold-owned light button and handheld shoulder input.
    /// </summary>
    public sealed class NintendoDsLightToggleOverlayComponent : UpdateComponent {
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
        /// Interactable host used by the scaffold-owned bottom-screen light button.
        /// </summary>
        InteractableComponent BoundInteractable;

        /// <summary>
        /// Cached preview square component whose fill color mirrors the current light state.
        /// </summary>
        RoundedRectComponent IndicatorSwatch;

        /// <summary>
        /// Tracks whether the active pointer press began inside the bound interactable.
        /// </summary>
        bool PointerPressStartedInside;

        /// <summary>
        /// Tracks the current fixed light-cycle state.
        /// </summary>
        int CurrentLightStateIndex;

        /// <summary>
        /// Initializes one Nintendo DS light-toggle overlay component.
        /// </summary>
        public NintendoDsLightToggleOverlayComponent() {
            LightStates = new List<DemoDiscDirectionalLightToggleState>();
            CurrentLightStateIndex = 0;
        }

        /// <summary>
        /// Captures the scaffold-owned interactable, directional lights, and swatch after the full scene hierarchy has initialized.
        /// </summary>
        /// <param name="entity">Initialized component owner entity.</param>
        public override void ComponentInitialized(Entity entity) {
            base.ComponentInitialized(entity);
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            TryBindInteractable();
            CaptureDirectionalLightStates();
            CaptureIndicatorSwatch();
            ApplyCurrentLightState();
        }

        /// <summary>
        /// Advances the scaffold-owned light cycle when the current frame requests it through touch or shoulder input.
        /// </summary>
        public override void Update() {
            TryBindInteractable();
            if (!WasToggleRequestedFromInput()) {
                return;
            }

            AdvanceLightState();
        }

        /// <summary>
        /// Releases the current interactable and runtime caches when the component leaves the scene.
        /// </summary>
        /// <param name="entity">Owning entity.</param>
        public override void ComponentRemoved(Entity entity) {
            UnbindInteractable();
            IndicatorSwatch = null;
            LightStates.Clear();
            base.ComponentRemoved(entity);
        }

        /// <summary>
        /// Releases cached light-state and input references before the component is deleted.
        /// </summary>
        public override void Dispose() {
            UnbindInteractable();
            IndicatorSwatch = null;
            LightStates.Clear();
            base.Dispose();
        }

        /// <summary>
        /// Binds the sibling interactable used to receive pointer clicks from the scaffold-owned light button.
        /// </summary>
        void TryBindInteractable() {
            if (BoundInteractable != null) {
                return;
            } else if (Parent == null || Parent.Components == null) {
                return;
            }

            for (int componentIndex = 0; componentIndex < Parent.Components.Count; componentIndex++) {
                if (Parent.Components[componentIndex] is InteractableComponent interactable) {
                    BoundInteractable = interactable;
                    BoundInteractable.CursorEvent += HandleCursorEvent;
                    return;
                }
            }

            throw new InvalidOperationException("NintendoDsLightToggleOverlayComponent requires a sibling InteractableComponent.");
        }

        /// <summary>
        /// Releases the current interactable binding and clears active pointer state.
        /// </summary>
        void UnbindInteractable() {
            if (BoundInteractable == null) {
                return;
            }

            BoundInteractable.CursorEvent -= HandleCursorEvent;
            BoundInteractable = null;
            PointerPressStartedInside = false;
        }

        /// <summary>
        /// Handles pointer press and release events from the scaffold-owned light-button interactable.
        /// </summary>
        /// <param name="relativePosition">Pointer position relative to the interactable.</param>
        /// <param name="delta">Pointer delta reported by the shared interaction router.</param>
        /// <param name="interaction">Current pointer interaction state.</param>
        void HandleCursorEvent(int2 relativePosition, int2 delta, PointerInteraction interaction) {
            if (interaction == PointerInteraction.Press) {
                PointerPressStartedInside = true;
                return;
            }
            if (interaction == PointerInteraction.Release) {
                bool shouldAdvance = PointerPressStartedInside;
                PointerPressStartedInside = false;
                if (shouldAdvance) {
                    AdvanceLightState();
                }
                return;
            }
            if (interaction == PointerInteraction.Leave) {
                PointerPressStartedInside = false;
            }
        }

        /// <summary>
        /// Captures the authored directional-light states from the active object manager.
        /// </summary>
        void CaptureDirectionalLightStates() {
            LightStates.Clear();
            if (Core.Instance == null || Core.Instance.ObjectManager == null) {
                throw new InvalidOperationException("NintendoDsLightToggleOverlayComponent requires an initialized object manager.");
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
        /// Captures the scaffold-owned light swatch required by the handheld light cycle.
        /// </summary>
        void CaptureIndicatorSwatch() {
            if (Parent == null) {
                throw new InvalidOperationException("NintendoDsLightToggleOverlayComponent must be attached to an initialized bottom-screen light button.");
            }

            IndicatorSwatch = FindRequiredIndicatorSwatch(Parent);
        }

        /// <summary>
        /// Returns whether the current frame requested a light-cycle advance from handheld shoulder input.
        /// </summary>
        /// <returns>True when the current frame should advance the light cycle.</returns>
        bool WasToggleRequestedFromInput() {
            InputSystem inputSystem = Core.Instance.Input;
            if (inputSystem == null) {
                throw new InvalidOperationException("NintendoDsLightToggleOverlayComponent requires an initialized input system.");
            }

            return inputSystem.WasGamepadButtonPressed(0, InputGamepadButton.RightShoulder);
        }

        /// <summary>
        /// Advances the current light cycle by one fixed state and applies it immediately.
        /// </summary>
        void AdvanceLightState() {
            CurrentLightStateIndex++;
            if (CurrentLightStateIndex > OffLightStateIndex) {
                CurrentLightStateIndex = 0;
            }

            ApplyCurrentLightState();
        }

        /// <summary>
        /// Applies the current fixed light-cycle state to all captured directional lights and to the scaffold-owned indicator swatch.
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
                throw new InvalidOperationException("NintendoDsLightToggleOverlayComponent requires a scaffold-owned light indicator swatch.");
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
        /// Finds the scaffold-owned light indicator swatch beneath the supplied light-button subtree.
        /// </summary>
        /// <param name="rootEntity">Bottom-screen light-button root that owns the swatch.</param>
        /// <returns>Rounded-rectangle swatch component.</returns>
        RoundedRectComponent FindRequiredIndicatorSwatch(Entity rootEntity) {
            if (rootEntity == null) {
                throw new ArgumentNullException(nameof(rootEntity));
            }

            RoundedRectComponent indicatorSwatch = FindIndicatorSwatchRecursive(rootEntity);
            if (indicatorSwatch != null) {
                return indicatorSwatch;
            }

            throw new InvalidOperationException("NintendoDsLightToggleOverlayComponent requires a scaffold-owned light indicator swatch.");
        }

        /// <summary>
        /// Walks the light-button subtree until it finds the scaffold-owned indicator swatch component.
        /// </summary>
        /// <param name="entity">Current light-button subtree entity being inspected.</param>
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
    }
}
