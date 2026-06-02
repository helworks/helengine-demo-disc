#ifndef HE_CPP_SYSTEM_THREADING_SPIN_WAIT_HPP
#define HE_CPP_SYSTEM_THREADING_SPIN_WAIT_HPP

#include <chrono>
#include <cstdint>
#include <thread>

class SpinWait {
public:
    SpinWait() = default;

    void Reset() {
        count = 0;
    }

    void SpinOnce(int32_t sleep1Threshold = -1) {
        if (sleep1Threshold >= 0 && count >= sleep1Threshold) {
            std::this_thread::sleep_for(std::chrono::milliseconds(1));
        } else {
            std::this_thread::yield();
        }

        ++count;
    }
private:
    int32_t count = 0;
};

#endif
