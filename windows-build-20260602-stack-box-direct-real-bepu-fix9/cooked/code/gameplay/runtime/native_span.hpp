#pragma once

#include <algorithm>
#include <cstddef>
#include <cstdint>
#include <cstring>
#include <initializer_list>
#include <memory>
#include <type_traits>
#include <utility>
#include "array.hpp"

template <typename T, typename = void>
struct he_span_has_value_member : std::false_type {
};

template <typename T>
struct he_span_has_value_member<T, std::void_t<decltype(std::declval<T&>().Value)>> : std::true_type {
};

template <typename T>
using he_span_value_member_t = decltype(std::declval<T&>().Value);

template <typename T, typename = void>
struct he_span_managed_size {
    static constexpr size_t Value = sizeof(T);
};

template <typename T>
struct he_span_managed_size<T, std::enable_if_t<he_span_has_value_member<T>::value>> {
    static constexpr size_t Value = sizeof(std::remove_reference_t<he_span_value_member_t<T>>);
};

template <typename T>
constexpr size_t he_span_managed_size_v = he_span_managed_size<T>::Value;

template <typename TTo, typename TFrom>
TTo he_span_convert_value(const TFrom& value) {
    if constexpr (std::is_same_v<TTo, TFrom>) {
        return value;
    } else if constexpr (std::is_constructible_v<TTo, TFrom>) {
        return TTo(value);
    } else if constexpr (he_span_has_value_member<TFrom>::value &&
                         std::is_convertible_v<he_span_value_member_t<TFrom>, TTo>) {
        return static_cast<TTo>(value.Value);
    } else if constexpr (he_span_has_value_member<TTo>::value) {
        TTo result{};
        result.Value = static_cast<std::remove_reference_t<he_span_value_member_t<TTo>>>(value);
        return result;
    } else if constexpr (sizeof(TTo) == sizeof(TFrom) &&
                         std::is_trivially_copyable_v<TTo> &&
                         std::is_trivially_copyable_v<TFrom>) {
        TTo result;
        std::memcpy(&result, &value, sizeof(TTo));
        return result;
    } else {
        static_assert(sizeof(TTo) == 0, "Unsupported span element conversion.");
    }
}

template <typename T>
class Span {
public:
    template <typename>
    friend class ReadOnlySpan;

    T* Data;
    size_t Length;
    std::shared_ptr<void> Owner;

    Span()
        : Data(nullptr),
          Length(0),
          Owner() {
    }

    Span(T* data, size_t length)
        : Data(data),
          Length(length),
          Owner() {
    }

    Span(T* data, size_t start, size_t length)
        : Data(data + start),
          Length(length),
          Owner() {
    }

    Span(T& item)
        : Data(&item),
          Length(1),
          Owner() {
    }

    Span(Array<T>* array)
        : Data(array != nullptr ? array->Data : nullptr),
          Length(array != nullptr ? static_cast<size_t>(array->Length) : 0),
          Owner() {
    }

    Span(Array<T>* array, size_t start, size_t length)
        : Data(array != nullptr ? array->Data + start : nullptr),
          Length(length),
          Owner() {
    }

    template <typename TBuffer, typename = decltype(std::declval<TBuffer>().Memory, std::declval<TBuffer>().get_Length())>
    Span(TBuffer buffer)
        : Data(buffer.Memory),
          Length(static_cast<size_t>(buffer.get_Length())),
          Owner() {
    }

    template <typename U, typename = std::enable_if_t<!std::is_same_v<T, U>>>
    Span(U* data, size_t length)
        : Data(reinterpret_cast<T*>(data)),
          Length(length),
          Owner() {
    }

    template <typename U, typename = std::enable_if_t<!std::is_same_v<T, U>>>
    Span(const Span<U>& span)
        : Data(nullptr),
          Length(span.Length),
          Owner() {
        InitializeOwnedBuffer(span.Data, span.Length);
    }

    template <size_t N>
    Span(T (&buffer)[N])
        : Data(buffer),
          Length(N),
          Owner() {
    }

    Span(std::initializer_list<T> values)
        : Data(nullptr),
          Length(values.size()),
          Owner() {
        InitializeOwnedBuffer(values.begin(), values.size());
    }

    Span Slice(size_t offset) const {
        Span slice(Data + offset, Length - offset);
        slice.Owner = Owner;
        return slice;
    }

    Span Slice(size_t offset, size_t length) const {
        Span slice(Data + offset, length);
        slice.Owner = Owner;
        return slice;
    }

    void CopyTo(Span<T> target) const {
        std::copy_n(Data, std::min(Length, target.Length), target.Data);
    }

    void Fill(const T& value) const {
        std::fill_n(Data, Length, value);
    }

