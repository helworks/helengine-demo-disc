#pragma once

#include <type_traits>

#include "runtime/native_span.hpp"

/// <summary>
/// Provides a portable subset of System.Runtime.InteropServices.MemoryMarshal for span reinterpretation scenarios.
/// </summary>
class MemoryMarshal {
public:
    template <typename TTo, typename TFrom>
    static Span<TTo> Cast(Span<TFrom> source) {
        static_assert(
            he_span_managed_size_v<TFrom> == he_span_managed_size_v<TTo>,
            "Portable MemoryMarshal::Cast only supports element types with matching managed storage sizes.");
        return Span<TTo>(source);
    }

};
