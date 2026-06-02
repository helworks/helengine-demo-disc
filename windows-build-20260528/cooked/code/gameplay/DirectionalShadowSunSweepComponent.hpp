#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "UpdateComponent.hpp"
#include "runtime/native_disposable.hpp"
#include "IUpdateable.hpp"
#include "UpdateComponent.hpp"
#include "float4.hpp"
#include "float4.hpp"
#include "Entity.hpp"

class DirectionalShadowSunSweepComponent : public UpdateComponent
{
public:
    virtual ~DirectionalShadowSunSweepComponent() = default;

    DirectionalShadowSunSweepComponent();

    float MaxYawRadians;

    float get_MaxYawRadians();
    void set_MaxYawRadians(float value);

    float MinYawRadians;

    float get_MinYawRadians();
    void set_MinYawRadians(float value);

    float PitchRadians;

    float get_PitchRadians();
    void set_PitchRadians(float value);

    float SweepSpeedRadians;

    float get_SweepSpeedRadians();
    void set_SweepSpeedRadians(float value);

    void ComponentAdded(Entity* entity);

    void Update();
private:
    double ElapsedSeconds;
};
