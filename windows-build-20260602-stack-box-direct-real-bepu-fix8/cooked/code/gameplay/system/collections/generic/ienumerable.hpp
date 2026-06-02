#pragma once

/// <summary>
/// Represents the managed generic IEnumerable contract expected by transpiled collection enumerables.
/// </summary>
template <typename T>
class IEnumerable {
public:
    /// <summary>
    /// Releases the interface vtable through the expected polymorphic base destructor.
    /// </summary>
    virtual ~IEnumerable() = default;
};
