#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "runtime/native_disposable.hpp"
#include "runtime/native_string.hpp"

class MenuPanelComponent : public ::Component
{
public:
    virtual ~MenuPanelComponent() = default;

    inline static const uint8_t CurrentVersion = 1;

    inline static const std::string SerializedComponentTypeId = "city.menu.MenuPanelComponent, gameplay";

    const std::string& get_PanelId();

    void set_PanelId(std::string value);

    MenuPanelComponent();
private:
    std::string PanelIdValue;
};
