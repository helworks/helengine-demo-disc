#pragma once
#ifdef DrawText
#undef DrawText
#endif
#include <cstdint>

class IMenuDefinitionProvider;

#include "runtime/native_string.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_type.hpp"
#include "runtime/native_exceptions.hpp"
#include "IMenuDefinitionProvider.hpp"
#include "IMenuDefinitionProvider.hpp"
#include "IScriptTypeResolver.hpp"

class MenuDefinitionProviderResolver
{
public:
    virtual ~MenuDefinitionProviderResolver() = default;

    MenuDefinitionProviderResolver(IScriptTypeResolver* scriptTypeResolver);

    ::IMenuDefinitionProvider* Resolve(std::string providerTypeName);
private:
    IScriptTypeResolver* ScriptTypeResolver;
};
