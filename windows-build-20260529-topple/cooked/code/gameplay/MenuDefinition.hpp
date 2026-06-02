#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

class MenuOverlayImageDefinition;
class MenuPanelDefinition;
class MenuPlatformInfoDefinition;

#include "byte4.hpp"
#include "runtime/native_string.hpp"
#include "runtime/array.hpp"

class MenuDefinition
{
public:
    virtual ~MenuDefinition() = default;

    byte4 AccentColor;

    byte4 get_AccentColor();

    byte4 AccentSecondaryColor;

    byte4 get_AccentSecondaryColor();

    byte4 BackgroundColor;

    byte4 get_BackgroundColor();

    std::string BodyFontPath;

    const std::string& get_BodyFontPath();

    std::string InitialPanelId;

    const std::string& get_InitialPanelId();

    byte4 MutedTextColor;

    byte4 get_MutedTextColor();

    ::MenuOverlayImageDefinition* OverlayImage;

    ::MenuOverlayImageDefinition* get_OverlayImage();

    Array<::MenuPanelDefinition*>* Panels;

    Array<::MenuPanelDefinition*>* get_Panels();

    ::MenuPlatformInfoDefinition* PlatformInfoOverlay;

    ::MenuPlatformInfoDefinition* get_PlatformInfoOverlay();

    std::string Subtitle;

    const std::string& get_Subtitle();

    byte4 SurfaceBorderColor;

    byte4 get_SurfaceBorderColor();

    byte4 SurfaceColor;

    byte4 get_SurfaceColor();

    byte4 TextColor;

    byte4 get_TextColor();

    std::string Title;

    const std::string& get_Title();

    std::string TitleFontPath;

    const std::string& get_TitleFontPath();

    MenuDefinition(std::string title, std::string subtitle, std::string initialPanelId, std::string titleFontPath, std::string bodyFontPath, byte4 backgroundColor, byte4 surfaceColor, byte4 surfaceBorderColor, byte4 accentColor, byte4 accentSecondaryColor, byte4 textColor, byte4 mutedTextColor, Array<::MenuPanelDefinition*>* panels, ::MenuOverlayImageDefinition* overlayImage, ::MenuPlatformInfoDefinition* platformInfoOverlay);
};
