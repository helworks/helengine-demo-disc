#ifndef ACTION_HPP
#define ACTION_HPP

#include <functional>

template<typename... TArgs>
class Action {
private:
    using FuncType = std::function<void(TArgs...)>;
    FuncType func{};

public:
    Action() = default;

    explicit Action(FuncType f);

    template<typename TCallable>
    explicit Action(TCallable f);

    void operator()(TArgs... args) const;

    explicit operator bool() const;
};

// Include implementation for template functions
#include "action.tpp"

#endif // ACTION_HPP
