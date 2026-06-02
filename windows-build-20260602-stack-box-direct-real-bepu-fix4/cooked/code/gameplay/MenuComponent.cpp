#ifdef DrawText
#undef DrawText
#endif
#include "MenuComponent.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_string.hpp"
#include "runtime/native_dictionary.hpp"
#include "runtime/native_list.hpp"
#include "runtime/array.hpp"
#include "MenuPanelRuntime.hpp"
#include "MenuItemRuntime.hpp"
#include "MenuItemComponent.hpp"
#include "MenuPanelComponent.hpp"
#include "MenuActionKind.hpp"
#include "DemoMenuLayout.hpp"
#include "MenuComponent.hpp"
#include "system/string_comparer.hpp"
#include "system/action.hpp"
#include "system/math.hpp"
#include "Component.hpp"
#include "ComponentExecutionContext.hpp"
#include "ComponentExecutionMode.hpp"
#include "Core.hpp"
#include "Entity.hpp"
#include "InputGamepadButton.hpp"
#include "InputGamepadState.hpp"
#include "InputSystem.hpp"
#include "Keys.hpp"
#include "RoundedRectComponent.hpp"
#include "SceneLoadMode.hpp"
#include "SceneManager.hpp"
#include "SceneMapComponent.hpp"
#include "ScrollComponent.hpp"
#include "StandardPlatformAction.hpp"
#include "StandardPlatformInput.hpp"
#include "UpdateComponent.hpp"
#include "ViewportComponent.hpp"
#include "float3.hpp"
#include "float4.hpp"
#include "int2.hpp"
#include "system/math.hpp"
#include "runtime/array.hpp"
#include "runtime/native_cast.hpp"
#include "runtime/native_dictionary.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_list.hpp"
#include "runtime/native_string.hpp"
#include "runtime/native_type.hpp"
#include "system/string_comparer.hpp"

const std::string& MenuComponent::get_ActivePanelId()
{
return this->ActivePanelIdValue;}

const std::string& MenuComponent::get_InitialPanelId()
{
return this->InitialPanelIdValue;}

void MenuComponent::set_InitialPanelId(std::string value)
{
this->InitialPanelIdValue = value;
}

bool MenuComponent::get_IsInitialized()
{
return this->IsInitialized;
}

void MenuComponent::set_IsInitialized(bool value)
{
this->IsInitialized = value;
}

const std::string& MenuComponent::get_ProviderTypeName()
{
return this->ProviderTypeNameValue;}

void MenuComponent::set_ProviderTypeName(std::string value)
{
this->ProviderTypeNameValue = value;
}

const std::string& MenuComponent::get_SelectedItemId()
{
return this->SelectedItemIdValue;}

void MenuComponent::ComponentAdded(Entity* entity)
{
UpdateComponent::ComponentAdded(entity);
    if (entity == nullptr)
    {
throw new ArgumentNullException("entity");
    }
    if (String::IsNullOrWhiteSpace(this->InitialPanelIdValue))
    {
throw new InvalidOperationException("Menu components require an initial panel id.");
    }
}

void MenuComponent::ComponentRemoved(Entity* entity)
{
this->ReleasePanelRuntimes();
this->PanelsById->Clear();
this->PanelRuntimes->Clear();
this->PanelHistory->Clear();
this->ActivePanel = nullptr;
this->PressedPointerItem = nullptr;
this->ActivePanelIdValue = String::Empty;
this->SelectedItemIdValue = String::Empty;
this->set_IsInitialized(false);
UpdateComponent::ComponentRemoved(entity);
}

void MenuComponent::Dispose()
{
this->ReleasePanelRuntimes();
this->PanelsById->Clear();
this->PanelRuntimes->Clear();
this->PanelHistory->Clear();
this->ActivePanel = nullptr;
this->PressedPointerItem = nullptr;
this->ActivePanelIdValue = String::Empty;
this->SelectedItemIdValue = String::Empty;
this->set_IsInitialized(false);
}

