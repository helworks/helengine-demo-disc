#ifdef DrawText
#undef DrawText
#endif
#include "MenuItemComponent.hpp"
#include "runtime/native_string.hpp"
#include "MenuActionKind.hpp"
#include "byte4.hpp"
#include "runtime/native_string.hpp"

uint8_t MenuItemComponent::CurrentVersion = 1;

std::string MenuItemComponent::SerializedComponentTypeId = "city.menu.MenuItemComponent, gameplay";

::MenuActionKind MenuItemComponent::get_ActionKind()
{
return this->ActionKind;
}

void MenuItemComponent::set_ActionKind(::MenuActionKind value)
{
this->ActionKind = value;
}

byte4 MenuItemComponent::get_IdleBorderColor()
{
return this->IdleBorderColor;
}

void MenuItemComponent::set_IdleBorderColor(byte4 value)
{
this->IdleBorderColor = value;
}

byte4 MenuItemComponent::get_IdleFillColor()
{
return this->IdleFillColor;
}

void MenuItemComponent::set_IdleFillColor(byte4 value)
{
this->IdleFillColor = value;
}

const std::string& MenuItemComponent::get_ItemId()
{
return this->ItemIdValue;}

void MenuItemComponent::set_ItemId(std::string value)
{
this->ItemIdValue = value;
}

const std::string& MenuItemComponent::get_PanelId()
{
return this->PanelIdValue;}

void MenuItemComponent::set_PanelId(std::string value)
{
this->PanelIdValue = value;
}

byte4 MenuItemComponent::get_SelectedBorderColor()
{
return this->SelectedBorderColor;
}

void MenuItemComponent::set_SelectedBorderColor(byte4 value)
{
this->SelectedBorderColor = value;
}

byte4 MenuItemComponent::get_SelectedFillColor()
{
return this->SelectedFillColor;
}

void MenuItemComponent::set_SelectedFillColor(byte4 value)
{
this->SelectedFillColor = value;
}

const std::string& MenuItemComponent::get_TargetId()
{
return this->TargetIdValue;}

void MenuItemComponent::set_TargetId(std::string value)
{
this->TargetIdValue = value;
}

MenuItemComponent::MenuItemComponent() : ActionKind(), IdleBorderColor(), IdleFillColor(), SelectedBorderColor(), SelectedFillColor(), ItemIdValue(), PanelIdValue(), TargetIdValue()
{
this->PanelIdValue = String::Empty;
this->ItemIdValue = String::Empty;
this->TargetIdValue = String::Empty;
}

