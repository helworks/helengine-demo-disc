namespace city.menu {
    /// <summary>
    /// Applies the current platform name and version to the demo-disc menu overlay text.
    /// </summary>
    public sealed class PlatformInfoTextComponent : UpdateComponent {
        /// <summary>
        /// Stable child entity name used for the platform name text line.
        /// </summary>
        const string PlatformNameTextEntityName = "DemoDiscPlatformInfoNameText";

        /// <summary>
        /// Stable child entity name used for the platform version text line.
        /// </summary>
        const string PlatformVersionTextEntityName = "DemoDiscPlatformInfoVersionText";

        /// <summary>
        /// Tracks whether the overlay text has already been populated.
        /// </summary>
        bool Applied;

        /// <summary>
        /// Applies the current platform name and version to the child text entities once.
        /// </summary>
        public override void Update() {
            base.Update();

            if (Applied) {
                return;
            } else if (Parent == null) {
                throw new InvalidOperationException("PlatformInfoTextComponent requires a parent entity.");
            } else if (Parent.Children == null || Parent.Children.Count < 2) {
                throw new InvalidOperationException("Platform-info overlay requires two child text entities.");
            }

            Entity nameEntity = Parent.Children[0];
            Entity versionEntity = Parent.Children[1];
            TextComponent nameText = FindTextComponent(nameEntity);
            TextComponent versionText = FindTextComponent(versionEntity);
            string platformName = Core.Instance.PlatformInfo.Name;
            string platformVersion = Core.Instance.PlatformInfo.Version;
            ApplyText(nameEntity, nameText, platformName, 0f);
            ApplyText(versionEntity, versionText, platformVersion, nameText.Size.Y + 6f);
            Applied = true;
        }

        /// <summary>
        /// Applies one platform-info text line to one child entity.
        /// </summary>
        /// <param name="entity">Child text entity that should be updated.</param>
        /// <param name="textComponent">Text component that renders the value.</param>
        /// <param name="text">Text content to display.</param>
        /// <param name="topOffset">Vertical offset from the overlay container.</param>
        void ApplyText(Entity entity, TextComponent textComponent, string text, float topOffset) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (textComponent == null) {
                throw new ArgumentNullException(nameof(textComponent));
            }

            textComponent.Text = text;
            float2 measuredSize = textComponent.Font.MeasureString(text);
            textComponent.Size = new int2((int)Math.Ceiling(measuredSize.X), (int)Math.Ceiling(measuredSize.Y));
            entity.LocalPosition = new float3(-textComponent.Size.X, topOffset, 0f);
        }

        /// <summary>
        /// Finds the text component attached to one child entity.
        /// </summary>
        /// <param name="entity">Child entity whose text component should be returned.</param>
        /// <returns>Attached text component.</returns>
        TextComponent FindTextComponent(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is TextComponent textComponent) {
                    return textComponent;
                }
            }

            throw new InvalidOperationException("Platform-info overlay child must include a text component.");
        }
    }
}
