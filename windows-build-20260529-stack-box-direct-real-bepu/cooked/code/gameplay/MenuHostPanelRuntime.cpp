#ifdef DrawText
#undef DrawText
#endif
#include "MenuHostPanelRuntime.hpp"
#include "MenuPanelDefinition.hpp"
#include "runtime/array.hpp"
#include "MenuHostItemRuntime.hpp"
#include "runtime/native_exceptions.hpp"
#include "MenuHostPanelRuntime.hpp"
#include "runtime/array.hpp"
#include "runtime/native_exceptions.hpp"

::MenuPanelDefinition* MenuHostPanelRuntime::get_Definition()
{
return this->Definition;
}

Array<::MenuHostItemRuntime*>* MenuHostPanelRuntime::get_Items()
{
return this->Items;
}

Entity* MenuHostPanelRuntime::get_RootEntity()
{
return this->RootEntity;
}

int32_t MenuHostPanelRuntime::get_ScrollOffset()
{
return this->ScrollOffset;
}

void MenuHostPanelRuntime::set_ScrollOffset(int32_t value)
{
this->ScrollOffset = value;
}

int32_t MenuHostPanelRuntime::get_SelectedItemIndex()
{
return this->SelectedItemIndex;
}

void MenuHostPanelRuntime::set_SelectedItemIndex(int32_t value)
{
this->SelectedItemIndex = value;
}

MenuHostPanelRuntime::MenuHostPanelRuntime(::MenuPanelDefinition* definition, Entity* rootEntity, Array<::MenuHostItemRuntime*>* items) : Definition(), Items(), RootEntity(), ScrollOffset(0), SelectedItemIndex(0)
{
this->Definition = (definition != nullptr ? definition : throw new ArgumentNullException("definition"));
this->RootEntity = (rootEntity != nullptr ? rootEntity : throw new ArgumentNullException("rootEntity"));
this->Items = (items != nullptr ? items : throw new ArgumentNullException("items"));
this->set_SelectedItemIndex(-1);
}

