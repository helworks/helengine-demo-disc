#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "runtime/native_disposable.hpp"

class DirectionalShadowOrbitComponent : public ::UpdateComponent
{
public:
    virtual ~DirectionalShadowOrbitComponent() = default;

    DirectionalShadowOrbitComponent();

    float AngularSpeedRadians;

    float get_AngularSpeedRadians();
    void set_AngularSpeedRadians(float value);

    float BaseAngleRadians;

    float get_BaseAngleRadians();
    void set_BaseAngleRadians(float value);

    float3 OrbitCenter;

    float3 get_OrbitCenter();
    void set_OrbitCenter(float3 value);

    float OrbitHeight;

    float get_OrbitHeight();
    void set_OrbitHeight(float value);

    float OrbitRadius;

    float get_OrbitRadius();
    void set_OrbitRadius(float value);

    void Update();
};
