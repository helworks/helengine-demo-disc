#ifndef HE_CPP_SYSTEM_THREADING_AUTO_RESET_EVENT_HPP
#define HE_CPP_SYSTEM_THREADING_AUTO_RESET_EVENT_HPP

#include <condition_variable>
#include <cstdint>
#include <mutex>

class AutoResetEvent {
public:
    explicit AutoResetEvent(bool initialState = false)
        : signaled(initialState) {
    }

    void Set() {
        {
            std::lock_guard<std::mutex> lock(mutex);
            signaled = true;
        }

        condition.notify_one();
    }

    void WaitOne() {
        std::unique_lock<std::mutex> lock(mutex);
        condition.wait(lock, [this]() {
            return signaled;
        });
        signaled = false;
    }

    void Dispose() {
    }

private:
    std::condition_variable condition;
    std::mutex mutex;
    bool signaled;
};

#endif
