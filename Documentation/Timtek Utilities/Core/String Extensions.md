# String extensions

Helpers for common string manipulation tasks: filtering characters, extracting head/tail segments, trimming from either end, and rendering strings as hexadecimal. All methods are extension methods on `string` and return new strings (do not mutate the original).

See also
- [[Core/ASCII Mnemonic Expansion|ASCII Mnemonic Expansion]] — show control characters as mnemonics (e.g., \<CR\>)

## Clean: remove unwanted characters

Removes all characters that appear in the supplied `clean` set and preserves the rest.

```csharp
var source = "A-B_C";
var cleaned = source.Clean("-_");   // "ABC"
```

Notes
- If `source` is `null` or empty, returns `string.Empty`.
- If `clean` is `null` or empty, returns `source` unchanged.
- Complexity is O(n × m) where n is the source length and m is `clean.Length`. A HashSet can be faster when `clean` is very large, but for typical small sets the simple approach is competitive.

## Keep: keep only wanted characters

The inverse of Clean. Keeps only characters that appear in the supplied `keep` set and removes all others.

```csharp
var digits = "a1b2c3".Keep("0123456789"); // "123"
```

Notes
- If `source` is `null` or empty, returns `string.Empty`.

## Head: take the first N characters

Returns the first `length` characters of the string.

```csharp
"abcdef".Head(3);     // "abc"
```

Exceptions
- `ArgumentNullException` if `source` is `null`.
- `ArgumentOutOfRangeException` if `length` < 0 or `length` > `source.Length`.

## Tail: take the last N characters

Returns the last `length` characters of the string.

```csharp
"abcdef".Tail(2);     // "ef"
```

Exceptions
- `ArgumentOutOfRangeException` if `length` > `source.Length`.

## RemoveHead: drop N characters from the start

Removes `length` characters from the start; returns the remainder.

```csharp
"abcdef".RemoveHead(2);   // "cdef"
```

Notes
- If `length` < 1, returns `source` unchanged.
- If `length` exceeds the source length, `Tail()` will throw; ensure the value is within range if you do not want an exception.

## RemoveTail: drop N characters from the end

Removes `length` characters from the end; returns the remainder.

```csharp
"abcdef".RemoveTail(2);   // "abcd"
```

Notes
- If `length` < 1, returns `source` unchanged.
- If `length` exceeds the source length, `Head()` will throw; ensure the value is within range if you do not want an exception.

## ToHex: visualise character codes

Renders each character’s numeric value in lower‑case hexadecimal, surrounded by braces.

```csharp
"Az".ToHex();   // "{41, 7a}"
```

Notes
- Useful for diagnostics and debugger output when inspecting non‑printable characters or code points.

## Read lines helpers (related)

There is a companion set of helpers for splitting a string or stream into lines without allocating the entire array up front.

```csharp
// From a string
foreach (var line in bigText.GetLines()) { /* ... */ }

// From a stream
await using var fs = File.OpenRead(path);
foreach (var line in fs.GetLines()) { /* ... */ }
```

Rules
- A line ends at CR (0x0D), LF (0x0A), CRLF, `Environment.NewLine`, or end‑of‑input; terminators are not included in the returned lines.

See also
- [[Core/ASCII Mnemonic Expansion|ASCII Mnemonic Expansion]]
