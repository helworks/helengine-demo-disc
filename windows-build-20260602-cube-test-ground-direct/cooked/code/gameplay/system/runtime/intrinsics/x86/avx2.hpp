#pragma once

#include "../vector128.hpp"

class Avx2 {
public:
    static bool get_IsSupported() {
        return false;
    }

    template <typename TValue, typename TShift>
    static Vector128_1<TValue> ShiftRightLogicalVariable(const Vector128_1<TValue>& value, const Vector128_1<TShift>& shift) {
        Vector128_1<TValue> result;
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<TValue>::LaneCount; ++laneIndex) {
            uint32_t laneValue = static_cast<uint32_t>(value.Values[laneIndex]);
            uint32_t shiftCount = static_cast<uint32_t>(shift.Values[laneIndex]) & 31u;
            result.Values[laneIndex] = static_cast<TValue>(laneValue >> shiftCount);
        }

        return result;
    }
};
