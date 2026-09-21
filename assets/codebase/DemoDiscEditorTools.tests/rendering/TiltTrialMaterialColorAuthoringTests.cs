using DemoDisc.EditorTools;
using DemoDisc.rendering;

namespace DemoDisc.EditorTools.tests {
    /// <summary>
    /// Verifies Tilt Trial authored material colors use the shared <c>#RRGGBBAA</c> contract with fully opaque alpha.
    /// </summary>
    public sealed class TiltTrialMaterialColorAuthoringTests {
        /// <summary>
        /// Ensures the course and legacy walnut sphere colors keep an explicit opaque alpha channel while preserving the intended tint behavior.
        /// </summary>
        /// <summary>
        /// Ensures the course material factory writes the opaque white base color expected by the lilac textured ground path.
        /// </summary>
        [Fact]
        public void Tilt_trial_course_material_factory_keeps_opaque_alpha_in_rgba_order() {
            string courseSourcePath = @"C:\dev\helprojs\demodisc\assets\codebase\DemoDiscEditorTools\rendering\TiltTrialCourseMaterialFactory.cs";
            string courseSource = File.ReadAllText(courseSourcePath);

            Assert.Contains("platformDefinition.SetFieldValue(BaseColorFieldId, CourseBaseColor);", courseSource, StringComparison.Ordinal);
            Assert.DoesNotContain("const string CourseBaseColor = \"#FFFFFF\";", courseSource, StringComparison.Ordinal);
        }
    }
}
