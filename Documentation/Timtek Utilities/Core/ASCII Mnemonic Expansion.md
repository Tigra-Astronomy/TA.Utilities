# ASCII Mnemonic Expansion

When dealing with streams of ASCII-encoded data, it is often helpful to be able to see non-printing and white space characters.
This is especially useful when logging.
The `ExpandAscii()` extension method makes this simple.
Us `string.ExpansAscii()` and cahacters such as carriage return, for example, will be rendered as `\<CR\>` instead of causing an ugly line break in your log output.

`ExpandAscii()` uses the mnemonics defined in the `AsciiSymbols` enumerated type.
