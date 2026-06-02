#ifdef DrawText
#undef DrawText
#endif
#include "DemoDiscReturnToMenuComponent.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_string.hpp"
#include "runtime/array.hpp"
#include "DemoDiscReturnToMenuComponent.hpp"
#include "runtime/native_list.hpp"
#include "system/action.hpp"
#include "Component.hpp"
#include "Core.hpp"
#include "CoreInitializationOptions.hpp"
#include "Entity.hpp"
#include "InputGamepadState.hpp"
#include "InputSystem.hpp"
#include "InteractableComponent.hpp"
#include "Keys.hpp"
#include "PointerInteraction.hpp"
#include "RuntimeSceneCatalog.hpp"
#include "RuntimeSceneCatalogEntry.hpp"
#include "SceneLoadMode.hpp"
#include "SceneManager.hpp"
#include "SceneMapComponent.hpp"
#include "StandardPlatformAction.hpp"
#include "StandardPlatformInput.hpp"
#include "UpdateComponent.hpp"
#include "runtime/array.hpp"
#include "runtime/native_cast.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_list.hpp"
#include "runtime/native_string.hpp"

DemoDiscReturnToMenuComponent::DemoDiscReturnToMenuComponent() : AllowGamepadReturn(true), AllowKeyboardReturn(true), AllowPointerReturn(true), BoundInteractable(), PointerPressStartedInside(), SceneLoadWasRequested()
{
}

bool DemoDiscReturnToMenuComponent::get_AllowGamepadReturn()
{
return this->AllowGamepadReturn;
}

void DemoDiscReturnToMenuComponent::set_AllowGamepadReturn(bool value)
{
this->AllowGamepadReturn = value;
}

bool DemoDiscReturnToMenuComponent::get_AllowKeyboardReturn()
{
return this->AllowKeyboardReturn;
}

void DemoDiscReturnToMenuComponent::set_AllowKeyboardReturn(bool value)
{
this->AllowKeyboardReturn = value;
}

bool DemoDiscReturnToMenuComponent::get_AllowPointerReturn()
{
return this->AllowPointerReturn;
}

void DemoDiscReturnToMenuComponent::set_AllowPointerReturn(bool value)
{
this->AllowPointerReturn = value;
}

void DemoDiscReturnToMenuComponent::ComponentAdded(Entity* entity)
{
UpdateComponent::ComponentAdded(entity);
    if (this->AllowPointerReturn)
    {
this->TryBindInteractable();
    }
}

void DemoDiscReturnToMenuComponent::ComponentRemoved(Entity* entity)
{
this->UnbindInteractable();
UpdateComponent::ComponentRemoved(entity);
}

void DemoDiscReturnToMenuComponent::Dispose()
{
this->UnbindInteractable();
}

void DemoDiscReturnToMenuComponent::Update()
{
    if (this->AllowPointerReturn)
    {
this->TryBindInteractable();
    }
InputSystem *inputSystem = Core::get_Instance()->get_Input();
const bool wasReturnPressed = (this->AllowKeyboardReturn && this->WasKeyboardReturnPressed(inputSystem)) || (this->AllowGamepadReturn && this->WasGamepadReturnPressed(inputSystem));
    if (wasReturnPressed)
    {
this->LoadResolvedMainMenuScene();
    }
}

bool DemoDiscReturnToMenuComponent::CanLoadRuntimeScene(std::string sceneId)
{
    if (String::IsNullOrWhiteSpace(sceneId))
    {
throw ([&]() {
auto __ctor_arg_00000092 = "Scene id must be provided.";
auto __ctor_arg_00000093 = "sceneId";
return new ArgumentException(__ctor_arg_00000092, __ctor_arg_00000093);
})();
    }
    if (Core::get_Instance() == nullptr)
    {
throw new InvalidOperationException("A core instance must exist before loading runtime scenes.");
    }
    if (Core::get_Instance()->get_SceneManager() == nullptr)
    {
throw new InvalidOperationException("Core scene manager must be initialized before runtime scene loading can occur.");
    }
CoreInitializationOptions *initializationOptions = Core::get_Instance()->get_InitializationOptions();
RuntimeSceneCatalog *sceneCatalog = initializationOptions != nullptr ? initializationOptions->get_SceneCatalog() : nullptr;
    if (sceneCatalog == nullptr)
    {
return true;    }
Array<RuntimeSceneCatalogEntry*> *entries = sceneCatalog->get_Entries();
    if (entries == nullptr)
    {
return false;    }
for (int32_t entryIndex = 0; entryIndex < entries->get_Length(); entryIndex++) {
RuntimeSceneCatalogEntry *entry = (*entries)[entryIndex];
    if (entry == nullptr)
    {
continue;
    }
    if (String::Equals(entry->get_SceneId(), sceneId, StringComparison::OrdinalIgnoreCase))
    {
return true;    }
}
return false;}

