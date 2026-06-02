#pragma once

#include <algorithm>
#include <cmath>
#include <cstdint>
#include <cstring>
#include <sstream>
#include <string>
#include <type_traits>
#include "../../runtime/native_span.hpp"

template <typename T>
class Vector128_1;

template <typename T>
class Vector256;

template <typename T>
class Vector_1 {
public:
    static constexpr int32_t LaneCount = sizeof(T) >= 16 ? 1 : static_cast<int32_t>(16 / sizeof(T));

    T Values[LaneCount];

    Vector_1() {
        for (int32_t laneIndex = 0; laneIndex < LaneCount; ++laneIndex) {
            Values[laneIndex] = T();
        }
    }

    explicit Vector_1(const T& value) {
        for (int32_t laneIndex = 0; laneIndex < LaneCount; ++laneIndex) {
            Values[laneIndex] = value;
        }
    }

    explicit Vector_1(Span<T> values)
        : Vector_1() {
        int32_t copyCount = std::min(get_Count(), values.get_Length());
        for (int32_t laneIndex = 0; laneIndex < copyCount; ++laneIndex) {
            Values[laneIndex] = values[static_cast<size_t>(laneIndex)];
        }
    }

    explicit Vector_1(ReadOnlySpan<T> values)
        : Vector_1() {
        int32_t copyCount = std::min(get_Count(), values.get_Length());
        for (int32_t laneIndex = 0; laneIndex < copyCount; ++laneIndex) {
            Values[laneIndex] = values[static_cast<size_t>(laneIndex)];
        }
    }

    template <typename U, typename = std::enable_if_t<!std::is_same_v<T, U>>>
    Vector_1(const Vector_1<U>& values)
        : Vector_1() {
        int32_t copyCount = std::min(get_Count(), Vector_1<U>::get_Count());
        for (int32_t laneIndex = 0; laneIndex < copyCount; ++laneIndex) {
            Values[laneIndex] = static_cast<T>(values.Values[laneIndex]);
        }
    }

    template <size_t N>
    explicit Vector_1(T (&values)[N])
        : Vector_1() {
        int32_t copyCount = std::min(get_Count(), static_cast<int32_t>(N));
        for (int32_t laneIndex = 0; laneIndex < copyCount; ++laneIndex) {
            Values[laneIndex] = values[laneIndex];
        }
    }

    template <size_t N>
    explicit Vector_1(const T (&values)[N])
        : Vector_1() {
        int32_t copyCount = std::min(get_Count(), static_cast<int32_t>(N));
        for (int32_t laneIndex = 0; laneIndex < copyCount; ++laneIndex) {
            Values[laneIndex] = values[laneIndex];
        }
    }

    static int32_t get_Count() {
        return LaneCount;
    }

    static Vector_1 get_Zero() {
        return Vector_1(T());
    }

    static Vector_1 get_One() {
        return Vector_1(static_cast<T>(1));
    }

    static bool get_IsSupported() {
        return true;
    }

    T& get_Item(int32_t index) {
        return Values[index];
    }

    const T& get_Item(int32_t index) const {
        return Values[index];
    }

    T& operator[](size_t index) {
        return Values[index];
    }

    const T& operator[](size_t index) const {
        return Values[index];
    }

    Vector128_1<T> AsVector128() const;

    template <typename TTo>
    Vector128_1<TTo> AsVector128() const;

    Vector256<T> AsVector256() const;

    template <typename TTo>
    Vector256<TTo> AsVector256() const;

    Vector_1 operator-() const {
        Vector_1 result;
        for (int32_t laneIndex = 0; laneIndex < LaneCount; ++laneIndex) {
            result.Values[laneIndex] = -Values[laneIndex];
        }
        return result;
    }

    void Validate(int32_t laneCount = -1, int32_t = -1) const {
        int32_t validatedLaneCount = laneCount < 0 || laneCount > LaneCount
            ? LaneCount
            : laneCount;

        if constexpr (std::is_same_v<T, float>) {
            for (int32_t laneIndex = 0; laneIndex < validatedLaneCount; ++laneIndex) {
                volatile bool isFinite = std::isfinite(Values[laneIndex]);
                (void)isFinite;
            }
        }
    }

    std::string ToString() const {
        std::ostringstream builder;
        builder << "<";
        for (int32_t laneIndex = 0; laneIndex < LaneCount; ++laneIndex) {
            if (laneIndex > 0) {
                builder << ", ";
            }

            builder << Values[laneIndex];
        }

        builder << ">";
        return builder.str();
    }
};

