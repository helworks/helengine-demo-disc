#ifdef DrawText
#undef DrawText
#endif
#include "MenuOverlayImageDefinition.hpp"
#include "runtime/native_string.hpp"
#include "runtime/native_exceptions.hpp"
#include "MenuOverlayImageDefinition.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_string.hpp"

int32_t MenuOverlayImageDefinition::get_BottomMargin()
{
return this->BottomMargin;
}

int32_t MenuOverlayImageDefinition::get_Height()
{
return this->Height;
}

int32_t MenuOverlayImageDefinition::get_RightMargin()
{
return this->RightMargin;
}

const std::string& MenuOverlayImageDefinition::get_TexturePath()
{
return this->TexturePath;
}

int32_t MenuOverlayImageDefinition::get_Width()
{
return this->Width;
}

MenuOverlayImageDefinition::MenuOverlayImageDefinition(std::string texturePath, int32_t width, int32_t height, int32_t bottomMargin, int32_t rightMargin) : BottomMargin(0), Height(0), RightMargin(0), TexturePath(), Width(0)
{
    if (String::IsNullOrWhiteSpace(texturePath))
    {
throw ([&]() {
auto __ctor_arg_0000003E = "Texture path must be provided.";
auto __ctor_arg_0000003F = "texturePath";
return new ArgumentException(__ctor_arg_0000003E, __ctor_arg_0000003F);
})();
    }
    if (width < 1)
    {
throw ([&]() {
auto __ctor_arg_00000040 = "width";
auto __ctor_arg_00000041 = "Overlay width must be positive.";
return new ArgumentOutOfRangeException(__ctor_arg_00000040, __ctor_arg_00000041);
})();
    }
    if (height < 1)
    {
throw ([&]() {
auto __ctor_arg_00000042 = "height";
auto __ctor_arg_00000043 = "Overlay height must be positive.";
return new ArgumentOutOfRangeException(__ctor_arg_00000042, __ctor_arg_00000043);
})();
    }
    if (bottomMargin < 0)
    {
throw ([&]() {
auto __ctor_arg_00000044 = "bottomMargin";
auto __ctor_arg_00000045 = "Bottom margin must not be negative.";
return new ArgumentOutOfRangeException(__ctor_arg_00000044, __ctor_arg_00000045);
})();
    }
    if (rightMargin < 0)
    {
throw ([&]() {
auto __ctor_arg_00000046 = "rightMargin";
auto __ctor_arg_00000047 = "Right margin must not be negative.";
return new ArgumentOutOfRangeException(__ctor_arg_00000046, __ctor_arg_00000047);
})();
    }
this->TexturePath = texturePath;
this->Width = width;
this->Height = height;
this->BottomMargin = bottomMargin;
this->RightMargin = rightMargin;
}

