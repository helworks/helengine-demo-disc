#ifdef DrawText
#undef DrawText
#endif
#include "MenuPanelComponent.hpp"
#include "runtime/native_string.hpp"
#include "runtime/native_string.hpp"

uint8_t MenuPanelComponent::CurrentVersion = 1;

std::string MenuPanelComponent::SerializedComponentTypeId = "city.menu.MenuPanelComponent, gameplay";

const std::string& MenuPanelComponent::get_PanelId()
{
return this->PanelIdValue;}

void MenuPanelComponent::set_PanelId(std::string value)
{
this->PanelIdValue = value;
}

MenuPanelComponent::MenuPanelComponent() : PanelIdValue()
{
this->PanelIdValue = String::Empty;
}

