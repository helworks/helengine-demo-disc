#ifdef DrawText
#undef DrawText
#endif
#include "MenuPlatformInfoDefinition.hpp"
#include "runtime/native_exceptions.hpp"
#include "MenuPlatformInfoDefinition.hpp"
#include "runtime/native_exceptions.hpp"

int32_t MenuPlatformInfoDefinition::get_LineSpacing()
{
return this->LineSpacing;
}

int32_t MenuPlatformInfoDefinition::get_RightMargin()
{
return this->RightMargin;
}

int32_t MenuPlatformInfoDefinition::get_TopMargin()
{
return this->TopMargin;
}

MenuPlatformInfoDefinition::MenuPlatformInfoDefinition(int32_t topMargin, int32_t rightMargin, int32_t lineSpacing) : LineSpacing(0), RightMargin(0), TopMargin(0)
{
    if (topMargin < 0)
    {
throw ([&]() {
auto __ctor_arg_0000004E = "topMargin";
auto __ctor_arg_0000004F = "Top margin must be zero or greater.";
return new ArgumentOutOfRangeException(__ctor_arg_0000004E, __ctor_arg_0000004F);
})();
    }
    if (rightMargin < 0)
    {
throw ([&]() {
auto __ctor_arg_00000050 = "rightMargin";
auto __ctor_arg_00000051 = "Right margin must be zero or greater.";
return new ArgumentOutOfRangeException(__ctor_arg_00000050, __ctor_arg_00000051);
})();
    }
    if (lineSpacing < 0)
    {
throw ([&]() {
auto __ctor_arg_00000052 = "lineSpacing";
auto __ctor_arg_00000053 = "Line spacing must be zero or greater.";
return new ArgumentOutOfRangeException(__ctor_arg_00000052, __ctor_arg_00000053);
})();
    }
this->TopMargin = topMargin;
this->RightMargin = rightMargin;
this->LineSpacing = lineSpacing;
}

