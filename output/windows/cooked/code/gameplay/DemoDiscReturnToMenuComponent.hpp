#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "UpdateComponent.hpp"
#include "InputSystem.hpp"
#include "runtime/native_exceptions.hpp"
#include "SceneAsset.hpp"
#include "InputGamepadState.hpp"
#include "runtime/native_string.hpp"
#include "InputGamepadButton.hpp"

class DemoDiscReturnToMenuComponent : public UpdateComponent
{
public:
    virtual ~DemoDiscReturnToMenuComponent() = default;

    DemoDiscReturnToMenuComponent();

    static std::string MainMenuSceneId;

    static std::string MainMenuScenePath;

    void Update();
private:
    InputGamepadState* PreviousGamepadState;

    InputGamepadState* ReadPrimaryGamepadState(InputSystem* inputSystem);

    void ReturnToMainMenu();

    bool WasGamepadButtonPressed(InputGamepadState* currentState, InputGamepadState* previousState, InputGamepadButton* button);

    bool WasReturnPressed(InputSystem* inputSystem);
};
