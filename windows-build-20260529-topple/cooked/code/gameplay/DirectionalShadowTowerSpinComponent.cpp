#ifdef DrawText
#undef DrawText
#endif
#include "DirectionalShadowTowerSpinComponent.hpp"
#include "UpdateComponent.hpp"
#include "float4.hpp"
#include "Core.hpp"
#include "Entity.hpp"

DirectionalShadowTowerSpinComponent::DirectionalShadowTowerSpinComponent() : AngularSpeedRadians(), BaseYawRadians(), ElapsedSeconds(0)
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

void DirectionalShadowTowerSpinComponent::ComponentAdded(Entity* entity)
{
UpdateComponent::ComponentAdded(entity);
this->ElapsedSeconds = 0.0;
}

void DirectionalShadowTowerSpinComponent::Update()
{
UpdateComponent::Update();
this->ElapsedSeconds += Core::get_Instance()->get_FrameDeltaSeconds();
const double yawRadians = this->BaseYawRadians + (this->AngularSpeedRadians * this->ElapsedSeconds);
float4 orientation;
float4::CreateFromYawPitchRoll(static_cast<float>(yawRadians), 0.0f, 0.0f, orientation);
orientation.Normalize();
Parent->set_LocalOrientation(orientation);
}

