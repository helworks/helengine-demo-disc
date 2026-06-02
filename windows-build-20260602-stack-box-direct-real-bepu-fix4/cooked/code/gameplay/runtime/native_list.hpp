#pragma once

#include <algorithm>
#include <cstdint>
#include <string>
#include <type_traits>
#include <vector>

#include "array.hpp"
#include "native_string.hpp"

template<typename T>
class NativeListEqual {
public:
    bool operator()(const T& left, const T& right) const {
        if constexpr (std::is_pointer_v<T>) {
            return left == right;
        } else if constexpr (requires(T value) { value.Equals(right); }) {
            return const_cast<T&>(left).Equals(right);
        } else {
            return left == right;
        }
    }
};

template<typename T>
class List : public std::vector<T> {
public:
    List()
        : std::vector<T>() {
    }

    explicit List(int32_t capacity)
        : std::vector<T>() {
        if (capacity > 0) {
            this->reserve(static_cast<size_t>(capacity));
        }
    }

    List(std::initializer_list<T> values)
        : std::vector<T>(values) {
    }

    explicit List(const std::vector<T>& values)
        : std::vector<T>(values) {
    }

    explicit List(const Array<T>* values) {
        if (values == nullptr || values->Length <= 0 || values->Data == nullptr) {
            return;
        }

        this->reserve(values->Length);
        for (int32_t index = 0; index < values->Length; index++) {
            this->push_back((*values)[index]);
        }
    }

    void Add(const T& value) {
        this->push_back(value);
    }

    void Clear() {
        this->clear();
    }

    bool Contains(const T& value) const {
        NativeListEqual<T> equal;
        return std::find_if(this->begin(), this->end(), [&](const T& candidate) { return equal(candidate, value); }) != this->end();
    }

    bool Remove(const T& value) {
        NativeListEqual<T> equal;
        typename std::vector<T>::iterator iterator = std::find_if(this->begin(), this->end(), [&](const T& candidate) { return equal(candidate, value); });
        if (iterator == this->end()) {
            return false;
        }

        this->erase(iterator);
        return true;
    }

    int32_t Count() const {
        return static_cast<int32_t>(this->size());
    }

    T& get_Item(int32_t index) {
        return (*this)[static_cast<size_t>(index)];
    }

    const T& get_Item(int32_t index) const {
        return (*this)[static_cast<size_t>(index)];
    }

    void set_Item(int32_t index, const T& value) {
        (*this)[static_cast<size_t>(index)] = value;
    }

    int32_t get_Count() const {
        return Count();
    }

    int32_t Capacity() const {
        return static_cast<int32_t>(this->capacity());
    }

    int32_t get_Capacity() const {
        return Capacity();
    }

    void SetCapacity(int32_t capacity) {
        if (capacity <= 0) {
            return;
        }

        if (capacity > Capacity()) {
            this->reserve(static_cast<size_t>(capacity));
        }
    }

    void set_Capacity(int32_t capacity) {
        SetCapacity(capacity);
    }

    void Insert(int32_t index, const T& value) {
        if (index < 0) {
            index = 0;
        }

        if (index >= Count()) {
            this->push_back(value);
            return;
        }

        this->insert(this->begin() + index, value);
    }

    void RemoveAt(int32_t index) {
        if (index < 0 || index >= Count()) {
            return;
        }

        this->erase(this->begin() + index);
    }

    Array<T>* ToArray() const {
        Array<T>* values = new Array<T>(Count());
        for (int32_t index = 0; index < values->Length; index++) {
            (*values)[index] = (*this)[index];
        }

        return values;
    }
};