MenuComponent::MenuComponent() : IsInitialized(), ActivePanel(), ActivePanelIdValue(), InitialPanelIdValue(), PanelHistory(), PanelRuntimes(), PanelsById(), PressedPointerItem(), ProviderTypeNameValue(), SelectedItemIdValue()
{
this->PanelsById = new Dictionary<std::string, ::MenuPanelRuntime*>(StringComparer::get_Ordinal());
this->PanelRuntimes = new List<::MenuPanelRuntime*>();
this->PanelHistory = new List<std::string>();
this->ProviderTypeNameValue = String::Empty;
this->InitialPanelIdValue = String::Empty;
this->ActivePanelIdValue = String::Empty;
this->SelectedItemIdValue = String::Empty;
}

void MenuComponent::Update()
{
    if (!this->IsInitialized)
    {
this->TryInitialize();
    if (!this->IsInitialized)
    {
return;    }
    }
InputSystem *inputSystem = Core::get_Instance()->get_Input();
this->HandleKeyboardInput(inputSystem);
this->HandleMouseInput(inputSystem);
this->HandleGamepadInput(inputSystem);
}

void MenuComponent::ActivatePanel(std::string panelId, bool pushHistory)
{
::MenuPanelRuntime* nextPanel;
    if (!this->PanelsById->TryGetValue(panelId, nextPanel))
    {
throw new InvalidOperationException(std::string("Baked menu panel '") + panelId + std::string("' was not registered."));
    }
    if (this->ActivePanel != nullptr)
    {
    if (pushHistory && !String::Equals(this->ActivePanel->Definition->get_PanelId(), panelId, StringComparison::Ordinal))
    {
this->PanelHistory->Add(this->ActivePanel->Definition->get_PanelId());
    }
    }
for (int32_t panelIndex = 0; panelIndex < this->PanelRuntimes->get_Count(); panelIndex++) {
::MenuPanelRuntime *panelRuntime = (*this->PanelRuntimes).get_Item(panelIndex);
panelRuntime->RootEntity->set_Enabled(false);
this->ClearSelectionVisuals(panelRuntime);
}
this->ActivePanel = nextPanel;
this->ActivePanel->RootEntity->set_Enabled(true);
this->ActivePanelIdValue = nextPanel->Definition->get_PanelId();
this->PressedPointerItem = nullptr;
this->SetSelection(nextPanel, this->ResolveSelectedIndex(nextPanel));
}

void MenuComponent::ApplyItemVisualState(::MenuItemRuntime* runtimeItem, bool isSelected)
{
    if (isSelected)
    {
runtimeItem->Background->set_FillColor(runtimeItem->Definition->SelectedFillColor);
runtimeItem->Background->set_BorderColor(runtimeItem->Definition->SelectedBorderColor);
    }
else {
runtimeItem->Background->set_FillColor(runtimeItem->Definition->IdleFillColor);
runtimeItem->Background->set_BorderColor(runtimeItem->Definition->IdleBorderColor);
}
}

void MenuComponent::ApplyItemsScrollOffset(Entity* itemsRootEntity, int32_t scrollOffset)
{
    if (itemsRootEntity == nullptr)
    {
throw new ArgumentNullException("itemsRootEntity");
    }
const float itemStep = this->ResolveItemsScrollStep(itemsRootEntity);
itemsRootEntity->set_LocalPosition(float3(0.0f, -scrollOffset * itemStep, 0.0f));
}

