#ifndef HE_CPP_SYSTEM_DELEGATE_HPP
#define HE_CPP_SYSTEM_DELEGATE_HPP

#include <functional>
#include <utility>

template <typename TResult, typename... TArgs>
class Delegate {
public:
    using FuncType = std::function<TResult(TArgs...)>;

    Delegate() = default;

    explicit Delegate(FuncType value)
        : func(std::move(value)) {
    }

    template <typename TCallable>
    explicit Delegate(TCallable value)
        : func(std::move(value)) {
    }

    TResult operator()(TArgs... args) const {
        return func(std::forward<TArgs>(args)...);
    }

    explicit operator bool() const {
        return static_cast<bool>(func);
    }
private:
    FuncType func{};
};

#endif
