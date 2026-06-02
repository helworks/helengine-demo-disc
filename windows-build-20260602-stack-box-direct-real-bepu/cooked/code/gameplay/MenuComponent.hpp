#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

class MenuPanelRuntime;
class MenuItemRuntime;
class MenuItemComponent;

#include "runtime/native_disposable.hpp"
#include "runtime/native_string.hpp"
#include "runtime/native_list.hpp"
#include "runtime/native_list.hpp"
#include "runtime/native_dictionary.hpp"
#include "runtime/array.hpp"
#include "runtime/native_list.hpp"

class MenuComponent : public ::UpdateComponent
{
public:
    virtual ~MenuComponent() = default;

    inline static const uint8_t CurrentVersion = 1;

    inline static const std::string SerializedComponentTypeId = "city.menu.MenuComponent, gameplay";

    const std::string& get_ActivePanelId();

    const std::string& get_InitialPanelId();

    void set_InitialPanelId(std::string value);

    bool IsInitialized;

    bool get_IsInitialized();
    void set_IsInitialized(bool value);

    const std::string& get_ProviderTypeName();

    void set_ProviderTypeName(std::string value);

    const std::string& get_SelectedItemId();

    void ComponentAdded(Entity* entity);

    void ComponentRemoved(Entity* entity);

    void Dispose();

    MenuComponent();

    void Update();
private:
    ::MenuPanelRuntime* ActivePanel;

    std::string ActivePanelIdValue;

    std::string InitialPanelIdValue;

    List<std::string>* PanelHistory;

    List<::MenuPanelRuntime*>* PanelRuntimes;

    Dictionary<std::string, ::MenuPanelRuntime*>* PanelsById;

    ::MenuItemRuntime* PressedPointerItem;

    std::string ProviderTypeNameValue;

    std::string SelectedItemIdValue;

    void ActivatePanel(std::string panelId, bool pushHistory);

    void ApplyItemVisualState(::MenuItemRuntime* runtimeItem, bool isSelected);

    void ApplyItemsScrollOffset(Entity* itemsRootEntity, int32_t scrollOffset);

    Array<::MenuItemRuntime*>* BindItems(Entity* panelEntity, std::string panelId);

    void BindPanels(Entity* rootEntity);

    void ClearSelectionVisuals(::MenuPanelRuntime* panelRuntime);

    template <typename TComponent>
    void CollectEntitiesWithComponent(Entity* entity, List<Entity*>* entities);

    void ConfirmSelection(Keys key);

    bool ContainsPointer(::MenuItemRuntime* runtimeItem, int32_t pointerX, int32_t pointerY);

    std::string DescribeEntity(Entity* entity);

    void EnsureSelectedItemVisible(::MenuPanelRuntime* panelRuntime, int32_t selectedItemIndex);

    void ExecuteAction(::MenuItemComponent* itemComponent);

    template <typename TComponent>
    TComponent FindFirstComponent(Entity* entity);

    Entity* FindGeneratedRootEntity(Entity* rootEntity);

    ::MenuItemRuntime* FindHoveredItem(::MenuPanelRuntime* panelRuntime, int32_t pointerX, int32_t pointerY);

    template <typename TComponent>
    TComponent FindRequiredComponent(Entity* entity);

    void HandleGamepadInput(InputSystem* inputSystem);

    void HandleItemsScrollOffsetChanged(ScrollComponent* scrollComponent, int32_t scrollOffset);

    void HandleKeyboardInput(InputSystem* inputSystem);

    void HandleMouseInput(InputSystem* inputSystem);

    bool IsMouseHoverSelectionUpdateRequired(InputSystem* inputSystem);

    bool IsSameRuntimeItem(::MenuItemRuntime* left, ::MenuItemRuntime* right);

    void LoadScene(std::string sceneId);

    void MoveSelection(int32_t delta);

    void NavigateBack();

    void ReleaseItemRuntimes(Array<::MenuItemRuntime*>* itemRuntimes);

    void ReleasePanelRuntime(::MenuPanelRuntime* panelRuntime);

    void ReleasePanelRuntimes();

    ScrollComponent* ResolveItemsScrollComponent(Entity* panelEntity, std::string panelId);

    float ResolveItemsScrollStep(Entity* itemsRootEntity);

    Entity* ResolveItemsViewportEntity(ScrollComponent* scrollComponent, std::string panelId);

    float4 ResolveMenuViewportBounds();

    int32_t ResolvePointerXInMenuSpace(InputSystem* inputSystem);

    int32_t ResolvePointerYInMenuSpace(InputSystem* inputSystem);

    int32_t ResolveSelectedIndex(::MenuPanelRuntime* panelRuntime);

    void SetSelection(::MenuPanelRuntime* panelRuntime, int32_t itemIndex);

    template <typename TComponent>
    bool TryFindComponent__out1(Entity* entity, TComponent& component);

    void TryInitialize();
};
