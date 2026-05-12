#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

class DemoDiscMenuTheme;
class DemoDiscSceneCatalog;

#include "IMenuDefinitionProvider.hpp"
#include "DemoDiscMenuTheme.hpp"
#include "DemoDiscSceneCatalog.hpp"
#include "MenuDefinition.hpp"

class DemoDiscMenuDefinitionProvider : public IMenuDefinitionProvider
{
public:
    virtual ~DemoDiscMenuDefinitionProvider() = default;

    MenuDefinition* CreateMenuDefinition();
};
