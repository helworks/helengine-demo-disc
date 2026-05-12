#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "runtime/array.hpp"
#include "runtime/array.hpp"
#include "MenuItemDefinition.hpp"

class DemoDiscSceneCatalog
{
public:
    virtual ~DemoDiscSceneCatalog() = default;

    Array<MenuItemDefinition*>* CreateSceneItems();
};
