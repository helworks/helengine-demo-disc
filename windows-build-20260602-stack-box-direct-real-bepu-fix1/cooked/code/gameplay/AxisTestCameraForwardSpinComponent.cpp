#ifdef DrawText
#undef DrawText
#endif
#include "AxisTestCameraForwardSpinComponent.hpp"
#include "AxisTestCameraForwardSpinComponent.hpp"
#include "Component.hpp"
#include "Core.hpp"
#include "Entity.hpp"
#include "UpdateComponent.hpp"
#include "float3.hpp"
#include "float4.hpp"

AxisTestCameraForwardSpinComponent::AxisTestCameraForwardSpinComponent() : AngularSpeedRadians(), BaseAngleRadians(), CameraForwardAxisX(), CameraForwardAxisY(), CameraForwardAxisZ()
{
}

float AxisTestCameraForwardSpinComponent::get_AngularSpeedRadians()
{
return this->AngularSpeedRadians;
}

void AxisTestCameraForwardSpinComponent::set_AngularSpeedRadians(float value)
{
this->AngularSpeedRadians = value;
}

float AxisTestCameraForwardSpinComponent::get_BaseAngleRadians()
{
return this->BaseAngleRadians;
}

void AxisTestCameraForwardSpinComponent::set_BaseAngleRadians(float value)
{
this->BaseAngleRadians = value;
}

float AxisTestCameraForwardSpinComponent::get_CameraForwardAxisX()
{
return this->CameraForwardAxisX;
}

void AxisTestCameraForwardSpinComponent::set_CameraForwardAxisX(float value)
{
this->CameraForwardAxisX = value;
}

float AxisTestCameraForwardSpinComponent::get_CameraForwardAxisY()
{
return this->CameraForwardAxisY;
}

void AxisTestCameraForwardSpinComponent::set_CameraForwardAxisY(float value)
{
this->CameraForwardAxisY = value;
}

float AxisTestCameraForwardSpinComponent::get_CameraForwardAxisZ()
{
return this->CameraForwardAxisZ;
}

void AxisTestCameraForwardSpinComponent::set_CameraForwardAxisZ(float value)
{
this->CameraForwardAxisZ = value;
}

void AxisTestCameraForwardSpinComponent::Update()
{
UpdateComponent::Update();
const double angleRadians = this->BaseAngleRadians + (this->AngularSpeedRadians * Core::get_Instance()->get_TotalElapsedSeconds());
float3 axis = float3::Normalize(float3(this->CameraForwardAxisX, this->CameraForwardAxisY, this->CameraForwardAxisZ));
float4 orientation;
float4::CreateFromAxisAngle__ref0_out2(axis, static_cast<float>(angleRadians), orientation);
orientation.Normalize();
this->Parent->set_LocalOrientation(orientation);
}

