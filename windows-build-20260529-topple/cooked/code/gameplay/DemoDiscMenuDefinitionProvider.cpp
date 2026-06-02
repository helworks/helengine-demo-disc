#ifdef DrawText
#undef DrawText
#endif
#include "DemoDiscMenuDefinitionProvider.hpp"
#include "DemoDiscMenuTheme.hpp"
#include "DemoDiscSceneCatalog.hpp"
#include "MenuDefinition.hpp"
#include "MenuPanelDefinition.hpp"
#include "MenuItemDefinition.hpp"
#include "MenuActionDefinition.hpp"
#include "MenuActionKind.hpp"
#include "MenuOverlayImageDefinition.hpp"
#include "MenuPlatformInfoDefinition.hpp"
#include "byte4.hpp"
#include "runtime/native_string.hpp"
#include "runtime/array.hpp"
#include "runtime/native_list.hpp"
#include "DemoDiscPhysicsSceneEntry.hpp"
#include "runtime/array.hpp"
#include "runtime/finally.hpp"
#include "runtime/native_string.hpp"

::MenuDefinition* DemoDiscMenuDefinitionProvider::CreateMenuDefinition()
{
::DemoDiscMenuTheme *theme = new ::DemoDiscMenuTheme();
auto __localDeleteGuard_00000055 = he_cpp_make_scope_exit([&]() {
delete theme;
});
::DemoDiscSceneCatalog *sceneCatalog = new ::DemoDiscSceneCatalog();
auto __localDeleteGuard_00000056 = he_cpp_make_scope_exit([&]() {
delete sceneCatalog;
});
return ([&]() {
auto __ctor_arg_00000057 = String::Empty;
auto __ctor_arg_00000058 = String::Empty;
auto __ctor_arg_00000059 = "main";
auto __ctor_arg_0000005A = theme->get_TitleFontPath();
auto __ctor_arg_0000005B = theme->get_BodyFontPath();
auto __ctor_arg_0000005C = theme->get_BackgroundColor();
auto __ctor_arg_0000005D = theme->get_SurfaceColor();
auto __ctor_arg_0000005E = theme->get_SurfaceBorderColor();
auto __ctor_arg_0000005F = theme->get_AccentColor();
auto __ctor_arg_00000060 = theme->get_AccentSecondaryColor();
auto __ctor_arg_00000061 = theme->get_TextColor();
auto __ctor_arg_00000062 = theme->get_MutedTextColor();
auto __ctor_arg_00000063 = new Array<MenuPanelDefinition*>({ ([&]() {
auto __ctor_arg_00000064 = "main";
auto __ctor_arg_00000065 = "Main Menu";
auto __ctor_arg_00000066 = 6;
auto __ctor_arg_00000067 = new Array<MenuItemDefinition*>({ ([&]() {
auto __ctor_arg_00000068 = "main-scenes";
auto __ctor_arg_00000069 = "Demo Scenes";
auto __ctor_arg_0000006A = true;
auto __ctor_arg_0000006B = new ::MenuActionDefinition(MenuActionKind::OpenPanel, "scene-select");
return new ::MenuItemDefinition(__ctor_arg_00000068, __ctor_arg_00000069, __ctor_arg_0000006A, __ctor_arg_0000006B);
})(), ([&]() {
auto __ctor_arg_0000006C = "main-physics";
auto __ctor_arg_0000006D = "Physics Scenes";
auto __ctor_arg_0000006E = true;
auto __ctor_arg_0000006F = new ::MenuActionDefinition(MenuActionKind::OpenPanel, "physics-select");
return new ::MenuItemDefinition(__ctor_arg_0000006C, __ctor_arg_0000006D, __ctor_arg_0000006E, __ctor_arg_0000006F);
})(), ([&]() {
auto __ctor_arg_00000070 = "main-options";
auto __ctor_arg_00000071 = "Options";
auto __ctor_arg_00000072 = true;
auto __ctor_arg_00000073 = new ::MenuActionDefinition(MenuActionKind::OpenPanel, "options");
return new ::MenuItemDefinition(__ctor_arg_00000070, __ctor_arg_00000071, __ctor_arg_00000072, __ctor_arg_00000073);
})() });
return new ::MenuPanelDefinition(__ctor_arg_00000064, __ctor_arg_00000065, __ctor_arg_00000066, __ctor_arg_00000067);
})(), ([&]() {
auto __ctor_arg_00000074 = "scene-select";
auto __ctor_arg_00000075 = "Demo Scenes";
auto __ctor_arg_00000076 = 4;
auto __ctor_arg_00000077 = sceneCatalog->CreateDemoSceneItems();
return new ::MenuPanelDefinition(__ctor_arg_00000074, __ctor_arg_00000075, __ctor_arg_00000076, __ctor_arg_00000077);
})(), ([&]() {
auto __ctor_arg_00000078 = "physics-select";
auto __ctor_arg_00000079 = "Physics Scenes";
auto __ctor_arg_0000007A = 4;
auto __ctor_arg_0000007B = sceneCatalog->CreatePhysicsSceneItems();
return new ::MenuPanelDefinition(__ctor_arg_00000078, __ctor_arg_00000079, __ctor_arg_0000007A, __ctor_arg_0000007B);
})(), ([&]() {
auto __ctor_arg_0000007C = "options";
auto __ctor_arg_0000007D = "Options";
auto __ctor_arg_0000007E = 6;
auto __ctor_arg_0000007F = new Array<MenuItemDefinition*>({ ([&]() {
auto __ctor_arg_00000080 = "options-display";
auto __ctor_arg_00000081 = "Display";
auto __ctor_arg_00000082 = true;
auto __ctor_arg_00000083 = new ::MenuActionDefinition(MenuActionKind::None, String::Empty);
return new ::MenuItemDefinition(__ctor_arg_00000080, __ctor_arg_00000081, __ctor_arg_00000082, __ctor_arg_00000083);
})(), ([&]() {
auto __ctor_arg_00000084 = "options-audio";
auto __ctor_arg_00000085 = "Audio";
auto __ctor_arg_00000086 = true;
auto __ctor_arg_00000087 = new ::MenuActionDefinition(MenuActionKind::None, String::Empty);
return new ::MenuItemDefinition(__ctor_arg_00000084, __ctor_arg_00000085, __ctor_arg_00000086, __ctor_arg_00000087);
})(), ([&]() {
auto __ctor_arg_00000088 = "options-controls";
auto __ctor_arg_00000089 = "Controls";
auto __ctor_arg_0000008A = true;
auto __ctor_arg_0000008B = new ::MenuActionDefinition(MenuActionKind::None, String::Empty);
return new ::MenuItemDefinition(__ctor_arg_00000088, __ctor_arg_00000089, __ctor_arg_0000008A, __ctor_arg_0000008B);
})(), ([&]() {
auto __ctor_arg_0000008C = "options-back";
auto __ctor_arg_0000008D = "Back";
auto __ctor_arg_0000008E = true;
auto __ctor_arg_0000008F = new ::MenuActionDefinition(MenuActionKind::Back, String::Empty);
return new ::MenuItemDefinition(__ctor_arg_0000008C, __ctor_arg_0000008D, __ctor_arg_0000008E, __ctor_arg_0000008F);
})() });
return new ::MenuPanelDefinition(__ctor_arg_0000007C, __ctor_arg_0000007D, __ctor_arg_0000007E, __ctor_arg_0000007F);
})() });
auto __ctor_arg_00000090 = new ::MenuOverlayImageDefinition(theme->get_LogoTexturePath(), theme->get_LogoWidth(), theme->get_LogoHeight(), theme->get_LogoBottomMargin(), theme->get_LogoRightMargin());
auto __ctor_arg_00000091 = new ::MenuPlatformInfoDefinition(theme->get_PlatformInfoTopMargin(), theme->get_PlatformInfoRightMargin(), theme->get_PlatformInfoLineSpacing());
return new ::MenuDefinition(__ctor_arg_00000057, __ctor_arg_00000058, __ctor_arg_00000059, __ctor_arg_0000005A, __ctor_arg_0000005B, __ctor_arg_0000005C, __ctor_arg_0000005D, __ctor_arg_0000005E, __ctor_arg_0000005F, __ctor_arg_00000060, __ctor_arg_00000061, __ctor_arg_00000062, __ctor_arg_00000063, __ctor_arg_00000090, __ctor_arg_00000091);
})();}

