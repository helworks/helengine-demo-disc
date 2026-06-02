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
#include "runtime/native_exceptions.hpp"
#include "runtime/native_list.hpp"
#include "float2.hpp"
#include "TextComponent.hpp"
#include "FontAsset.hpp"
#include "Entity.hpp"
#include "float3.hpp"
#include "TextComponent.hpp"
#include "runtime/native_string.hpp"
#include "TextAlignment.hpp"

class PlatformInfoTextComponent : public UpdateComponent
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

    bool TryFindTextComponent(Entity* entity, TextComponent*& textComponent);

    void TryInitialize();
};
