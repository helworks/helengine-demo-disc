#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

class MenuItemDefinition;
class MenuHostPanelRuntime;

#include "ButtonComponent.hpp"
#include "Entity.hpp"

class MenuHostItemRuntime
{
public:
    virtual ~MenuHostItemRuntime() = default;

    ButtonComponent* Button;

    ButtonComponent* get_Button();

    ::MenuItemDefinition* Definition;

    ::MenuItemDefinition* get_Definition();

    Entity* Entity;

    Entity* get_Entity();

    int32_t Index;

    int32_t get_Index();

    ::MenuHostPanelRuntime* Panel;

    ::MenuHostPanelRuntime* get_Panel();

    MenuHostItemRuntime(::MenuHostPanelRuntime* panel, ::MenuItemDefinition* definition, int32_t index, Entity* entity, ButtonComponent* button);
};
