#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

class MenuPanelComponent;
class MenuItemRuntime;

#include "runtime/array.hpp"

class MenuPanelRuntime
{
public:
    virtual ~MenuPanelRuntime() = default;

    ::MenuPanelComponent* Definition;

    ::MenuPanelComponent* get_Definition();

    Array<::MenuItemRuntime*>* Items;

    Array<::MenuItemRuntime*>* get_Items();

    Entity* ItemsRootEntity;

    Entity* get_ItemsRootEntity();

    ScrollComponent* ItemsScrollComponent;

    ScrollComponent* get_ItemsScrollComponent();

    Entity* RootEntity;

    Entity* get_RootEntity();

    int32_t SelectedItemIndex;

    int32_t get_SelectedItemIndex();
    void set_SelectedItemIndex(int32_t value);

    MenuPanelRuntime(::MenuPanelComponent* definition, Entity* rootEntity, Entity* itemsRootEntity, ScrollComponent* itemsScrollComponent, Array<::MenuItemRuntime*>* items);
};
