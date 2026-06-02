#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "runtime/native_disposable.hpp"
#include "runtime/native_string.hpp"

class DemoDiscReturnToMenuComponent : public ::UpdateComponent
{
public:
    virtual ~DemoDiscReturnToMenuComponent() = default;

    DemoDiscReturnToMenuComponent();

    inline static const std::string MainMenuSceneId = "DemoDiscMainMenu";

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

    bool CanLoadRuntimeScene(std::string sceneId);

    void HandleCursorEvent(int2 relativePosition, int2 delta, PointerInteraction interaction);

    void LoadResolvedMainMenuScene();

    void TryBindInteractable();

    void UnbindInteractable();

    static bool WasGamepadButtonPressed(InputGamepadState currentState, InputGamepadState previousState, InputGamepadButton button);

    bool WasGamepadReturnPressed(InputSystem* inputSystem);

    bool WasKeyboardReturnPressed(InputSystem* inputSystem);
};
