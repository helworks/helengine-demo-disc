#ifdef DrawText
#undef DrawText
#endif
#include "MenuPanelRuntime.hpp"
#include "MenuPanelComponent.hpp"
#include "runtime/array.hpp"
#include "MenuItemRuntime.hpp"
#include "runtime/native_exceptions.hpp"
#include "MenuPanelRuntime.hpp"
#include "runtime/array.hpp"
#include "runtime/native_exceptions.hpp"

::MenuPanelComponent* MenuPanelRuntime::get_Definition()
{
return this->Definition;
}

Array<::MenuItemRuntime*>* MenuPanelRuntime::get_Items()
{
return this->Items;
}

Entity* MenuPanelRuntime::get_ItemsRootEntity()
{
return this->ItemsRootEntity;
}

ScrollComponent* MenuPanelRuntime::get_ItemsScrollComponent()
{
return this->ItemsScrollComponent;
}

Entity* MenuPanelRuntime::get_RootEntity()
{
return this->RootEntity;
}

int32_t MenuPanelRuntime::get_SelectedItemIndex()
{
return this->SelectedItemIndex;
}

void MenuPanelRuntime::set_SelectedItemIndex(int32_t value)
{
this->SelectedItemIndex = value;
}

MenuPanelRuntime::MenuPanelRuntime(::MenuPanelComponent* definition, Entity* rootEntity, Entity* itemsRootEntity, ScrollComponent* itemsScrollComponent, Array<::MenuItemRuntime*>* items) : Definition(), Items(), ItemsRootEntity(), ItemsScrollComponent(), RootEntity(), SelectedItemIndex(0)
{
this->Definition = (definition != nullptr ? definition : throw new ArgumentNullException("definition"));
this->RootEntity = (rootEntity != nullptr ? rootEntity : throw new ArgumentNullException("rootEntity"));
this->ItemsRootEntity = (itemsRootEntity != nullptr ? itemsRootEntity : throw new ArgumentNullException("itemsRootEntity"));
this->ItemsScrollComponent = (itemsScrollComponent != nullptr ? itemsScrollComponent : throw new ArgumentNullException("itemsScrollComponent"));
this->Items = (items != nullptr ? items : throw new ArgumentNullException("items"));
this->set_SelectedItemIndex(-1);
}

