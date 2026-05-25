namespace city.rendering.tools {
    /// <summary>
    /// Stores one platform's schema id plus authored material field values.
    /// </summary>
    public sealed class GeneratedMaterialPlatformDefinition {
        /// <summary>
        /// Initializes one empty platform material definition.
        /// </summary>
        public GeneratedMaterialPlatformDefinition() {
            FieldValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            SchemaId = string.Empty;
        }

        /// <summary>
        /// Gets or sets the target platform schema identifier.
        /// </summary>
        public string SchemaId { get; set; }

        /// <summary>
        /// Gets the authored material field values keyed by field id.
        /// </summary>
        public Dictionary<string, string> FieldValues { get; }

        /// <summary>
        /// Sets or replaces one authored material field value.
        /// </summary>
        /// <param name="fieldId">Stable material field identifier.</param>
        /// <param name="value">Serialized field value.</param>
        public void SetFieldValue(string fieldId, string value) {
            if (string.IsNullOrWhiteSpace(fieldId)) {
                throw new ArgumentException("Field id must be provided.", nameof(fieldId));
            }

            FieldValues[fieldId] = value ?? string.Empty;
        }
    }
}
