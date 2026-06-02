#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "runtime/native_disposable.hpp"

class RotateComponent : public ::UpdateComponent
{
public:
    virtual ~RotateComponent() = default;

    RotateComponent();

    float RadiansPerFrame;

    float get_RadiansPerFrame();
    void set_RadiansPerFrame(float value);

    void Update();
};
