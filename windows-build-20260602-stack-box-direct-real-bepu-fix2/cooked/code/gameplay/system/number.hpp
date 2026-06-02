#pragma once

#include <charconv>
#include <cmath>
#include <cstdint>
#include <functional>
#include <limits>
#include <string>

/// <summary>
/// Provides lightweight managed numeric helpers used by transpiled static primitive calls.
/// </summary>
class Number {
public:
    inline static constexpr double Epsilon = 2.2204460492503131e-16;

    /// <summary>
    /// Attempts to parse a signed 32-bit integer from text.
    /// </summary>
    /// <param name="text">Source text to parse.</param>
    /// <param name="value">Parsed value when the conversion succeeds.</param>
    /// <returns>True when parsing succeeds; otherwise false.</returns>
    static bool TryParse(const std::string& text, int32_t& value) {
        const char* begin = text.data();
        const char* end = begin + text.size();
        std::from_chars_result result = std::from_chars(begin, end, value);
        return result.ec == std::errc() && result.ptr == end;
    }

    /// <summary>
    /// Determines whether the supplied single-precision value is positive infinity.
    /// </summary>
    /// <param name="value">Value to inspect.</param>
    /// <returns>True when the value is positive infinity; otherwise false.</returns>
    static bool IsPositiveInfinity(float value) {
        return std::isinf(value) && value > 0.0f;
    }

    /// <summary>
    /// Determines whether the supplied double-precision value is positive infinity.
    /// </summary>
    /// <param name="value">Value to inspect.</param>
    /// <returns>True when the value is positive infinity; otherwise false.</returns>
    static bool IsPositiveInfinity(double value) {
        return std::isinf(value) && value > 0.0;
    }

    /// <summary>
    /// Determines whether the supplied single-precision value is not-a-number.
    /// </summary>
    /// <param name="value">Value to inspect.</param>
    /// <returns>True when the value is NaN; otherwise false.</returns>
    static bool IsNaN(float value) {
        return std::isnan(value);
    }

    /// <summary>
    /// Determines whether the supplied double-precision value is not-a-number.
    /// </summary>
    /// <param name="value">Value to inspect.</param>
    /// <returns>True when the value is NaN; otherwise false.</returns>
    static bool IsNaN(double value) {
        return std::isnan(value);
    }

    /// <summary>
    /// Determines whether the supplied single-precision value is infinite.
    /// </summary>
    /// <param name="value">Value to inspect.</param>
    /// <returns>True when the value is infinite; otherwise false.</returns>
    static bool IsInfinity(float value) {
        return std::isinf(value);
    }

    /// <summary>
    /// Determines whether the supplied double-precision value is infinite.
    /// </summary>
    /// <param name="value">Value to inspect.</param>
    /// <returns>True when the value is infinite; otherwise false.</returns>
    static bool IsInfinity(double value) {
        return std::isinf(value);
    }

    /// <summary>
    /// Returns a positive-infinity value for the requested floating-point type.
    /// </summary>
    /// <typeparam name="T">Floating-point type whose positive infinity should be returned.</typeparam>
    /// <returns>Positive-infinity constant for <typeparamref name="T"/>.</returns>
    template <typename T>
    static T PositiveInfinity() {
        return std::numeric_limits<T>::infinity();
    }

    /// <summary>
    /// Returns a negative-infinity value for the requested floating-point type.
    /// </summary>
    /// <typeparam name="T">Floating-point type whose negative infinity should be returned.</typeparam>
    /// <returns>Negative-infinity constant for <typeparamref name="T"/>.</returns>
    template <typename T>
    static T NegativeInfinity() {
        return -std::numeric_limits<T>::infinity();
    }

    /// <summary>
    /// Returns a quiet NaN value for the requested floating-point type.
    /// </summary>
    /// <typeparam name="T">Floating-point type whose NaN constant should be returned.</typeparam>
    /// <returns>Quiet NaN constant for <typeparamref name="T"/>.</returns>
    template <typename T>
    static T NaN() {
        return std::numeric_limits<T>::quiet_NaN();
    }

    /// <summary>
    /// Produces a stable integer hash code for a primitive value using the native standard hash surface.
    /// </summary>
    /// <typeparam name="T">Primitive value type to hash.</typeparam>
    /// <param name="value">Value whose hash code should be produced.</param>
    /// <returns>Signed 32-bit hash code for the supplied value.</returns>
    template <typename T>
    static int32_t GetHashCode(const T& value) {
        return static_cast<int32_t>(std::hash<T>{}(value));
    }
};
