#ifndef HE_CPP_SYSTEM_BUFFER_HPP
#define HE_CPP_SYSTEM_BUFFER_HPP

#include <cstddef>
#include <cstring>

class Buffer {
public:
    static void MemoryCopy(const void* source, void* destination, std::size_t destinationSizeInBytes, std::size_t sourceBytesToCopy) {
        if (destination == nullptr || source == nullptr || sourceBytesToCopy == 0) {
            return;
        }

        std::size_t clampedByteCount = sourceBytesToCopy;
        if (destinationSizeInBytes < clampedByteCount) {
            clampedByteCount = destinationSizeInBytes;
        }

        std::memmove(destination, source, clampedByteCount);
    }
};

#endif
