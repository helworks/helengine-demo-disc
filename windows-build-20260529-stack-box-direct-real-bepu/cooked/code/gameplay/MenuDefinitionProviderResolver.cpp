#ifdef DrawText
#undef DrawText
#endif
#include "MenuDefinitionProviderResolver.hpp"
#include "runtime/native_string.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_type.hpp"
#include "IMenuDefinitionProvider.hpp"
#include "MenuDefinition.hpp"
#include "MenuDefinitionProviderResolver.hpp"
#include "runtime/array.hpp"
#include "IScriptTypeResolver.hpp"
#include "runtime/array.hpp"
#include "runtime/native_cast.hpp"
#include "runtime/native_exceptions.hpp"
#include "runtime/native_string.hpp"
#include "runtime/native_type.hpp"

MenuDefinitionProviderResolver::MenuDefinitionProviderResolver(IScriptTypeResolver* scriptTypeResolver) : ScriptTypeResolver()
{
this->ScriptTypeResolver = scriptTypeResolver;
}

::IMenuDefinitionProvider* MenuDefinitionProviderResolver::Resolve(std::string providerTypeName)
{
    if (String::IsNullOrWhiteSpace(providerTypeName))
    {
throw ([&]() {
auto __ctor_arg_00000036 = "Provider type name must be provided.";
auto __ctor_arg_00000037 = "providerTypeName";
return new ArgumentException(__ctor_arg_00000036, __ctor_arg_00000037);
})();
    }
Type *providerType = Type::GetType(providerTypeName, false);
    if (providerType == nullptr && this->ScriptTypeResolver != nullptr)
    {
providerType = this->ScriptTypeResolver->Resolve(providerTypeName);
    }
    if (providerType == nullptr)
    {
throw new InvalidOperationException(std::string("Menu provider type '") + providerTypeName + std::string("' could not be resolved."));
    }
    if (!he_cpp_type_of<IMenuDefinitionProvider>("IMenuDefinitionProvider")->IsAssignableFrom(providerType))
    {
throw new InvalidOperationException(std::string("Menu provider type '") + providerTypeName + std::string("' must implement ") + "IMenuDefinitionProvider" + std::string("."));
    }
ConstructorInfo *constructor = providerType->GetConstructor(Type::EmptyTypes);
    if (constructor == nullptr || !constructor->get_IsPublic())
    {
throw new InvalidOperationException(std::string("Menu provider type '") + providerTypeName + std::string("' must expose a public parameterless constructor."));
    }
const void *instance = Activator::CreateInstance(providerType);
::IMenuDefinitionProvider *provider = he_cpp_try_cast<IMenuDefinitionProvider>(instance);
    if (provider == nullptr)
    {
throw new InvalidOperationException(std::string("Menu provider type '") + providerTypeName + std::string("' could not be instantiated."));
    }
return provider;}

