#pragma once

#include <type_traits>

template <typename T>
class EqualityComparer {
public:
    static EqualityComparer<T>* get_Default() {
        static EqualityComparer<T> Instance;
        return &Instance;
    }

    bool Equals(T& left, T& right) {
        if constexpr (std::is_pointer_v<T>) {
            return left == right;
        } else if constexpr (std::is_arithmetic_v<T> || std::is_enum_v<T>) {
            return left == right;
        } else if constexpr (requires(T& a, T& b) { a.Equals(b); }) {
            return left.Equals(right);
        } else {
            return left == right;
        }
    }
};
