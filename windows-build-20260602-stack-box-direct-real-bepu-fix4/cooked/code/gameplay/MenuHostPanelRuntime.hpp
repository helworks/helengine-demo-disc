#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

class MenuPanelDefinition;
class MenuHostItemRuntime;

#include "runtime/array.hpp"

class MenuHostPanelRuntime
{
public:
    virtual ~MenuHostPanelRuntime() = default;

    ::MenuPanelDefinition* Definition;

    ::MenuPanelDefinition* get_Definition();

    Array<::MenuHostItemRuntime*>* Items;

    Array<::MenuHostItemRuntime*>* get_Items();

    Entity* RootEntity;

    Entity* get_RootEntity();

    int32_t ScrollOffset;

    int32_t get_ScrollOffset();
    void set_ScrollOffset(int32_t value);

    int32_t SelectedItemIndex;

    int32_t get_SelectedItemIndex();
    void set_SelectedItemIndex(int32_t value);

    MenuHostPanelRuntime(::MenuPanelDefinition* definition, Entity* rootEntity, Array<::MenuHostItemRuntime*>* items);
};
