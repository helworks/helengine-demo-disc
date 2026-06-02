#pragma once

#include <algorithm>
#include <cstdint>
#include <initializer_list>

template<typename T>
class Array {
public:
    int32_t Length;
    T* Data;

    Array()
        : Length(0), Data(nullptr) {
    }

    explicit Array(int32_t length)
        : Length(length), Data(length > 0 ? new T[length]() : nullptr) {
    }

    Array(std::initializer_list<T> values)
        : Length(static_cast<int32_t>(values.size())), Data(values.size() > 0 ? new T[values.size()]() : nullptr) {
        int32_t index = 0;
        for (const T& value : values) {
            Data[index++] = value;
        }
    }

    ~Array() {
        delete[] Data;
        Data = nullptr;
        Length = 0;
    }

    static Array<T>* Empty() {
        static Array<T> EmptyArray(0);
        return &EmptyArray;
    }

    static void Copy(const Array<T>* source, Array<T>* destination, int32_t length) {
        if (source == nullptr || destination == nullptr || length <= 0) {
            return;
        }

        int32_t copyLength = std::min(length, std::min(source->Length, destination->Length));
        for (int32_t index = 0; index < copyLength; index++) {
            (*destination)[index] = (*source)[index];
        }
    }

    static void Copy(const Array<T>* source, int32_t sourceIndex, Array<T>* destination, int32_t destinationIndex, int32_t length) {
        if (source == nullptr || destination == nullptr || length <= 0) {
            return;
        }

        if (sourceIndex < 0) {
            sourceIndex = 0;
        }

        if (destinationIndex < 0) {
            destinationIndex = 0;
        }

        if (sourceIndex >= source->Length || destinationIndex >= destination->Length) {
            return;
        }

        int32_t sourceAvailable = source->Length - sourceIndex;
        int32_t destinationAvailable = destination->Length - destinationIndex;
        int32_t copyLength = std::min(length, std::min(sourceAvailable, destinationAvailable));
        if (copyLength <= 0) {
            return;
        }

        if (source == destination && destinationIndex > sourceIndex) {
            for (int32_t index = copyLength - 1; index >= 0; --index) {
                (*destination)[destinationIndex + index] = (*source)[sourceIndex + index];
            }
            return;
        }

        for (int32_t index = 0; index < copyLength; ++index) {
            (*destination)[destinationIndex + index] = (*source)[sourceIndex + index];
        }
    }

    static void Resize(Array<T>*& array, int32_t newLength) {
        if (newLength < 0) {
            newLength = 0;
        }

        Array<T>* resized = new Array<T>(newLength);
        if (array != nullptr) {
            Copy(array, resized, std::min(array->Length, newLength));
        }

        if (array != nullptr && array != Empty()) {
            delete array;
        }

        array = resized;
    }

    static void Clear(Array<T>* array, int32_t index, int32_t length) {
        if (array == nullptr || array->Data == nullptr || length <= 0) {
            return;
        }

        if (index < 0) {
            index = 0;
        }

        if (index >= array->Length) {
            return;
        }

        int32_t clearLength = std::min(length, array->Length - index);
        for (int32_t elementIndex = 0; elementIndex < clearLength; ++elementIndex) {
            array->Data[index + elementIndex] = T();
        }
    }

    int32_t get_Length() const {
        return Length;
    }

    T& operator[](int32_t index) {
        return Data[index];
    }

    const T& operator[](int32_t index) const {
        return Data[index];
    }

    T* begin() {
        return Data;
    }

    T* end() {
        return Data + Length;
    }

    const T* begin() const {
        return Data;
    }

    const T* end() const {
        return Data + Length;
    }
};

template<typename T>
T* begin(Array<T>* values) {
    return values == nullptr ? nullptr : values->begin();
}

template<typename T>
T* end(Array<T>* values) {
    return values == nullptr ? nullptr : values->end();
}

template<typename T>
const T* begin(const Array<T>* values) {
    return values == nullptr ? nullptr : values->begin();
}

template<typename T>
const T* end(const Array<T>* values) {
    return values == nullptr ? nullptr : values->end();
}