Array<::MenuItemRuntime*>* MenuComponent::BindItems(Entity* panelEntity, std::string panelId)
{
List<Entity*> *itemEntities = new List<Entity*>();
CollectEntitiesWithComponent<MenuItemComponent*>(panelEntity, itemEntities);
Array<::MenuItemRuntime*> *itemRuntimes = new Array<MenuItemRuntime*>(itemEntities->get_Count());
for (int32_t itemIndex = 0; itemIndex < itemEntities->get_Count(); itemIndex++) {
Entity *itemEntity = (*itemEntities).get_Item(itemIndex);
::MenuItemComponent *itemComponent = FindRequiredComponent<MenuItemComponent*>(itemEntity);
    if (!String::Equals(itemComponent->get_PanelId(), panelId, StringComparison::Ordinal))
    {
throw new InvalidOperationException(std::string("Baked menu item '") + itemComponent->get_ItemId() + std::string("' does not match panel '") + panelId + std::string("'."));
    }
RoundedRectComponent *backgroundComponent = FindRequiredComponent<RoundedRectComponent*>(itemEntity);
(*itemRuntimes)[itemIndex] = new ::MenuItemRuntime(itemComponent, itemIndex, itemEntity, backgroundComponent);
}
    if (itemRuntimes->get_Length() == 0)
    {
throw new InvalidOperationException(std::string("Baked menu panel '") + panelId + std::string("' does not contain any items."));
    }
return itemRuntimes;}

void MenuComponent::BindPanels(Entity* rootEntity)
{
this->PanelsById->Clear();
this->PanelRuntimes->Clear();
this->PanelHistory->Clear();
Entity *generatedRootEntity = this->FindGeneratedRootEntity(rootEntity);
    if (generatedRootEntity == nullptr)
    {
throw new InvalidOperationException(std::string("Menu root '") + this->DescribeEntity(rootEntity) + std::string("' is missing the generated menu subtree."));
    }
List<Entity*> *panelEntities = new List<Entity*>();
CollectEntitiesWithComponent<MenuPanelComponent*>(generatedRootEntity, panelEntities);
for (int32_t panelIndex = 0; panelIndex < panelEntities->get_Count(); panelIndex++) {
Entity *panelEntity = (*panelEntities).get_Item(panelIndex);
::MenuPanelComponent *panelComponent = FindRequiredComponent<MenuPanelComponent*>(panelEntity);
Array<::MenuItemRuntime*> *itemRuntimes = this->BindItems(panelEntity, panelComponent->get_PanelId());
ScrollComponent *itemsScrollComponent = this->ResolveItemsScrollComponent(panelEntity, panelComponent->get_PanelId());
itemsScrollComponent->set_ItemCount(itemRuntimes->get_Length());
itemsScrollComponent->set_ClipOriginEntity(this->ResolveItemsViewportEntity(itemsScrollComponent, panelComponent->get_PanelId()));
::MenuPanelRuntime *panelRuntime = new ::MenuPanelRuntime(panelComponent, panelEntity, itemsScrollComponent->get_Parent(), itemsScrollComponent, itemRuntimes);
panelRuntime->ItemsScrollComponent->ScrollOffsetChanged += &MenuComponent::HandleItemsScrollOffsetChanged;
this->ApplyItemsScrollOffset(panelRuntime->ItemsRootEntity, panelRuntime->ItemsScrollComponent->get_ScrollOffset());
    if (this->PanelsById->ContainsKey(panelComponent->get_PanelId()))
    {
throw new InvalidOperationException(std::string("Duplicate baked menu panel id '") + panelComponent->get_PanelId() + std::string("' was found."));
    }
this->PanelsById->Add(panelComponent->get_PanelId(), panelRuntime);
this->PanelRuntimes->Add(panelRuntime);
}
    if (this->PanelsById->get_Count() == 0)
    {
throw new InvalidOperationException("The baked menu scene does not contain any panel metadata.");
    }
}

void MenuComponent::ClearSelectionVisuals(::MenuPanelRuntime* panelRuntime)
{
for (int32_t itemIndex = 0; itemIndex < panelRuntime->Items->get_Length(); itemIndex++) {
this->ApplyItemVisualState((*panelRuntime->Items)[itemIndex], false);
}
}

