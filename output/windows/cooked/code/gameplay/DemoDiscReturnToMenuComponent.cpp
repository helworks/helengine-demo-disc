#ifdef DrawText
#undef DrawText
#endif
#include "DemoDiscReturnToMenuComponent.hpp"
#include "InputSystem.hpp"
#include "runtime/native_exceptions.hpp"
#include "SceneAsset.hpp"
#include "InputGamepadState.hpp"
#include "runtime/native_exceptions.hpp"

DemoDiscReturnToMenuComponent::DemoDiscReturnToMenuComponent() : PreviousGamepadState()
{
}

std::string DemoDiscReturnToMenuComponent::MainMenuSceneId = "DemoDiscMainMenu";

std::string DemoDiscReturnToMenuComponent::MainMenuScenePath = "scenes/DemoDiscMainMenu.helen";

void DemoDiscReturnToMenuComponent::Update()
{
InputSystem *inputSystem = Core->Instance != nullptr ? Core->Instance->Input : nullptr;
    if (inputSystem == nullptr)
    {
this->PreviousGamepadState = nullptr;
return;    }
    if (!this->WasReturnPressed(inputSystem))
    {
this->PreviousGamepadState = this->ReadPrimaryGamepadState(inputSystem);
return;    }
this->ReturnToMainMenu();
}

InputGamepadState* DemoDiscReturnToMenuComponent::ReadPrimaryGamepadState(InputSystem* inputSystem)
{
    if (inputSystem == nullptr)
    {
return nullptr;    }
return inputSystem->GetGamepadState(0);}

void DemoDiscReturnToMenuComponent::ReturnToMainMenu()
{
    if (Core->Instance == nullptr)
    {
throw new InvalidOperationException("A core instance must exist before returning to the demo-disc main menu.");
    }
    if (ComponentExecutionContext->CurrentMode == ComponentExecutionMode->Editor)
    {
    if (Core->Instance->SceneLoadService == nullptr)
    {
throw new InvalidOperationException("Core scene loading services must be initialized before returning to the demo-disc main menu.");
    }
SceneAsset *sceneAsset = Core->Instance->ContentManager->Load<SceneAsset*>(MainMenuScenePath, RuntimeContentProcessorIds->SceneAsset);
Core->Instance->SceneLoadService->Load(sceneAsset);
    if (Parent != nullptr)
    {
Parent->Enabled = false;
    }
    }
else     if (Core->Instance->SceneManager == nullptr)
    {
throw new InvalidOperationException("Core scene manager must be initialized before returning to the demo-disc main menu.");
    }
else {
Core->Instance->SceneManager->LoadScene(MainMenuSceneId, SceneLoadMode->Single);
}
}

bool DemoDiscReturnToMenuComponent::WasGamepadButtonPressed(InputGamepadState* currentState, InputGamepadState* previousState, InputGamepadButton* button)
{
return currentState->IsButtonDown(button) && !previousState->IsButtonDown(button);}

bool DemoDiscReturnToMenuComponent::WasReturnPressed(InputSystem* inputSystem)
{
    if (inputSystem->WasKeyPressed(Keys->Escape) || inputSystem->WasKeyPressed(Keys->Back))
    {
return true;    }
InputGamepadState *currentGamepadState = this->ReadPrimaryGamepadState(inputSystem);
    if (!currentGamepadState->Connected)
    {
this->PreviousGamepadState = currentGamepadState;
return false;    }
return this->WasGamepadButtonPressed(currentGamepadState, this->PreviousGamepadState, InputGamepadButton->East) || this->WasGamepadButtonPressed(currentGamepadState, this->PreviousGamepadState, InputGamepadButton->North) || this->WasGamepadButtonPressed(currentGamepadState, this->PreviousGamepadState, InputGamepadButton->Select);}

