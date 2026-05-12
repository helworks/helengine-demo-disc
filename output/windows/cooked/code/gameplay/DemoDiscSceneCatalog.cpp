#ifdef DrawText
#undef DrawText
#endif
#include "DemoDiscSceneCatalog.hpp"
#include "runtime/array.hpp"
#include "runtime/array.hpp"
#include "runtime/native_string.hpp"

Array<MenuItemDefinition*>* DemoDiscSceneCatalog::CreateSceneItems()
{
return new Array<MenuItemDefinition*>({ ([&]() {
auto __ctor_arg_00000000 = "scene-cube-test";
auto __ctor_arg_00000001 = "Cube Test";
auto __ctor_arg_00000002 = "Minimal one-cube rendering validation scene.";
auto __ctor_arg_00000003 = true;
auto __ctor_arg_00000004 = new MenuActionDefinition(MenuActionKind->LoadScene, "cube_test");
return new MenuItemDefinition(__ctor_arg_00000000, __ctor_arg_00000001, __ctor_arg_00000002, __ctor_arg_00000003, __ctor_arg_00000004);
})(), ([&]() {
auto __ctor_arg_00000005 = "scene-colored-cube-grid";
auto __ctor_arg_00000006 = "Colored Cube Grid";
auto __ctor_arg_00000007 = "Sixteen rotating cubes with distinct lit material colors.";
auto __ctor_arg_00000008 = true;
auto __ctor_arg_00000009 = new MenuActionDefinition(MenuActionKind->LoadScene, "colored_cube_grid");
return new MenuItemDefinition(__ctor_arg_00000005, __ctor_arg_00000006, __ctor_arg_00000007, __ctor_arg_00000008, __ctor_arg_00000009);
})(), ([&]() {
auto __ctor_arg_0000000A = "scene-textured-cube-grid";
auto __ctor_arg_0000000B = "Textured Cube Grid";
auto __ctor_arg_0000000C = "Sixteen rotating cubes with distinct lit texture materials.";
auto __ctor_arg_0000000D = true;
auto __ctor_arg_0000000E = new MenuActionDefinition(MenuActionKind->LoadScene, "textured_cube_grid");
return new MenuItemDefinition(__ctor_arg_0000000A, __ctor_arg_0000000B, __ctor_arg_0000000C, __ctor_arg_0000000D, __ctor_arg_0000000E);
})(), ([&]() {
auto __ctor_arg_0000000F = "scene-directional-shadow-plaza";
auto __ctor_arg_00000010 = "Directional Shadow Plaza";
auto __ctor_arg_00000011 = "Directional light showcase scene with shadowed plaza lighting.";
auto __ctor_arg_00000012 = true;
auto __ctor_arg_00000013 = new MenuActionDefinition(MenuActionKind->LoadScene, "directional_shadow_plaza");
return new MenuItemDefinition(__ctor_arg_0000000F, __ctor_arg_00000010, __ctor_arg_00000011, __ctor_arg_00000012, __ctor_arg_00000013);
})(), ([&]() {
auto __ctor_arg_00000014 = "scene-spotlight-street-slice";
auto __ctor_arg_00000015 = "Spotlight Street Slice";
auto __ctor_arg_00000016 = "Spotlight showcase scene with a narrow street and bright pool lighting.";
auto __ctor_arg_00000017 = true;
auto __ctor_arg_00000018 = new MenuActionDefinition(MenuActionKind->LoadScene, "spotlight_street_slice");
return new MenuItemDefinition(__ctor_arg_00000014, __ctor_arg_00000015, __ctor_arg_00000016, __ctor_arg_00000017, __ctor_arg_00000018);
})(), ([&]() {
auto __ctor_arg_00000019 = "scene-back";
auto __ctor_arg_0000001A = "Back";
auto __ctor_arg_0000001B = "Returns to the main menu.";
auto __ctor_arg_0000001C = true;
auto __ctor_arg_0000001D = new MenuActionDefinition(MenuActionKind->Back, String::Empty);
return new MenuItemDefinition(__ctor_arg_00000019, __ctor_arg_0000001A, __ctor_arg_0000001B, __ctor_arg_0000001C, __ctor_arg_0000001D);
})() });}