    int32_t get_Length() const {
        return static_cast<int32_t>(Length);
    }

    T& get_Item(int32_t index) const {
        return Data[index];
    }

    T& operator[](size_t index) const {
        return Data[index];
    }

private:
    template <typename U>
    void InitializeOwnedBuffer(U* source, size_t length) {
        if (source == nullptr || length == 0) {
            Data = nullptr;
            Length = 0;
            Owner.reset();
            return;
        }

        T* ownedData = new T[length];
        Owner = std::shared_ptr<void>(
            ownedData,
            [](void* pointer) {
                delete[] static_cast<T*>(pointer);
            });
        Data = ownedData;

        for (size_t index = 0; index < length; ++index) {
            Data[index] = he_span_convert_value<T>(source[index]);
        }
    }
};

template <typename T>
class ReadOnlySpan {
public:
    const T* Data;
    size_t Length;
    std::shared_ptr<void> Owner;

    ReadOnlySpan()
        : Data(nullptr),
          Length(0),
          Owner() {
    }

    ReadOnlySpan(const T* data, size_t length)
        : Data(data),
          Length(length),
          Owner() {
    }

    ReadOnlySpan(const T* data, size_t start, size_t length)
        : Data(data + start),
          Length(length),
          Owner() {
    }

    ReadOnlySpan(const T& item)
        : Data(&item),
          Length(1),
          Owner() {
    }

    ReadOnlySpan(Array<T>* array)
        : Data(array != nullptr ? array->Data : nullptr),
          Length(array != nullptr ? static_cast<size_t>(array->Length) : 0),
          Owner() {
    }

    ReadOnlySpan(Array<T>* array, size_t start, size_t length)
        : Data(array != nullptr ? array->Data + start : nullptr),
          Length(length),
          Owner() {
    }

    template <typename TBuffer, typename = decltype(std::declval<TBuffer>().Memory, std::declval<TBuffer>().get_Length())>
    ReadOnlySpan(TBuffer buffer)
        : Data(buffer.Memory),
          Length(static_cast<size_t>(buffer.get_Length())),
          Owner() {
    }

    ReadOnlySpan(Span<T> span)
        : Data(span.Data),
          Length(span.Length),
          Owner(span.Owner) {
    }

    template <typename U, typename = std::enable_if_t<!std::is_same_v<T, U>>>
    ReadOnlySpan(const U* data, size_t length)
        : Data(reinterpret_cast<const T*>(data)),
          Length(length),
          Owner() {
    }

    template <typename U, typename = std::enable_if_t<!std::is_same_v<T, U>>>
    ReadOnlySpan(const Span<U>& span)
        : Data(nullptr),
          Length(span.Length),
          Owner() {
        InitializeOwnedBuffer(span.Data, span.Length);
    }

    template <size_t N>
    ReadOnlySpan(const T (&buffer)[N])
        : Data(buffer),
          Length(N),
          Owner() {
    }

    ReadOnlySpan(std::initializer_list<T> values)
        : Data(nullptr),
          Length(values.size()),
          Owner() {
        InitializeOwnedBuffer(values.begin(), values.size());
    }

    ReadOnlySpan Slice(size_t offset) const {
        ReadOnlySpan slice(Data + offset, Length - offset);
        slice.Owner = Owner;
        return slice;
    }

    ReadOnlySpan Slice(size_t offset, size_t length) const {
        ReadOnlySpan slice(Data + offset, length);
        slice.Owner = Owner;
        return slice;
    }

    void CopyTo(Span<T> target) const {
        std::copy_n(Data, std::min(Length, target.Length), target.Data);
    }

    Array<T>* ToArray() const {
        Array<T>* copy = new Array<T>(static_cast<int32_t>(Length));
        for (size_t index = 0; index < Length; ++index) {
            (*copy)[static_cast<int32_t>(index)] = Data[index];
        }

        return copy;
    }

    int32_t get_Length() const {
        return static_cast<int32_t>(Length);
    }

    const T& get_Item(int32_t index) const {
        return Data[index];
    }

    const T& operator[](size_t index) const {
        return Data[index];
    }

private:
    template <typename U>
    void InitializeOwnedBuffer(const U* source, size_t length) {
        if (source == nullptr || length == 0) {
            Data = nullptr;
            Length = 0;
            Owner.reset();
            return;
        }

        T* ownedData = new T[length];
        Owner = std::shared_ptr<void>(
            ownedData,
            [](void* pointer) {
                delete[] static_cast<T*>(pointer);
            });
        Data = ownedData;

        for (size_t index = 0; index < length; ++index) {
            ownedData[index] = he_span_convert_value<T>(source[index]);
        }
    }
};
