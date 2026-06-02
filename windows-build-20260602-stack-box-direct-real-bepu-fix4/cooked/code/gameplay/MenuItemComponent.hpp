#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "runtime/native_disposable.hpp"
#include "runtime/native_string.hpp"
#include "MenuActionKind.hpp"

class MenuItemComponent : public ::Component
{
public:
    virtual ~MenuItemComponent() = default;

    inline static const uint8_t CurrentVersion = 1;

    inline static const std::string SerializedComponentTypeId = "city.menu.MenuItemComponent, gameplay";

    ::MenuActionKind ActionKind;

    ::MenuActionKind get_ActionKind();
    void set_ActionKind(::MenuActionKind value);

    byte4 IdleBorderColor;

    byte4 get_IdleBorderColor();
    void set_IdleBorderColor(byte4 value);

    byte4 IdleFillColor;

    byte4 get_IdleFillColor();
    void set_IdleFillColor(byte4 value);

    const std::string& get_ItemId();

    void set_ItemId(std::string value);

    const std::string& get_PanelId();

    void set_PanelId(std::string value);

    byte4 SelectedBorderColor;

    byte4 get_SelectedBorderColor();
    void set_SelectedBorderColor(byte4 value);

    byte4 SelectedFillColor;

    byte4 get_SelectedFillColor();
    void set_SelectedFillColor(byte4 value);

    const std::string& get_TargetId();

    void set_TargetId(std::string value);

    MenuItemComponent();
private:
    std::string ItemIdValue;

    std::string PanelIdValue;

    std::string TargetIdValue;
};
