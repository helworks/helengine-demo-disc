#ifdef DrawText
#undef DrawText
#endif
#include "DemoDiscOrbitCameraComponent.hpp"
#include "runtime/native_exceptions.hpp"
#include "system/math.hpp"
#include "DemoDiscOrbitCameraComponent.hpp"
#include "Component.hpp"
#include "Core.hpp"
#include "Entity.hpp"
#include "InputGamepadButton.hpp"
#include "InputGamepadState.hpp"
#include "InputSystem.hpp"
#include "Keys.hpp"
#include "UpdateComponent.hpp"
#include "float3.hpp"
#include "float4.hpp"
#include "system/math.hpp"
#include "runtime/native_exceptions.hpp"

float DemoDiscOrbitCameraComponent::get_AutoPitchReturnSpeedRadians()
{
return this->AutoPitchReturnSpeedRadians;
}

void DemoDiscOrbitCameraComponent::set_AutoPitchReturnSpeedRadians(float value)
{
this->AutoPitchReturnSpeedRadians = value;
}

double DemoDiscOrbitCameraComponent::get_AutoReturnBlendSpeed()
{
return this->AutoReturnBlendSpeed;
}

void DemoDiscOrbitCameraComponent::set_AutoReturnBlendSpeed(double value)
{
this->AutoReturnBlendSpeed = value;
}

float DemoDiscOrbitCameraComponent::get_AutoYawSpeedRadians()
{
return this->AutoYawSpeedRadians;
}

void DemoDiscOrbitCameraComponent::set_AutoYawSpeedRadians(float value)
{
this->AutoYawSpeedRadians = value;
}

double DemoDiscOrbitCameraComponent::get_IdleReturnDelaySeconds()
{
return this->IdleReturnDelaySeconds;
}

void DemoDiscOrbitCameraComponent::set_IdleReturnDelaySeconds(double value)
{
this->IdleReturnDelaySeconds = value;
}

float DemoDiscOrbitCameraComponent::get_ManualPitchSpeedRadians()
{
return this->ManualPitchSpeedRadians;
}

void DemoDiscOrbitCameraComponent::set_ManualPitchSpeedRadians(float value)
{
this->ManualPitchSpeedRadians = value;
}

float DemoDiscOrbitCameraComponent::get_ManualYawSpeedRadians()
{
return this->ManualYawSpeedRadians;
}

void DemoDiscOrbitCameraComponent::set_ManualYawSpeedRadians(float value)
{
this->ManualYawSpeedRadians = value;
}

float DemoDiscOrbitCameraComponent::get_MaximumPitchRadians()
{
return this->MaximumPitchRadians;
}

void DemoDiscOrbitCameraComponent::set_MaximumPitchRadians(float value)
{
this->MaximumPitchRadians = value;
}

float DemoDiscOrbitCameraComponent::get_MinimumPitchRadians()
{
return this->MinimumPitchRadians;
}

void DemoDiscOrbitCameraComponent::set_MinimumPitchRadians(float value)
{
this->MinimumPitchRadians = value;
}

float3 DemoDiscOrbitCameraComponent::get_OrbitCenter()
{
return this->OrbitCenter;
}

void DemoDiscOrbitCameraComponent::set_OrbitCenter(float3 value)
{
this->OrbitCenter = value;
}

DemoDiscOrbitCameraComponent::DemoDiscOrbitCameraComponent() : AutoPitchReturnSpeedRadians(), AutoReturnBlendSpeed(0), AutoYawSpeedRadians(), IdleReturnDelaySeconds(0), ManualPitchSpeedRadians(), ManualYawSpeedRadians(), MaximumPitchRadians(), MinimumPitchRadians(), OrbitCenter(), AutoOrbitBlend(0), AutoPitchRadians(), CurrentOrbitRadius(), CurrentPitchRadians(), CurrentYawRadians(), IdleElapsedSeconds(0), IsOrbitInitialized()
{
this->set_AutoYawSpeedRadians(0.07f);
this->set_ManualYawSpeedRadians(1.5f);
this->set_ManualPitchSpeedRadians(1.2f);
this->set_IdleReturnDelaySeconds(10.0);
this->set_AutoReturnBlendSpeed(0.35);
this->set_AutoPitchReturnSpeedRadians(0.8f);
this->set_MinimumPitchRadians(-1.2f);
this->set_MaximumPitchRadians(0.2f);
this->AutoOrbitBlend = 1.0;
this->IdleElapsedSeconds = this->IdleReturnDelaySeconds;
}

