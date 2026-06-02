#pragma once

#include <algorithm>
#include <cmath>
#include <cstdint>
#include <cstring>
#include <type_traits>

#include "system/numerics/vector.hpp"

template <typename T>
class Vector128_1 {
    static T AllBitsSetValue() {
        if constexpr (std::is_same_v<T, float>) {
            uint32_t bits = 0xFFFFFFFFu;
            T value;
            std::memcpy(&value, &bits, sizeof(T));
            return value;
        } else if constexpr (std::is_integral_v<T>) {
            return static_cast<T>(~T());
        } else {
            static_assert(std::is_same_v<T, float> || std::is_integral_v<T>, "Unsupported Vector128 mask lane type.");
        }
    }

public:
    static constexpr int32_t LaneCount = sizeof(T) >= 16 ? 1 : static_cast<int32_t>(16 / sizeof(T));

    T Values[LaneCount];

    Vector128_1() {
        for (int32_t laneIndex = 0; laneIndex < LaneCount; ++laneIndex) {
            Values[laneIndex] = T();
        }
    }

    explicit Vector128_1(const T& value) {
        for (int32_t laneIndex = 0; laneIndex < LaneCount; ++laneIndex) {
            Values[laneIndex] = value;
        }
    }

    template <typename TFrom, typename TTo>
    Vector128_1<TTo> As() const {
        static_assert(sizeof(TFrom) == sizeof(TTo), "Vector128 lane reinterpretation requires equal lane sizes.");
        Vector128_1<TTo> result;
        for (int32_t laneIndex = 0; laneIndex < LaneCount; ++laneIndex) {
            std::memcpy(&result.Values[laneIndex], &Values[laneIndex], sizeof(TTo));
        }
        return result;
    }

    static Vector128_1 get_Zero() {
        return Vector128_1(T());
    }

    static Vector128_1 get_AllBitsSet() {
        return Vector128_1(AllBitsSetValue());
    }

    Vector128_1<int32_t> AsInt32() const {
        return As<T, int32_t>();
    }

    Vector_1<T> AsVector() const {
        Vector_1<T> result;
        int32_t copyCount = std::min(Vector_1<T>::get_Count(), LaneCount);
        for (int32_t laneIndex = 0; laneIndex < copyCount; ++laneIndex) {
            result.Values[laneIndex] = Values[laneIndex];
        }
        return result;
    }

    template <typename TTo>
    Vector_1<TTo> AsVector() const {
        static_assert(sizeof(T) == sizeof(TTo), "Vector128 lane reinterpretation requires equal lane sizes.");
        Vector_1<TTo> result;
        int32_t copyCount = std::min(Vector_1<TTo>::get_Count(), LaneCount);
        for (int32_t laneIndex = 0; laneIndex < copyCount; ++laneIndex) {
            std::memcpy(&result.Values[laneIndex], &Values[laneIndex], sizeof(TTo));
        }
        return result;
    }

    T& operator[](size_t index) {
        return Values[index];
    }

    const T& operator[](size_t index) const {
        return Values[index];
    }

    Vector128_1 operator-() const {
        Vector128_1 result;
        for (int32_t laneIndex = 0; laneIndex < LaneCount; ++laneIndex) {
            result.Values[laneIndex] = -Values[laneIndex];
        }

        return result;
    }
};

template <typename T>
Vector128_1<T> operator|(const Vector128_1<T>& left, const Vector128_1<T>& right);

template <typename T>
Vector128_1<T> operator+(const Vector128_1<T>& left, const Vector128_1<T>& right) {
    Vector128_1<T> result;
    for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
        result.Values[laneIndex] = left.Values[laneIndex] + right.Values[laneIndex];
    }
    return result;
}

template <typename T>
Vector128_1<T>& operator+=(Vector128_1<T>& left, const Vector128_1<T>& right) {
    for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
        left.Values[laneIndex] += right.Values[laneIndex];
    }
    return left;
}

template <typename T>
Vector128_1<T> Vector_1<T>::AsVector128() const {
    Vector128_1<T> result;
    int32_t copyCount = std::min(Vector128_1<T>::LaneCount, get_Count());
    for (int32_t laneIndex = 0; laneIndex < copyCount; ++laneIndex) {
        result.Values[laneIndex] = Values[laneIndex];
    }
    return result;
}

