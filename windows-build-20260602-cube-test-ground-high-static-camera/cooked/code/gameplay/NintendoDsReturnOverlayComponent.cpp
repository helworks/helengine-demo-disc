#ifdef DrawText
#undef DrawText
#endif
#include "NintendoDsReturnOverlayComponent.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_string.hpp"
#include "NintendoDsReturnOverlayComponent.hpp"
#include "runtime/native_list.hpp"
#include "system/action.hpp"
#include "Component.hpp"
#include "Core.hpp"
#include "Entity.hpp"
#include "InteractableComponent.hpp"
#include "PointerInteraction.hpp"
#include "SceneLoadMode.hpp"
#include "SceneManager.hpp"
#include "SceneMapComponent.hpp"
#include "StandardPlatformAction.hpp"
#include "StandardPlatformInput.hpp"
#include "UpdateComponent.hpp"
#include "runtime/native_cast.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_list.hpp"

NintendoDsReturnOverlayComponent::NintendoDsReturnOverlayComponent() : BoundInteractable(), PointerPressStartedInside(), SceneLoadWasRequested()
{
}

void NintendoDsReturnOverlayComponent::ComponentAdded(Entity* entity)
{
UpdateComponent::ComponentAdded(entity);
this->TryBindInteractable();
}

void NintendoDsReturnOverlayComponent::ComponentRemoved(Entity* entity)
{
this->UnbindInteractable();
UpdateComponent::ComponentRemoved(entity);
}

void NintendoDsReturnOverlayComponent::Dispose()
{
this->UnbindInteractable();
}

void NintendoDsReturnOverlayComponent::Update()
{
this->TryBindInteractable();
InputSystem *inputSystem = Core::get_Instance()->get_Input();
    if (this->WasGamepadReturnPressed(inputSystem))
    {
this->LoadResolvedMainMenuScene();
    }
}

void NintendoDsReturnOverlayComponent::HandleCursorEvent(int2 relativePosition, int2 delta, PointerInteraction interaction)
{
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

void NintendoDsReturnOverlayComponent::LoadResolvedMainMenuScene()
{
    if (this->SceneLoadWasRequested)
    {
return;    }
    if (Core::get_Instance() == nullptr)
    {
throw new InvalidOperationException("A core instance must exist before returning to the main menu.");
    }
    if (Core::get_Instance()->get_SceneManager() == nullptr)
    {
throw new InvalidOperationException("Core scene manager must be initialized before runtime menu scene loading can occur.");
    }
const std::string resolvedSceneId = SceneMapComponent::ResolveSceneId(MainMenuSceneId);
this->SceneLoadWasRequested = true;
Core::get_Instance()->get_SceneManager()->LoadScene(resolvedSceneId, SceneLoadMode::Single);
}

void NintendoDsReturnOverlayComponent::TryBindInteractable()
{
    if (this->BoundInteractable != nullptr)
    {
return;    }
else {
    if (this->get_Parent() == nullptr || this->get_Parent()->get_Components() == nullptr)
    {
return;    }
}
for (int32_t componentIndex = 0; componentIndex < this->get_Parent()->get_Components()->get_Count(); componentIndex++) {
    InteractableComponent* interactable = he_cpp_try_cast<InteractableComponent>((*this->get_Parent()->get_Components()).get_Item(componentIndex));
    if (interactable != nullptr)
    {
this->BoundInteractable = interactable;
this->BoundInteractable->CursorEvent += &NintendoDsReturnOverlayComponent::HandleCursorEvent;
return;    }
}
throw new InvalidOperationException("NintendoDsReturnOverlayComponent requires a sibling InteractableComponent.");
}

void NintendoDsReturnOverlayComponent::UnbindInteractable()
{
    if (this->BoundInteractable == nullptr)
    {
return;    }
this->BoundInteractable->CursorEvent -= &NintendoDsReturnOverlayComponent::HandleCursorEvent;
this->BoundInteractable = nullptr;
this->PointerPressStartedInside = false;
}

bool NintendoDsReturnOverlayComponent::WasGamepadReturnPressed(InputSystem* inputSystem)
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

