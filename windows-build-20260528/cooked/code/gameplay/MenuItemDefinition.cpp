#ifdef DrawText
#undef DrawText
#endif
#include "MenuItemDefinition.hpp"
#include "MenuActionDefinition.hpp"
#include "runtime/native_string.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_string.hpp"

::MenuActionDefinition* MenuItemDefinition::get_Action()
{
return this->Action;
}

bool MenuItemDefinition::get_Enabled()
{
return this->Enabled;
}

const std::string& MenuItemDefinition::get_ItemId()
{
return this->ItemId;
}

const std::string& MenuItemDefinition::get_Label()
{
return this->Label;
}

MenuItemDefinition::MenuItemDefinition(std::string itemId, std::string label, bool enabled, ::MenuActionDefinition* action) : Action(), Enabled(), ItemId(), Label()
{
    if (String::IsNullOrWhiteSpace(itemId))
    {
throw ([&]() {
auto __ctor_arg_0000003A = "Menu item id must be provided.";
auto __ctor_arg_0000003B = "itemId";
return new ArgumentException(__ctor_arg_0000003A, __ctor_arg_0000003B);
})();
    }
    if (String::IsNullOrWhiteSpace(label))
    {
throw ([&]() {
auto __ctor_arg_0000003C = "Menu item label must be provided.";
auto __ctor_arg_0000003D = "label";
return new ArgumentException(__ctor_arg_0000003C, __ctor_arg_0000003D);
})();
    }
    if (action == nullptr)
    {
throw new ArgumentNullException("action");
    }
this->ItemId = itemId;
this->Label = label;
this->Enabled = enabled;
this->Action = action;
}

