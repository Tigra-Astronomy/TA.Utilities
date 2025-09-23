# Octet

An `Octet` is an immutable type that represents 8 bits, or a byte.
In most cases, it can be directly used in place of a `byte` as there are implicit conversions to and from a `byte`.
There are also explicit conversions to and from `int`.
The latter is explicit because there is potentially data loss, so use with care.

Note that conversion from `uint` has been deprecated because `uint` is not CLS Compliant, which can cause issues with other languages.

In an `Octet`, each bit position is directly addressable as an array element.
You can access `octet[0]` through `octet[7]`.

You can set a bit with `octet.WithBitSet(n)`.
You can clear a bit with `octet.WithBitClear(n)`.

Remember `Octet` is _immutable_ so this gives you a new `Octet` and leaves the original unchanged.

You can perform logical bitwise operations using the `&` anf `|` operators.
You can test octets for equality and compare them using `==`, `!=`, `>`, `\<`, etc.