template <typename T>
Vector_1<T> operator+(const Vector_1<T>& left, const Vector_1<T>& right) {
    Vector_1<T> result;
    for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
        result.Values[laneIndex] = left.Values[laneIndex] + right.Values[laneIndex];
    }
    return result;
}

template <typename T>
Vector_1<T> operator-(const Vector_1<T>& left, const Vector_1<T>& right) {
    Vector_1<T> result;
    for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
        result.Values[laneIndex] = left.Values[laneIndex] - right.Values[laneIndex];
    }
    return result;
}

template <typename T>
Vector_1<T> operator*(const Vector_1<T>& left, const Vector_1<T>& right) {
    Vector_1<T> result;
    for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
        result.Values[laneIndex] = left.Values[laneIndex] * right.Values[laneIndex];
    }
    return result;
}

template <typename T>
Vector_1<T> operator/(const Vector_1<T>& left, const Vector_1<T>& right) {
    Vector_1<T> result;
    for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
        result.Values[laneIndex] = left.Values[laneIndex] / right.Values[laneIndex];
    }
    return result;
}

template <typename T>
Vector_1<T> operator+(const Vector_1<T>& left, const T& right) {
    Vector_1<T> result;
    for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
        result.Values[laneIndex] = left.Values[laneIndex] + right;
    }
    return result;
}

template <typename T>
Vector_1<T> operator-(const Vector_1<T>& left, const T& right) {
    Vector_1<T> result;
    for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
        result.Values[laneIndex] = left.Values[laneIndex] - right;
    }
    return result;
}

template <typename T>
Vector_1<T> operator*(const Vector_1<T>& left, const T& right) {
    Vector_1<T> result;
    for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
        result.Values[laneIndex] = left.Values[laneIndex] * right;
    }
    return result;
}

template <typename T>
Vector_1<T> operator*(const T& left, const Vector_1<T>& right) {
    return right * left;
}

template <typename T>
Vector_1<T> operator/(const Vector_1<T>& left, const T& right) {
    Vector_1<T> result;
    for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
        result.Values[laneIndex] = left.Values[laneIndex] / right;
    }
    return result;
}

class Vector {
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
    static T BitwiseAndScalar(const T& left, const T& right) {
        if constexpr (std::is_integral_v<T>) {
            return static_cast<T>(left & right);
        } else if constexpr (std::is_same_v<T, float>) {
            uint32_t bits = BitCast<uint32_t>(left) & BitCast<uint32_t>(right);
            return BitCastBack<float>(bits);
        } else {
            static_assert(std::is_integral_v<T> || std::is_same_v<T, float>, "Unsupported Vector bitwise operand type.");
        }
    }

    template <typename T>
    static T BitwiseOrScalar(const T& left, const T& right) {
        if constexpr (std::is_integral_v<T>) {
            return static_cast<T>(left | right);
        } else if constexpr (std::is_same_v<T, float>) {
            uint32_t bits = BitCast<uint32_t>(left) | BitCast<uint32_t>(right);
            return BitCastBack<float>(bits);
        } else {
            static_assert(std::is_integral_v<T> || std::is_same_v<T, float>, "Unsupported Vector bitwise operand type.");
        }
    }

    template <typename T>
    static T XorScalar(const T& left, const T& right) {
        if constexpr (std::is_integral_v<T>) {
            return static_cast<T>(left ^ right);
        } else if constexpr (std::is_same_v<T, float>) {
            uint32_t bits = BitCast<uint32_t>(left) ^ BitCast<uint32_t>(right);
            return BitCastBack<float>(bits);
        } else {
            static_assert(std::is_integral_v<T> || std::is_same_v<T, float>, "Unsupported Vector xor operand type.");
        }
    }

    template <typename T>
    static T OnesComplementScalar(const T& value) {
        if constexpr (std::is_integral_v<T>) {
            return static_cast<T>(~value);
        } else if constexpr (std::is_same_v<T, float>) {
            uint32_t bits = ~BitCast<uint32_t>(value);
            return BitCastBack<float>(bits);
        } else {
            static_assert(std::is_integral_v<T> || std::is_same_v<T, float>, "Unsupported Vector complement operand type.");
        }
    }

