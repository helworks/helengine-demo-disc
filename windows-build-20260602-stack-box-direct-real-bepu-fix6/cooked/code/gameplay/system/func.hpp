#pragma once

template <typename... TArgs>
class Func {
};

template <typename TResult>
class Func<TResult> {
public:
    using FuncType = TResult(*)();

    Func()
        : func(nullptr) {
    }

    explicit Func(FuncType value)
        : func(value) {
    }

    TResult operator()() const {
        return func();
    }

private:
    FuncType func;
};

template <typename TArg, typename TResult>
class Func<TArg, TResult> {
public:
    using FuncType = TResult(*)(TArg);

    Func()
        : func(nullptr) {
    }

    explicit Func(FuncType value)
        : func(value) {
    }

    TResult operator()(TArg arg) const {
        return func(arg);
    }

private:
    FuncType func;
};
