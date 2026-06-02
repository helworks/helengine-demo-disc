#ifdef DrawText
#undef DrawText
#endif
#include "DemoDiscDirectionalLightToggleState.hpp"
#include "DirectionalLightComponent.hpp"

DemoDiscDirectionalLightToggleState::DemoDiscDirectionalLightToggleState() : Intensity(), Light(), ShadowsEnabled()
{
}

float DemoDiscDirectionalLightToggleState::get_Intensity()
{
return this->Intensity;
}

void DemoDiscDirectionalLightToggleState::set_Intensity(float value)
{
this->Intensity = value;
}

DirectionalLightComponent* DemoDiscDirectionalLightToggleState::get_Light()
{
return this->Light;
}

void DemoDiscDirectionalLightToggleState::set_Light(DirectionalLightComponent* value)
{
this->Light = value;
}

bool DemoDiscDirectionalLightToggleState::get_ShadowsEnabled()
{
return this->ShadowsEnabled;
}

void DemoDiscDirectionalLightToggleState::set_ShadowsEnabled(bool value)
{
this->ShadowsEnabled = value;
}

