#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

class DemoDiscDirectionalLightToggleState;

#include "runtime/native_disposable.hpp"
#include "runtime/native_list.hpp"

class DemoDiscLightToggleComponent : public ::UpdateComponent
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