template <typename TComponent>
void MenuComponent::CollectEntitiesWithComponent(Entity* entity, List<Entity*>* entities)
{
    if (entity == nullptr)
    {
throw new ArgumentNullException("entity");
    }
    if (entities == nullptr)
    {
throw new ArgumentNullException("entities");
    }
TComponent component;
    if (TryFindComponent__out1<TComponent>(entity, component))
    {
entities->Add(entity);
    }
    if (entity->get_Children() == nullptr)
    {
return;    }
for (int32_t childIndex = 0; childIndex < entity->get_Children()->get_Count(); childIndex++) {
CollectEntitiesWithComponent<TComponent>((*entity->get_Children()).get_Item(childIndex), entities);
}
}

void MenuComponent::ConfirmSelection(Keys key)
{
    if (this->ActivePanel == nullptr)
    {
return;    }
    if (this->ActivePanel->SelectedItemIndex < 0 || this->ActivePanel->SelectedItemIndex >= this->ActivePanel->Items->get_Length())
    {
return;    }
this->ExecuteAction((*this->ActivePanel->Items)[this->ActivePanel->SelectedItemIndex]->Definition);
}

bool MenuComponent::ContainsPointer(::MenuItemRuntime* runtimeItem, int32_t pointerX, int32_t pointerY)
{
    if (runtimeItem == nullptr)
    {
throw new ArgumentNullException("runtimeItem");
    }
float3 position = runtimeItem->OwnerEntity->get_Position();
const int32_t width = runtimeItem->Background->get_Size().X;
const int32_t height = runtimeItem->Background->get_Size().Y;
return pointerX >= position.X && pointerX < position.X + width && pointerY >= position.Y && pointerY < position.Y + height;}

std::string MenuComponent::DescribeEntity(Entity* entity)
{
return he_cpp_type_of<Entity>("Entity")->Name;}

void MenuComponent::EnsureSelectedItemVisible(::MenuPanelRuntime* panelRuntime, int32_t selectedItemIndex)
{
    if (panelRuntime == nullptr)
    {
throw new ArgumentNullException("panelRuntime");
    }
    if (selectedItemIndex < 0 || selectedItemIndex >= panelRuntime->Items->get_Length())
    {
throw ([&]() {
auto __ctor_arg_00000098 = "selectedItemIndex";
auto __ctor_arg_00000099 = "Selected baked menu item index must be valid.";
return new ArgumentOutOfRangeException(__ctor_arg_00000098, __ctor_arg_00000099);
})();
    }
const int32_t visibleItemCount = panelRuntime->ItemsScrollComponent->get_VisibleItemCount();
const int32_t scrollOffset = panelRuntime->ItemsScrollComponent->get_ScrollOffset();
const int32_t visibleEndExclusive = scrollOffset + visibleItemCount;
    if (selectedItemIndex < scrollOffset)
    {
panelRuntime->ItemsScrollComponent->ScrollTo(selectedItemIndex);
return;    }
    if (selectedItemIndex >= visibleEndExclusive)
    {
panelRuntime->ItemsScrollComponent->ScrollTo(selectedItemIndex - visibleItemCount + 1);
    }
}

void MenuComponent::ExecuteAction(::MenuItemComponent* itemComponent)
{
    if (itemComponent == nullptr)
    {
throw new ArgumentNullException("itemComponent");
    }
    if (itemComponent->ActionKind == MenuActionKind::None)
    {
return;    }
else {
    if (itemComponent->ActionKind == MenuActionKind::OpenPanel)
    {
this->ActivatePanel(itemComponent->get_TargetId(), true);
    }
else {
    if (itemComponent->ActionKind == MenuActionKind::LoadScene)
    {
this->LoadScene(itemComponent->get_TargetId());
    }
else {
    if (itemComponent->ActionKind == MenuActionKind::Back)
    {
this->NavigateBack();
    }
else {
throw new InvalidOperationException(std::string("Unsupported baked menu action kind '") + std::to_string(static_cast<int32_t>(itemComponent->ActionKind)) + std::string("'."));
}
}
}
}
}

