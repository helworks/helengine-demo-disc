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

class DirectionalShadowTowerSpinComponent : public UpdateComponent
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
