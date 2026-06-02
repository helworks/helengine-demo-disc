#pragma once

#include <algorithm>
#include <cstdint>
#include <cstring>

#include "system/numerics/vector.hpp"

template <typename T>
class Vector256 {
public:
    static constexpr int32_t LaneCount = sizeof(T) >= 32 ? 1 : static_cast<int32_t>(32 / sizeof(T));

    T Values[LaneCount];

    Vector256() {
        for (int32_t laneIndex = 0; laneIndex < LaneCount; ++laneIndex) {
            Values[laneIndex] = T();
        }
    }

    explicit Vector256(const T& value) {
        for (int32_t laneIndex = 0; laneIndex < LaneCount; ++laneIndex) {
            Values[laneIndex] = value;
        }
    }

    static Vector256 get_Zero() {
        return Vector256(T());
    }

    static Vector256 get_AllBitsSet() {
        if constexpr (std::is_same_v<T, float>) {
            uint32_t bits = 0xFFFFFFFFu;
            T value;
            std::memcpy(&value, &bits, sizeof(T));
            return Vector256(value);
        } else {
            return Vector256(static_cast<T>(~T()));
        }
    }

    template <typename TFrom, typename TTo>
    Vector256<TTo> As() const {
        static_assert(sizeof(TFrom) == sizeof(TTo), "Vector256 lane reinterpretation requires equal lane sizes.");
        Vector256<TTo> result;
        int32_t copyCount = std::min(Vector256<TTo>::LaneCount, LaneCount);
        for (int32_t laneIndex = 0; laneIndex < copyCount; ++laneIndex) {
            std::memcpy(&result.Values[laneIndex], &Values[laneIndex], sizeof(TTo));
        }
        return result;
    }

    Vector256<int32_t> AsInt32() const {
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
        static_assert(sizeof(T) == sizeof(TTo), "Vector lane reinterpretation requires equal lane sizes.");
        Vector_1<TTo> result;
        int32_t copyCount = std::min(Vector_1<TTo>::get_Count(), LaneCount);
        for (int32_t laneIndex = 0; laneIndex < copyCount; ++laneIndex) {
            std::memcpy(&result.Values[laneIndex], &Values[laneIndex], sizeof(TTo));
        }
        return result;
    }

    template <typename TTo = T>
    Vector128_1<TTo> GetLower() const {
        static_assert(sizeof(T) == sizeof(TTo), "Vector256 lane reinterpretation requires equal lane sizes.");
        Vector128_1<TTo> result;
        int32_t copyCount = std::min(Vector128_1<TTo>::LaneCount, LaneCount / 2);
        for (int32_t laneIndex = 0; laneIndex < copyCount; ++laneIndex) {
            std::memcpy(&result.Values[laneIndex], &Values[laneIndex], sizeof(TTo));
        }
        return result;
    }

    template <typename TTo = T>
    Vector128_1<TTo> GetUpper() const {
        static_assert(sizeof(T) == sizeof(TTo), "Vector256 lane reinterpretation requires equal lane sizes.");
        Vector128_1<TTo> result;
        int32_t upperOffset = LaneCount / 2;
        int32_t copyCount = std::min(Vector128_1<TTo>::LaneCount, LaneCount - upperOffset);
        for (int32_t laneIndex = 0; laneIndex < copyCount; ++laneIndex) {
            std::memcpy(&result.Values[laneIndex], &Values[upperOffset + laneIndex], sizeof(TTo));
        }
        return result;
    }

    T& operator[](size_t index) {
        return Values[index];
    }

    const T& operator[](size_t index) const {
        return Values[index];
    }
};

template <typename T>
Vector256<T> operator+(const Vector256<T>& left, const Vector256<T>& right) {
    Vector256<T> result;
    for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
        result.Values[laneIndex] = left.Values[laneIndex] + right.Values[laneIndex];
    }

    return result;
}