template <typename TComponent>
TComponent MenuComponent::FindFirstComponent(Entity* entity)
{
TComponent component;
    if (TryFindComponent__out1<TComponent>(entity, component))
    {
return component;    }
return nullptr;}

Entity* MenuComponent::FindGeneratedRootEntity(Entity* rootEntity)
{
    if (rootEntity->get_Children() == nullptr)
    {
return nullptr;    }
    if (rootEntity->get_Children()->get_Count() == 1)
    {
return (*rootEntity->get_Children()).get_Item(0);    }
return nullptr;}

::MenuItemRuntime* MenuComponent::FindHoveredItem(::MenuPanelRuntime* panelRuntime, int32_t pointerX, int32_t pointerY)
{
    if (panelRuntime == nullptr)
    {
throw new ArgumentNullException("panelRuntime");
    }
for (int32_t itemIndex = 0; itemIndex < panelRuntime->Items->get_Length(); itemIndex++) {
::MenuItemRuntime *runtimeItem = (*panelRuntime->Items)[itemIndex];
    if (!this->ContainsPointer(runtimeItem, pointerX, pointerY))
    {
continue;
    }
return runtimeItem;}
return nullptr;}

template <typename TComponent>
TComponent MenuComponent::FindRequiredComponent(Entity* entity)
{
TComponent component;
    if (TryFindComponent__out1<TComponent>(entity, component))
    {
return component;    }
throw new InvalidOperationException(std::string("Entity '") + this->DescribeEntity(entity) + std::string("' is missing required component '") + he_cpp_type_of<TComponent>("TComponent")->get_Name() + std::string("'."));
}

void MenuComponent::HandleGamepadInput(InputSystem* inputSystem)
{
InputGamepadState currentGamepadState = inputSystem->GetGamepadState(0);
    if (!currentGamepadState.get_Connected())
    {
return;    }
    if (inputSystem->WasGamepadButtonPressed(0, InputGamepadButton::DPadUp))
    {
this->MoveSelection(-1);
    }
else {
    if (inputSystem->WasGamepadButtonPressed(0, InputGamepadButton::DPadDown))
    {
this->MoveSelection(1);
    }
else {
    if (Core::get_Instance()->get_StandardPlatformInput()->WasActionPressed(StandardPlatformAction::Accept))
    {
this->ConfirmSelection(Keys::Enter);
    }
else {
    if (Core::get_Instance()->get_StandardPlatformInput()->WasActionPressed(StandardPlatformAction::Return) || inputSystem->WasGamepadButtonPressed(0, InputGamepadButton::Select))
    {
this->NavigateBack();
    }
}
}
}
}

void MenuComponent::HandleItemsScrollOffsetChanged(ScrollComponent* scrollComponent, int32_t scrollOffset)
{
    if (scrollComponent == nullptr)
    {
throw new ArgumentNullException("scrollComponent");
    }
this->ApplyItemsScrollOffset(scrollComponent->get_Parent(), scrollOffset);
}

void MenuComponent::HandleKeyboardInput(InputSystem* inputSystem)
{
    if (inputSystem->WasKeyPressed(Keys::Up) || inputSystem->WasKeyPressed(Keys::W))
    {
this->MoveSelection(-1);
    }
else {
    if (inputSystem->WasKeyPressed(Keys::Down) || inputSystem->WasKeyPressed(Keys::S))
    {
this->MoveSelection(1);
    }
else {
    if (inputSystem->WasKeyPressed(Keys::Enter))
    {
this->ConfirmSelection(Keys::Enter);
    }
else {
    if (inputSystem->WasKeyPressed(Keys::Space))
    {
this->ConfirmSelection(Keys::Space);
    }
else {
    if (inputSystem->WasKeyPressed(Keys::Escape) || inputSystem->WasKeyPressed(Keys::Back))
    {
this->NavigateBack();
    }
}
}
}
}
}

