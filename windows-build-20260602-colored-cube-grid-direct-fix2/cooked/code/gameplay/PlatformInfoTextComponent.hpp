#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "runtime/native_disposable.hpp"
#include "runtime/native_string.hpp"
#include "runtime/native_list.hpp"

class PlatformInfoTextComponent : public ::UpdateComponent
{
public:
    virtual ~PlatformInfoTextComponent() = default;

    PlatformInfoTextComponent();

    void ComponentAdded(Entity* entity);

    void ComponentRemoved(Entity* entity);

    void Update();
private:
    bool IsInitialized;

    Entity* OwnerEntity;

    float3 PlatformNameBaseLocalPosition;

    TextComponent* PlatformNameTextComponent;

    Entity* PlatformNameTextEntity;

    float3 PlatformVersionBaseLocalPosition;

    TextComponent* PlatformVersionTextComponent;

    Entity* PlatformVersionTextEntity;

    void ApplyCurrentPlatformInfo();

    void ApplyHorizontalText(Entity* entity, TextComponent* textComponent, std::string text, float baseX, float baseY, TextAlignment alignment);

    void ApplyText(Entity* entity, TextComponent* textComponent, std::string text, float topOffset);

    bool AreTextComponentsReadyForLayout();

    void ClearBinding();

    void CollectChildTextEntities(Entity* parentEntity, List<Entity*>* textEntities);

    TextComponent* FindTextComponent(Entity* entity);

    bool TryBindTextEntities(Entity* entity);

    bool TryFindTextComponent__out1(Entity* entity, TextComponent*& textComponent);

    void TryInitialize();
};
