#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "runtime/native_disposable.hpp"

class DirectionalShadowTowerSpinComponent : public ::UpdateComponent
{
public:
    virtual ~DirectionalShadowTowerSpinComponent() = default;

    DirectionalShadowTowerSpinComponent();

    float AngularSpeedRadians;

    float get_AngularSpeedRadians();
    void set_AngularSpeedRadians(float value);

    float BaseYawRadians;

    float get_BaseYawRadians();
    void set_BaseYawRadians(float value);

    void ComponentAdded(Entity* entity);

    void Update();
private:
    double ElapsedSeconds;
};
