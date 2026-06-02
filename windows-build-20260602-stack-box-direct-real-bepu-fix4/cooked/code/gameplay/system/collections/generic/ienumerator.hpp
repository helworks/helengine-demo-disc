#pragma once

#include "runtime/native_disposable.hpp"

/// <summary>
/// Represents the managed generic IEnumerator contract expected by transpiled collection enumerators.
/// </summary>
template <typename T>
class IEnumerator : public IDisposable {
public:
    /// <summary>
    /// Releases the interface vtable through the expected polymorphic base destructor.
    /// </summary>
    virtual ~IEnumerator() = default;

    /// <summary>
    /// Gets the current element exposed by the active enumerator position.
    /// </summary>
    virtual T get_Current() = 0;

    /// <summary>
    /// Advances the enumerator to the next element in the sequence.
    /// </summary>
    virtual bool MoveNext() = 0;

    /// <summary>
    /// Resets the enumerator to the position before the first element.
    /// </summary>
    virtual void Reset() = 0;
};
