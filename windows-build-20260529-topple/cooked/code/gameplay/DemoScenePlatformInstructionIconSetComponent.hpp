#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "UpdateComponent.hpp"
#include "runtime/native_disposable.hpp"
#include "IUpdateable.hpp"
#include "UpdateComponent.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_string.hpp"
#include "runtime/native_exceptions.hpp"
#include "Entity.hpp"

class DemoScenePlatformInstructionIconSetComponent : public UpdateComponent
{
public:
    virtual ~DemoScenePlatformInstructionIconSetComponent() = default;

    DemoScenePlatformInstructionIconSetComponent();

    void ComponentAdded(Entity* entity);

    void ComponentRemoved(Entity* entity);

    void Update();
private:
    static int32_t Ps2GroupChildIndex;

    static int32_t SwitchGroupChildIndex;

    static int32_t Xbox360GroupChildIndex;

    bool IsConfigured;

    Entity* OwnerEntity;

    bool ContainsNormalizedPlatformToken(std::string normalizedPlatformName, std::string token);

    int32_t ResolveSelectedGroupIndex(std::string platformName);

    void TryApplyPlatformSelection();
};
