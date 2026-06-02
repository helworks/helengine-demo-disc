#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "UpdateComponent.hpp"
#include "runtime/native_disposable.hpp"
#include "IUpdateable.hpp"
#include "UpdateComponent.hpp"
#include "InputSystem.hpp"
#include "Core.hpp"
#include "Core.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_string.hpp"
#include "SceneMapComponent.hpp"
#include "SceneMapComponent.hpp"
#include "SceneManager.hpp"
#include "InteractableComponent.hpp"
#include "Entity.hpp"
#include "int2.hpp"
#include "PointerInteraction.hpp"
#include "InputGamepadState.hpp"
#include "InputGamepadButton.hpp"

class DemoDiscReturnToMenuComponent : public UpdateComponent
{
public:
    virtual ~DemoDiscReturnToMenuComponent() = default;

    DemoDiscReturnToMenuComponent();

    static std::string MainMenuSceneId;

    bool AllowGamepadReturn;

    bool get_AllowGamepadReturn();
    void set_AllowGamepadReturn(bool value);

    bool AllowKeyboardReturn;

    bool get_AllowKeyboardReturn();
    void set_AllowKeyboardReturn(bool value);

    bool AllowPointerReturn;

    bool get_AllowPointerReturn();
    void set_AllowPointerReturn(bool value);

    void ComponentAdded(Entity* entity);

    void ComponentRemoved(Entity* entity);

    void Dispose();

    void Update();
private:
    InteractableComponent* BoundInteractable;

    bool PointerPressStartedInside;

    bool SceneLoadWasRequested;

    void HandleCursorEvent(int2 relativePosition, int2 delta, PointerInteraction interaction);

    void LoadResolvedMainMenuScene();

    void TryBindInteractable();

    void UnbindInteractable();

    static bool WasGamepadButtonPressed(InputGamepadState currentState, InputGamepadState previousState, InputGamepadButton button);

    bool WasGamepadReturnPressed(InputSystem* inputSystem);

    bool WasKeyboardReturnPressed(InputSystem* inputSystem);
};
