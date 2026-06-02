#ifdef DrawText
#undef DrawText
#endif
#include "DemoDiscLightToggleComponent.hpp"
#include "UpdateComponent.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_list.hpp"
#include "Core.hpp"
#include "ObjectManager.hpp"
#include "InputSystem.hpp"
#include "DemoDiscDirectionalLightToggleState.hpp"
#include "Keys.hpp"
#include "InputGamepadButton.hpp"
#include "Entity.hpp"
#include "DirectionalLightComponent.hpp"
#include "runtime/native_cast.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_list.hpp"

void DemoDiscLightToggleComponent::ComponentAdded(Entity* entity)
{
UpdateComponent::ComponentAdded(entity);
    if (entity == nullptr)
    {
throw new ArgumentNullException("entity");
    }
this->CaptureDirectionalLightStates();
}

void DemoDiscLightToggleComponent::ComponentRemoved(Entity* entity)
{
UpdateComponent::ComponentRemoved(entity);
}

DemoDiscLightToggleComponent::DemoDiscLightToggleComponent() : LightStates(), LightsEnabled()
{
this->LightStates = new List<::DemoDiscDirectionalLightToggleState*>();
this->LightsEnabled = true;
}

void DemoDiscLightToggleComponent::Dispose()
{
}

void DemoDiscLightToggleComponent::Update()
{
    if (!this->WasToggleRequested())
    {
return;    }
this->LightsEnabled = !this->LightsEnabled;
this->ApplyDirectionalLightState();
}

void DemoDiscLightToggleComponent::ApplyDirectionalLightState()
{
for (int32_t lightIndex = 0; lightIndex < this->LightStates->Count(); lightIndex++) {
::DemoDiscDirectionalLightToggleState *lightState = (*this->LightStates)[lightIndex];
    if (lightState == nullptr || lightState->get_Light() == nullptr)
    {
continue;
    }
DirectionalLightComponent *directionalLightComponent = lightState->get_Light();
    if (this->LightsEnabled)
    {
directionalLightComponent->set_Intensity(lightState->get_Intensity());
directionalLightComponent->set_ShadowsEnabled(lightState->get_ShadowsEnabled());
continue;
    }
directionalLightComponent->set_Intensity(0.0f);
directionalLightComponent->set_ShadowsEnabled(false);
}
}

void DemoDiscLightToggleComponent::CaptureDirectionalLightStates()
{
this->LightStates->Clear();
    if (Core::get_Instance() == nullptr || Core::get_Instance()->get_ObjectManager() == nullptr)
    {
throw new InvalidOperationException("Light toggle component requires an initialized object manager.");
    }
List<Entity*> *entities = Core::get_Instance()->get_ObjectManager()->get_Entities();
for (int32_t entityIndex = 0; entityIndex < entities->Count(); entityIndex++) {
Entity *entity = (*entities)[entityIndex];
    if (entity == nullptr || entity->get_Components() == nullptr)
    {
continue;
    }
for (int32_t componentIndex = 0; componentIndex < entity->get_Components()->get_Count(); componentIndex++) {
    DirectionalLightComponent* directionalLightComponent = he_cpp_try_cast<DirectionalLightComponent>((*entity->get_Components())[componentIndex]);
    if (directionalLightComponent != nullptr)
    {
this->LightStates->Add(([&]() {
auto __object_00000054 = new ::DemoDiscDirectionalLightToggleState();
__object_00000054->set_Light(directionalLightComponent);
__object_00000054->set_Intensity(directionalLightComponent->get_Intensity());
__object_00000054->set_ShadowsEnabled(directionalLightComponent->get_ShadowsEnabled());
return __object_00000054;
})());
    }
}
}
}

bool DemoDiscLightToggleComponent::WasToggleRequested()
{
InputSystem *inputSystem = Core::get_Instance()->get_Input();
    if (inputSystem == nullptr)
    {
throw new InvalidOperationException("Light toggle component requires an initialized input system.");
    }
return inputSystem->WasKeyPressed(Keys::L) || inputSystem->WasGamepadButtonPressed(0, InputGamepadButton::RightShoulder);}

