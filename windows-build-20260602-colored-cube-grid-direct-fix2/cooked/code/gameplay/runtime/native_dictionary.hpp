#pragma once

#include <cstddef>
#include <cstdint>
#include <functional>
#include <type_traits>
#include <unordered_map>
#include <vector>

#include "native_string.hpp"

class StringComparer;

template<typename TKey>
class NativeDictionaryHash {
public:
    std::size_t operator()(const TKey& key) const {
        if constexpr (std::is_pointer_v<TKey>) {
            return std::hash<TKey>{}(key);
        } else if constexpr (requires(TKey value) { value.GetHashCode(); }) {
            return static_cast<std::size_t>(const_cast<TKey&>(key).GetHashCode());
        } else {
            return std::hash<TKey>{}(key);
        }
    }
};

template<typename TKey>
class NativeDictionaryEqual {
public:
    bool operator()(const TKey& left, const TKey& right) const {
        if constexpr (std::is_pointer_v<TKey>) {
            return left == right;
        } else if constexpr (requires(TKey value) { value.Equals(right); }) {
            return const_cast<TKey&>(left).Equals(right);
        } else {
            return left == right;
        }
    }
};

template<typename TKey, typename TValue>
class Dictionary : public std::unordered_map<TKey, TValue, NativeDictionaryHash<TKey>, NativeDictionaryEqual<TKey>> {
public:
    using std::unordered_map<TKey, TValue, NativeDictionaryHash<TKey>, NativeDictionaryEqual<TKey>>::unordered_map;

    explicit Dictionary(const StringComparer&) {
    }

    ~Dictionary() {
        this->clear();
    }

    void Add(const TKey& key, const TValue& value) {
        this->insert_or_assign(key, value);
    }

    TValue& get_Item(const TKey& key) {
        return (*this)[key];
    }

    const TValue& get_Item(const TKey& key) const {
        return this->at(key);
    }

    void set_Item(const TKey& key, const TValue& value) {
        this->insert_or_assign(key, value);
    }

    bool ContainsKey(const TKey& key) const {
        return this->find(key) != this->end();
    }

    bool Remove(const TKey& key) {
        return this->erase(key) > 0;
    }

    void Clear() {
        this->clear();
    }

    bool TryGetValue(const TKey& key, TValue& value) const {
        auto iterator = this->find(key);
        if (iterator == this->end()) {
            return false;
        }

        value = iterator->second;
        return true;
    }

    std::vector<TKey> Keys() const {
        std::vector<TKey> keys;
        keys.reserve(this->size());
        for (const auto& pair : *this) {
            keys.push_back(pair.first);
        }

        return keys;
    }

    int32_t Count() const {
        return static_cast<int32_t>(this->size());
    }

    int32_t get_Count() const {
        return Count();
    }
};
