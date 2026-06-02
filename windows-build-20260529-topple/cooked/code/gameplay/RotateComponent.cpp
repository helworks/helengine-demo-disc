#ifdef DrawText
#undef DrawText
#endif
#include "RotateComponent.hpp"
#include "UpdateComponent.hpp"
#include "float4.hpp"
#include "float3.hpp"
#include "Entity.hpp"

RotateComponent::RotateComponent() : RadiansPerFrame(0.07f)
{
}

float RotateComponent::get_RadiansPerFrame()
{
return this->RadiansPerFrame;
}

void RotateComponent::set_RadiansPerFrame(float value)
{
this->RadiansPerFrame = value;
}

void RotateComponent::Update()
{
UpdateComponent::Update();
float4 deltaRotation;
float3 axis = float3(0.0f, 1.0f, 0.0f);
float4::CreateFromAxisAngle(axis, this->RadiansPerFrame, deltaRotation);
float4 orientation = this->get_Parent()->get_Orientation();
orientation = deltaRotation * orientation;
orientation.Normalize();
Parent->set_Orientation(orientation);
}