void MenuComponent::HandleMouseInput(InputSystem* inputSystem)
{
    if (this->ActivePanel == nullptr)
    {
this->PressedPointerItem = nullptr;
return;    }
const int32_t pointerX = this->ResolvePointerXInMenuSpace(inputSystem);
const int32_t pointerY = this->ResolvePointerYInMenuSpace(inputSystem);
::MenuItemRuntime *hoveredItem = this->FindHoveredItem(this->ActivePanel, pointerX, pointerY);
    if (hoveredItem != nullptr && hoveredItem->Index != this->ActivePanel->SelectedItemIndex && this->IsMouseHoverSelectionUpdateRequired(inputSystem))
    {
this->SetSelection(this->ActivePanel, hoveredItem->Index);
    }
    if (inputSystem->WasMouseLeftButtonPressed())
    {
this->PressedPointerItem = hoveredItem;
return;    }
    if (inputSystem->WasMouseLeftButtonReleased())
    {
    if (this->IsSameRuntimeItem(this->PressedPointerItem, hoveredItem))
    {
this->ExecuteAction(hoveredItem->Definition);
    }
this->PressedPointerItem = nullptr;
    }
}

bool MenuComponent::IsMouseHoverSelectionUpdateRequired(InputSystem* inputSystem)
{
    if (inputSystem == nullptr)
    {
throw new ArgumentNullException("inputSystem");
    }
    if (inputSystem->GetMouseDeltaX() != 0 || inputSystem->GetMouseDeltaY() != 0)
    {
return true;    }
return inputSystem->WasMouseLeftButtonPressed();}

bool MenuComponent::IsSameRuntimeItem(::MenuItemRuntime* left, ::MenuItemRuntime* right)
{
    if (left == nullptr || right == nullptr)
    {
return false;    }
return left->Index == right->Index && String::Equals(left->Definition->get_PanelId(), right->Definition->get_PanelId(), StringComparison::Ordinal) && String::Equals(left->Definition->get_ItemId(), right->Definition->get_ItemId(), StringComparison::Ordinal);}

void MenuComponent::LoadScene(std::string sceneId)
{
    if (String::IsNullOrWhiteSpace(sceneId))
    {
throw new InvalidOperationException("Scene-loading baked menu items must provide a scene id.");
    }
    if (Core::get_Instance() == nullptr)
    {
throw new InvalidOperationException("A core instance must exist before loading a scene from the baked menu.");
    }
    if (Core::get_Instance()->get_SceneManager() == nullptr)
    {
throw new InvalidOperationException("Core scene manager must be initialized before runtime menu scene loading can occur.");
    }
const std::string resolvedSceneId = SceneMapComponent::ResolveSceneId(sceneId);
Core::get_Instance()->get_SceneManager()->LoadScene(resolvedSceneId, SceneLoadMode::Single);
    if (ComponentExecutionContext::get_CurrentMode() == ComponentExecutionMode::Editor && this->get_Parent() != nullptr)
    {
this->Parent->set_Enabled(false);
    }
}

void MenuComponent::MoveSelection(int32_t delta)
{
    if (this->ActivePanel == nullptr || this->ActivePanel->Items->get_Length() == 0)
    {
return;    }
    if (delta == 0)
    {
return;    }
int32_t nextIndex = this->ActivePanel->SelectedItemIndex;
    if (nextIndex < 0)
    {
nextIndex = 0;
    }
nextIndex += delta;
    if (nextIndex < 0)
    {
nextIndex = this->ActivePanel->Items->get_Length() - 1;
    }
else {
    if (nextIndex >= this->ActivePanel->Items->get_Length())
    {
nextIndex = 0;
    }
}
this->SetSelection(this->ActivePanel, nextIndex);
}

void MenuComponent::NavigateBack()
{
    if (this->PanelHistory->get_Count() == 0)
    {
return;    }
const std::string previousPanelId = (*this->PanelHistory).get_Item(this->PanelHistory->get_Count() - 1);
this->PanelHistory->RemoveAt(this->PanelHistory->get_Count() - 1);
this->ActivatePanel(previousPanelId, false);
}

