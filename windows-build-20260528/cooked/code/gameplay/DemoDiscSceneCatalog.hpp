#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

class MenuItemDefinition;
class DemoDiscPhysicsSceneEntry;

#include "runtime/array.hpp"
#include "runtime/native_list.hpp"
#include "runtime/array.hpp"
#include "runtime/native_list.hpp"
#include "runtime/array.hpp"

class DemoDiscSceneCatalog
{
public:
    virtual ~DemoDiscSceneCatalog() = default;

    Array<::MenuItemDefinition*>* CreateDemoSceneItems();

    List<::DemoDiscPhysicsSceneEntry*>* CreatePhysicsSceneEntries();

    Array<::MenuItemDefinition*>* CreatePhysicsSceneItems();
};
