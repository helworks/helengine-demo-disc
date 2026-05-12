#ifdef DrawText
#undef DrawText
#endif
#include "DemoDiscMenuTheme.hpp"

byte4* DemoDiscMenuTheme::get_AccentColor()
{
return new byte4(201, 147, 255, 255);
}

byte4* DemoDiscMenuTheme::get_AccentSecondaryColor()
{
return new byte4(118, 219, 209, 255);
}

byte4* DemoDiscMenuTheme::get_BackgroundColor()
{
return new byte4(30, 17, 41, 255);
}

std::string DemoDiscMenuTheme::get_BodyFontPath()
{
return "Fonts/DemoDiscBody.ttf";
}

byte4* DemoDiscMenuTheme::get_MutedTextColor()
{
return new byte4(211, 198, 228, 255);
}

byte4* DemoDiscMenuTheme::get_SurfaceBorderColor()
{
return new byte4(135, 94, 163, 255);
}

byte4* DemoDiscMenuTheme::get_SurfaceColor()
{
return new byte4(60, 41, 76, 232);
}

byte4* DemoDiscMenuTheme::get_TextColor()
{
return new byte4(249, 243, 255, 255);
}

std::string DemoDiscMenuTheme::get_TitleFontPath()
{
return "Fonts/DemoDiscTitle.ttf";
}

