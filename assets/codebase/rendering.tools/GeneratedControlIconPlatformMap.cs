namespace city.rendering.tools {
    /// <summary>
    /// Maps authored platform ids to generated control-icon families.
    /// </summary>
    public static class GeneratedControlIconPlatformMap {
        static readonly Dictionary<string, string> FamilyIdsByPlatformId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            ["windows"] = "keyboard",
            ["win32"] = "keyboard",
            ["xbox360"] = "xbox360",
            ["switch"] = "switch",
            ["gamecube"] = "gamecube",
            ["wii"] = "wii",
            ["ds"] = "ds",
            ["3ds"] = "3ds",
            ["psp"] = "psp",
            ["ps2"] = "ps2",
            ["psvita"] = "psvita",
            ["n64"] = "n64",
            ["dreamcast"] = "dreamcast",
            ["ps1"] = "ps1",
            ["ps3"] = "ps3",
            ["xbox"] = "xbox",
            ["steamdeck"] = "steamdeck"
        };

        public static string ResolveFamilyId(string platformId) {
            if (string.IsNullOrWhiteSpace(platformId)) {
                throw new ArgumentException("Platform id must be provided.", nameof(platformId));
            }

            if (FamilyIdsByPlatformId.TryGetValue(platformId.Trim(), out string familyId)) {
                return familyId;
            }

            throw new InvalidOperationException($"Generated control icon family mapping was not found for platform '{platformId}'.");
        }

        public static IReadOnlyList<string> EnumerateMappedPlatformIds() {
            return FamilyIdsByPlatformId.Keys.OrderBy(static value => value, StringComparer.OrdinalIgnoreCase).ToArray();
        }
    }
}
