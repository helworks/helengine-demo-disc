#ifndef HE_CPP_SYSTEM_BIT_OPERATIONS_HPP
#define HE_CPP_SYSTEM_BIT_OPERATIONS_HPP

#include <cstdint>

class BitOperations {
public:
    static int32_t TrailingZeroCount(uint32_t value) {
        if (value == 0) {
            return 32;
        }

        int32_t count = 0;
        while ((value & 1u) == 0u) {
            value >>= 1;
            ++count;
        }

        return count;
    }

    static int32_t TrailingZeroCount(int32_t value) {
        return TrailingZeroCount(static_cast<uint32_t>(value));
    }

    static int32_t LeadingZeroCount(uint32_t value) {
        if (value == 0) {
            return 32;
        }

        int32_t count = 0;
        uint32_t mask = 0x80000000u;
        while ((value & mask) == 0u) {
            mask >>= 1;
            ++count;
        }

        return count;
    }

    static int32_t LeadingZeroCount(uint64_t value) {
        if (value == 0) {
            return 64;
        }

        int32_t count = 0;
        uint64_t mask = 0x8000000000000000ull;
        while ((value & mask) == 0ull) {
            mask >>= 1;
            ++count;
        }

        return count;
    }

    static int32_t PopCount(uint32_t value) {
        int32_t count = 0;
        while (value != 0u) {
            count += static_cast<int32_t>(value & 1u);
            value >>= 1;
        }

        return count;
    }

    static int32_t Log2(uint32_t value) {
        if (value == 0u) {
            return 0;
        }

        return 31 - LeadingZeroCount(value);
    }

    static int32_t Log2(int32_t value) {
        return Log2(static_cast<uint32_t>(value));
    }

    static uint32_t RoundUpToPowerOf2(uint32_t value) {
        if (value == 0u) {
            return 0u;
        }

        --value;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        return value + 1u;
    }

    static uint64_t RoundUpToPowerOf2(uint64_t value) {
        if (value == 0ull) {
            return 0ull;
        }

        --value;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        value |= value >> 32;
        return value + 1ull;
    }
};

#endif
