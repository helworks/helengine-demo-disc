#pragma once

#include <cmath>
#include <cstdint>
#include <cstring>
#include <type_traits>

#include "../vector128.hpp"
#include "system/runtime/intrinsics/vector256.hpp"

class Avx {
    template <typename T>
    static T ComparisonTrueValue() {
        if constexpr (std::is_same_v<T, float>) {
            uint32_t bits = 0xFFFFFFFFu;
            T value;
            std::memcpy(&value, &bits, sizeof(T));
            return value;
        } else {
            return static_cast<T>(-1);
        }
    }

public:
    static bool get_IsSupported() {
        return false;
    }

    template <typename T>
    static Vector256<T> LoadAlignedVector256(const T* source) {
        Vector256<T> result;
        if (source == nullptr) {
            return result;
        }

        for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = source[laneIndex];
        }

        return result;
    }

    template <typename T>
    static Vector256<T> LoadVector256(const T* source) {
        return LoadAlignedVector256(source);
    }

    template <typename T>
    static Vector256<T> UnpackLow(const Vector256<T>& left, const Vector256<T>& right) {
        return left;
    }

    template <typename T>
    static Vector256<T> UnpackHigh(const Vector256<T>& left, const Vector256<T>& right) {
        return right;
    }

    template <typename T>
    static Vector256<T> Shuffle(const Vector256<T>& left, const Vector256<T>& right, int32_t control) {
        return (control & 1) == 0 ? left : right;
    }

    template <typename T>
    static Vector256<T> Permute2x128(const Vector256<T>& left, const Vector256<T>& right, int32_t control) {
        return (control & 1) == 0 ? left : right;
    }

    template <typename T>
    static Vector256<T> CompareEqual(const Vector256<T>& left, const Vector256<T>& right) {
        Vector256<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = left.Values[laneIndex] == right.Values[laneIndex] ? ComparisonTrueValue<T>() : T();
        }
        return result;
    }

    template <typename T>
    static Vector256<T> CompareGreaterThan(const Vector256<T>& left, const Vector256<T>& right) {
        Vector256<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = left.Values[laneIndex] > right.Values[laneIndex] ? ComparisonTrueValue<T>() : T();
        }
        return result;
    }

    template <typename T>
    static Vector256<T> CompareLessThanOrEqual(const Vector256<T>& left, const Vector256<T>& right) {
        Vector256<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = left.Values[laneIndex] <= right.Values[laneIndex] ? ComparisonTrueValue<T>() : T();
        }
        return result;
    }

    template <typename T>
    static Vector256<T> Reciprocal(const Vector256<T>& value) {
        Vector256<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = static_cast<T>(1) / value.Values[laneIndex];
        }

        return result;
    }

    template <typename T>
    static Vector256<T> ReciprocalSqrt(const Vector256<T>& value) {
        Vector256<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = static_cast<T>(1) / static_cast<T>(std::sqrt(value.Values[laneIndex]));
        }

        return result;
    }

    template <typename T>
    static int32_t MoveMask(const Vector256<T>& value) {
        int32_t mask = 0;
        for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
            if (value.Values[laneIndex] != T()) {
                mask |= 1 << laneIndex;
            }
        }

        return mask;
    }

    template <typename T>
    static void StoreAligned(T* target, const Vector256<T>& value) {
        if (target == nullptr) {
            return;
        }

        for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
            target[laneIndex] = value.Values[laneIndex];
        }
    }

    template <typename TIndex, typename TValue>
    static Vector128_1<TValue> PermuteVar(const Vector128_1<TValue>& value, const Vector128_1<TIndex>& indices) {
        Vector128_1<TValue> result;
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<TValue>::LaneCount; ++laneIndex) {
            int32_t index = static_cast<int32_t>(indices.Values[laneIndex]);
            if (index < 0) {
                index = 0;
            }

            if (Vector128_1<TValue>::LaneCount > 0) {
                index %= Vector128_1<TValue>::LaneCount;
            }

            result.Values[laneIndex] = value.Values[index];
        }

        return result;
    }
};
