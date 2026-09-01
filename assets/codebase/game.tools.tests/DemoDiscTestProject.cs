using System.Runtime.CompilerServices;

namespace city.testing {
    public static class DemoDiscTestProject {
        public static readonly string RootPath = ResolveRootPath();

        public static string GetPath(params string[] relativeParts) {
            string path = RootPath;
            for (int partIndex = 0; partIndex < relativeParts.Length; partIndex++) {
                path = Path.Combine(path, relativeParts[partIndex]);
            }
            return path;
        }

        static string ResolveRootPath([CallerFilePath] string sourceFilePath = "") {
            string configuredRoot = Environment.GetEnvironmentVariable("HELENGINE_TEST_PROJECT_ROOT");
            if (!string.IsNullOrWhiteSpace(configuredRoot)) {
                return Path.GetFullPath(configuredRoot);
            }
            string sourceDirectory = Path.GetDirectoryName(sourceFilePath);
            if (string.IsNullOrWhiteSpace(sourceDirectory)) {
                throw new InvalidOperationException("DemoDisc test source directory could not be resolved.");
            }
            return Path.GetFullPath(Path.Combine(sourceDirectory, "..", "..", ".."));
        }
    }
}
