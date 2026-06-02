#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

class DemoDiscDirectionalLightToggleState;

#include "UpdateComponent.hpp"
#include "runtime/native_disposable.hpp"
#include "IUpdateable.hpp"
#include "UpdateComponent.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_list.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_list.hpp"
#include "Core.hpp"
#include "Core.hpp"
#include "ObjectManager.hpp"
#include "InputSystem.hpp"
#include "runtime/native_list.hpp"
#include "Entity.hpp"

class DemoDiscLightToggleComponent : public UpdateComponent
{
public:
    virtual ~DemoDiscLightToggleComponent() = default;

    void ComponentAdded(Entity* entity);

    void ComponentRemoved(Entity* entity);

    DemoDiscLightToggleComponent();

    void Dispose();

    void Update();
private:
    List<::DemoDiscDirectionalLightToggleState*>* LightStates;

    bool LightsEnabled;

    void ApplyDirectionalLightState();

    void CaptureDirectionalLightStates();

    bool WasToggleRequested();
};
