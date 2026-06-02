#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "runtime/native_enum.hpp"
#include "IComparable.hpp"
#include "IConvertible.hpp"
#include "ISpanFormattable.hpp"
#include "IFormattable.hpp"

enum class MenuActionKind
{
    None = 0,
    OpenPanel = 1,
    LoadScene = 2,
    Back = 3
};
