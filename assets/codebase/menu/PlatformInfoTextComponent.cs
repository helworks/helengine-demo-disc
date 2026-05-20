namespace city.menu {
    /// <summary>
    /// Applies the current platform name and version to the demo-disc menu overlay text.
    /// </summary>
    public sealed class PlatformInfoTextComponent : UpdateComponent {
        /// <summary>
        /// Stable child entity name used for the platform name text line.
        /// </summary>
        /// <summary>
        /// Cached child entity that renders the platform name.
        /// </summary>
        Entity PlatformNameTextEntity;
        /// <summary>
        /// Cached child entity that renders the platform version.
        /// </summary>
        Entity PlatformVersionTextEntity;
        /// <summary>
        /// Cached text component that renders the platform name.
        /// </summary>
        TextComponent PlatformNameTextComponent;
        /// <summary>
        /// Cached text component that renders the platform version.
        /// </summary>
        TextComponent PlatformVersionTextComponent;

        /// <summary>
        /// Resolves the child text entities and applies the platform information once the component is attached.
        /// </summary>
        /// <param name="entity">Owning attached entity.</param>
        public override void ComponentAdded(Entity entity) {
            base.ComponentAdded(entity);
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            BindTextEntities(entity);
            string platformName = Core.Instance.PlatformInfo.Name;
            string platformVersion = Core.Instance.PlatformInfo.Version;
            ApplyText(PlatformNameTextEntity, PlatformNameTextComponent, platformName, 0f);
            ApplyText(PlatformVersionTextEntity, PlatformVersionTextComponent, platformVersion, PlatformNameTextComponent.Size.Y + 6f);
        }

        /// <summary>
        /// Resolves and caches the platform-info child text entities and components from the initialized subtree.
        /// </summary>
        /// <param name="entity">Owning initialized entity.</param>
        void BindTextEntities(Entity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            } else if (Parent == null) {
                throw new InvalidOperationException("PlatformInfoTextComponent requires a parent entity.");
            } else if (Parent.Children == null || Parent.Children.Count < 2) {
                throw new InvalidOperationException("Platform-info overlay requires two child text entities.");
            }

            PlatformNameTextEntity = FindRequiredChildEntity(entity, 0);
            PlatformVersionTextEntity = FindRequiredChildEntity(entity, 1);
            PlatformNameTextComponent = FindTextComponent(PlatformNameTextEntity);
            PlatformVersionTextComponent = FindTextComponent(PlatformVersionTextEntity);
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
        /// Finds one required named child entity beneath the platform-info host.
        /// </summary>
        /// <param name="parentEntity">Parent entity whose direct children should be searched.</param>
        /// <param name="childIndex">Direct child index to resolve.</param>
        /// <returns>Resolved child entity.</returns>
        Entity FindRequiredChildEntity(Entity parentEntity, int childIndex) {
            if (parentEntity == null) {
                throw new ArgumentNullException(nameof(parentEntity));
            } else if (parentEntity.Children == null) {
                throw new InvalidOperationException("Platform-info overlay requires child entities.");
            } else if (childIndex < 0 || childIndex >= parentEntity.Children.Count) {
                throw new InvalidOperationException($"Platform-info overlay is missing child entity at index {childIndex}.");
            }

            return parentEntity.Children[childIndex];
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
