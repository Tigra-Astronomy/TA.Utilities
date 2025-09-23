# Maybe\<T\>

One of the most insideous bug producers in .NET code is the null value.
Do you return `null` to mean "no value"?
What's the caller supposed to do with that?
Did you mean there was an error?
Did you mean there wans't an error but you can't give an answer?
Did you mean the answer was empty?
Or did someone just forget to initialize the variable?

I'm not a fan of Microsoft's solution to the Null Reference problem.
I think their _Nullable Reference Types_ [sic] are clumsy and make the code look ugly.
They couldn't even get the name right - reference types were *always* nullable!

The ambiguity around "error" vs. "no value" is why we created `Maybe\<T\>`.

`Maybe\<T\>` is a type that either has a value, or doesn't, but it is never null.
The idea is that by using a `Maybe\<T\>` you clearly communicate your intentions to the caller.
By returning `Maybe\<T\>` you nail down the ambiguity:
"there might not be a value and you have to check".

Strictly, a `Maybe\<T\>` is an `IEnumerable\<T\>` which is either empty (no value) or has exactly one element.
Because it is `IEnumerable` you can use certain LINQ operators:

- `maybe.Any()` will be true if there is a value.
- `maybe.Single()` gets you the value.
- `maybe.SingleOrDefault()` gets you the value or `null`.

Creating a maybe can be done by:

- `object.AsMaybe();` - wrap a non-null object.
- `Maybe<int>.From(7);` - works with value types, and also safe for null references.
- `Maybe\<T\>.Empty` - a maybe without a value.

`Maybe\<T\>` has a `ToString()` method so you can write it to a stream or use in a string interpolation, and you will get the serialized value or "`{no value}`".

Try returning a `Maybe\<T\>` whenever you have a situation wehere there may be no value, instead of `null`.
You may find that your bug count tends to diminish.
