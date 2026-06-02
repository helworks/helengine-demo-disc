#ifndef HE_CPP_SYSTEM_THREADING_VOLATILE_HPP
#define HE_CPP_SYSTEM_THREADING_VOLATILE_HPP

#include <atomic>

class Volatile {
public:
    template <typename T>
    static T Read(T& location) {
        std::atomic_ref<T> atomicLocation(location);
        return atomicLocation.load(std::memory_order_acquire);
    }

    template <typename T, typename TValue>
    static void Write(T& location, TValue value) {
        std::atomic_ref<T> atomicLocation(location);
        atomicLocation.store(static_cast<T>(value), std::memory_order_release);
    }
};

#endif
