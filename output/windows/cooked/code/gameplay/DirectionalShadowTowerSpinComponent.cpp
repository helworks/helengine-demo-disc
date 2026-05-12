#ifdef DrawText
#undef DrawText
#endif
#include "DirectionalShadowTowerSpinComponent.hpp"
#include "UpdateComponent.hpp"
#include "float4.hpp"

DirectionalShadowTowerSpinComponent::DirectionalShadowTowerSpinComponent() : AngularSpeedRadians(), BaseYawRadians()
{
}

float DirectionalShadowTowerSpinComponent::get_AngularSpeedRadians()
{
return this->AngularSpeedRadians;
}

void DirectionalShadowTowerSpinComponent::set_AngularSpeedRadians(float value)
{
this->AngularSpeedRadians = value;
}

float DirectionalShadowTowerSpinComponent::get_BaseYawRadians()
{
return this->BaseYawRadians;
}

void DirectionalShadowTowerSpinComponent::set_BaseYawRadians(float value)
{
this->BaseYawRadians = value;
}

void DirectionalShadowTowerSpinComponent::Update()
{
UpdateComponent::Update();
const double yawRadians = this->BaseYawRadians + (this->AngularSpeedRadians * Core->Instance->TotalElapsedSeconds);
float4 orientation;
float4::CreateFromYawPitchRoll(static_cast<float>(yawRadians), 0.0f, 0.0f, orientation);
orientation.Normalize();
Parent->LocalOrientation = orientation;
}