template <typename T>
Vector256<T> operator-(const Vector256<T>& left, const Vector256<T>& right) {
    Vector256<T> result;
    for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
        result.Values[laneIndex] = left.Values[laneIndex] - right.Values[laneIndex];
    }

    return result;
}

template <typename T>
Vector256<T> operator*(const Vector256<T>& left, const Vector256<T>& right) {
    Vector256<T> result;
    for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
        result.Values[laneIndex] = left.Values[laneIndex] * right.Values[laneIndex];
    }

    return result;
}

template <typename T>
Vector256<T> Vector_1<T>::AsVector256() const {
    Vector256<T> result;
    int32_t copyCount = std::min(Vector256<T>::LaneCount, get_Count());
    for (int32_t laneIndex = 0; laneIndex < copyCount; ++laneIndex) {
        result.Values[laneIndex] = Values[laneIndex];
    }
    return result;
}

class Vector256Runtime {
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
    static T AllBitsSetValue() {
        if constexpr (std::is_same_v<T, float>) {
            return BitCastBack<float>(0xFFFFFFFFu);
        } else if constexpr (std::is_integral_v<T>) {
            return static_cast<T>(~T());
        } else {
            static_assert(std::is_same_v<T, float> || std::is_integral_v<T>, "Unsupported Vector256 mask lane type.");
        }
    }

public:
    static bool get_IsHardwareAccelerated() {
        return false;
    }

    template <typename T>
    static Vector256<T> AsVector256(const Vector_1<T>& value) {
        return value.template AsVector256<T>();
    }

    template <typename T>
    static Vector256<T> As(const Vector256<T>& value) {
        return value;
    }

    template <typename TTo, typename TFrom>
    static Vector256<TTo> As(const Vector256<TFrom>& value) {
        return value.template As<TFrom, TTo>();
    }

    template <typename TTo, typename TFrom>
    static Vector_1<TTo> AsVector(const Vector256<TFrom>& value) {
        return value.template AsVector<TTo>();
    }

    template <typename TFrom>
    static Vector256<int32_t> AsInt32(const Vector256<TFrom>& value) {
        return value.template As<TFrom, int32_t>();
    }

    template <typename TFrom>
    static Vector256<float> AsSingle(const Vector256<TFrom>& value) {
        return value.template As<TFrom, float>();
    }

    template <typename TTo, typename TFrom>
    static Vector128_1<TTo> GetLower(const Vector256<TFrom>& value) {
        return value.template GetLower<TTo>();
    }

    template <typename TTo, typename TFrom>
    static Vector128_1<TTo> GetUpper(const Vector256<TFrom>& value) {
        return value.template GetUpper<TTo>();
    }

    template <typename T>
    static Vector256<T> Create(const T& value) {
        return Vector256<T>(value);
    }

    template <typename T, typename... TRest>
    static Vector256<T> Create(T first, TRest... rest) {
        Vector256<T> result;
        T values[] = { first, static_cast<T>(rest)... };
        int32_t valueCount = static_cast<int32_t>(sizeof...(rest) + 1);
        int32_t copyCount = std::min(Vector256<T>::LaneCount, valueCount);
        for (int32_t laneIndex = 0; laneIndex < copyCount; ++laneIndex) {
            result.Values[laneIndex] = values[laneIndex];
        }
        for (int32_t laneIndex = copyCount; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = T();
        }
        return result;
    }

    template <typename T>
    static Vector256<T> Load(const T* source) {
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
    static void Store(const Vector256<T>& value, T* target) {
        if (target == nullptr) {
            return;
        }

        for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
            target[laneIndex] = value.Values[laneIndex];
        }
    }

