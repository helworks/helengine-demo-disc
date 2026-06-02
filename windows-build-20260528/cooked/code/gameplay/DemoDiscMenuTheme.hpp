#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "byte4.hpp"
#include "runtime/native_string.hpp"

class DemoDiscMenuTheme
{
public:
    virtual ~DemoDiscMenuTheme() = default;

    byte4 get_AccentColor();

    byte4 get_AccentSecondaryColor();

    byte4 get_BackgroundColor();

    const std::string& get_BodyFontPath();

    int32_t get_LogoBottomMargin();

    int32_t get_LogoHeight();

    int32_t get_LogoRightMargin();

    const std::string& get_LogoTexturePath();

    int32_t get_LogoWidth();

    byte4 get_MutedTextColor();

    int32_t get_PlatformInfoLineSpacing();

    int32_t get_PlatformInfoRightMargin();

    int32_t get_PlatformInfoTopMargin();

    byte4 get_SurfaceBorderColor();

    byte4 get_SurfaceColor();

    byte4 get_TextColor();

    const std::string& get_TitleFontPath();
};
