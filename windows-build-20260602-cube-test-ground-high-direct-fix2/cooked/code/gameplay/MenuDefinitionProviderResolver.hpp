#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

class IMenuDefinitionProvider;

#include "runtime/native_string.hpp"

class MenuDefinitionProviderResolver
{
public:
    virtual ~MenuDefinitionProviderResolver() = default;

    MenuDefinitionProviderResolver(IScriptTypeResolver* scriptTypeResolver);

    ::IMenuDefinitionProvider* Resolve(std::string providerTypeName);
private:
    IScriptTypeResolver* ScriptTypeResolver;
};
