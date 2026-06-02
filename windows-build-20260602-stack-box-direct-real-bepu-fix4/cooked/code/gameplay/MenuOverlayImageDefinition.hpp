#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "runtime/native_string.hpp"

class MenuOverlayImageDefinition
{
public:
    virtual ~MenuOverlayImageDefinition() = default;

    int32_t BottomMargin;

    int32_t get_BottomMargin();

    int32_t Height;

    int32_t get_Height();

    int32_t RightMargin;

    int32_t get_RightMargin();

    std::string TexturePath;

    const std::string& get_TexturePath();

    int32_t Width;

    int32_t get_Width();

    MenuOverlayImageDefinition(std::string texturePath, int32_t width, int32_t height, int32_t bottomMargin, int32_t rightMargin);
};
