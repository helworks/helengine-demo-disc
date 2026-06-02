#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

enum class MenuActionKind
{
    None = 0,
    OpenPanel = 1,
    LoadScene = 2,
    Back = 3
};
