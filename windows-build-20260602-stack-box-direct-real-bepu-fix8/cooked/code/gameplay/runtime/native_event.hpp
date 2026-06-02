#pragma once

#include <array>
#include <cstddef>
#include <functional>
#include <memory>
#include <type_traits>
#include <utility>
#include <vector>

/// <summary>
/// Represents a lightweight managed event bridge used by transpiled engine members during native execution.
/// </summary>
class Event {
public:
    /// <summary>
    /// Initializes an empty event bridge.
    /// </summary>
    Event() = default;

    /// <summary>
    /// Registers one free or static function subscriber that can be invoked later with a matching argument list.
    /// </summary>
    /// <typeparam name="TArgs">Native handler argument types.</typeparam>
    /// <param name="handler">Free or static function handler being attached to the event.</param>
    /// <returns>The current event so chained subscriptions remain compilable.</returns>
    template <typename... TArgs>
    Event& operator+=(void (*handler)(TArgs...)) {
        if (handler == nullptr) {
            return *this;
        }

        Subscribers.push_back(Subscriber {
            sizeof...(TArgs),
            [handler](void** arguments) {
                InvokeFunctionPointer(handler, arguments, std::index_sequence_for<TArgs...> {});
            }
        });
        return *this;
    }

    /// <summary>
    /// Accepts unsupported subscriber shapes so existing generated unbound instance-method subscriptions remain compilable.
    /// </summary>
    /// <typeparam name="THandler">Native handler shape provided by the caller.</typeparam>
    /// <param name="handler">Handler instance being attached to the event.</param>
    /// <returns>The current event so chained subscriptions remain compilable.</returns>
    template <typename THandler>
    Event& operator+=(THandler handler) {
        (void)handler;
        return *this;
    }

    /// <summary>
    /// Unregisters a subscriber from the event placeholder.
    /// </summary>
    /// <typeparam name="THandler">Native handler shape provided by the caller.</typeparam>
    /// <param name="handler">Handler instance being detached from the event.</param>
    /// <returns>The current event so chained removals remain compilable.</returns>
    template <typename THandler>
    Event& operator-=(THandler handler) {
        (void)handler;
        return *this;
    }

    /// <summary>
    /// Invokes all subscribers that were registered with the same arity as the supplied argument list.
    /// </summary>
    /// <typeparam name="TArgs">Native argument shapes forwarded by the transpiled call site.</typeparam>
    /// <param name="args">Arguments supplied by the transpiled call site.</param>
    template <typename... TArgs>
    void Invoke(TArgs... args) {
        std::array<void*, sizeof...(TArgs)> argumentPointers { const_cast<void*>(static_cast<const void*>(std::addressof(args)))... };
        for (Subscriber& subscriber : Subscribers) {
            if (subscriber.ArgumentCount == sizeof...(TArgs)) {
                subscriber.Invoke(argumentPointers.data());
            }
        }
    }

private:
    /// <summary>
    /// Stores one type-erased free or static function subscriber.
    /// </summary>
    struct Subscriber {
        /// <summary>
        /// Number of arguments expected by the subscriber.
        /// </summary>
        std::size_t ArgumentCount;

        /// <summary>
        /// Type-erased invocation thunk that reads arguments from the packed invocation array.
        /// </summary>
        std::function<void(void**)> Invoke;
    };

    /// <summary>
    /// Invokes one stored free or static function by unpacking the argument pointer array.
    /// </summary>
    /// <typeparam name="TArgs">Argument shapes supplied by the transpiled event invocation.</typeparam>
    /// <typeparam name="TIndexes">Compile-time argument indexes used to unpack the argument pointer array.</typeparam>
    /// <param name="handler">Free or static function subscriber.</param>
    /// <param name="arguments">Packed addresses of the invocation arguments.</param>
    template <typename... TArgs, std::size_t... TIndexes>
    static void InvokeFunctionPointer(void (*handler)(TArgs...), void** arguments, std::index_sequence<TIndexes...>) {
        handler((*static_cast<std::remove_reference_t<TArgs>*>(arguments[TIndexes]))...);
    }

    /// <summary>
    /// Subscribers currently attached to this event.
    /// </summary>
    std::vector<Subscriber> Subscribers;
};
