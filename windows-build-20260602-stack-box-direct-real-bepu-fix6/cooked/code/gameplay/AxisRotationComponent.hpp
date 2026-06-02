#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "runtime/native_disposable.hpp"

class AxisRotationComponent : public ::UpdateComponent
{
public:
    virtual ~AxisRotationComponent() = default;

    AxisRotationComponent();

    float AngularSpeedRadiansPerSecond;

    float get_AngularSpeedRadiansPerSecond();
    void set_AngularSpeedRadiansPerSecond(float value);

    float3 Axis;

    float3 get_Axis();
    void set_Axis(float3 value);

    void Update();
};
