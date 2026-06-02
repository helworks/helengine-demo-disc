#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "UpdateComponent.hpp"
#include "runtime/native_disposable.hpp"
#include "IUpdateable.hpp"
#include "UpdateComponent.hpp"
#include "float3.hpp"
#include "float3.hpp"
#include "float4.hpp"
#include "float4.hpp"

class AxisTestCameraForwardSpinComponent : public UpdateComponent
{
public:
    virtual ~AxisTestCameraForwardSpinComponent() = default;

    AxisTestCameraForwardSpinComponent();

    float AngularSpeedRadians;

    float get_AngularSpeedRadians();
    void set_AngularSpeedRadians(float value);

    float BaseAngleRadians;

    float get_BaseAngleRadians();
    void set_BaseAngleRadians(float value);

    float CameraForwardAxisX;

    float get_CameraForwardAxisX();
    void set_CameraForwardAxisX(float value);

    float CameraForwardAxisY;

    float get_CameraForwardAxisY();
    void set_CameraForwardAxisY(float value);

    float CameraForwardAxisZ;

    float get_CameraForwardAxisZ();
    void set_CameraForwardAxisZ(float value);

    void Update();
};
