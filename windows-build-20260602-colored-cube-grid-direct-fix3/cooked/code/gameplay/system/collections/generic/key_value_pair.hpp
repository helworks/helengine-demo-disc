#pragma once

/// <summary>
/// Represents the managed KeyValuePair value contract expected by transpiled dictionary and enumerator payloads.
/// </summary>
template <typename TKey, typename TValue>
class KeyValuePair {
public:
    /// <summary>
    /// Initializes one pair with default-initialized key and value fields.
    /// </summary>
    KeyValuePair()
        : Key(),
          Value() {
    }

    /// <summary>
    /// Initializes one pair with the supplied key and value payloads.
    /// </summary>
    /// <param name="key">Key payload to store.</param>
    /// <param name="value">Value payload to store.</param>
    KeyValuePair(TKey key, TValue value)
        : Key(key),
          Value(value) {
    }

    /// <summary>
    /// Gets the stored key payload.
    /// </summary>
    /// <returns>The stored key.</returns>
    TKey get_Key() const {
        return Key;
    }

    /// <summary>
    /// Gets the stored value payload.
    /// </summary>
    /// <returns>The stored value.</returns>
    TValue get_Value() const {
        return Value;
    }

    /// <summary>
    /// Updates the stored key payload.
    /// </summary>
    /// <param name="value">Replacement key payload.</param>
    void set_Key(TKey value) {
        Key = value;
    }

    /// <summary>
    /// Updates the stored value payload.
    /// </summary>
    /// <param name="value">Replacement value payload.</param>
    void set_Value(TValue value) {
        Value = value;
    }

private:
    TKey Key;
    TValue Value;
};
