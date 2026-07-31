namespace city.tests {
    /// <summary>
    /// Protects non-desktop generated game cores from authored keyboard references.
    /// </summary>
    public sealed class DesktopKeyboardSourceContractTests {
        /// <summary>
        /// Absolute path to the authored runtime source root.
        /// </summary>
        const string RuntimeSourceRootPath = @"C:\dev\helprojs\demodisc\assets\codebase";

        /// <summary>
        /// Runtime source directories that may be compiled into console and handheld game cores.
        /// </summary>
        static readonly string[] RuntimeSourceDirectoryNames = ["game", "menu", "rendering"];

        /// <summary>
        /// Input member names available exclusively to desktop game cores.
        /// </summary>
        static readonly string[] DesktopOnlyInputReferenceFragments = ["Keys.", "GetMouse", "WasMouse", "IsMouse"];

        /// <summary>
        /// Ensures keyboard fallback input is emitted only when desktop compilation is active.
        /// </summary>
        [Fact]
        public void Runtime_input_sources_keep_keyboard_references_inside_desktop_guards() {
            for (int directoryIndex = 0; directoryIndex < RuntimeSourceDirectoryNames.Length; directoryIndex++) {
                string runtimeSourceDirectoryPath = Path.Combine(RuntimeSourceRootPath, RuntimeSourceDirectoryNames[directoryIndex]);
                string[] sourceFilePaths = Directory.GetFiles(runtimeSourceDirectoryPath, "*.cs", SearchOption.AllDirectories);
                for (int fileIndex = 0; fileIndex < sourceFilePaths.Length; fileIndex++) {
                    string sourceFilePath = sourceFilePaths[fileIndex];
                    string nonDesktopSource = RemoveDesktopOnlySource(File.ReadAllText(sourceFilePath));
                    for (int referenceIndex = 0; referenceIndex < DesktopOnlyInputReferenceFragments.Length; referenceIndex++) {
                        Assert.DoesNotContain(DesktopOnlyInputReferenceFragments[referenceIndex], nonDesktopSource, StringComparison.Ordinal);
                    }
                }
            }
        }

        /// <summary>
        /// Removes source lines compiled exclusively when the desktop platform symbol is defined.
        /// </summary>
        /// <param name="source">Authored source text to inspect.</param>
        /// <returns>Source text compiled by non-desktop targets.</returns>
        static string RemoveDesktopOnlySource(string source) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }

            StringReader reader = new StringReader(source);
            StringWriter writer = new StringWriter();
            int desktopConditionalDepth = 0;
            string line;
            while ((line = reader.ReadLine()) != null) {
                string trimmedLine = line.Trim();
                if (string.Equals(trimmedLine, "#if DESKTOP_PLATFORM", StringComparison.Ordinal)) {
                    desktopConditionalDepth++;
                    continue;
                }
                if (string.Equals(trimmedLine, "#endif", StringComparison.Ordinal) && desktopConditionalDepth > 0) {
                    desktopConditionalDepth--;
                    continue;
                }
                if (desktopConditionalDepth == 0) {
                    writer.WriteLine(line);
                }
            }

            if (desktopConditionalDepth != 0) {
                throw new InvalidOperationException("Desktop platform source guard is not balanced.");
            }

            return writer.ToString();
        }
    }
}
