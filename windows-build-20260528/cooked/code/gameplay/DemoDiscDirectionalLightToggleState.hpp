#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "DirectionalLightComponent.hpp"

class DemoDiscDirectionalLightToggleState
{
public:
    virtual ~DemoDiscDirectionalLightToggleState() = default;

    DemoDiscDirectionalLightToggleState();

    float Intensity;

    float get_Intensity();
    void set_Intensity(float value);

    DirectionalLightComponent* Light;

    DirectionalLightComponent* get_Light();
    void set_Light(DirectionalLightComponent* value);

    bool ShadowsEnabled;

    bool get_ShadowsEnabled();
    void set_ShadowsEnabled(bool value);
};