template <typename T>
template <typename TTo>
Vector128_1<TTo> Vector_1<T>::AsVector128() const {
    static_assert(sizeof(T) == sizeof(TTo), "Vector128 lane reinterpretation requires equal lane sizes.");
    Vector128_1<TTo> result;
    int32_t copyCount = std::min(Vector128_1<TTo>::LaneCount, get_Count());
    for (int32_t laneIndex = 0; laneIndex < copyCount; ++laneIndex) {
        std::memcpy(&result.Values[laneIndex], &Values[laneIndex], sizeof(TTo));
    }
    return result;
}

class Vector128 {
    template <typename TBits, typename TValue>
    static TBits BitCast(const TValue& value) {
        static_assert(sizeof(TBits) == sizeof(TValue));
        TBits bits;
        std::memcpy(&bits, &value, sizeof(TBits));
        return bits;
    }

    template <typename TValue, typename TBits>
    static TValue BitCastBack(const TBits& bits) {
        static_assert(sizeof(TValue) == sizeof(TBits));
        TValue value;
        std::memcpy(&value, &bits, sizeof(TValue));
        return value;
    }

    template <typename T>
    static bool MaskLaneIsSet(const T& value) {
        if constexpr (std::is_same_v<T, float>) {
            return BitCast<uint32_t>(value) != 0;
        } else {
            return value != 0;
        }
    }

    template <typename T>
    static T AllBitsSetValue() {
        if constexpr (std::is_same_v<T, float>) {
            return BitCastBack<float>(0xFFFFFFFFu);
        } else if constexpr (std::is_integral_v<T>) {
            return static_cast<T>(~T());
        } else {
            static_assert(std::is_same_v<T, float> || std::is_integral_v<T>, "Unsupported Vector128 mask lane type.");
        }
    }

    template <typename T>
    static T ZeroValue() {
        return T();
    }

    template <typename T>
    static T BitwiseAndScalar(const T& left, const T& right) {
        if constexpr (std::is_same_v<T, float>) {
            return BitCastBack<float>(BitCast<uint32_t>(left) & BitCast<uint32_t>(right));
        } else {
            return static_cast<T>(left & right);
        }
    }

    template <typename T>
    static T BitwiseOrScalar(const T& left, const T& right) {
        if constexpr (std::is_same_v<T, float>) {
            return BitCastBack<float>(BitCast<uint32_t>(left) | BitCast<uint32_t>(right));
        } else {
            return static_cast<T>(left | right);
        }
    }

public:
    template <typename T>
    static Vector128_1<T> AsVector128(const Vector_1<T>& value) {
        return value.template AsVector128<T>();
    }

    template <typename TValue>
    static Vector128_1<float> AsVector128(const TValue& value) {
        using RawValue = std::remove_cv_t<std::remove_reference_t<TValue>>;
        Vector128_1<float> result;
        std::memset(result.Values, 0, sizeof(result.Values));
        constexpr size_t CopySize = sizeof(RawValue) < sizeof(result.Values) ? sizeof(RawValue) : sizeof(result.Values);
        std::memcpy(result.Values, &value, CopySize);
        return result;
    }

    template <typename T>
    static Vector128_1<T> As(const Vector128_1<T>& value) {
        return value;
    }

    template <typename TFrom, typename TTo>
    static Vector128_1<TTo> As(const Vector128_1<TFrom>& value) {
        return value.template As<TFrom, TTo>();
    }

    template <typename TFrom>
    static Vector128_1<int32_t> AsInt32(const Vector128_1<TFrom>& value) {
        return value.template As<TFrom, int32_t>();
    }

    template <typename TFrom>
    static Vector128_1<float> AsSingle(const Vector128_1<TFrom>& value) {
        return value.template As<TFrom, float>();
    }

    template <typename TTo, typename TFrom>
    static Vector_1<TTo> AsVector(const Vector128_1<TFrom>& value) {
        return value.template AsVector<TTo>();
    }

    static bool get_IsHardwareAccelerated() {
        return false;
    }

