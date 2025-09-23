# Math extensions

Utility methods for simple mathematical operations not provided by the BCL.

## Clip<T>(minimum, maximum)

Constrains a comparable value to a closed interval [minimum, maximum]. Works with any `T : IComparable`.

```csharp
5.2.Clip(0.0, 5.0);   // 5.0
(-1).Clip(0, 10);      // 0
'c'.Clip('a', 'b');    // 'b'
```

Notes
- If `input > maximum`, returns `maximum`; if `input < minimum`, returns `minimum`; otherwise returns `input`.
- For reference types that implement `IComparable`, semantics follow the type’s comparison implementation.
