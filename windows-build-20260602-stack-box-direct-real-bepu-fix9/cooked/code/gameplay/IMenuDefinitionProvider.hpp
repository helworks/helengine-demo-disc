#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

class MenuDefinition;

class IMenuDefinitionProvider
{
public:
    virtual ::MenuDefinition* CreateMenuDefinition() = 0;
};
