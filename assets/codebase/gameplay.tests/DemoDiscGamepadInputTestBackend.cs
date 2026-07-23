using helengine;

namespace city.tests {
    /// <summary>
    /// Supplies deterministic gamepad frames to aggregation tests without requiring a platform backend.
    /// </summary>
    public sealed class DemoDiscGamepadInputTestBackend : IInputBackend {
        /// <summary>
        /// Frame returned when the queued test sequence is exhausted.
        /// </summary>
        InputFrameState LastFrame;

        /// <summary>
        /// Initializes an empty deterministic backend.
        /// </summary>
        public DemoDiscGamepadInputTestBackend() {
            Frames = new Queue<InputFrameState>();
        }

        /// <summary>
        /// Gets the queued frames that will be returned in capture order.
        /// </summary>
        Queue<InputFrameState> Frames { get; }

        /// <summary>
        /// Gets or sets whether the deterministic backend accepts background input.
        /// </summary>
        public bool ReceiveInputInBackground { get; set; }

        /// <summary>
        /// Adds one frame to the deterministic capture sequence.
        /// </summary>
        /// <param name="frame">Frame to return from a future capture.</param>
        public void Enqueue(InputFrameState frame) {
            Frames.Enqueue(frame);
            LastFrame = frame;
        }

        /// <summary>
        /// Returns the next queued frame, retaining the last frame after the sequence ends.
        /// </summary>
        /// <returns>Next deterministic input frame.</returns>
        public InputFrameState CaptureFrame() {
            if (Frames.Count > 0) {
                LastFrame = Frames.Dequeue();
            }

            return LastFrame;
        }
    }
}
