#ifdef DrawText
#undef DrawText
#endif
#include "DirectionalShadowOrbitComponent.hpp"
#include "UpdateComponent.hpp"
#include "float4.hpp"
#include "system/math.hpp"

DirectionalShadowOrbitComponent::DirectionalShadowOrbitComponent() : AngularSpeedRadians(), BaseAngleRadians(), OrbitCenter(), OrbitHeight(), OrbitRadius()
{
}

float DirectionalShadowOrbitComponent::get_AngularSpeedRadians()
{
return this->AngularSpeedRadians;
}

void DirectionalShadowOrbitComponent::set_AngularSpeedRadians(float value)
{
this->AngularSpeedRadians = value;
}

float DirectionalShadowOrbitComponent::get_BaseAngleRadians()
{
return this->BaseAngleRadians;
}

void DirectionalShadowOrbitComponent::set_BaseAngleRadians(float value)
{
this->BaseAngleRadians = value;
}

float3* DirectionalShadowOrbitComponent::get_OrbitCenter()
{
return this->OrbitCenter;
}

void DirectionalShadowOrbitComponent::set_OrbitCenter(float3* value)
{
this->OrbitCenter = value;
}

float DirectionalShadowOrbitComponent::get_OrbitHeight()
{
return this->OrbitHeight;
}

void DirectionalShadowOrbitComponent::set_OrbitHeight(float value)
{
this->OrbitHeight = value;
}

float DirectionalShadowOrbitComponent::get_OrbitRadius()
{
return this->OrbitRadius;
}

void DirectionalShadowOrbitComponent::set_OrbitRadius(float value)
{
this->OrbitRadius = value;
}

void DirectionalShadowOrbitComponent::Update()
{
UpdateComponent::Update();
const double angleRadians = this->BaseAngleRadians + (this->AngularSpeedRadians * Core->Instance->TotalElapsedSeconds);
const double x = this->OrbitCenter->X + (Math::Sin(angleRadians) * this->OrbitRadius);
const double z = this->OrbitCenter->Z + (Math::Cos(angleRadians) * this->OrbitRadius);
Parent->LocalPosition = new float3(static_cast<float>(x), this->OrbitCenter->Y + this->OrbitHeight, static_cast<float>(z));
const double inwardYawRadians = Math::Atan2(this->OrbitCenter->X - x, -(this->OrbitCenter->Z - z));
float4 orientation;
float4::CreateFromYawPitchRoll(static_cast<float>(inwardYawRadians), 0.0f, 0.0f, orientation);
orientation.Normalize();
Parent->LocalOrientation = orientation;
}

