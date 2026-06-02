#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

class MenuItemComponent;

#include "RoundedRectComponent.hpp"
#include "Entity.hpp"

class MenuItemRuntime
{
public:
    virtual ~MenuItemRuntime() = default;

    RoundedRectComponent* Background;

    RoundedRectComponent* get_Background();

    ::MenuItemComponent* Definition;

    ::MenuItemComponent* get_Definition();

    int32_t Index;

    int32_t get_Index();

    Entity* OwnerEntity;

    Entity* get_OwnerEntity();

    MenuItemRuntime(::MenuItemComponent* definition, int32_t index, Entity* entity, RoundedRectComponent* background);
};
