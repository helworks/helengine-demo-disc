namespace city.tests {
    /// <summary>
    /// Protects non-desktop generated game cores from authored keyboard references.
    /// </summary>
    public sealed class DesktopKeyboardSourceContractTests {
        /// <summary>
        /// Absolute path to the authored runtime source root.
        /// </summary>
        static readonly string RuntimeSourceRootPath = global::city.testing.DemoDiscTestProject.GetPath("assets", "codebase");

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

        [Fact]
        public void RemoveDesktopOnlySource_keeps_the_non_desktop_branch_of_an_inverse_guard() {
            string source = "#if !DESKTOP_PLATFORM\nreturn false;\n#else\nreturn Keys.Enter;\n#endif";

            string nonDesktopSource = RemoveDesktopOnlySource(source);

            Assert.Contains("return false;", nonDesktopSource, StringComparison.Ordinal);
            Assert.DoesNotContain("Keys.", nonDesktopSource, StringComparison.Ordinal);
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
            Stack<(bool IsDesktopConditional, bool IncludeBranch)> conditionalStack = new Stack<(bool, bool)>();
            string line;
            while ((line = reader.ReadLine()) != null) {
                string trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("#if ", StringComparison.Ordinal)) {
                    bool isDesktopConditional = string.Equals(trimmedLine, "#if DESKTOP_PLATFORM", StringComparison.Ordinal);
                    bool isInverseDesktopConditional = string.Equals(trimmedLine, "#if !DESKTOP_PLATFORM", StringComparison.Ordinal);
                    conditionalStack.Push((
                        isDesktopConditional || isInverseDesktopConditional,
                        isInverseDesktopConditional || (!isDesktopConditional && !isInverseDesktopConditional)));
                    continue;
                }
                if (string.Equals(trimmedLine, "#else", StringComparison.Ordinal)) {
                    if (conditionalStack.Count == 0) {
                        throw new InvalidOperationException("Desktop platform source guard has an unmatched #else.");
                    }
                    (bool isDesktopConditional, bool includeBranch) = conditionalStack.Pop();
                    conditionalStack.Push((isDesktopConditional, isDesktopConditional ? !includeBranch : true));
                    continue;
                }
                if (string.Equals(trimmedLine, "#endif", StringComparison.Ordinal)) {
                    if (conditionalStack.Count == 0) {
                        throw new InvalidOperationException("Desktop platform source guard has an unmatched #endif.");
                    }
                    conditionalStack.Pop();
                    continue;
                }

                bool includeLine = true;
                foreach ((bool IsDesktopConditional, bool IncludeBranch) frame in conditionalStack) {
                    if (!frame.IncludeBranch) {
                        includeLine = false;
                        break;
                    }
                }
                if (includeLine) {
                    writer.WriteLine(line);
                }
            }

            if (conditionalStack.Count != 0) {
                throw new InvalidOperationException("Desktop platform source guard is not balanced.");
            }

            return writer.ToString();
        }
    }
}
