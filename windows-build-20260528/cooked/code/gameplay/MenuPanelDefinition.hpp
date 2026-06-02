#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

class MenuItemDefinition;

#include "runtime/native_string.hpp"
#include "runtime/array.hpp"

class MenuPanelDefinition
{
public:
    virtual ~MenuPanelDefinition() = default;

    std::string Heading;

    const std::string& get_Heading();

    Array<::MenuItemDefinition*>* Items;

    Array<::MenuItemDefinition*>* get_Items();

    std::string PanelId;

    const std::string& get_PanelId();

    int32_t VisibleItemCount;

    int32_t get_VisibleItemCount();

    MenuPanelDefinition(std::string panelId, std::string heading, int32_t visibleItemCount, Array<::MenuItemDefinition*>* items);
};
