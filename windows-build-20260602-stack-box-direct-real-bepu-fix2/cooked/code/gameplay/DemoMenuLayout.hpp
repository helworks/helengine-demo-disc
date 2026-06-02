#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "runtime/native_string.hpp"

class DemoMenuLayout
{
public:
    virtual ~DemoMenuLayout() = default;

    inline static const int32_t ButtonHeight = 96;

    inline static const int32_t ButtonSpacing = 14;

    inline static const int32_t ButtonWidth = 512;

    inline static const int32_t CanvasHeight = 720;

    inline static const int32_t CanvasWidth = 1280;

    inline static const std::string GeneratedRootEntityName = "DemoDiscGeneratedMenu";

    inline static const int32_t PanelHeight = 420;

    inline static const int32_t PanelWidth = 560;
};
