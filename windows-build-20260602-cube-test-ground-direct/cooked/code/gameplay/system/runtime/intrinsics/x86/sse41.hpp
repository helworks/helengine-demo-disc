#pragma once

#include "../vector128.hpp"

class Sse41 {
public:
    static bool get_IsSupported() {
        return false;
    }

    template <typename T>
    static Vector128_1<T> Blend(const Vector128_1<T>& left, const Vector128_1<T>& right, int32_t control) {
        Vector128_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            bool useRight = ((control >> laneIndex) & 1) != 0;
            result.Values[laneIndex] = useRight ? right.Values[laneIndex] : left.Values[laneIndex];
        }
        return result;
    }
};
