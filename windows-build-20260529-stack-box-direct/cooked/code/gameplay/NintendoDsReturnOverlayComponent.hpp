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

class NintendoDsReturnOverlayComponent : public UpdateComponent
{
public:
    virtual ~NintendoDsReturnOverlayComponent() = default;

    NintendoDsReturnOverlayComponent();

    static std::string MainMenuSceneId;

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
