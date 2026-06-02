#ifdef DrawText
#undef DrawText
#endif
#include "MenuPanelComponent.hpp"
#include "runtime/native_string.hpp"
#include "MenuPanelComponent.hpp"
#include "runtime/native_string.hpp"

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