void DemoDiscOrbitCameraComponent::Update()
{
UpdateComponent::Update();
    if (this->get_Parent() == nullptr)
    {
throw new InvalidOperationException("DemoDiscOrbitCameraComponent requires an attached parent camera entity.");
    }
this->EnsureOrbitInitialized();
Core *core = (Core::get_Instance() != nullptr ? Core::get_Instance() : throw new InvalidOperationException("A core instance must exist before orbit camera updates can run."));
InputSystem *inputSystem = core->get_Input();
const double elapsedSeconds = core->get_FrameDeltaSeconds();
const double yawInput = this->ResolveYawInput(inputSystem);
const double pitchInput = this->ResolvePitchInput(inputSystem);
const bool hasManualInput = Math::Abs(yawInput) > 0.0001 || Math::Abs(pitchInput) > 0.0001;
    if (hasManualInput)
    {
this->IdleElapsedSeconds = 0.0;
this->AutoOrbitBlend = 0.0;
this->CurrentYawRadians += static_cast<float>((yawInput * this->ManualYawSpeedRadians * elapsedSeconds));
this->CurrentPitchRadians -= static_cast<float>((pitchInput * this->ManualPitchSpeedRadians * elapsedSeconds));
this->CurrentPitchRadians = this->ClampPitch(this->CurrentPitchRadians);
    }
else {
this->IdleElapsedSeconds += elapsedSeconds;
    if (this->IdleElapsedSeconds >= this->IdleReturnDelaySeconds)
    {
this->AutoOrbitBlend = Math::Min(1.0, this->AutoOrbitBlend + (this->AutoReturnBlendSpeed * elapsedSeconds));
    }
else {
this->AutoOrbitBlend = 0.0;
}
    if (this->AutoOrbitBlend > 0.0)
    {
this->CurrentYawRadians += static_cast<float>((this->AutoYawSpeedRadians * elapsedSeconds * this->AutoOrbitBlend));
this->CurrentPitchRadians = this->MoveToward(this->CurrentPitchRadians, this->AutoPitchRadians, this->AutoPitchReturnSpeedRadians * static_cast<float>((elapsedSeconds * this->AutoOrbitBlend)));
    }
}
this->ApplyOrbitPose();
}

void DemoDiscOrbitCameraComponent::ApplyOrbitPose()
{
const double horizontalRadius = Math::Cos(this->CurrentPitchRadians) * this->CurrentOrbitRadius;
const double x = this->OrbitCenter.X + (Math::Sin(this->CurrentYawRadians) * horizontalRadius);
const double y = this->OrbitCenter.Y - (Math::Sin(this->CurrentPitchRadians) * this->CurrentOrbitRadius);
const double z = this->OrbitCenter.Z + (Math::Cos(this->CurrentYawRadians) * horizontalRadius);
this->Parent->set_LocalPosition(float3(static_cast<float>(x), static_cast<float>(y), static_cast<float>(z)));
float4 orientation;
float4::CreateFromYawPitchRoll__out3(this->CurrentYawRadians, this->CurrentPitchRadians, 0.0f, orientation);
orientation.Normalize();
this->Parent->set_LocalOrientation(orientation);
}

float DemoDiscOrbitCameraComponent::ClampPitch(float pitchRadians)
{
return static_cast<float>(Math::Clamp(pitchRadians, this->MinimumPitchRadians, this->MaximumPitchRadians));}

