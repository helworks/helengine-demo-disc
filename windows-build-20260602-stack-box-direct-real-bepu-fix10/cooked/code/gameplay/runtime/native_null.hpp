#ifndef HE_CPP_RUNTIME_NATIVE_NULL_HPP
#define HE_CPP_RUNTIME_NATIVE_NULL_HPP

#include <type_traits>
#include <utility>

template <typename TValue, typename = void>
struct he_cpp_supports_null_equality : std::false_type {
};

template <typename TValue>
struct he_cpp_supports_null_equality<TValue, std::void_t<decltype(std::declval<const TValue&>() == nullptr)>> : std::true_type {
};

template <typename TValue>
inline bool he_cpp_is_null(const TValue& value) {
    using DecayedType = std::remove_cv_t<std::remove_reference_t<TValue>>;

    if constexpr (std::is_pointer_v<DecayedType> || std::is_null_pointer_v<DecayedType>) {
        return value == nullptr;
    } else if constexpr (he_cpp_supports_null_equality<DecayedType>::value) {
        return value == nullptr;
    } else {
        return false;
    }
}

#endif
