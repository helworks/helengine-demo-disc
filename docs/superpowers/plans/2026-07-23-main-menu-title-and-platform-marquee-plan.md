# Main Menu Title and Platform Marquee Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Put the fixed two-line Demo Disc title at top-right and append runtime platform/version to the footer marquee.

**Architecture:** Change only the standard-menu factory and footer marquee component; source-contract tests lock the exact authored title and runtime platform concatenation.

**Tech Stack:** C#, HelEngine text components, xUnit.

## Global Constraints

- Standard menu only.
- Use the runtime platform metadata for footer platform/version.
- Keep the footer signature text unchanged before the appended metadata.

### Task 1: Author and test title and footer copy

- [ ] Add a failing source-contract test for `HELENGINE`, `DEMO DISC`, and the footer platform/version concatenation.
- [ ] Change the standard title overlay and footer marquee component.
- [ ] Run the focused test until it passes.
- [ ] Regenerate `DemoDiscMainMenu.helen` and build Windows.
