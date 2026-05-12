#ifdef DrawText
#undef DrawText
#endif
#include "DirectionalShadowSunSweepComponent.hpp"
#include "UpdateComponent.hpp"
#include "float4.hpp"
#include "system/math.hpp"

DirectionalShadowSunSweepComponent::DirectionalShadowSunSweepComponent() : MaxYawRadians(), MinYawRadians(), PitchRadians(), SweepSpeedRadians()
{
}

float DirectionalShadowSunSweepComponent::get_MaxYawRadians()
{
return this->MaxYawRadians;
}

void DirectionalShadowSunSweepComponent::set_MaxYawRadians(float value)
{
this->MaxYawRadians = value;
}

float DirectionalShadowSunSweepComponent::get_MinYawRadians()
{
return this->MinYawRadians;
}

void DirectionalShadowSunSweepComponent::set_MinYawRadians(float value)
{
this->MinYawRadians = value;
}

float DirectionalShadowSunSweepComponent::get_PitchRadians()
{
return this->PitchRadians;
}

void DirectionalShadowSunSweepComponent::set_PitchRadians(float value)
{
this->PitchRadians = value;
}

float DirectionalShadowSunSweepComponent::get_SweepSpeedRadians()
{
return this->SweepSpeedRadians;
}

void DirectionalShadowSunSweepComponent::set_SweepSpeedRadians(float value)
{
this->SweepSpeedRadians = value;
}

void DirectionalShadowSunSweepComponent::Update()
{
UpdateComponent::Update();
const double normalized = (Math::Sin(Core->Instance->TotalElapsedSeconds * this->SweepSpeedRadians) * 0.5) + 0.5;
const double yawRadians = this->MinYawRadians + ((this->MaxYawRadians - this->MinYawRadians) * normalized);
float4 orientation;
float4::CreateFromYawPitchRoll(static_cast<float>(yawRadians), this->PitchRadians, 0.0f, orientation);
orientation.Normalize();
Parent->LocalOrientation = orientation;
}

