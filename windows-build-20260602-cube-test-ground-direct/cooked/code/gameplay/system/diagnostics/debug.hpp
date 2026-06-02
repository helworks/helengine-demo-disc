#pragma once

#include <stdexcept>

#include "runtime/native_string.hpp"

namespace System {
namespace Diagnostics {
class Debug {
public:
    static void Assert(bool condition) {
        (void)condition;
    }

    static void Assert(bool condition, const std::string& message) {
        (void)condition;
        (void)message;
    }

    static void WriteLine(const std::string& text) {
        (void)text;
    }

    static void Fail(const std::string& message) {
        throw std::runtime_error(message);
    }
};
}
}
