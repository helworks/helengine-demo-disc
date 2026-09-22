using helengine.editor;

namespace DemoDisc.EditorTools {
    /// <summary>
    /// Gives 240p status and scene labels the same UI font and effective size as the FPS overlay.
    /// </summary>
    public static class MicroSdTextComponentOverrideService {
        /// <summary>
        /// Persists the reference-canvas text settings on sd/msd without changing the shared presentation.
        /// The 1280-wide canvas scales by one quarter at 320x240, so scale four resolves to FPS scale one.
        /// </summary>
        public static void Apply(TextComponent text, EntitySaveComponent save, FontAsset uiFont, int referenceWidth) {
            if (text == null || save == null || uiFont == null) {
                throw new ArgumentNullException(nameof(text));
            }
            if (referenceWidth <= 0) {
                throw new ArgumentOutOfRangeException(nameof(referenceWidth));
            }

            save.OverrideLevelOrder = DemoDiscOverrideScopes.CreateGroupFirstLevelOrder();
            ComponentPlatformEditingService editing = new ComponentPlatformEditingService();
            EditorOverrideScope scope = DemoDiscOverrideScopes.MicroSd;
            TextComponent scopedText = (TextComponent)editing.EnsureScopeOverrideComponent(text, save, scope);
            scopedText.Font = uiFont;
            scopedText.FontScale = 4f;
            scopedText.Size = new int2(referenceWidth, 64);
            editing.MarkScopePropertyOverride(text, save, scope, nameof(TextComponent.Font));
            editing.MarkScopePropertyOverride(text, save, scope, nameof(TextComponent.FontScale));
            editing.MarkScopePropertyOverride(text, save, scope, nameof(TextComponent.Size));
            editing.StoreScopeAssetReference(text, scopedText, save, scope, nameof(TextComponent.Font),
                DemoDiscSceneComponentRecordFactory.CreateEditorUiFontReference());
        }
    }
}