void DemoDiscOrbitCameraComponent::EnsureOrbitInitialized()
{
    if (this->IsOrbitInitialized)
    {
return;    }
float3 offset = this->get_Parent()->get_LocalPosition() - this->OrbitCenter;
const double orbitRadius = Math::Sqrt((offset.X * offset.X) + (offset.Y * offset.Y) + (offset.Z * offset.Z));
    if (orbitRadius <= MinimumOrbitRadius)
    {
throw new InvalidOperationException("DemoDiscOrbitCameraComponent requires the authored camera pose to be offset from the orbit center.");
    }
const double horizontalRadius = Math::Sqrt((offset.X * offset.X) + (offset.Z * offset.Z));
const double yawRadians = Math::Atan2(offset.X, offset.Z);
const double pitchRadians = -Math::Atan2(offset.Y, horizontalRadius);
this->CurrentOrbitRadius = static_cast<float>(orbitRadius);
this->CurrentYawRadians = static_cast<float>(yawRadians);
this->CurrentPitchRadians = this->ClampPitch(static_cast<float>(pitchRadians));
this->AutoPitchRadians = this->CurrentPitchRadians;
this->IsOrbitInitialized = true;
}

float DemoDiscOrbitCameraComponent::MoveToward(float currentValue, float targetValue, float maximumStep)
{
    if (maximumStep <= 0.0f)
    {
return currentValue;    }
const double delta = targetValue - currentValue;
    if (Math::Abs(delta) <= maximumStep)
    {
return targetValue;    }
    if (delta > 0.0f)
    {
return currentValue + maximumStep;    }
return currentValue - maximumStep;}

double DemoDiscOrbitCameraComponent::NormalizeStickAxis(int16_t axisValue)
{
const double normalized = axisValue / 32767.0;
    if (Math::Abs(normalized) < GamepadDeadzone)
    {
return 0.0;    }
return Math::Clamp(normalized, -1.0, 1.0);}

double DemoDiscOrbitCameraComponent::ResolvePitchInput(InputSystem* inputSystem)
{
    if (inputSystem == nullptr)
    {
throw new ArgumentNullException("inputSystem");
    }
double keyboardPitch = 0.0;
    if (inputSystem->IsKeyDown(Keys::W))
    {
keyboardPitch += 1.0;
    }
    if (inputSystem->IsKeyDown(Keys::S))
    {
keyboardPitch -= 1.0;
    }
InputGamepadState gamepadState = inputSystem->GetGamepadState(0);
double gamepadPitch = 0.0;
    if (gamepadState.get_Connected())
    {
    if (gamepadState.IsButtonDown(InputGamepadButton::DPadUp))
    {
gamepadPitch += 1.0;
    }
    if (gamepadState.IsButtonDown(InputGamepadButton::DPadDown))
    {
gamepadPitch -= 1.0;
    }
gamepadPitch += -this->NormalizeStickAxis(gamepadState.get_LeftStickY());
    }
return Math::Clamp(keyboardPitch + gamepadPitch, -1.0, 1.0);}

double DemoDiscOrbitCameraComponent::ResolveYawInput(InputSystem* inputSystem)
{
    if (inputSystem == nullptr)
    {
throw new ArgumentNullException("inputSystem");
    }
double keyboardYaw = 0.0;
    if (inputSystem->IsKeyDown(Keys::A))
    {
keyboardYaw -= 1.0;
    }
    if (inputSystem->IsKeyDown(Keys::D))
    {
keyboardYaw += 1.0;
    }
InputGamepadState gamepadState = inputSystem->GetGamepadState(0);
double gamepadYaw = 0.0;
    if (gamepadState.get_Connected())
    {
    if (gamepadState.IsButtonDown(InputGamepadButton::DPadLeft))
    {
gamepadYaw -= 1.0;
    }
    if (gamepadState.IsButtonDown(InputGamepadButton::DPadRight))
    {
gamepadYaw += 1.0;
    }
gamepadYaw += this->NormalizeStickAxis(gamepadState.get_LeftStickX());
    }
return Math::Clamp(keyboardYaw + gamepadYaw, -1.0, 1.0);}

