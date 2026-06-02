#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "UpdateComponent.hpp"
#include "runtime/native_disposable.hpp"
#include "IUpdateable.hpp"
#include "UpdateComponent.hpp"
#include "runtime/native_exceptions.hpp"
#include "Core.hpp"
#include "InputSystem.hpp"
#include "Core.hpp"
#include "float3.hpp"
#include "runtime/native_exceptions.hpp"
#include "InputSystem.hpp"
#include "InputGamepadState.hpp"
#include "InputGamepadState.hpp"
#include "float4.hpp"
#include "float4.hpp"

class DemoDiscOrbitCameraComponent : public UpdateComponent
{
public:
    virtual ~DemoDiscOrbitCameraComponent() = default;

    float AutoPitchReturnSpeedRadians;

    float get_AutoPitchReturnSpeedRadians();
    void set_AutoPitchReturnSpeedRadians(float value);

    double AutoReturnBlendSpeed;

    double get_AutoReturnBlendSpeed();
    void set_AutoReturnBlendSpeed(double value);

    float AutoYawSpeedRadians;

    float get_AutoYawSpeedRadians();
    void set_AutoYawSpeedRadians(float value);

    double IdleReturnDelaySeconds;

    double get_IdleReturnDelaySeconds();
    void set_IdleReturnDelaySeconds(double value);

    float ManualPitchSpeedRadians;

    float get_ManualPitchSpeedRadians();
    void set_ManualPitchSpeedRadians(float value);

    float ManualYawSpeedRadians;

    float get_ManualYawSpeedRadians();
    void set_ManualYawSpeedRadians(float value);

    float MaximumPitchRadians;

    float get_MaximumPitchRadians();
    void set_MaximumPitchRadians(float value);

    float MinimumPitchRadians;

    float get_MinimumPitchRadians();
    void set_MinimumPitchRadians(float value);

    float3 OrbitCenter;

    float3 get_OrbitCenter();
    void set_OrbitCenter(float3 value);

    DemoDiscOrbitCameraComponent();

    void Update();
private:
    static double GamepadDeadzone;

    static double MinimumOrbitRadius;

    double AutoOrbitBlend;

    float AutoPitchRadians;

    float CurrentOrbitRadius;

    float CurrentPitchRadians;

    float CurrentYawRadians;

    double IdleElapsedSeconds;

    bool IsOrbitInitialized;

    void ApplyOrbitPose();

    float ClampPitch(float pitchRadians);

    void EnsureOrbitInitialized();

    float MoveToward(float currentValue, float targetValue, float maximumStep);

    double NormalizeStickAxis(int16_t axisValue);

    double ResolvePitchInput(InputSystem* inputSystem);

    double ResolveYawInput(InputSystem* inputSystem);
};
