# Compile-Time DI Design

The original draft was split into focused documents to keep each proposal maintainable:

- [Scope](./compile-time-di-scope.md)
- [Option 1: Dictionary-based factory resolution](./compile-time-di-option-1.md)
- [Option 2: Slot-based service resolution](./compile-time-di-option-2.md)
- [Option 3: Generic typed fast path + dictionary fallback](./compile-time-di-option-3.md)
- [Option 4: Instance provider parts with owned caches](./compile-time-di-option-4.md)
- [V2 ideas](./compile-time-di-v2.md)

All code in those files is intentionally pseudocode-level and may omit non-essential plumbing for clarity.
