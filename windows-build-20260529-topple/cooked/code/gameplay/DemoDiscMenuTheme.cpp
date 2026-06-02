#ifdef DrawText
#undef DrawText
#endif
#include "DemoDiscMenuTheme.hpp"
#include "byte4.hpp"
#include "runtime/native_string.hpp"

byte4 DemoDiscMenuTheme::get_AccentColor()
{
return byte4(201, 147, 255, 255);
}

byte4 DemoDiscMenuTheme::get_AccentSecondaryColor()
{
return byte4(118, 219, 209, 255);
}

byte4 DemoDiscMenuTheme::get_BackgroundColor()
{
return byte4(30, 17, 41, 255);
}

const std::string& DemoDiscMenuTheme::get_BodyFontPath()
{
return "Fonts/DemoDiscBody.ttf";
}

int32_t DemoDiscMenuTheme::get_LogoBottomMargin()
{
return 36;
}

int32_t DemoDiscMenuTheme::get_LogoHeight()
{
return 440;
}

int32_t DemoDiscMenuTheme::get_LogoRightMargin()
{
return 44;
}

const std::string& DemoDiscMenuTheme::get_LogoTexturePath()
{
return "Images/Menu/helengine-logo.png";
}

int32_t DemoDiscMenuTheme::get_LogoWidth()
{
return 440;
}

byte4 DemoDiscMenuTheme::get_MutedTextColor()
{
return byte4(211, 198, 228, 255);
}

int32_t DemoDiscMenuTheme::get_PlatformInfoLineSpacing()
{
return 6;
}

int32_t DemoDiscMenuTheme::get_PlatformInfoRightMargin()
{
return 44;
}

int32_t DemoDiscMenuTheme::get_PlatformInfoTopMargin()
{
return 28;
}

byte4 DemoDiscMenuTheme::get_SurfaceBorderColor()
{
return byte4(135, 94, 163, 255);
}

byte4 DemoDiscMenuTheme::get_SurfaceColor()
{
return byte4(60, 41, 76, 232);
}

byte4 DemoDiscMenuTheme::get_TextColor()
{
return byte4(249, 243, 255, 255);
}

const std::string& DemoDiscMenuTheme::get_TitleFontPath()
{
return "Fonts/DemoDiscTitle.ttf";
}

