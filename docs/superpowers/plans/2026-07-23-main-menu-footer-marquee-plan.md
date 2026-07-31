# Main Menu Footer Marquee Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Scroll the footer identity text steadily from right to left inside the clipped footer strip.

**Architecture:** Add one update component to the footer surface. It owns the text reference and resets the text position once it has fully left the strip; the strip itself supplies the clip boundary.

### Task 1: Author and verify the marquee

- [ ] Add a focused test for a 35px/second leftward marquee with right-edge restart.
- [ ] Create a footer-local update component with serialized references to the strip and text entities.
- [ ] Add a clip rectangle to the footer strip and attach the component through the standard menu factory.
- [ ] Regenerate the standard scene, build Windows, and test the marquee in the packaged player.