    template <typename T>
    static Vector128_1<T> BitwiseAnd(const Vector128_1<T>& left, const Vector128_1<T>& right) {
        Vector128_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = BitwiseAndScalar(left.Values[laneIndex], right.Values[laneIndex]);
        }
        return result;
    }

    template <typename T>
    static Vector128_1<T> BitwiseOr(const Vector128_1<T>& left, const Vector128_1<T>& right) {
        Vector128_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = BitwiseOrScalar(left.Values[laneIndex], right.Values[laneIndex]);
        }
        return result;
    }

    template <typename T>
    static Vector128_1<T> ConditionalSelect(const Vector128_1<T>& condition, const Vector128_1<T>& left, const Vector128_1<T>& right) {
        Vector128_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = MaskLaneIsSet(condition.Values[laneIndex]) ? left.Values[laneIndex] : right.Values[laneIndex];
        }
        return result;
    }

    template <typename T>
    static Vector128_1<T> Create(const T& value) {
        return Vector128_1<T>(value);
    }

    template <typename T>
    static Vector128_1<T> Create(T value0, T value1, T value2, T value3) {
        Vector128_1<T> result;
        if (Vector128_1<T>::LaneCount > 0) {
            result.Values[0] = value0;
        }
        if (Vector128_1<T>::LaneCount > 1) {
            result.Values[1] = value1;
        }
        if (Vector128_1<T>::LaneCount > 2) {
            result.Values[2] = value2;
        }
        if (Vector128_1<T>::LaneCount > 3) {
            result.Values[3] = value3;
        }
        return result;
    }

    template <typename T, typename... TRest>
    static Vector128_1<T> Create(T first, TRest... rest) {
        Vector128_1<T> result;
        T values[] = { first, static_cast<T>(rest)... };
        int32_t valueCount = static_cast<int32_t>(sizeof...(rest) + 1);
        int32_t copyCount = std::min(Vector128_1<T>::LaneCount, valueCount);
        for (int32_t laneIndex = 0; laneIndex < copyCount; ++laneIndex) {
            result.Values[laneIndex] = values[laneIndex];
        }

        for (int32_t laneIndex = copyCount; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = T();
        }

        return result;
    }

    template <typename T>
    static Vector128_1<T> Equals(const Vector128_1<T>& left, const Vector128_1<T>& right) {
        Vector128_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = left.Values[laneIndex] == right.Values[laneIndex] ? AllBitsSetValue<T>() : ZeroValue<T>();
        }
        return result;
    }

    template <typename T>
    static int32_t ExtractMostSignificantBits(const Vector128_1<T>& value) {
        int32_t result = 0;
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            bool laneMsbSet;
            if constexpr (std::is_same_v<T, float>) {
                laneMsbSet = (BitCast<uint32_t>(value.Values[laneIndex]) & 0x80000000u) != 0;
            } else {
                laneMsbSet = value.Values[laneIndex] < 0;
            }

            if (laneMsbSet) {
                result |= (1 << laneIndex);
            }
        }
        return result;
    }

    template <typename T>
    static Vector128_1<T> LessThan(const Vector128_1<T>& left, const Vector128_1<T>& right) {
        Vector128_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = left.Values[laneIndex] < right.Values[laneIndex] ? AllBitsSetValue<T>() : ZeroValue<T>();
        }
        return result;
    }

    template <typename T>
    static Vector128_1<T> LessThanOrEqual(const Vector128_1<T>& left, const Vector128_1<T>& right) {
        Vector128_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = left.Values[laneIndex] <= right.Values[laneIndex] ? AllBitsSetValue<T>() : ZeroValue<T>();
        }
        return result;
    }

    template <typename T>
    static Vector128_1<T> Load(const T* source) {
        Vector128_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = source[laneIndex];
        }
        return result;
    }

    template <typename T>
    static Vector128_1<T> LoadUnsafe(const T& source) {
        const T* sourcePointer = &source;
        return Load(sourcePointer);
    }

    template <typename T>
    static Vector128_1<T> Max(const Vector128_1<T>& left, const Vector128_1<T>& right) {
        Vector128_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = std::max(left.Values[laneIndex], right.Values[laneIndex]);
        }
        return result;
    }

    template <typename T>
    static Vector128_1<T> Min(const Vector128_1<T>& left, const Vector128_1<T>& right) {
        Vector128_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = std::min(left.Values[laneIndex], right.Values[laneIndex]);
        }
        return result;
    }

    template <typename T>
    static void Store(const Vector128_1<T>& value, T* destination) {
        for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
            destination[laneIndex] = value.Values[laneIndex];
        }
    }

    template <typename T>
    static T ToScalar(const Vector128_1<T>& value) {
        return value.Values[0];
    }

    template <typename T>
    static T GetElement(const Vector128_1<T>& value, int32_t index) {
        return value.Values[index];
    }
};

template <typename T>
Vector128_1<T> operator|(const Vector128_1<T>& left, const Vector128_1<T>& right) {
    return Vector128::BitwiseOr(left, right);
}

template <typename T>
Vector128_1<T> operator-(const Vector128_1<T>& left, const Vector128_1<T>& right) {
    Vector128_1<T> result;
    for (int32_t laneIndex = 0; laneIndex < Vector128_1<T>::LaneCount; ++laneIndex) {
        result.Values[laneIndex] = left.Values[laneIndex] - right.Values[laneIndex];
    }

    return result;
}
