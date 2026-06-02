#ifndef HE_CPP_SYSTEM_THREADING_INTERLOCKED_HPP
#define HE_CPP_SYSTEM_THREADING_INTERLOCKED_HPP

#include <atomic>

class Interlocked {
public:
    template <typename T>
    static T Increment(T& location) {
        std::atomic_ref<T> atomicLocation(location);
        return atomicLocation.fetch_add(static_cast<T>(1), std::memory_order_acq_rel) + static_cast<T>(1);
    }

    template <typename T>
    static T Decrement(T& location) {
        std::atomic_ref<T> atomicLocation(location);
        return atomicLocation.fetch_sub(static_cast<T>(1), std::memory_order_acq_rel) - static_cast<T>(1);
    }

    template <typename T>
    static T Add(T& location, T value) {
        std::atomic_ref<T> atomicLocation(location);
        return atomicLocation.fetch_add(value, std::memory_order_acq_rel) + value;
    }

    template <typename T>
    static T CompareExchange(T& location, T value, T comparand) {
        std::atomic_ref<T> atomicLocation(location);
        atomicLocation.compare_exchange_strong(comparand, value, std::memory_order_acq_rel);
        return comparand;
    }
};

#endif
