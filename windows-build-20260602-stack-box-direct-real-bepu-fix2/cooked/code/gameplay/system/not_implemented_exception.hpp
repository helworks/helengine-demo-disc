#pragma once

#include <stdexcept>

class NotImplementedException : public std::logic_error {
public:
    NotImplementedException()
        : std::logic_error("Not implemented.") {
    }

    explicit NotImplementedException(const std::string& message)
        : std::logic_error(message) {
    }

    explicit NotImplementedException(const char* message)
        : std::logic_error(message == nullptr ? "Not implemented." : message) {
    }
};