void MenuComponent::ReleaseItemRuntimes(Array<::MenuItemRuntime*>* itemRuntimes)
{
    if (itemRuntimes == nullptr)
    {
return;    }
}

void MenuComponent::ReleasePanelRuntime(::MenuPanelRuntime* panelRuntime)
{
    if (panelRuntime == nullptr)
    {
return;    }
panelRuntime->ItemsScrollComponent->ScrollOffsetChanged -= &MenuComponent::HandleItemsScrollOffsetChanged;
this->ReleaseItemRuntimes(panelRuntime->Items);
}

void MenuComponent::ReleasePanelRuntimes()
{
for (int32_t panelIndex = 0; panelIndex < this->PanelRuntimes->get_Count(); panelIndex++) {
this->ReleasePanelRuntime((*this->PanelRuntimes).get_Item(panelIndex));
}
}

ScrollComponent* MenuComponent::ResolveItemsScrollComponent(Entity* panelEntity, std::string panelId)
{
List<Entity*> *scrollEntities = new List<Entity*>();
CollectEntitiesWithComponent<ScrollComponent*>(panelEntity, scrollEntities);
    if (scrollEntities->get_Count() != 1)
    {
throw new InvalidOperationException(std::string("Baked menu panel '") + panelId + std::string("' must contain exactly one scroll component."));
    }
ScrollComponent *scrollComponent = FindRequiredComponent<ScrollComponent*>((*scrollEntities).get_Item(0));
    if (scrollComponent->get_VisibleItemCount() < 1)
    {
throw new InvalidOperationException(std::string("Baked menu panel '") + panelId + std::string("' must expose at least one visible item row."));
    }
return scrollComponent;}

float MenuComponent::ResolveItemsScrollStep(Entity* itemsRootEntity)
{
    if (itemsRootEntity->get_Children() == nullptr || itemsRootEntity->get_Children()->get_Count() == 0)
    {
return DemoMenuLayout::ButtonHeight + DemoMenuLayout::ButtonSpacing;    }
    if (itemsRootEntity->get_Children()->get_Count() >= 2)
    {
const float step = (*itemsRootEntity->get_Children()).get_Item(1)->get_LocalPosition().Y - (*itemsRootEntity->get_Children()).get_Item(0)->get_LocalPosition().Y;
    if (step > 0.0f)
    {
return step;    }
    }
RoundedRectComponent *background = FindFirstComponent<RoundedRectComponent*>((*itemsRootEntity->get_Children()).get_Item(0));
    if (background != nullptr && background->get_Size().Y > 0)
    {
return background->get_Size().Y;    }
return DemoMenuLayout::ButtonHeight + DemoMenuLayout::ButtonSpacing;}

Entity* MenuComponent::ResolveItemsViewportEntity(ScrollComponent* scrollComponent, std::string panelId)
{
    if (scrollComponent == nullptr)
    {
throw new ArgumentNullException("scrollComponent");
    }
Entity *itemsRootEntity = scrollComponent->get_Parent();
    if (itemsRootEntity == nullptr || itemsRootEntity->get_Parent() == nullptr)
    {
throw new InvalidOperationException(std::string("Baked menu panel '") + panelId + std::string("' must parent its scroll root under a viewport entity."));
    }
Entity *viewportEntity = itemsRootEntity->get_Parent();
ClipRectComponent *clipComponent = FindFirstComponent<ClipRectComponent*>(viewportEntity);
    if (clipComponent == nullptr)
    {
throw new InvalidOperationException(std::string("Baked menu panel '") + panelId + std::string("' must contain a clip viewport above its scroll root."));
    }
return viewportEntity;}