void DemoDiscReturnToMenuComponent::HandleCursorEvent(int2 relativePosition, int2 delta, PointerInteraction interaction)
{
    if (!this->AllowPointerReturn)
    {
return;    }
    if (interaction == PointerInteraction::Press)
    {
this->PointerPressStartedInside = true;
return;    }
    if (interaction == PointerInteraction::Release)
    {
const bool shouldReturnToMenu = this->PointerPressStartedInside;
this->PointerPressStartedInside = false;
    if (shouldReturnToMenu)
    {
this->LoadResolvedMainMenuScene();
    }
return;    }
    if (interaction == PointerInteraction::Leave)
    {
this->PointerPressStartedInside = false;
    }
}

void DemoDiscReturnToMenuComponent::LoadResolvedMainMenuScene()
{
    if (this->SceneLoadWasRequested)
    {
return;    }
const std::string resolvedSceneId = SceneMapComponent::ResolveSceneId(MainMenuSceneId);
    if (!this->CanLoadRuntimeScene(resolvedSceneId))
    {
return;    }
this->SceneLoadWasRequested = true;
Core::get_Instance()->get_SceneManager()->LoadScene(resolvedSceneId, SceneLoadMode::Single);
}

void DemoDiscReturnToMenuComponent::TryBindInteractable()
{
    if (!this->AllowPointerReturn)
    {
return;    }
else {
    if (this->BoundInteractable != nullptr || this->get_Parent() == nullptr || this->get_Parent()->get_Components() == nullptr)
    {
return;    }
}
for (int32_t componentIndex = 0; componentIndex < this->get_Parent()->get_Components()->get_Count(); componentIndex++) {
    InteractableComponent* interactable = he_cpp_try_cast<InteractableComponent>((*this->get_Parent()->get_Components()).get_Item(componentIndex));
    if (interactable != nullptr)
    {
this->BoundInteractable = interactable;
this->BoundInteractable->CursorEvent += &DemoDiscReturnToMenuComponent::HandleCursorEvent;
return;    }
}
}

void DemoDiscReturnToMenuComponent::UnbindInteractable()
{
    if (this->BoundInteractable == nullptr)
    {
return;    }
this->BoundInteractable->CursorEvent -= &DemoDiscReturnToMenuComponent::HandleCursorEvent;
this->BoundInteractable = nullptr;
this->PointerPressStartedInside = false;
}

bool DemoDiscReturnToMenuComponent::WasGamepadButtonPressed(InputGamepadState currentState, InputGamepadState previousState, InputGamepadButton button)
{
return currentState.IsButtonDown(button) && !previousState.IsButtonDown(button);}

bool DemoDiscReturnToMenuComponent::WasGamepadReturnPressed(InputSystem* inputSystem)
{
    if (inputSystem == nullptr)
    {
throw new ArgumentNullException("inputSystem");
    }
    if (Core::get_Instance() == nullptr)
    {
throw new InvalidOperationException("A core instance must exist before querying the standard platform return action.");
    }
return Core::get_Instance()->get_StandardPlatformInput()->WasActionPressed(StandardPlatformAction::Return);}

bool DemoDiscReturnToMenuComponent::WasKeyboardReturnPressed(InputSystem* inputSystem)
{
return inputSystem->WasKeyPressed(Keys::Escape) || inputSystem->WasKeyPressed(Keys::Back);}

