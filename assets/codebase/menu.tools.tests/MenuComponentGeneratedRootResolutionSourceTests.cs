namespace city.tests {
    /// <summary>
    /// Verifies the runtime menu binder resolves the generated menu subtree by its runtime panel-component subtree instead of assuming the menu root has exactly one child.
    /// </summary>
    public sealed class MenuComponentGeneratedRootResolutionSourceTests {
        const string MenuComponentSourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\menu\MenuComponent.cs";

        /// <summary>
        /// Ensures menu runtime binding remains compatible with additional menu-root helper children such as the shared looping music entity.
        /// </summary>
        [Fact]
        public void Menu_component_resolves_generated_root_by_panel_subtree() {
            string source = File.ReadAllText(MenuComponentSourcePath);

            Assert.Contains("ContainsComponentInSubtree<MenuPanelComponent>", source, StringComparison.Ordinal);
            Assert.DoesNotContain("if (rootEntity.Children.Count == 1) {\r\n                return rootEntity.Children[0];\r\n            }\r\n\r\n            return null;", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures gamepad face-button confirmation follows the same menu activation path as the standard platform Accept action.
        /// </summary>
        [Fact]
        public void Menu_component_confirms_selection_with_primary_gamepad_button() {
            string source = File.ReadAllText(MenuComponentSourcePath);

            Assert.Contains("DemoDiscGamepadInput.WasButtonPressed(inputSystem, InputGamepadButton.South)", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the shared menu accepts one-shot left-stick vertical movement on every platform that exposes analog gamepad axes.
        /// </summary>
        [Fact]
        public void Menu_component_navigates_with_left_stick_vertical_threshold_crossings() {
            string source = File.ReadAllText(MenuComponentSourcePath);

            Assert.Contains("LeftStickY", source, StringComparison.Ordinal);
            Assert.Contains("DemoDiscGamepadInput.GetLeftStickY(inputSystem)", source, StringComparison.Ordinal);
            Assert.Contains("DemoDiscGamepadInput.GetPreviousLeftStickY(inputSystem)", source, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures menu-owned runtime resources are released through the inherited component disposal contract.
        /// </summary>
        [Fact]
        public void Menu_runtime_components_override_component_dispose() {
            string menuComponentSource = File.ReadAllText(MenuComponentSourcePath);
            string returnOverlaySource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\menu\DemoDiscReturnToMenuComponent.cs");
            string handheldReturnOverlaySource = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\menu\NintendoDsReturnOverlayComponent.cs");

            Assert.Contains("public override void Dispose()", menuComponentSource, StringComparison.Ordinal);
            Assert.Contains("public override void Dispose()", returnOverlaySource, StringComparison.Ordinal);
            Assert.Contains("public override void Dispose()", handheldReturnOverlaySource, StringComparison.Ordinal);
            Assert.Contains("base.Dispose();", menuComponentSource, StringComparison.Ordinal);
            Assert.Contains("base.Dispose();", returnOverlaySource, StringComparison.Ordinal);
            Assert.Contains("base.Dispose();", handheldReturnOverlaySource, StringComparison.Ordinal);
        }

        /// <summary>
        /// Ensures the generated handheld menu receives the same platform cook-time exclusions as the standard menu.
        /// </summary>
        [Fact]
        public void Demo_disc_scene_generator_applies_build_scene_availability_to_handheld_menu() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\menu.tools\DemoDiscSceneGenerator.cs");

            int firstAvailabilityCall = source.IndexOf(
                "MenuBuildSceneAuthoringService.ApplyBuildSceneAvailability(projectRootPath, standardSceneDefinition, definition);",
                StringComparison.Ordinal);
            int handheldDefinitionIndex = source.IndexOf(
                "GeneratedAuthoringSceneDefinition handheldSceneDefinition = SceneFactory.CreateHandheldSceneDefinition(providerTypeName, definition);",
                StringComparison.Ordinal);
            int secondAvailabilityCall = source.IndexOf(
                "MenuBuildSceneAuthoringService.ApplyBuildSceneAvailability(projectRootPath, handheldSceneDefinition, definition);",
                StringComparison.Ordinal);

            Assert.True(firstAvailabilityCall >= 0);
            Assert.True(handheldDefinitionIndex > firstAvailabilityCall);
            Assert.True(secondAvailabilityCall > handheldDefinitionIndex);
        }

        /// <summary>
        /// Ensures handheld cook filtering recognizes the platform-specific Tilt Trial selector id for the canonical menu entry.
        /// </summary>
        [Fact]
        public void Demo_disc_menu_cook_filter_resolves_handheld_scene_aliases() {
            string source = File.ReadAllText(@"C:\dev\helprojs\demodisc\assets\codebase\menu.tools\DemoDiscMenuBuildSceneAuthoringService.cs");

            Assert.Contains("ResolveConfiguredSceneIdForPlatform", source, StringComparison.Ordinal);
            Assert.Contains("sceneId + \"_ds\"", source, StringComparison.Ordinal);
        }
    }
}
