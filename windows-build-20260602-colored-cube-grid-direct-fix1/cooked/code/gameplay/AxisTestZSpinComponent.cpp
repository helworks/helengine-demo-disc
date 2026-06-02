#ifdef DrawText
#undef DrawText
#endif
#include "AxisTestZSpinComponent.hpp"
#include "AxisTestZSpinComponent.hpp"
#include "Component.hpp"
#include "Core.hpp"
#include "Entity.hpp"
#include "UpdateComponent.hpp"
#include "float4.hpp"

AxisTestZSpinComponent::AxisTestZSpinComponent() : AngularSpeedRadians(), BaseRollRadians()
{
}

float AxisTestZSpinComponent::get_AngularSpeedRadians()
{
return this->AngularSpeedRadians;
}

void AxisTestZSpinComponent::set_AngularSpeedRadians(float value)
{
this->AngularSpeedRadians = value;
}

float AxisTestZSpinComponent::get_BaseRollRadians()
{
return this->BaseRollRadians;
}

void AxisTestZSpinComponent::set_BaseRollRadians(float value)
{
this->BaseRollRadians = value;
}

void AxisTestZSpinComponent::Update()
{
UpdateComponent::Update();
const double rollRadians = this->BaseRollRadians + (this->AngularSpeedRadians * Core::get_Instance()->get_TotalElapsedSeconds());
float4 orientation;
float4::CreateFromYawPitchRoll__out3(0.0f, 0.0f, static_cast<float>(rollRadians), orientation);
orientation.Normalize();
this->Parent->set_LocalOrientation(orientation);
}

