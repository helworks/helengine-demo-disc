#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

class IMenuDefinitionProvider;
class MenuDefinition;

#include "IMenuDefinitionProvider.hpp"

class DemoDiscMenuDefinitionProvider : public ::IMenuDefinitionProvider
{
public:
    virtual ~DemoDiscMenuDefinitionProvider() = default;

    ::MenuDefinition* CreateMenuDefinition();
};
