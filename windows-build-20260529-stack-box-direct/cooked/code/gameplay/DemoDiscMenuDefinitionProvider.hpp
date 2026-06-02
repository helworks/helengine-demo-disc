#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

class IMenuDefinitionProvider;
class DemoDiscMenuTheme;
class DemoDiscSceneCatalog;
class MenuDefinition;

#include "IMenuDefinitionProvider.hpp"
#include "DemoDiscMenuTheme.hpp"
#include "DemoDiscSceneCatalog.hpp"
#include "MenuDefinition.hpp"

class DemoDiscMenuDefinitionProvider : public IMenuDefinitionProvider
{
public:
    virtual ~DemoDiscMenuDefinitionProvider() = default;

    ::MenuDefinition* CreateMenuDefinition();
};
