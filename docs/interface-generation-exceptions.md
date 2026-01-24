# Interface Generation: Exceptional Cases

This document summarizes the edge cases and exclusions for AttributedDI's interface
generation. Use it as a checklist when a generated interface doesn't look the way you
expect.

## What is generated

- Public instance methods, properties, events, and indexers declared on the type.
- Generic type parameters and constraints are preserved (including nullability).
- Optional parameters with default values are preserved.

## What is not generated

- Static members and operators.
- Non-public members.
- Members declared on base types (inherited members are ignored).
- Overrides of base class members (treated as inherited).
- Explicit interface implementations.
- Nested types.
- `ref` returns or `ref`/`in`/`out` parameters.
- Members marked with `[ExcludeInterfaceMember]`.
- Members excluded via conditional compilation.

## Naming and namespace rules

- Default interface name is `I{TypeName}` in the same namespace as the target type.
- You can override the name/namespace via `GenerateInterface` or `RegisterAsGeneratedInterface`.
- If `interfaceNamespace` is provided, it overrides any namespace embedded in `interfaceName`.
- If `interfaceName` is fully qualified and `interfaceNamespace` is not provided, the embedded
  namespace is used.
- A leading `global::` prefix in `interfaceName` or `interfaceNamespace` is ignored.

## Reminder

If you want interface generation without registration, use `[GenerateInterface]`. If you want
both interface generation and registration against that interface, use
`[RegisterAsGeneratedInterface]`.
