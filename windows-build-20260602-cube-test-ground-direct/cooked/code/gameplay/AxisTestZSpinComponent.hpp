#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "runtime/native_disposable.hpp"

class AxisTestZSpinComponent : public ::UpdateComponent
{
public:
    virtual ~AxisTestZSpinComponent() = default;

    AxisTestZSpinComponent();

    float AngularSpeedRadians;

    float get_AngularSpeedRadians();
    void set_AngularSpeedRadians(float value);

    float BaseRollRadians;

    float get_BaseRollRadians();
    void set_BaseRollRadians(float value);

    void Update();
};
