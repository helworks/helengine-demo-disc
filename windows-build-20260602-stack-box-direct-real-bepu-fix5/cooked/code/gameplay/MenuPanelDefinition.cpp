#ifdef DrawText
#undef DrawText
#endif
#include "MenuPanelDefinition.hpp"
#include "runtime/native_string.hpp"
#include "runtime/array.hpp"
#include "MenuItemDefinition.hpp"
#include "runtime/native_exceptions.hpp"
#include "MenuPanelDefinition.hpp"
#include "runtime/array.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_string.hpp"

const std::string& MenuPanelDefinition::get_Heading()
{
return this->Heading;
}

Array<::MenuItemDefinition*>* MenuPanelDefinition::get_Items()
{
return this->Items;
}

const std::string& MenuPanelDefinition::get_PanelId()
{
return this->PanelId;
}

int32_t MenuPanelDefinition::get_VisibleItemCount()
{
return this->VisibleItemCount;
}

MenuPanelDefinition::MenuPanelDefinition(std::string panelId, std::string heading, int32_t visibleItemCount, Array<::MenuItemDefinition*>* items) : Heading(), Items(), PanelId(), VisibleItemCount(0)
{
    if (String::IsNullOrWhiteSpace(panelId))
    {
throw ([&]() {
auto __ctor_arg_00000048 = "Panel id must be provided.";
auto __ctor_arg_00000049 = "panelId";
return new ArgumentException(__ctor_arg_00000048, __ctor_arg_00000049);
})();
    }
    if (String::IsNullOrWhiteSpace(heading))
    {
throw ([&]() {
auto __ctor_arg_0000004A = "Panel heading must be provided.";
auto __ctor_arg_0000004B = "heading";
return new ArgumentException(__ctor_arg_0000004A, __ctor_arg_0000004B);
})();
    }
    if (visibleItemCount < 1)
    {
throw ([&]() {
auto __ctor_arg_0000004C = "visibleItemCount";
auto __ctor_arg_0000004D = "Visible item count must be at least one.";
return new ArgumentOutOfRangeException(__ctor_arg_0000004C, __ctor_arg_0000004D);
})();
    }
    if (items == nullptr)
    {
throw new ArgumentNullException("items");
    }
    if (items->get_Length() == 0)
    {
throw new InvalidOperationException("Menu panels must contain at least one item.");
    }
this->PanelId = panelId;
this->Heading = heading;
this->VisibleItemCount = visibleItemCount;
this->Items = items;
}

