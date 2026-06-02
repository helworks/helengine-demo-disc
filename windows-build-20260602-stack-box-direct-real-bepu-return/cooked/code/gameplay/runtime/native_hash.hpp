#ifndef HE_CPP_RUNTIME_NATIVE_HASH_HPP
#define HE_CPP_RUNTIME_NATIVE_HASH_HPP

#include <cstdint>
#include <functional>
#include <type_traits>
#include <utility>

#include "system/number.hpp"

template <typename TValue>
inline int32_t he_cpp_get_hash_code(const TValue& value) {
    using DecayedType = std::remove_cv_t<std::remove_reference_t<TValue>>;

    if constexpr (std::is_pointer_v<DecayedType>) {
        using PointeeType = std::remove_pointer_t<DecayedType>;
        if (value == nullptr) {
            return 0;
        }

        if constexpr (requires(PointeeType* instance) { instance->GetHashCode(); }) {
            return value->GetHashCode();
        } else {
            return Number::GetHashCode(reinterpret_cast<std::uintptr_t>(value));
        }
    } else if constexpr (std::is_arithmetic_v<DecayedType> || std::is_enum_v<DecayedType>) {
        return Number::GetHashCode(value);
    } else if constexpr (requires(const DecayedType& instance) { instance.GetHashCode(); }) {
        return value.GetHashCode();
    } else {
        return static_cast<int32_t>(std::hash<DecayedType>{}(value));
    }
}

#endif