float4 MenuComponent::ResolveMenuViewportBounds()
{
    if (this->get_Parent() == nullptr)
    {
return float4(0.0f, 0.0f, 0.0f, 0.0f);    }
ViewportComponent *viewportComponent = FindFirstComponent<ViewportComponent*>(this->get_Parent());
    if (viewportComponent == nullptr)
    {
return float4(0.0f, 0.0f, 0.0f, 0.0f);    }
    if (viewportComponent->get_BindingMode() != ViewportComponent::AncestorCameraBindingMode && viewportComponent->get_BindingMode() != ViewportComponent::ExplicitCameraBindingMode)
    {
return float4(0.0f, 0.0f, 0.0f, 0.0f);    }
return viewportComponent->get_ResolvedViewportBounds();}

int32_t MenuComponent::ResolvePointerXInMenuSpace(InputSystem* inputSystem)
{
    if (inputSystem == nullptr)
    {
throw new ArgumentNullException("inputSystem");
    }
float4 viewportBounds = this->ResolveMenuViewportBounds();
return inputSystem->GetMouseX() - static_cast<int32_t>(Math::Round(viewportBounds.X));}

int32_t MenuComponent::ResolvePointerYInMenuSpace(InputSystem* inputSystem)
{
    if (inputSystem == nullptr)
    {
throw new ArgumentNullException("inputSystem");
    }
float4 viewportBounds = this->ResolveMenuViewportBounds();
return inputSystem->GetMouseY() - static_cast<int32_t>(Math::Round(viewportBounds.Y));}

int32_t MenuComponent::ResolveSelectedIndex(::MenuPanelRuntime* panelRuntime)
{
    if (panelRuntime->SelectedItemIndex >= 0 && panelRuntime->SelectedItemIndex < panelRuntime->Items->get_Length())
    {
return panelRuntime->SelectedItemIndex;    }
return 0;}

void MenuComponent::SetSelection(::MenuPanelRuntime* panelRuntime, int32_t itemIndex)
{
    if (panelRuntime == nullptr)
    {
throw new ArgumentNullException("panelRuntime");
    }
    if (itemIndex < 0 || itemIndex >= panelRuntime->Items->get_Length())
    {
throw ([&]() {
auto __ctor_arg_0000009A = "itemIndex";
auto __ctor_arg_0000009B = "Selected baked menu item index must be valid.";
return new ArgumentOutOfRangeException(__ctor_arg_0000009A, __ctor_arg_0000009B);
})();
    }
panelRuntime->set_SelectedItemIndex(itemIndex);
for (int32_t index = 0; index < panelRuntime->Items->get_Length(); index++) {
::MenuItemRuntime *runtimeItem = (*panelRuntime->Items)[index];
const bool isSelected = index == itemIndex;
this->ApplyItemVisualState(runtimeItem, isSelected);
}
::MenuItemRuntime *selectedItem = (*panelRuntime->Items)[itemIndex];
this->SelectedItemIdValue = selectedItem->Definition->get_ItemId();
this->EnsureSelectedItemVisible(panelRuntime, itemIndex);
this->ApplyItemsScrollOffset(panelRuntime->ItemsRootEntity, panelRuntime->ItemsScrollComponent->get_ScrollOffset());
}

template <typename TComponent>
bool MenuComponent::TryFindComponent__out1(Entity* entity, TComponent& component)
{
    if (entity != nullptr && entity->get_Components() != nullptr)
    {
for (int32_t componentIndex = 0; componentIndex < entity->get_Components()->get_Count(); componentIndex++) {
    TComponent typedComponent = he_cpp_try_cast<TComponent>((*entity->get_Components()).get_Item(componentIndex));
    if (typedComponent != nullptr)
    {
component = typedComponent;
return true;    }
}
    }
component = nullptr;
return false;}

void MenuComponent::TryInitialize()
{
    if (this->get_Parent() == nullptr)
    {
return;    }
Entity *generatedRootEntity = this->FindGeneratedRootEntity(this->get_Parent());
    if (generatedRootEntity == nullptr)
    {
return;    }
this->BindPanels(this->get_Parent());
this->ActivatePanel(this->InitialPanelIdValue, false);
this->set_IsInitialized(true);
}

