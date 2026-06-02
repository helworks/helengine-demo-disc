#pragma once

#include "runtime/native_datetime.hpp"

namespace System {
namespace Diagnostics {

/// <summary>
/// Provides lightweight managed-style stopwatch timing for player runtime diagnostics.
/// </summary>
class Stopwatch {
public:
    class LiveMilliseconds {
    public:
        LiveMilliseconds()
            : Owner(nullptr) {
        }

        explicit LiveMilliseconds(const Stopwatch* owner)
            : Owner(owner) {
        }

        operator double() const {
            return Owner != nullptr ? Owner->ComputeElapsedMilliseconds() : 0.0;
        }

    private:
        const Stopwatch* Owner;
    };

    class LiveTimeSpan {
    public:
        LiveTimeSpan()
            : TotalMilliseconds(), Owner(nullptr) {
        }

        explicit LiveTimeSpan(const Stopwatch* owner)
            : TotalMilliseconds(owner), Owner(owner) {
        }

        operator TimeSpan() const {
            return Owner != nullptr ? Owner->ComputeElapsed() : TimeSpan();
        }

        LiveMilliseconds TotalMilliseconds;

    private:
        const Stopwatch* Owner;
    };

    /// <summary>
    /// Initializes a new stopwatch in the stopped state.
    /// </summary>
    Stopwatch()
        : Elapsed(this), IsRunningValue(false), StartTime(), TotalElapsedMilliseconds(0.0) {
    }

    /// <summary>
    /// Creates and starts one stopwatch instance in a single call.
    /// </summary>
    /// <returns>Started stopwatch instance.</returns>
    static Stopwatch* StartNew() {
        Stopwatch* stopwatch = new Stopwatch();
        stopwatch->Start();
        return stopwatch;
    }

    /// <summary>
    /// Gets a value indicating whether the stopwatch is currently running.
    /// </summary>
    /// <returns>True when the stopwatch has been started and not stopped yet.</returns>
    bool get_IsRunning() const {
        return IsRunningValue;
    }

    /// <summary>
    /// Starts or resumes the stopwatch.
    /// </summary>
    void Start() {
        if (!IsRunningValue) {
            StartTime = DateTime::Now();
            IsRunningValue = true;
        }
    }

    /// <summary>
    /// Restarts the stopwatch from zero elapsed time.
    /// </summary>
    void Restart() {
        TotalElapsedMilliseconds = 0.0;
        StartTime = DateTime::Now();
        IsRunningValue = true;
    }

    /// <summary>
    /// Stops the stopwatch and freezes the current elapsed time.
    /// </summary>
    void Stop() {
        if (IsRunningValue) {
            TotalElapsedMilliseconds += (DateTime::Now() - StartTime).TotalMilliseconds;
            IsRunningValue = false;
        }
    }

    /// <summary>
    /// Gets the accumulated elapsed time.
    /// </summary>
    /// <returns>Elapsed time as a managed-style duration value.</returns>
    TimeSpan get_Elapsed() {
        return ComputeElapsed();
    }

    /// <summary>
    /// Gets the accumulated elapsed time in a field shape that matches the generated C++ property lowering.
    /// </summary>
    LiveTimeSpan Elapsed;

private:
    /// <summary>
    /// Tracks whether the stopwatch is currently running.
    /// </summary>
    bool IsRunningValue;

    /// <summary>
    /// Captures the instant at which the current running interval started.
    /// </summary>
    DateTime StartTime;

    /// <summary>
    /// Accumulates elapsed time across stopped and running intervals.
    /// </summary>
    double TotalElapsedMilliseconds;

    double ComputeElapsedMilliseconds() const {
        if (IsRunningValue) {
            return TotalElapsedMilliseconds + (DateTime::Now() - StartTime).TotalMilliseconds;
        }

        return TotalElapsedMilliseconds;
    }

    TimeSpan ComputeElapsed() const {
        return TimeSpan(ComputeElapsedMilliseconds());
    }
};

}
}

using Stopwatch = System::Diagnostics::Stopwatch;
