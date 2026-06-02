#ifdef DrawText
#undef DrawText
#endif
#include "MenuItemRuntime.hpp"
#include "MenuItemComponent.hpp"
#include "runtime/native_exceptions.hpp"
#include "MenuItemRuntime.hpp"
#include "runtime/native_exceptions.hpp"

RoundedRectComponent* MenuItemRuntime::get_Background()
{
return this->Background;
}

::MenuItemComponent* MenuItemRuntime::get_Definition()
{
return this->Definition;
}

int32_t MenuItemRuntime::get_Index()
{
return this->Index;
}

Entity* MenuItemRuntime::get_OwnerEntity()
{
return this->OwnerEntity;
}

MenuItemRuntime::MenuItemRuntime(::MenuItemComponent* definition, int32_t index, Entity* entity, RoundedRectComponent* background) : Background(), Definition(), Index(0), OwnerEntity()
{
this->Definition = (definition != nullptr ? definition : throw new ArgumentNullException("definition"));
this->Index = index;
this->OwnerEntity = (entity != nullptr ? entity : throw new ArgumentNullException("entity"));
this->Background = (background != nullptr ? background : throw new ArgumentNullException("background"));
}

