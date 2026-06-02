#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "runtime/native_disposable.hpp"
#include "runtime/native_string.hpp"

class DemoScenePlatformInstructionIconSetComponent : public ::UpdateComponent
{
public:
    virtual ~DemoScenePlatformInstructionIconSetComponent() = default;

    DemoScenePlatformInstructionIconSetComponent();

    void ComponentAdded(Entity* entity);

    void ComponentRemoved(Entity* entity);

    void Update();
private:
    inline static const int32_t Ps2GroupChildIndex = 1;

    inline static const int32_t SwitchGroupChildIndex = 2;

    inline static const int32_t Xbox360GroupChildIndex = 0;

    bool IsConfigured;

    Entity* OwnerEntity;

    bool ContainsNormalizedPlatformToken(std::string normalizedPlatformName, std::string token);

    int32_t ResolveSelectedGroupIndex(std::string platformName);

    void TryApplyPlatformSelection();
};
