#pragma once

#include <algorithm>
#include <cstddef>
#include <cstdint>
#include <cstdlib>
#include <malloc.h>

/// <summary>
/// Provides a portable aligned unmanaged allocation surface compatible with System.Runtime.InteropServices.NativeMemory.
/// </summary>
class NativeMemory {
public:
    /// <summary>
    /// Allocates one aligned unmanaged memory block.
    /// </summary>
    /// <param name="byteCount">Number of bytes requested by the caller.</param>
    /// <param name="alignment">Required alignment in bytes.</param>
    /// <returns>Aligned unmanaged pointer on success; otherwise null.</returns>
    static void* AlignedAlloc(uintptr_t byteCount, uintptr_t alignment) {
        size_t normalizedAlignment = std::max<size_t>(static_cast<size_t>(alignment), alignof(void*));
        size_t normalizedByteCount = static_cast<size_t>(byteCount);
        size_t alignedByteCount = normalizedByteCount;
        if (normalizedAlignment > 0) {
            size_t remainder = normalizedByteCount % normalizedAlignment;
            if (remainder != 0) {
                alignedByteCount += normalizedAlignment - remainder;
            }
        }

#if defined(_MSC_VER)
        return _aligned_malloc(alignedByteCount, normalizedAlignment);
#else
        return std::aligned_alloc(normalizedAlignment, alignedByteCount);
#endif
    }

    /// <summary>
    /// Releases one aligned unmanaged memory block allocated by <see cref="AlignedAlloc"/>.
    /// </summary>
    /// <param name="value">Pointer returned by one prior aligned allocation.</param>
    static void AlignedFree(void* value) {
        if (value == nullptr) {
            return;
        }

#if defined(_MSC_VER)
        _aligned_free(value);
#else
        std::free(value);
#endif
    }
};
