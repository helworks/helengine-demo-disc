#ifdef DrawText
#undef DrawText
#endif
#include "PlatformInfoTextComponent.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_list.hpp"
#include "runtime/native_string.hpp"
#include "PlatformInfoTextComponent.hpp"
#include "system/math.hpp"
#include "Component.hpp"
#include "Core.hpp"
#include "Entity.hpp"
#include "FontAsset.hpp"
#include "PlatformInfo.hpp"
#include "TextAlignment.hpp"
#include "TextComponent.hpp"
#include "UpdateComponent.hpp"
#include "float2.hpp"
#include "float3.hpp"
#include "int2.hpp"
#include "system/math.hpp"
#include "runtime/native_cast.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_list.hpp"

PlatformInfoTextComponent::PlatformInfoTextComponent() : IsInitialized(), OwnerEntity(), PlatformNameBaseLocalPosition(), PlatformNameTextComponent(), PlatformNameTextEntity(), PlatformVersionBaseLocalPosition(), PlatformVersionTextComponent(), PlatformVersionTextEntity()
{
}

void PlatformInfoTextComponent::ComponentAdded(Entity* entity)
{
UpdateComponent::ComponentAdded(entity);
    if (entity == nullptr)
    {
throw new ArgumentNullException("entity");
    }
this->OwnerEntity = entity;
this->IsInitialized = false;
this->ClearBinding();
this->TryInitialize();
}

void PlatformInfoTextComponent::ComponentRemoved(Entity* entity)
{
this->ClearBinding();
this->OwnerEntity = nullptr;
this->IsInitialized = false;
UpdateComponent::ComponentRemoved(entity);
}

void PlatformInfoTextComponent::Update()
{
    if (!this->IsInitialized)
    {
this->TryInitialize();
    }
}

void PlatformInfoTextComponent::ApplyCurrentPlatformInfo()
{
    if (Core::get_Instance() == nullptr)
    {
throw new InvalidOperationException("Platform info requires an active Core instance.");
    }
else {
    if (Core::get_Instance()->get_PlatformInfo() == nullptr)
    {
throw new InvalidOperationException("Platform info requires initialized runtime platform metadata.");
    }
}
const bool useHorizontalRowLayout = Math::Abs(this->PlatformNameBaseLocalPosition.Y - this->PlatformVersionBaseLocalPosition.Y) < 0.01f;
    if (useHorizontalRowLayout)
    {
this->ApplyHorizontalText(this->PlatformNameTextEntity, this->PlatformNameTextComponent, Core::get_Instance()->get_PlatformInfo()->get_Name(), this->PlatformNameBaseLocalPosition.X, this->PlatformNameBaseLocalPosition.Y, TextAlignment::Left);
this->ApplyHorizontalText(this->PlatformVersionTextEntity, this->PlatformVersionTextComponent, Core::get_Instance()->get_PlatformInfo()->get_Version(), this->PlatformVersionBaseLocalPosition.X, this->PlatformVersionBaseLocalPosition.Y, TextAlignment::Right);
return;    }
this->ApplyText(this->PlatformNameTextEntity, this->PlatformNameTextComponent, Core::get_Instance()->get_PlatformInfo()->get_Name(), 0.0f);
this->ApplyText(this->PlatformVersionTextEntity, this->PlatformVersionTextComponent, Core::get_Instance()->get_PlatformInfo()->get_Version(), this->PlatformNameTextComponent->get_Size().Y + 6.0f);
}

void PlatformInfoTextComponent::ApplyHorizontalText(Entity* entity, TextComponent* textComponent, std::string text, float baseX, float baseY, TextAlignment alignment)
{
    if (entity == nullptr)
    {
throw new ArgumentNullException("entity");
    }
else {
    if (textComponent == nullptr)
    {
throw new ArgumentNullException("textComponent");
    }
}
textComponent->set_Alignment(alignment);
textComponent->set_Text(text);
float2 measuredSize = textComponent->get_Font()->MeasureString(text);
const double fontScale = textComponent->get_FontScale();
textComponent->set_Size(([&]() {
auto __ctor_arg_0000009E = static_cast<int32_t>(Math::Ceiling(measuredSize.X * fontScale));
auto __ctor_arg_0000009F = static_cast<int32_t>(Math::Ceiling(measuredSize.Y * fontScale));
return int2(__ctor_arg_0000009E, __ctor_arg_0000009F);
})());
    if (alignment == TextAlignment::Right)
    {
entity->set_LocalPosition(float3(baseX - textComponent->get_Size().X, baseY, 0.0f));
return;    }
entity->set_LocalPosition(float3(baseX, baseY, 0.0f));
}

