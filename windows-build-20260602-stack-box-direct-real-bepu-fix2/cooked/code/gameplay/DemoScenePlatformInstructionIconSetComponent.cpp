#ifdef DrawText
#undef DrawText
#endif
#include "DemoScenePlatformInstructionIconSetComponent.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_string.hpp"
#include "DemoScenePlatformInstructionIconSetComponent.hpp"
#include "runtime/native_list.hpp"
#include "Component.hpp"
#include "Core.hpp"
#include "Entity.hpp"
#include "PlatformInfo.hpp"
#include "UpdateComponent.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_list.hpp"
#include "runtime/native_string.hpp"

DemoScenePlatformInstructionIconSetComponent::DemoScenePlatformInstructionIconSetComponent() : IsConfigured(), OwnerEntity()
{
}

void DemoScenePlatformInstructionIconSetComponent::ComponentAdded(Entity* entity)
{
UpdateComponent::ComponentAdded(entity);
    if (entity == nullptr)
    {
throw new ArgumentNullException("entity");
    }
this->OwnerEntity = entity;
this->IsConfigured = false;
this->TryApplyPlatformSelection();
}

void DemoScenePlatformInstructionIconSetComponent::ComponentRemoved(Entity* entity)
{
this->OwnerEntity = nullptr;
this->IsConfigured = false;
UpdateComponent::ComponentRemoved(entity);
}

void DemoScenePlatformInstructionIconSetComponent::Update()
{
    if (this->IsConfigured)
    {
return;    }
this->TryApplyPlatformSelection();
}

bool DemoScenePlatformInstructionIconSetComponent::ContainsNormalizedPlatformToken(std::string normalizedPlatformName, std::string token)
{
    if (String::IsNullOrWhiteSpace(normalizedPlatformName))
    {
throw ([&]() {
auto __ctor_arg_00000092 = "Normalized platform name must be provided.";
auto __ctor_arg_00000093 = "normalizedPlatformName";
return new ArgumentException(__ctor_arg_00000092, __ctor_arg_00000093);
})();
    }
else {
    if (String::IsNullOrWhiteSpace(token))
    {
throw ([&]() {
auto __ctor_arg_00000094 = "Platform token must be provided.";
auto __ctor_arg_00000095 = "token";
return new ArgumentException(__ctor_arg_00000094, __ctor_arg_00000095);
})();
    }
else {
    if (static_cast<int32_t>(token.size()) > static_cast<int32_t>(normalizedPlatformName.size()))
    {
return false;    }
}
}
const int32_t lastStartIndex = static_cast<int32_t>(normalizedPlatformName.size()) - static_cast<int32_t>(token.size());
for (int32_t startIndex = 0; startIndex <= lastStartIndex; startIndex++) {
bool matched = true;
for (int32_t tokenIndex = 0; tokenIndex < static_cast<int32_t>(token.size()); tokenIndex++) {
    if (normalizedPlatformName[startIndex + tokenIndex] == token[tokenIndex])
    {
continue;
    }
matched = false;
break;
}
    if (matched)
    {
return true;    }
}
return false;}

int32_t DemoScenePlatformInstructionIconSetComponent::ResolveSelectedGroupIndex(std::string platformName)
{
    if (String::IsNullOrWhiteSpace(platformName))
    {
throw ([&]() {
auto __ctor_arg_00000096 = "Platform name must be provided.";
auto __ctor_arg_00000097 = "platformName";
return new ArgumentException(__ctor_arg_00000096, __ctor_arg_00000097);
})();
    }
const std::string normalizedPlatformName = String::ToLowerInvariant(String::Trim(platformName));
    if (this->ContainsNormalizedPlatformToken(normalizedPlatformName, "3ds") || normalizedPlatformName == "ds")
    {
return SwitchGroupChildIndex;    }
else {
    if (this->ContainsNormalizedPlatformToken(normalizedPlatformName, "ps2") || this->ContainsNormalizedPlatformToken(normalizedPlatformName, "psp"))
    {
return Ps2GroupChildIndex;    }
else {
    if (this->ContainsNormalizedPlatformToken(normalizedPlatformName, "windows") || this->ContainsNormalizedPlatformToken(normalizedPlatformName, "win32") || this->ContainsNormalizedPlatformToken(normalizedPlatformName, "gamecube"))
    {
return Xbox360GroupChildIndex;    }
}
}
return Xbox360GroupChildIndex;}

void DemoScenePlatformInstructionIconSetComponent::TryApplyPlatformSelection()
{
    if (this->OwnerEntity == nullptr || this->OwnerEntity->get_Children() == nullptr || this->OwnerEntity->get_Children()->get_Count() == 0)
    {
return;    }
else {
    if (Core::get_Instance() == nullptr || Core::get_Instance()->get_PlatformInfo() == nullptr || String::IsNullOrWhiteSpace(Core::get_Instance()->get_PlatformInfo()->get_Name()))
    {
return;    }
}
const int32_t selectedGroupIndex = this->ResolveSelectedGroupIndex(Core::get_Instance()->get_PlatformInfo()->get_Name());
for (int32_t childIndex = 0; childIndex < this->OwnerEntity->get_Children()->get_Count(); childIndex++) {
Entity *childEntity = (*this->OwnerEntity->get_Children()).get_Item(childIndex);
    if (childEntity == nullptr)
    {
continue;
    }
childEntity->set_Enabled(childIndex == selectedGroupIndex);
}
this->IsConfigured = true;
}

