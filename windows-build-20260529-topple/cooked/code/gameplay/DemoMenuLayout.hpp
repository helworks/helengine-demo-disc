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

    static int32_t ButtonHeight;

    static int32_t ButtonSpacing;

    static int32_t ButtonWidth;

    static int32_t CanvasHeight;

    static int32_t CanvasWidth;

    static std::string GeneratedRootEntityName;

    static int32_t PanelHeight;

    static int32_t PanelWidth;
};