    template <typename T>
    static Vector256<T> BitwiseOr(const Vector256<T>& left, const Vector256<T>& right) {
        Vector256<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
            if constexpr (std::is_same_v<T, float>) {
                result.Values[laneIndex] = BitCastBack<float>(BitCast<uint32_t>(left.Values[laneIndex]) | BitCast<uint32_t>(right.Values[laneIndex]));
            } else {
                result.Values[laneIndex] = static_cast<T>(left.Values[laneIndex] | right.Values[laneIndex]);
            }
        }
        return result;
    }

    template <typename T>
    static Vector256<T> BitwiseAnd(const Vector256<T>& left, const Vector256<T>& right) {
        Vector256<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
            if constexpr (std::is_same_v<T, float>) {
                result.Values[laneIndex] = BitCastBack<float>(BitCast<uint32_t>(left.Values[laneIndex]) & BitCast<uint32_t>(right.Values[laneIndex]));
            } else {
                result.Values[laneIndex] = static_cast<T>(left.Values[laneIndex] & right.Values[laneIndex]);
            }
        }
        return result;
    }

    template <typename T>
    static Vector256<T> Xor(const Vector256<T>& left, const Vector256<T>& right) {
        Vector256<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
            if constexpr (std::is_same_v<T, float>) {
                result.Values[laneIndex] = BitCastBack<float>(BitCast<uint32_t>(left.Values[laneIndex]) ^ BitCast<uint32_t>(right.Values[laneIndex]));
            } else {
                result.Values[laneIndex] = static_cast<T>(left.Values[laneIndex] ^ right.Values[laneIndex]);
            }
        }
        return result;
    }

    template <typename T>
    static Vector256<T> AndNot(const Vector256<T>& left, const Vector256<T>& right) {
        Vector256<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
            if constexpr (std::is_same_v<T, float>) {
                result.Values[laneIndex] = BitCastBack<float>((~BitCast<uint32_t>(left.Values[laneIndex])) & BitCast<uint32_t>(right.Values[laneIndex]));
            } else {
                result.Values[laneIndex] = static_cast<T>((~left.Values[laneIndex]) & right.Values[laneIndex]);
            }
        }
        return result;
    }

    template <typename T>
    static Vector256<T> LessThan(const Vector256<T>& left, const Vector256<T>& right) {
        Vector256<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = left.Values[laneIndex] < right.Values[laneIndex] ? AllBitsSetValue<T>() : T();
        }
        return result;
    }

    template <typename T>
    static Vector256<T> LessThanOrEqual(const Vector256<T>& left, const Vector256<T>& right) {
        Vector256<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = left.Values[laneIndex] <= right.Values[laneIndex] ? AllBitsSetValue<T>() : T();
        }
        return result;
    }

    template <typename T>
    static Vector256<T> Equals(const Vector256<T>& left, const Vector256<T>& right) {
        Vector256<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount; ++laneIndex) {
            result.Values[laneIndex] = left.Values[laneIndex] == right.Values[laneIndex] ? AllBitsSetValue<T>() : T();
        }
        return result;
    }

    template <typename T>
    static uint32_t ExtractMostSignificantBits(const Vector256<T>& value) {
        uint32_t bits = 0;
        for (int32_t laneIndex = 0; laneIndex < Vector256<T>::LaneCount && laneIndex < 32; ++laneIndex) {
            if constexpr (std::is_same_v<T, float>) {
                uint32_t laneBits = BitCast<uint32_t>(value.Values[laneIndex]);
                bits |= ((laneBits >> 31) & 1u) << laneIndex;
            } else {
                using TBits = std::make_unsigned_t<T>;
                TBits laneBits = static_cast<TBits>(value.Values[laneIndex]);
                bits |= ((laneBits >> ((sizeof(TBits) * 8) - 1)) & 1u) << laneIndex;
            }
        }
        return bits;
    }
};

template <typename T>
template <typename TTo>
Vector256<TTo> Vector_1<T>::AsVector256() const {
    static_assert(sizeof(T) == sizeof(TTo), "Vector256 lane reinterpretation requires equal lane sizes.");
    Vector256<TTo> result;
    int32_t copyCount = std::min(Vector256<TTo>::LaneCount, get_Count());
    for (int32_t laneIndex = 0; laneIndex < copyCount; ++laneIndex) {
        std::memcpy(&result.Values[laneIndex], &Values[laneIndex], sizeof(TTo));
    }
    return result;
}
