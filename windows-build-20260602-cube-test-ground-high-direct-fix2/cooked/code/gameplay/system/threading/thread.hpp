#ifndef HE_CPP_SYSTEM_THREADING_THREAD_HPP
#define HE_CPP_SYSTEM_THREADING_THREAD_HPP

#include <cstdint>
#include <thread>

#ifdef Yield
#undef Yield
#endif

#include "system/action.hpp"

/// <summary>
/// Provides a lightweight managed Thread wrapper backed by std::thread for portable worker execution.
/// </summary>
class Thread {
public:
    /// <summary>
    /// Yields the current execution slice so another runnable thread can proceed.
    /// </summary>
    static void Yield() {
        std::this_thread::yield();
    }

    /// <summary>
    /// Performs a short busy wait for one managed-style spin iteration count.
    /// </summary>
    /// <param name="iterations">Number of spin iterations to execute.</param>
    static void SpinWait(int32_t iterations) {
        volatile int32_t sink = 0;
        for (int32_t iterationIndex = 0; iterationIndex < iterations; ++iterationIndex) {
            sink += iterationIndex;
        }
    }

    /// <summary>
    /// Initializes a new thread wrapper from a parameterless managed delegate.
    /// </summary>
    /// <param name="start">Delegate invoked when the thread starts.</param>
    explicit Thread(Action<>* start)
        : background(false),
          parameterlessStart(start),
          parameterizedStart(nullptr) {
    }

    /// <summary>
    /// Initializes a new thread wrapper from a managed delegate that accepts an object parameter.
    /// </summary>
    /// <param name="start">Delegate invoked when the thread starts with the supplied parameter.</param>
    explicit Thread(Action<void*>* start)
        : background(false),
          parameterlessStart(nullptr),
          parameterizedStart(start) {
    }

    /// <summary>
    /// Sets whether the managed thread is considered a background worker.
    /// </summary>
    /// <param name="value">Background worker flag recorded for parity with managed APIs.</param>
    void set_IsBackground(bool value) {
        background = value;
    }

    /// <summary>
    /// Starts the wrapped thread without an explicit state object.
    /// </summary>
    void Start() {
        worker = std::thread([this]() {
            if (parameterlessStart != nullptr) {
                (*parameterlessStart)();
            } else if (parameterizedStart != nullptr) {
                (*parameterizedStart)(nullptr);
            }
        });
    }

    /// <summary>
    /// Starts the wrapped thread with an explicit boxed state pointer.
    /// </summary>
    /// <param name="parameter">State pointer forwarded to the managed-style delegate.</param>
    void Start(void* parameter) {
        worker = std::thread([this, parameter]() {
            if (parameterizedStart != nullptr) {
                (*parameterizedStart)(parameter);
            } else if (parameterlessStart != nullptr) {
                (*parameterlessStart)();
            }
        });
    }

    /// <summary>
    /// Starts the wrapped thread with a copied managed-style state object.
    /// </summary>
    /// <typeparam name="T">State object type to copy for the worker entry.</typeparam>
    /// <param name="parameter">State object forwarded to the managed-style delegate.</param>
    template <typename T>
    void Start(T parameter) {
        T* boxedParameter = new T(parameter);
        worker = std::thread([this, boxedParameter]() {
            if (parameterizedStart != nullptr) {
                (*parameterizedStart)(boxedParameter);
            } else if (parameterlessStart != nullptr) {
                (*parameterlessStart)();
            }

            delete boxedParameter;
        });
    }

    /// <summary>
    /// Waits for the wrapped thread to finish when it is joinable.
    /// </summary>
    void Join() {
        if (worker.joinable()) {
            worker.join();
        }
    }

private:
    bool background;
    Action<>* parameterlessStart;
    Action<void*>* parameterizedStart;
    std::thread worker;
};

#endif
