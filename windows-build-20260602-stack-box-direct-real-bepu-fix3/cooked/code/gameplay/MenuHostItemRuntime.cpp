#ifdef DrawText
#undef DrawText
#endif
#include "MenuHostItemRuntime.hpp"
#include "MenuItemDefinition.hpp"
#include "MenuHostPanelRuntime.hpp"
#include "runtime/native_exceptions.hpp"
#include "MenuHostItemRuntime.hpp"
#include "runtime/native_exceptions.hpp"

ButtonComponent* MenuHostItemRuntime::get_Button()
{
return this->Button;
}

::MenuItemDefinition* MenuHostItemRuntime::get_Definition()
{
return this->Definition;
}

Entity* MenuHostItemRuntime::get_Entity()
{
return this->Entity;
}

int32_t MenuHostItemRuntime::get_Index()
{
return this->Index;
}

::MenuHostPanelRuntime* MenuHostItemRuntime::get_Panel()
{
return this->Panel;
}

MenuHostItemRuntime::MenuHostItemRuntime(::MenuHostPanelRuntime* panel, ::MenuItemDefinition* definition, int32_t index, Entity* entity, ButtonComponent* button) : Button(), Definition(), Entity(), Index(0), Panel()
{
this->Panel = (panel != nullptr ? panel : throw new ArgumentNullException("panel"));
this->Definition = (definition != nullptr ? definition : throw new ArgumentNullException("definition"));
    if (index < 0)
    {
throw ([&]() {
auto __ctor_arg_00000038 = "index";
auto __ctor_arg_00000039 = "Menu item index must be non-negative.";
return new ArgumentOutOfRangeException(__ctor_arg_00000038, __ctor_arg_00000039);
})();
    }
this->Index = index;
this->Entity = (entity != nullptr ? entity : throw new ArgumentNullException("entity"));
this->Button = (button != nullptr ? button : throw new ArgumentNullException("button"));
}

