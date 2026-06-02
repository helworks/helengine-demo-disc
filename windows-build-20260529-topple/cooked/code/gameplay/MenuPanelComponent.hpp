#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "Component.hpp"
#include "runtime/native_disposable.hpp"
#include "runtime/native_string.hpp"

class MenuPanelComponent : public Component
{
public:
    virtual ~MenuPanelComponent() = default;

    static uint8_t CurrentVersion;

    static std::string SerializedComponentTypeId;

    const std::string& get_PanelId();

    void set_PanelId(std::string value);

    MenuPanelComponent();
private:
    std::string PanelIdValue;
};
