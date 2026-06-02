#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

class MenuActionDefinition;

#include "runtime/native_string.hpp"

class MenuItemDefinition
{
public:
    virtual ~MenuItemDefinition() = default;

    ::MenuActionDefinition* Action;

    ::MenuActionDefinition* get_Action();

    bool Enabled;

    bool get_Enabled();

    std::string ItemId;

    const std::string& get_ItemId();

    std::string Label;

    const std::string& get_Label();

    MenuItemDefinition(std::string itemId, std::string label, bool enabled, ::MenuActionDefinition* action);
};
