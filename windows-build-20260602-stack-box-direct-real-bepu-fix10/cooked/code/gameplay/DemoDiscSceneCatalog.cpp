#ifdef DrawText
#undef DrawText
#endif
#include "DemoDiscSceneCatalog.hpp"
#include "runtime/array.hpp"
#include "runtime/native_list.hpp"
#include "MenuItemDefinition.hpp"
#include "MenuActionDefinition.hpp"
#include "MenuActionKind.hpp"
#include "DemoDiscPhysicsSceneEntry.hpp"
#include "runtime/native_string.hpp"
#include "DemoDiscSceneCatalog.hpp"
#include "runtime/array.hpp"
#include "runtime/native_list.hpp"
#include "runtime/native_string.hpp"

Array<::MenuItemDefinition*>* DemoDiscSceneCatalog::CreateDemoSceneItems()
{
return new Array<MenuItemDefinition*>({ ([&]() {
auto __ctor_arg_00000008 = "scene-cube-test";
auto __ctor_arg_00000009 = "Cube Test";
auto __ctor_arg_0000000A = true;
auto __ctor_arg_0000000B = new ::MenuActionDefinition(MenuActionKind::LoadScene, "cube_test");
return new ::MenuItemDefinition(__ctor_arg_00000008, __ctor_arg_00000009, __ctor_arg_0000000A, __ctor_arg_0000000B);
})(), ([&]() {
auto __ctor_arg_0000000C = "scene-scaled-cube";
auto __ctor_arg_0000000D = "Scaled Cube";
auto __ctor_arg_0000000E = true;
auto __ctor_arg_0000000F = new ::MenuActionDefinition(MenuActionKind::LoadScene, "scaled_cube");
return new ::MenuItemDefinition(__ctor_arg_0000000C, __ctor_arg_0000000D, __ctor_arg_0000000E, __ctor_arg_0000000F);
})(), ([&]() {
auto __ctor_arg_00000010 = "scene-colored-cube-grid";
auto __ctor_arg_00000011 = "Colored Cubes";
auto __ctor_arg_00000012 = true;
auto __ctor_arg_00000013 = new ::MenuActionDefinition(MenuActionKind::LoadScene, "colored_cube_grid");
return new ::MenuItemDefinition(__ctor_arg_00000010, __ctor_arg_00000011, __ctor_arg_00000012, __ctor_arg_00000013);
})(), ([&]() {
auto __ctor_arg_00000014 = "scene-textured-cube-grid";
auto __ctor_arg_00000015 = "Textured Cubes";
auto __ctor_arg_00000016 = true;
auto __ctor_arg_00000017 = new ::MenuActionDefinition(MenuActionKind::LoadScene, "textured_cube_grid");
return new ::MenuItemDefinition(__ctor_arg_00000014, __ctor_arg_00000015, __ctor_arg_00000016, __ctor_arg_00000017);
})(), ([&]() {
auto __ctor_arg_00000018 = "scene-axis-test";
auto __ctor_arg_00000019 = "Axis 1";
auto __ctor_arg_0000001A = true;
auto __ctor_arg_0000001B = new ::MenuActionDefinition(MenuActionKind::LoadScene, "axis_test");
return new ::MenuItemDefinition(__ctor_arg_00000018, __ctor_arg_00000019, __ctor_arg_0000001A, __ctor_arg_0000001B);
})(), ([&]() {
auto __ctor_arg_0000001C = "scene-axis-test-2";
auto __ctor_arg_0000001D = "Axis 2";
auto __ctor_arg_0000001E = true;
auto __ctor_arg_0000001F = new ::MenuActionDefinition(MenuActionKind::LoadScene, "axis_test2");
return new ::MenuItemDefinition(__ctor_arg_0000001C, __ctor_arg_0000001D, __ctor_arg_0000001E, __ctor_arg_0000001F);
})(), ([&]() {
auto __ctor_arg_00000020 = "scene-directional-shadow-plaza";
auto __ctor_arg_00000021 = "Directional Shadow Plaza";
auto __ctor_arg_00000022 = true;
auto __ctor_arg_00000023 = new ::MenuActionDefinition(MenuActionKind::LoadScene, "directional_shadow_plaza");
return new ::MenuItemDefinition(__ctor_arg_00000020, __ctor_arg_00000021, __ctor_arg_00000022, __ctor_arg_00000023);
})(), ([&]() {
auto __ctor_arg_00000024 = "scene-back";
auto __ctor_arg_00000025 = "Back";
auto __ctor_arg_00000026 = true;
auto __ctor_arg_00000027 = new ::MenuActionDefinition(MenuActionKind::Back, String::Empty);
return new ::MenuItemDefinition(__ctor_arg_00000024, __ctor_arg_00000025, __ctor_arg_00000026, __ctor_arg_00000027);
})() });}

List<::DemoDiscPhysicsSceneEntry*>* DemoDiscSceneCatalog::CreatePhysicsSceneEntries()
{
return new List<DemoDiscPhysicsSceneEntry*>({ new ::DemoDiscPhysicsSceneEntry("physics-dynamic-stack-boxes", "Stacked Boxes", "test_scene_dynamic_stack_boxes", "test_scene_dynamic_stack_boxes_ds"), new ::DemoDiscPhysicsSceneEntry("physics-dynamic-sphere-stack", "Sphere Stack", "test_scene_dynamic_sphere_stack", "test_scene_dynamic_sphere_stack_ds"), new ::DemoDiscPhysicsSceneEntry("physics-dynamic-mixed-stack", "Mixed Stack", "test_scene_dynamic_mixed_stack", "test_scene_dynamic_mixed_stack_ds") });}

Array<::MenuItemDefinition*>* DemoDiscSceneCatalog::CreatePhysicsSceneItems()
{
List<::DemoDiscPhysicsSceneEntry*> *physicsSceneEntries = this->CreatePhysicsSceneEntries();
Array<::MenuItemDefinition*> *items = new Array<MenuItemDefinition*>(physicsSceneEntries->get_Count() + 1);
for (int32_t index = 0; index < physicsSceneEntries->get_Count(); index++) {
::DemoDiscPhysicsSceneEntry *sceneEntry = (*physicsSceneEntries).get_Item(index);
(*items)[index] = ([&]() {
auto __ctor_arg_00000028 = sceneEntry->MenuItemId;
auto __ctor_arg_00000029 = sceneEntry->DisplayName;
auto __ctor_arg_0000002A = true;
auto __ctor_arg_0000002B = new ::MenuActionDefinition(MenuActionKind::LoadScene, sceneEntry->SceneId);
return new ::MenuItemDefinition(__ctor_arg_00000028, __ctor_arg_00000029, __ctor_arg_0000002A, __ctor_arg_0000002B);
})();
}
(*items)[physicsSceneEntries->get_Count()] = ([&]() {
auto __ctor_arg_0000002C = "physics-back";
auto __ctor_arg_0000002D = "Back";
auto __ctor_arg_0000002E = true;
auto __ctor_arg_0000002F = new ::MenuActionDefinition(MenuActionKind::Back, String::Empty);
return new ::MenuItemDefinition(__ctor_arg_0000002C, __ctor_arg_0000002D, __ctor_arg_0000002E, __ctor_arg_0000002F);
})();
return items;}

