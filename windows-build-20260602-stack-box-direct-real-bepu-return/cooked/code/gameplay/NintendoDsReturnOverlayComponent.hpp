#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "runtime/native_disposable.hpp"
#include "runtime/native_string.hpp"

class NintendoDsReturnOverlayComponent : public ::UpdateComponent
{
public:
    virtual ~NintendoDsReturnOverlayComponent() = default;

    NintendoDsReturnOverlayComponent();

    inline static const std::string MainMenuSceneId = "DemoDiscMainMenu";

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

    bool WasGamepadReturnPressed(InputSystem* inputSystem);
};
