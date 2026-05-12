#ifdef DrawText
#undef DrawText
#endif
#include "DemoDiscMenuDefinitionProvider.hpp"
#include "DemoDiscMenuTheme.hpp"
#include "DemoDiscSceneCatalog.hpp"
#include "MenuDefinition.hpp"
#include "runtime/native_string.hpp"

MenuDefinition* DemoDiscMenuDefinitionProvider::CreateMenuDefinition()
{
::DemoDiscMenuTheme *theme = new ::DemoDiscMenuTheme();
::DemoDiscSceneCatalog *sceneCatalog = new ::DemoDiscSceneCatalog();
return ([&]() {
auto __ctor_arg_0000001E = String::Empty;
auto __ctor_arg_0000001F = String::Empty;
auto __ctor_arg_00000020 = "main";
auto __ctor_arg_00000021 = theme->get_TitleFontPath();
auto __ctor_arg_00000022 = theme->get_BodyFontPath();
auto __ctor_arg_00000023 = theme->get_BackgroundColor();
auto __ctor_arg_00000024 = theme->get_SurfaceColor();
auto __ctor_arg_00000025 = theme->get_SurfaceBorderColor();
auto __ctor_arg_00000026 = theme->get_AccentColor();
auto __ctor_arg_00000027 = theme->get_AccentSecondaryColor();
auto __ctor_arg_00000028 = theme->get_TextColor();
auto __ctor_arg_00000029 = theme->get_MutedTextColor();
auto __ctor_arg_0000002A = new Array<MenuPanelDefinition*>({ ([&]() {
auto __ctor_arg_0000002B = "main";
auto __ctor_arg_0000002C = "Main Menu";
auto __ctor_arg_0000002D = "Pick a destination or peek at the menu shell.";
auto __ctor_arg_0000002E = 6;
auto __ctor_arg_0000002F = new Array<MenuItemDefinition*>({ ([&]() {
auto __ctor_arg_00000030 = "main-scenes";
auto __ctor_arg_00000031 = "Select Scene";
auto __ctor_arg_00000032 = "Browse the curated demo-disc lineup.";
auto __ctor_arg_00000033 = true;
auto __ctor_arg_00000034 = new MenuActionDefinition(MenuActionKind->OpenPanel, "scene-select");
return new MenuItemDefinition(__ctor_arg_00000030, __ctor_arg_00000031, __ctor_arg_00000032, __ctor_arg_00000033, __ctor_arg_00000034);
})(), ([&]() {
auto __ctor_arg_00000035 = "main-options";
auto __ctor_arg_00000036 = "Options";
auto __ctor_arg_00000037 = "Preview the reusable options shell layout.";
auto __ctor_arg_00000038 = true;
auto __ctor_arg_00000039 = new MenuActionDefinition(MenuActionKind->OpenPanel, "options");
return new MenuItemDefinition(__ctor_arg_00000035, __ctor_arg_00000036, __ctor_arg_00000037, __ctor_arg_00000038, __ctor_arg_00000039);
})() });
return new MenuPanelDefinition(__ctor_arg_0000002B, __ctor_arg_0000002C, __ctor_arg_0000002D, __ctor_arg_0000002E, __ctor_arg_0000002F);
})(), ([&]() {
auto __ctor_arg_0000003A = "scene-select";
auto __ctor_arg_0000003B = "Select Scene";
auto __ctor_arg_0000003C = "Every entry here is explicitly curated and ordered from city-side code.";
auto __ctor_arg_0000003D = 4;
auto __ctor_arg_0000003E = sceneCatalog->CreateSceneItems();
return new MenuPanelDefinition(__ctor_arg_0000003A, __ctor_arg_0000003B, __ctor_arg_0000003C, __ctor_arg_0000003D, __ctor_arg_0000003E);
})(), ([&]() {
auto __ctor_arg_0000003F = "options";
auto __ctor_arg_00000040 = "Options";
auto __ctor_arg_00000041 = "Polished shell for future settings categories.";
auto __ctor_arg_00000042 = 6;
auto __ctor_arg_00000043 = new Array<MenuItemDefinition*>({ ([&]() {
auto __ctor_arg_00000044 = "options-display";
auto __ctor_arg_00000045 = "Display";
auto __ctor_arg_00000046 = "Placeholder row for future video settings.";
auto __ctor_arg_00000047 = true;
auto __ctor_arg_00000048 = new MenuActionDefinition(MenuActionKind->None, String::Empty);
return new MenuItemDefinition(__ctor_arg_00000044, __ctor_arg_00000045, __ctor_arg_00000046, __ctor_arg_00000047, __ctor_arg_00000048);
})(), ([&]() {
auto __ctor_arg_00000049 = "options-audio";
auto __ctor_arg_0000004A = "Audio";
auto __ctor_arg_0000004B = "Placeholder row for future volume settings.";
auto __ctor_arg_0000004C = true;
auto __ctor_arg_0000004D = new MenuActionDefinition(MenuActionKind->None, String::Empty);
return new MenuItemDefinition(__ctor_arg_00000049, __ctor_arg_0000004A, __ctor_arg_0000004B, __ctor_arg_0000004C, __ctor_arg_0000004D);
})(), ([&]() {
auto __ctor_arg_0000004E = "options-controls";
auto __ctor_arg_0000004F = "Controls";
auto __ctor_arg_00000050 = "Placeholder row for future input remapping.";
auto __ctor_arg_00000051 = true;
auto __ctor_arg_00000052 = new MenuActionDefinition(MenuActionKind->None, String::Empty);
return new MenuItemDefinition(__ctor_arg_0000004E, __ctor_arg_0000004F, __ctor_arg_00000050, __ctor_arg_00000051, __ctor_arg_00000052);
})(), ([&]() {
auto __ctor_arg_00000053 = "options-back";
auto __ctor_arg_00000054 = "Back";
auto __ctor_arg_00000055 = "Returns to the main menu.";
auto __ctor_arg_00000056 = true;
auto __ctor_arg_00000057 = new MenuActionDefinition(MenuActionKind->Back, String::Empty);
return new MenuItemDefinition(__ctor_arg_00000053, __ctor_arg_00000054, __ctor_arg_00000055, __ctor_arg_00000056, __ctor_arg_00000057);
})() });
return new MenuPanelDefinition(__ctor_arg_0000003F, __ctor_arg_00000040, __ctor_arg_00000041, __ctor_arg_00000042, __ctor_arg_00000043);
})() });
return new MenuDefinition(__ctor_arg_0000001E, __ctor_arg_0000001F, __ctor_arg_00000020, __ctor_arg_00000021, __ctor_arg_00000022, __ctor_arg_00000023, __ctor_arg_00000024, __ctor_arg_00000025, __ctor_arg_00000026, __ctor_arg_00000027, __ctor_arg_00000028, __ctor_arg_00000029, __ctor_arg_0000002A);
})();}

