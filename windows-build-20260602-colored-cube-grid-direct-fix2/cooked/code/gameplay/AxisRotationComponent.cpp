#ifdef DrawText
#undef DrawText
#endif
#include "AxisRotationComponent.hpp"
#include "runtime/native_exceptions.hpp"
#include "AxisRotationComponent.hpp"
#include "Component.hpp"
#include "Core.hpp"
#include "Entity.hpp"
#include "UpdateComponent.hpp"
#include "float3.hpp"
#include "float4.hpp"
#include "runtime/native_exceptions.hpp"

AxisRotationComponent::AxisRotationComponent() : AngularSpeedRadiansPerSecond(), Axis()
{
}

float AxisRotationComponent::get_AngularSpeedRadiansPerSecond()
{
return this->AngularSpeedRadiansPerSecond;
}

void AxisRotationComponent::set_AngularSpeedRadiansPerSecond(float value)
{
this->AngularSpeedRadiansPerSecond = value;
}

float3 AxisRotationComponent::get_Axis()
{
return this->Axis;
}

void AxisRotationComponent::set_Axis(float3 value)
{
this->Axis = value;
}

void AxisRotationComponent::Update()
{
UpdateComponent::Update();
    if (this->Axis == float3::get_Zero())
    {
throw new InvalidOperationException("AxisRotationComponent requires a non-zero axis.");
    }
float3 normalizedAxis = float3::Normalize(this->Axis);
const float deltaAngleRadians = this->AngularSpeedRadiansPerSecond * static_cast<float>(Core::get_Instance()->get_FrameDeltaSeconds());
float4 deltaRotation;
float4::CreateFromAxisAngle__ref0_out2(normalizedAxis, deltaAngleRadians, deltaRotation);
float4 currentOrientation = this->get_Parent()->get_LocalOrientation();
float4 orientation;
float4::Concatenate(currentOrientation, deltaRotation, orientation);
orientation.Normalize();
this->Parent->set_LocalOrientation(orientation);
}

