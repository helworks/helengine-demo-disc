## Stack Box Offset Design

### Goal
Adjust the authored `test_scene_dynamic_stack_boxes` physics validation scene so the four stacked cubes are slightly offset along positive X instead of perfectly centered.

### Change
Update the four dynamic stack-box entities in `C:\dev\helprojs\city\assets\codebase\physics.tools\PhysicsSceneFactory.cs` to use X positions `0.0f`, `0.2f`, `0.4f`, and `0.6f` while preserving their current Y heights, Z positions, body types, and materials.

### Validation
Add a narrow source-level test in `C:\dev\helworks\helengine\engine\helengine.editor.tests\managers\physics\PhysicsValidationSceneFactoryTests.cs` that creates the `DynamicStackBoxes` scene asset and asserts the four stack-box entities keep the expected authored X offsets.

### Build Impact
No runtime system changes are required. A normal Windows project rebuild is sufficient to package the updated scene.
