#pragma once

#include <cstddef>
#include <cstdint>
#include <cstring>
#include <type_traits>

#include "system/numerics/vector.hpp"

template <typename T>
class Vector512;

/// <summary>
/// Provides a portable subset of System.Runtime.Intrinsics.Vector512 used by transpiled managed numerics.
/// </summary>
class Vector512Runtime {
    template <typename TBits, typename TValue>
    static TBits BitCast(const TValue& value) {
        static_assert(sizeof(TBits) == sizeof(TValue));
        TBits bits;
        std::memcpy(&bits, &value, sizeof(TBits));
        return bits;
    }

public:
    template <typename T>
    static T AllBitsSetValue() {
        if constexpr (std::is_same_v<T, float>) {
            return BitCast<float>(0xFFFFFFFFu);
        } else if constexpr (std::is_same_v<T, double>) {
            return BitCast<double>(0xFFFFFFFFFFFFFFFFull);
        } else if constexpr (std::is_integral_v<T>) {
            return static_cast<T>(~T());
        } else {
            static_assert(std::is_same_v<T, float> || std::is_same_v<T, double> || std::is_integral_v<T>, "Unsupported Vector512 mask lane type.");
        }
    }

    static bool get_IsHardwareAccelerated() {
        return false;
    }

    template <typename T>
    static Vector512<T> Load(const T* source) {
        Vector512<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector512<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = source[laneIndex];
        }
        return result;
    }

    template <typename T>
    static void Store(const Vector512<T>& value, T* destination) {
        for (int32_t laneIndex = 0; laneIndex < Vector512<T>::LaneCount; ++laneIndex) {
            destination[laneIndex] = value.Values[laneIndex];
        }
    }

    template <typename T>
    static Vector512<T> BitwiseOr(const Vector512<T>& left, const Vector512<T>& right) {
        Vector512<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector512<T>::LaneCount; ++laneIndex) {
            using TBits = std::conditional_t<sizeof(T) == 8, uint64_t, uint32_t>;
            TBits leftBits = BitCast<TBits>(left.Values[laneIndex]);
            TBits rightBits = BitCast<TBits>(right.Values[laneIndex]);
            TBits resultBits = leftBits | rightBits;
            std::memcpy(&result.Values[laneIndex], &resultBits, sizeof(T));
        }
        return result;
    }

    template <typename T>
    static Vector512<T> Xor(const Vector512<T>& left, const Vector512<T>& right) {
        Vector512<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector512<T>::LaneCount; ++laneIndex) {
            using TBits = std::conditional_t<sizeof(T) == 8, uint64_t, uint32_t>;
            TBits leftBits = BitCast<TBits>(left.Values[laneIndex]);
            TBits rightBits = BitCast<TBits>(right.Values[laneIndex]);
            TBits resultBits = leftBits ^ rightBits;
            std::memcpy(&result.Values[laneIndex], &resultBits, sizeof(T));
        }
        return result;
    }

    template <typename T>
    static Vector512<T> AndNot(const Vector512<T>& left, const Vector512<T>& right) {
        Vector512<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector512<T>::LaneCount; ++laneIndex) {
            using TBits = std::conditional_t<sizeof(T) == 8, uint64_t, uint32_t>;
            TBits leftBits = BitCast<TBits>(left.Values[laneIndex]);
            TBits rightBits = BitCast<TBits>(right.Values[laneIndex]);
            TBits resultBits = leftBits & (~rightBits);
            std::memcpy(&result.Values[laneIndex], &resultBits, sizeof(T));
        }
        return result;
    }

    template <typename T>
    static Vector512<T> Equals(const Vector512<T>& left, const Vector512<T>& right) {
        Vector512<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector512<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = left.Values[laneIndex] == right.Values[laneIndex] ? AllBitsSetValue<T>() : T();
        }
        return result;
    }

    template <typename T>
    static uint64_t ExtractMostSignificantBits(const Vector512<T>& value) {
        uint64_t bits = 0;
        for (int32_t laneIndex = 0; laneIndex < Vector512<T>::LaneCount && laneIndex < 64; ++laneIndex) {
            using TBits = std::conditional_t<sizeof(T) == 8, uint64_t, uint32_t>;
            TBits laneBits = BitCast<TBits>(value.Values[laneIndex]);
            uint64_t signBit = static_cast<uint64_t>((laneBits >> ((sizeof(TBits) * 8) - 1)) & 1);
            bits |= signBit << laneIndex;
        }
        return bits;
    }
};

template <typename T>
class Vector512 {
public:
    static constexpr int32_t LaneCount = sizeof(T) >= 64 ? 1 : static_cast<int32_t>(64 / sizeof(T));

    T Values[LaneCount];

    Vector512() {
        for (int32_t laneIndex = 0; laneIndex < LaneCount; ++laneIndex) {
            Values[laneIndex] = T();
        }
    }

    explicit Vector512(const T& value) {
        for (int32_t laneIndex = 0; laneIndex < LaneCount; ++laneIndex) {
            Values[laneIndex] = value;
        }
    }

    static Vector512 get_Zero() {
        return Vector512(T());
    }

    static Vector512 get_AllBitsSet() {
        return Vector512(Vector512Runtime::AllBitsSetValue<T>());
    }

    T& operator[](size_t index) {
        return Values[index];
    }

    const T& operator[](size_t index) const {
        return Values[index];
    }
};
