#pragma once

#include <cstdint>
#include <cstring>
#include <type_traits>

template <typename TTarget, typename TSource>
inline TTarget& he_cpp_unsafe_as(TSource* source) {
    using TargetStorage = std::remove_const_t<TTarget>;
    using SourceStorage = std::remove_const_t<TSource>;
    return *reinterpret_cast<TargetStorage*>(const_cast<SourceStorage*>(source));
}

template <typename TTarget, typename TSource>
inline TTarget& he_cpp_unsafe_as(TSource& source) {
    return *reinterpret_cast<TTarget*>(&source);
}

template <typename TTarget, typename TSource>
inline TTarget& he_cpp_unsafe_as_ref(TSource* source) {
    using TargetStorage = std::remove_const_t<TTarget>;
    using SourceStorage = std::remove_const_t<TSource>;
    return *reinterpret_cast<TargetStorage*>(const_cast<SourceStorage*>(source));
}

template <typename TTarget, typename TSource>
inline TTarget& he_cpp_unsafe_as_ref(TSource& source) {
    return *reinterpret_cast<TTarget*>(&source);
}

template <typename TTarget, typename TSource>
inline TTarget* he_cpp_unsafe_as_pointer(TSource* source) {
    return reinterpret_cast<TTarget*>(source);
}

template <typename TTarget, typename TSource>
inline TTarget* he_cpp_unsafe_as_pointer(TSource& source) {
    return reinterpret_cast<TTarget*>(&source);
}

template <typename T>
inline constexpr int32_t he_cpp_unsafe_size_of() {
    return static_cast<int32_t>(sizeof(T));
}

template <typename T>
inline void he_cpp_unsafe_skip_init(T&) {
}

template <typename T>
inline T& he_cpp_unsafe_add(T* source, int32_t index) {
    return source[index];
}

template <typename T>
inline T& he_cpp_unsafe_add(T& source, int32_t index) {
    return *(reinterpret_cast<T*>(&source) + index);
}

inline void he_cpp_unsafe_init_block_unaligned(void* target, uint8_t value, uint32_t count) {
    std::memset(target, value, count);
}

inline void he_cpp_unsafe_copy_block_unaligned(void* target, const void* source, uint32_t count) {
    std::memcpy(target, source, count);
}

template <typename TTarget, typename TSource>
inline void he_cpp_unsafe_copy_block_unaligned(TTarget& target, TSource& source, uint32_t count) {
    std::memcpy(&target, &source, count);
}
