#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

#include "runtime/native_string.hpp"

class DemoDiscPhysicsSceneEntry
{
public:
    virtual ~DemoDiscPhysicsSceneEntry() = default;

    std::string DisplayName;

    const std::string& get_DisplayName();

    std::string MenuItemId;

    const std::string& get_MenuItemId();

    std::string NintendoDsSceneId;

    const std::string& get_NintendoDsSceneId();

    std::string SceneId;

    const std::string& get_SceneId();

    DemoDiscPhysicsSceneEntry(std::string menuItemId, std::string displayName, std::string sceneId, std::string nintendoDsSceneId);
};
