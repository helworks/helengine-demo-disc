#ifdef DrawText
#undef DrawText
#endif
#include "DirectionalShadowCameraOrbitComponent.hpp"
#include "system/math.hpp"
#include "DirectionalShadowCameraOrbitComponent.hpp"
#include "Component.hpp"
#include "Core.hpp"
#include "Entity.hpp"
#include "UpdateComponent.hpp"
#include "float3.hpp"
#include "float4.hpp"
#include "system/math.hpp"

DirectionalShadowCameraOrbitComponent::DirectionalShadowCameraOrbitComponent() : AngularSpeedRadians(), BaseAngleRadians(), LookDownPitchRadians(), OrbitCenter(), OrbitHeight(), OrbitRadius()
{
}

float DirectionalShadowCameraOrbitComponent::get_AngularSpeedRadians()
{
return this->AngularSpeedRadians;
}

void DirectionalShadowCameraOrbitComponent::set_AngularSpeedRadians(float value)
{
this->AngularSpeedRadians = value;
}

float DirectionalShadowCameraOrbitComponent::get_BaseAngleRadians()
{
return this->BaseAngleRadians;
}

void DirectionalShadowCameraOrbitComponent::set_BaseAngleRadians(float value)
{
this->BaseAngleRadians = value;
}

float DirectionalShadowCameraOrbitComponent::get_LookDownPitchRadians()
{
return this->LookDownPitchRadians;
}

void DirectionalShadowCameraOrbitComponent::set_LookDownPitchRadians(float value)
{
this->LookDownPitchRadians = value;
}

float3 DirectionalShadowCameraOrbitComponent::get_OrbitCenter()
{
return this->OrbitCenter;
}

void DirectionalShadowCameraOrbitComponent::set_OrbitCenter(float3 value)
{
this->OrbitCenter = value;
}

float DirectionalShadowCameraOrbitComponent::get_OrbitHeight()
{
return this->OrbitHeight;
}

void DirectionalShadowCameraOrbitComponent::set_OrbitHeight(float value)
{
this->OrbitHeight = value;
}

float DirectionalShadowCameraOrbitComponent::get_OrbitRadius()
{
return this->OrbitRadius;
}

void DirectionalShadowCameraOrbitComponent::set_OrbitRadius(float value)
{
this->OrbitRadius = value;
}

void DirectionalShadowCameraOrbitComponent::Update()
{
UpdateComponent::Update();
const double angleRadians = this->BaseAngleRadians + (this->AngularSpeedRadians * Core::get_Instance()->get_TotalElapsedSeconds());
const double x = this->OrbitCenter.X + (Math::Sin(angleRadians) * this->OrbitRadius);
const double z = this->OrbitCenter.Z + (Math::Cos(angleRadians) * this->OrbitRadius);
this->Parent->set_LocalPosition(float3(static_cast<float>(x), this->OrbitCenter.Y + this->OrbitHeight, static_cast<float>(z)));
float4 orientation;
float4::CreateFromYawPitchRoll__out3(static_cast<float>(angleRadians), this->LookDownPitchRadians, 0.0f, orientation);
orientation.Normalize();
this->Parent->set_LocalOrientation(orientation);
}

