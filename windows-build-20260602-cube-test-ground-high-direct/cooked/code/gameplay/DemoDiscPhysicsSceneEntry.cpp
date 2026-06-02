#ifdef DrawText
#undef DrawText
#endif
#include "DemoDiscPhysicsSceneEntry.hpp"
#include "runtime/native_string.hpp"
#include "runtime/native_exceptions.hpp"
#include "DemoDiscPhysicsSceneEntry.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_string.hpp"

const std::string& DemoDiscPhysicsSceneEntry::get_DisplayName()
{
return this->DisplayName;
}

const std::string& DemoDiscPhysicsSceneEntry::get_MenuItemId()
{
return this->MenuItemId;
}

const std::string& DemoDiscPhysicsSceneEntry::get_NintendoDsSceneId()
{
return this->NintendoDsSceneId;
}

const std::string& DemoDiscPhysicsSceneEntry::get_SceneId()
{
return this->SceneId;
}

DemoDiscPhysicsSceneEntry::DemoDiscPhysicsSceneEntry(std::string menuItemId, std::string displayName, std::string sceneId, std::string nintendoDsSceneId) : DisplayName(), MenuItemId(), NintendoDsSceneId(), SceneId()
{
    if (String::IsNullOrWhiteSpace(menuItemId))
    {
throw ([&]() {
auto __ctor_arg_00000000 = "Menu item id must be provided.";
auto __ctor_arg_00000001 = "menuItemId";
return new ArgumentException(__ctor_arg_00000000, __ctor_arg_00000001);
})();
    }
else {
    if (String::IsNullOrWhiteSpace(displayName))
    {
throw ([&]() {
auto __ctor_arg_00000002 = "Display name must be provided.";
auto __ctor_arg_00000003 = "displayName";
return new ArgumentException(__ctor_arg_00000002, __ctor_arg_00000003);
})();
    }
else {
    if (String::IsNullOrWhiteSpace(sceneId))
    {
throw ([&]() {
auto __ctor_arg_00000004 = "Scene id must be provided.";
auto __ctor_arg_00000005 = "sceneId";
return new ArgumentException(__ctor_arg_00000004, __ctor_arg_00000005);
})();
    }
else {
    if (String::IsNullOrWhiteSpace(nintendoDsSceneId))
    {
throw ([&]() {
auto __ctor_arg_00000006 = "Nintendo DS scene id must be provided.";
auto __ctor_arg_00000007 = "nintendoDsSceneId";
return new ArgumentException(__ctor_arg_00000006, __ctor_arg_00000007);
})();
    }
}
}
}
this->MenuItemId = menuItemId;
this->DisplayName = displayName;
this->SceneId = sceneId;
this->NintendoDsSceneId = nintendoDsSceneId;
}

