#pragma once

#include <cmath>
#include <cstdint>
#include <cstring>
#include <type_traits>

#include "system/runtime/intrinsics/vector128.hpp"

class Sse {
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
    static Vector128_1<T> CompareGreaterThan(const Vector128_1<T>& left, const Vector128_1<T>& right) {
        Vector128_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = left.Values[laneIndex] > right.Values[laneIndex] ? ComparisonTrueValue<T>() : T();
        }
        return result;
    }

    template <typename T>
    static Vector128_1<T> CompareLessThanOrEqual(const Vector128_1<T>& left, const Vector128_1<T>& right) {
        Vector128_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = left.Values[laneIndex] <= right.Values[laneIndex] ? ComparisonTrueValue<T>() : T();
        }
        return result;
    }

    template <typename T>
    static Vector128_1<T> Reciprocal(const Vector128_1<T>& value) {
        Vector128_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = static_cast<T>(1) / value.Values[laneIndex];
        }

        return result;
    }

    template <typename T>
    static Vector128_1<T> ReciprocalSqrt(const Vector128_1<T>& value) {
        Vector128_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = static_cast<T>(1) / static_cast<T>(std::sqrt(value.Values[laneIndex]));
        }

        return result;
    }

    template <typename T>
    static int32_t MoveMask(const Vector128_1<T>& value) {
        int32_t mask = 0;
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            if (value.Values[laneIndex] != T()) {
                mask |= 1 << laneIndex;
            }
        }

        return mask;
    }

    template <typename T>
    static void StoreAligned(T* destination, const Vector128_1<T>& value) {
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            destination[laneIndex] = value.Values[laneIndex];
        }
    }
};
