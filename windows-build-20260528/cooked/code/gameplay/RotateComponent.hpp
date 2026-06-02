#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "UpdateComponent.hpp"
#include "runtime/native_disposable.hpp"
#include "IUpdateable.hpp"
#include "UpdateComponent.hpp"
#include "float4.hpp"
#include "float3.hpp"
#include "float4.hpp"
#include "Entity.hpp"

class RotateComponent : public UpdateComponent
{
public:
    virtual ~RotateComponent() = default;

    RotateComponent();

    float RadiansPerFrame;

    float get_RadiansPerFrame();
    void set_RadiansPerFrame(float value);

    void Update();
};