    template <typename T>
    static int32_t ComparisonMask(bool condition) {
        return condition ? -1 : 0;
    }

public:
    template <typename T>
    static Vector_1<T> Abs(const Vector_1<T>& value) {
        Vector_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            if constexpr (std::is_same_v<T, float>) {
                result.Values[laneIndex] = std::fabs(value.Values[laneIndex]);
            } else {
                result.Values[laneIndex] = static_cast<T>(std::abs(value.Values[laneIndex]));
            }
        }
        return result;
    }

    template <typename T>
    static Vector_1<T> Abs__out1(const Vector_1<T>& value) {
        return Abs(value);
    }

    template <typename T>
    static Vector_1<T> AndNot(const Vector_1<T>& left, const Vector_1<T>& right) {
        return BitwiseAnd(OnesComplement(right), left);
    }

    template <typename T>
    static Vector_1<int32_t> AsVectorInt32(const Vector_1<T>& value) {
        Vector_1<int32_t> result;
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            if constexpr (std::is_same_v<T, int32_t>) {
                result.Values[laneIndex] = value.Values[laneIndex];
            } else if constexpr (sizeof(T) == sizeof(int32_t)) {
                result.Values[laneIndex] = BitCast<int32_t>(value.Values[laneIndex]);
            } else {
                result.Values[laneIndex] = static_cast<int32_t>(value.Values[laneIndex]);
            }
        }
        return result;
    }

    template <typename T>
    static Vector_1<uint32_t> AsVectorUInt32(const Vector_1<T>& value) {
        Vector_1<uint32_t> result;
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            if constexpr (std::is_same_v<T, uint32_t>) {
                result.Values[laneIndex] = value.Values[laneIndex];
            } else if constexpr (sizeof(T) == sizeof(uint32_t)) {
                result.Values[laneIndex] = BitCast<uint32_t>(value.Values[laneIndex]);
            } else {
                result.Values[laneIndex] = static_cast<uint32_t>(value.Values[laneIndex]);
            }
        }

        return result;
    }

    template <typename T>
    static Vector_1<T> BitwiseAnd(const Vector_1<T>& left, const Vector_1<T>& right) {
        Vector_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            result.Values[laneIndex] = BitwiseAndScalar(left.Values[laneIndex], right.Values[laneIndex]);
        }
        return result;
    }

    template <typename T>
    static Vector_1<T> BitwiseOr(const Vector_1<T>& left, const Vector_1<T>& right) {
        Vector_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            result.Values[laneIndex] = BitwiseOrScalar(left.Values[laneIndex], right.Values[laneIndex]);
        }
        return result;
    }

    template <typename T>
    static Vector_1<T> ConditionalSelect(const Vector_1<int32_t>& condition, const Vector_1<T>& left, const Vector_1<T>& right) {
        Vector_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            result.Values[laneIndex] = condition.Values[laneIndex] < 0 ? left.Values[laneIndex] : right.Values[laneIndex];
        }
        return result;
    }

    template <typename T>
    static Vector_1<T> ConditionalSelect__out3(const Vector_1<int32_t>& condition, const Vector_1<T>& left, const Vector_1<T>& right) {
        return ConditionalSelect(condition, left, right);
    }

    template <typename T>
    static T Dot(const Vector_1<T>& left, const Vector_1<T>& right) {
        T result = T();
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            result += left.Values[laneIndex] * right.Values[laneIndex];
        }
        return result;
    }

    template <typename T>
    static Vector_1<int32_t> Equals(const Vector_1<T>& left, const Vector_1<T>& right) {
        Vector_1<int32_t> result;
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            result.Values[laneIndex] = ComparisonMask<T>(left.Values[laneIndex] == right.Values[laneIndex]);
        }
        return result;
    }

    template <typename T>
    static bool EqualsAll(const Vector_1<T>& left, const Vector_1<T>& right) {
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            if (!(left.Values[laneIndex] == right.Values[laneIndex])) {
                return false;
            }
        }
        return true;
    }

    template <typename T>
    static bool EqualsAny(const Vector_1<T>& left, const Vector_1<T>& right) {
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            if (left.Values[laneIndex] == right.Values[laneIndex]) {
                return true;
            }
        }
        return false;
    }

    template <typename T>
    static Vector_1<T> Floor(const Vector_1<T>& value) {
        Vector_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            if constexpr (std::is_same_v<T, float>) {
                result.Values[laneIndex] = std::floor(value.Values[laneIndex]);
            } else {
                result.Values[laneIndex] = value.Values[laneIndex];
            }
        }
        return result;
    }

    template <typename T>
    static Vector_1<int32_t> GreaterThan(const Vector_1<T>& left, const Vector_1<T>& right) {
        Vector_1<int32_t> result;
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            result.Values[laneIndex] = ComparisonMask<T>(left.Values[laneIndex] > right.Values[laneIndex]);
        }
        return result;
    }

    template <typename T>
    static bool GreaterThanAny(const Vector_1<T>& left, const Vector_1<T>& right) {
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            if (left.Values[laneIndex] > right.Values[laneIndex]) {
                return true;
            }
        }
        return false;
    }

    template <typename T>
    static Vector_1<int32_t> GreaterThanOrEqual(const Vector_1<T>& left, const Vector_1<T>& right) {
        Vector_1<int32_t> result;
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            result.Values[laneIndex] = ComparisonMask<T>(left.Values[laneIndex] >= right.Values[laneIndex]);
        }
        return result;
    }

    template <typename T>
    static Vector_1<int32_t> LessThan(const Vector_1<T>& left, const Vector_1<T>& right) {
        Vector_1<int32_t> result;
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            result.Values[laneIndex] = ComparisonMask<T>(left.Values[laneIndex] < right.Values[laneIndex]);
        }
        return result;
    }

    template <typename T>
    static bool LessThanAll(const Vector_1<T>& left, const Vector_1<T>& right) {
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            if (!(left.Values[laneIndex] < right.Values[laneIndex])) {
                return false;
            }
        }
        return true;
    }

    template <typename T>
    static bool LessThanAny(const Vector_1<T>& left, const Vector_1<T>& right) {
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            if (left.Values[laneIndex] < right.Values[laneIndex]) {
                return true;
            }
        }
        return false;
    }

    template <typename T>
    static Vector_1<int32_t> LessThanOrEqual(const Vector_1<T>& left, const Vector_1<T>& right) {
        Vector_1<int32_t> result;
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            result.Values[laneIndex] = ComparisonMask<T>(left.Values[laneIndex] <= right.Values[laneIndex]);
        }
        return result;
    }

    template <typename T>
    static Vector_1<T> Max(const Vector_1<T>& left, const Vector_1<T>& right) {
        Vector_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            result.Values[laneIndex] = std::max(left.Values[laneIndex], right.Values[laneIndex]);
        }
        return result;
    }

    template <typename T>
    static Vector_1<T> Max__out2(const Vector_1<T>& left, const Vector_1<T>& right) {
        return Max(left, right);
    }

    template <typename T>
    static Vector_1<T> Min(const Vector_1<T>& left, const Vector_1<T>& right) {
        Vector_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            result.Values[laneIndex] = std::min(left.Values[laneIndex], right.Values[laneIndex]);
        }
        return result;
    }

    template <typename T>
    static Vector_1<T> Min__out2(const Vector_1<T>& left, const Vector_1<T>& right) {
        return Min(left, right);
    }

    template <typename T>
    static Vector_1<T> OnesComplement(const Vector_1<T>& value) {
        Vector_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            result.Values[laneIndex] = OnesComplementScalar(value.Values[laneIndex]);
        }
        return result;
    }

    template <typename T>
    static Vector_1<T> SquareRoot(const Vector_1<T>& value) {
        Vector_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            result.Values[laneIndex] = static_cast<T>(std::sqrt(value.Values[laneIndex]));
        }
        return result;
    }

    template <typename T>
    static Vector_1<T> Xor(const Vector_1<T>& left, const Vector_1<T>& right) {
        Vector_1<T> result;
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            result.Values[laneIndex] = XorScalar(left.Values[laneIndex], right.Values[laneIndex]);
        }
        return result;
    }

    template <typename TFrom, typename TTo>
    static void Widen(const Vector_1<TFrom>& value, Vector_1<TTo>& low, Vector_1<TTo>& high) {
        int32_t lowLaneCount = Vector_1<TTo>::get_Count();
        int32_t sourceLaneCount = Vector_1<TFrom>::get_Count();
        for (int32_t laneIndex = 0; laneIndex < lowLaneCount; ++laneIndex) {
            low.Values[laneIndex] = laneIndex < sourceLaneCount
                ? static_cast<TTo>(value.Values[laneIndex])
                : TTo();
        }

        for (int32_t laneIndex = 0; laneIndex < lowLaneCount; ++laneIndex) {
            int32_t sourceIndex = lowLaneCount + laneIndex;
            high.Values[laneIndex] = sourceIndex < sourceLaneCount
                ? static_cast<TTo>(value.Values[sourceIndex])
                : TTo();
        }
    }

    template <typename T>
    static T Sum(const Vector_1<T>& value) {
        T result = T();
        for (int32_t laneIndex = 0; laneIndex < Vector_1<T>::get_Count(); ++laneIndex) {
            result += value.Values[laneIndex];
        }

        return result;
    }
};

template <typename T>
Vector_1<T> operator&(const Vector_1<T>& left, const Vector_1<T>& right) {
    return Vector::BitwiseAnd(left, right);
}

template <typename T>
Vector_1<T> operator|(const Vector_1<T>& left, const Vector_1<T>& right) {
    return Vector::BitwiseOr(left, right);
}

template <typename T>
Vector_1<T> operator^(const Vector_1<T>& left, const Vector_1<T>& right) {
    return Vector::Xor(left, right);
}