void PlatformInfoTextComponent::ApplyText(Entity* entity, TextComponent* textComponent, std::string text, float topOffset)
{
    if (entity == nullptr)
    {
throw new ArgumentNullException("entity");
    }
else {
    if (textComponent == nullptr)
    {
throw new ArgumentNullException("textComponent");
    }
}
textComponent->set_Alignment(TextAlignment::Right);
textComponent->set_Text(text);
float2 measuredSize = textComponent->get_Font()->MeasureString(text);
const double fontScale = textComponent->get_FontScale();
textComponent->set_Size(([&]() {
auto __ctor_arg_000000A0 = static_cast<int32_t>(Math::Ceiling(measuredSize.X * fontScale));
auto __ctor_arg_000000A1 = static_cast<int32_t>(Math::Ceiling(measuredSize.Y * fontScale));
return int2(__ctor_arg_000000A0, __ctor_arg_000000A1);
})());
entity->set_LocalPosition(float3(-textComponent->get_Size().X, topOffset, 0.0f));
}

bool PlatformInfoTextComponent::AreTextComponentsReadyForLayout()
{
    if (this->PlatformNameTextComponent == nullptr || this->PlatformVersionTextComponent == nullptr)
    {
return false;    }
return this->PlatformNameTextComponent->get_Font() != nullptr && this->PlatformVersionTextComponent->get_Font() != nullptr;}

void PlatformInfoTextComponent::ClearBinding()
{
this->PlatformNameTextEntity = nullptr;
this->PlatformVersionTextEntity = nullptr;
this->PlatformNameTextComponent = nullptr;
this->PlatformVersionTextComponent = nullptr;
this->PlatformNameBaseLocalPosition = float3::get_Zero();
this->PlatformVersionBaseLocalPosition = float3::get_Zero();
}

void PlatformInfoTextComponent::CollectChildTextEntities(Entity* parentEntity, List<Entity*>* textEntities)
{
    if (parentEntity == nullptr)
    {
throw new ArgumentNullException("parentEntity");
    }
else {
    if (textEntities == nullptr)
    {
throw new ArgumentNullException("textEntities");
    }
else {
    if (parentEntity->get_Children() == nullptr)
    {
return;    }
}
}
for (int32_t childIndex = 0; childIndex < parentEntity->get_Children()->get_Count(); childIndex++) {
Entity *childEntity = (*parentEntity->get_Children()).get_Item(childIndex);
    if (childEntity == nullptr)
    {
continue;
    }
TextComponent* textComponent;
    if (this->TryFindTextComponent__out1(childEntity, textComponent))
    {
textEntities->Add(childEntity);
    if (textEntities->get_Count() >= 2)
    {
return;    }
    }
this->CollectChildTextEntities(childEntity, textEntities);
    if (textEntities->get_Count() >= 2)
    {
return;    }
}
}

TextComponent* PlatformInfoTextComponent::FindTextComponent(Entity* entity)
{
    if (entity == nullptr)
    {
throw new ArgumentNullException("entity");
    }
for (int32_t index = 0; index < entity->get_Components()->get_Count(); index++) {
    TextComponent* textComponent = he_cpp_try_cast<TextComponent>((*entity->get_Components()).get_Item(index));
    if (textComponent != nullptr)
    {
return textComponent;    }
}
throw new InvalidOperationException("Platform-info overlay child must include a text component.");
}

bool PlatformInfoTextComponent::TryBindTextEntities(Entity* entity)
{
    if (entity == nullptr)
    {
throw new ArgumentNullException("entity");
    }
List<Entity*> *textEntities = new List<Entity*>();
this->CollectChildTextEntities(entity, textEntities);
    if (textEntities->get_Count() < 2)
    {
return false;    }
this->PlatformNameTextEntity = (*textEntities).get_Item(0);
this->PlatformVersionTextEntity = (*textEntities).get_Item(1);
this->PlatformNameTextComponent = this->FindTextComponent(this->PlatformNameTextEntity);
this->PlatformVersionTextComponent = this->FindTextComponent(this->PlatformVersionTextEntity);
this->PlatformNameBaseLocalPosition = this->PlatformNameTextEntity->get_LocalPosition();
this->PlatformVersionBaseLocalPosition = this->PlatformVersionTextEntity->get_LocalPosition();
return true;}

bool PlatformInfoTextComponent::TryFindTextComponent__out1(Entity* entity, TextComponent*& textComponent)
{
    if (entity == nullptr)
    {
throw new ArgumentNullException("entity");
    }
for (int32_t index = 0; index < entity->get_Components()->get_Count(); index++) {
    TextComponent* foundTextComponent = he_cpp_try_cast<TextComponent>((*entity->get_Components()).get_Item(index));
    if (foundTextComponent != nullptr)
    {
textComponent = foundTextComponent;
return true;    }
}
textComponent = nullptr;
return false;}

void PlatformInfoTextComponent::TryInitialize()
{
    if (this->OwnerEntity == nullptr)
    {
return;    }
    if (!this->TryBindTextEntities(this->OwnerEntity))
    {
return;    }
else {
    if (!this->AreTextComponentsReadyForLayout())
    {
return;    }
}
this->ApplyCurrentPlatformInfo();
this->IsInitialized = true;
}

