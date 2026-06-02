#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

class MenuPlatformInfoDefinition
{
public:
    virtual ~MenuPlatformInfoDefinition() = default;

    int32_t LineSpacing;

    int32_t get_LineSpacing();

    int32_t RightMargin;

    int32_t get_RightMargin();

    int32_t TopMargin;

    int32_t get_TopMargin();

    MenuPlatformInfoDefinition(int32_t topMargin, int32_t rightMargin, int32_t lineSpacing);
};
