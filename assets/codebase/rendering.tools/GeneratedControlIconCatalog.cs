using System.Text.Json;

namespace city.rendering.tools {
    /// <summary>
    /// Loads and validates the generated control-icon manifest.
    /// </summary>
    public sealed class GeneratedControlIconCatalog {
        const string ManifestRelativePath = "assets/images/instructions/controls/generated/manifest.json";

        readonly Dictionary<string, HashSet<string>> ControlIdsByFamilyId;

        GeneratedControlIconCatalog(Dictionary<string, HashSet<string>> controlIdsByFamilyId) {
            ControlIdsByFamilyId = controlIdsByFamilyId ?? throw new ArgumentNullException(nameof(controlIdsByFamilyId));
        }

        public static GeneratedControlIconCatalog Load(string projectRootPath) {
            string fullProjectRootPath = Path.GetFullPath(projectRootPath ?? string.Empty);
            string fullManifestPath = Path.Combine(fullProjectRootPath, ManifestRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullManifestPath)) {
                throw new InvalidOperationException($"Generated control icon manifest was not found at '{fullManifestPath}'.");
            }

            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(fullManifestPath));
            JsonElement platformsElement = document.RootElement.GetProperty("platforms");
            Dictionary<string, HashSet<string>> controlsByFamily = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (JsonProperty platformProperty in platformsElement.EnumerateObject()) {
                JsonElement platformValue = platformProperty.Value;
                if (!platformValue.TryGetProperty("controls", out JsonElement controlsElement) || controlsElement.ValueKind != JsonValueKind.Array) {
                    throw new InvalidOperationException($"Generated control icon manifest entry '{platformProperty.Name}' did not contain a valid controls array.");
                }

                HashSet<string> controls = new HashSet<string>(StringComparer.Ordinal);
                foreach (JsonElement controlElement in controlsElement.EnumerateArray()) {
                    string controlId = controlElement.GetString();
                    if (string.IsNullOrWhiteSpace(controlId)) {
                        throw new InvalidOperationException($"Generated control icon manifest entry '{platformProperty.Name}' contained an empty control id.");
                    }

                    controls.Add(controlId);
                }

                controlsByFamily.Add(platformProperty.Name, controls);
            }

            return new GeneratedControlIconCatalog(controlsByFamily);
        }

        public string RequireControlPath(string familyId, string controlId) {
            if (string.IsNullOrWhiteSpace(familyId)) {
                throw new ArgumentException("Family id must be provided.", nameof(familyId));
            } else if (string.IsNullOrWhiteSpace(controlId)) {
                throw new ArgumentException("Control id must be provided.", nameof(controlId));
            }

            string normalizedFamilyId = familyId.Trim();
            string normalizedControlId = controlId.Trim();
            if (!ControlIdsByFamilyId.TryGetValue(normalizedFamilyId, out HashSet<string> controls)) {
                throw new InvalidOperationException($"Generated control icon family '{normalizedFamilyId}' was not found in the manifest.");
            }

            if (!controls.Contains(normalizedControlId)) {
                throw new InvalidOperationException($"Generated control icon '{normalizedFamilyId}/{normalizedControlId}' was not found in the manifest.");
            }

            return "images/instructions/controls/generated/" + normalizedFamilyId + "/" + normalizedControlId + ".png";
        }
    }
}
