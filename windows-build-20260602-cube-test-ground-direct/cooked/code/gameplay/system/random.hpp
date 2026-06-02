#pragma once

#include <algorithm>
#include <cstdint>
#include <limits>
#include <random>

/// <summary>
/// Provides a lightweight managed Random-compatible surface for transpiled code.
/// </summary>
class Random {
    std::mt19937 engine;

public:
    /// <summary>
    /// Initializes a random source with a nondeterministic seed when available.
    /// </summary>
    Random() : engine(std::random_device{}()) {}

    /// <summary>
    /// Initializes a random source with an explicit deterministic seed.
    /// </summary>
    /// <param name="seed">Seed used to initialize the generator.</param>
    explicit Random(int32_t seed) : engine(static_cast<std::mt19937::result_type>(seed)) {}

    /// <summary>
    /// Returns a nonnegative random integer.
    /// </summary>
    /// <returns>Random integer in the managed Random range.</returns>
    int32_t Next() {
        std::uniform_int_distribution<int32_t> distribution(0, std::numeric_limits<int32_t>::max() - 1);
        return distribution(engine);
    }

    /// <summary>
    /// Returns a nonnegative random integer less than the specified maximum.
    /// </summary>
    /// <param name="maxValue">Exclusive upper bound.</param>
    /// <returns>Random integer in [0, maxValue).</returns>
    int32_t Next(int32_t maxValue) {
        if (maxValue <= 0) {
            return 0;
        }

        std::uniform_int_distribution<int32_t> distribution(0, maxValue - 1);
        return distribution(engine);
    }

    /// <summary>
    /// Returns a random integer within the specified range.
    /// </summary>
    /// <param name="minValue">Inclusive lower bound.</param>
    /// <param name="maxValue">Exclusive upper bound.</param>
    /// <returns>Random integer in [minValue, maxValue).</returns>
    int32_t Next(int32_t minValue, int32_t maxValue) {
        if (maxValue <= minValue) {
            return minValue;
        }

        std::uniform_int_distribution<int32_t> distribution(minValue, maxValue - 1);
        return distribution(engine);
    }

    /// <summary>
    /// Returns a random floating point value in the range [0, 1).
    /// </summary>
    /// <returns>Pseudorandom fractional value.</returns>
    double NextDouble() {
        std::uniform_real_distribution<double> distribution(0.0, 1.0);
        return distribution(engine);
    }
};
