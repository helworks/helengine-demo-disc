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

    byte4* get_AccentColor();

    byte4* get_AccentSecondaryColor();

    byte4* get_BackgroundColor();

    std::string get_BodyFontPath();

    byte4* get_MutedTextColor();

    byte4* get_SurfaceBorderColor();

    byte4* get_SurfaceColor();

    byte4* get_TextColor();

    std::string get_TitleFontPath();
};
