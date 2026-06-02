#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "runtime/native_disposable.hpp"

class DirectionalShadowCameraOrbitComponent : public ::UpdateComponent
{
public:
    virtual ~DirectionalShadowCameraOrbitComponent() = default;

    DirectionalShadowCameraOrbitComponent();

    float AngularSpeedRadians;

    float get_AngularSpeedRadians();
    void set_AngularSpeedRadians(float value);

    float BaseAngleRadians;

    float get_BaseAngleRadians();
    void set_BaseAngleRadians(float value);

    float LookDownPitchRadians;

    float get_LookDownPitchRadians();
    void set_LookDownPitchRadians(float value);

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
