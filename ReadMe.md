# Tigra Astronomy Commonly Used Helpers and Utilities #

This library represents a collection of classes factored out of our production projects, that we found were being used over and over again.
Rather than re-using the code at source level, it is now collected together in this package as a general purpose reusable library and made freely available for you to use.

This was always the promise of _Object Oriented Design_, but it was not until the advent of [NuGet][nuget] and its widespread adoption that this became a practical reality.
It is easy to overlook the impact of [NuGet][nuget], as it seems so obvious and natural once you've used it.

> Dependency management is the key challenge in software at every scale  
> **Donald Knuth**, _The Art of Computer Programming_

NuGet has essentially solved a large chunk of the dependency management problem.
At Tigra Astronomy, we use NuGet it as a key component in our software design strategy.
We publish our open source code on a [public MyGet feed][myget].
We push both prerelease and release versions to [MyGet][myget].
When we make an official release, we promote that package from [MyGet][myget] to [NuGet][nuget].
You can consume our packages from either location, but if you want betas and release candidates, then you'll need to use [our MyGet feed][myget].

## Licensing ##

This software is released under the [Tigra MIT License][mit], which (in summary) means:
"Anyone can do anything at all with this software without limitation, but it's not our fault if anything goes wrong".

Our [philosophy of open source][yt-oss] is to [give wholeheartedly with no strings attached][yt-oss].
We have no time for "copyleft" licenses which we find irksome.

So here it is, for you to use however you like, no strings attached.

I tend to use "we" and "our" when talking about the company, but Tigra Astronomy is a one-man operation run by me, Tim Long.
I hope you find the software useful, and if you feel that my efforts are worth supporting, then it would make my day if you would [buying me some coffee][coffee].
I also wouldn't mind you giving us a mention, if you feel you are able to, as it helps the company grow. Donations and mentions really make a difference, so please think about it and do what you can.

If you are a company and need some work done, then consider hiring me as a freelance developer. I have decades of experience in product design, firmware development for embedded systems and PC driver and software development. I'm a professional; I believe in doing what's right, not what's expedient and I support my software.

## Description of Classes ##

### Versioning ###

Tigra Astronomy has settled on a versioning strategy based on [Semantic Versioning 2.0.0][semver].

We give all of our software a semantic version, which we display to the user in the About box and write out to log files on startup.
We use [GitVersion][gitversion] to [automatically assign a version number to every build][yt-gitversion] (even in [Arduino projects][yt-gitversion-arduino]).
We never manually set the version number, it happens as part of the build process.
So we can never forget to "bump the version" and we can never forget to set it.
Total automation.
If you examine one of our log files, you may well find something like this:

``` lang=log
21:16:59.2909|INFO |Server          |Git Commit ID: "229c1acc4a7bda494f78a8c7cc811c2a4d8e9132"
21:16:59.3069|INFO |Server          |Git Short ID: "229c1ac"
21:16:59.3069|INFO |Server          |Commit Date: "2020-07-11"
21:16:59.3069|INFO |Server          |Semantic version: "0.1.0-alpha.1"
21:16:59.3069|INFO |Server          |Full Semantic version: "0.1.0-alpha.1"
21:16:59.3069|INFO |Server          |Build metadata: "Branch.develop.Sha.229c1acc4a7bda494f78a8c7cc811c2a4d8e9132"
21:16:59.3069|INFO |Server          |Informational Version: "0.1.0-alpha.1+Branch.develop.Sha.229c1acc4a7bda494f78a8c7cc811c2a4d8e9132"
```

There's no mistaking where that build came from.

Our `GitVersion` class contains static properties for getting your semantic version metadata at runtime. We use it to write the log entries as shown above.

### Readability and Intention-revealing Code ###

#### Maybe? ####

One of the most insideous bug producers in .NET code is the null value.
Do you return `null` to mean "no value"?
What's the caller supposed to do with that?
Did you mean there was an error?
Did you mean there wans't an error but you can't give an answer?
Did you mean the answer was empty?
Or did someone just forget to initialize the variable?

The ambiguity around "error" vs. "no value" is why we created `Maybe<T>`.

`Maybe<T>` is a type that either has a value, or doesn't, but it is never null.
The idea is that by using a `Maybe<T>` you clearly communicate your intentions to the caller.
By returning `Maybe<T>` you nail down the ambiguity: "there might not be a value and you have to check".

Strictly, a `Maybe<T>` is an `IEnumerable<T>` which is either empty (no value) or has exactly one element.
Because it is `IEnumerable` you can use certain LINQ operators:

- `maybe.Any()` will be true if there is a value.
- `maybe.Single()` gets you the value.
- `maybe.SingleOrDefault()` gets you the value or `null`.
- extension method `maybe.None()` is `true` if there's no value.

Creating a maybe can be done by:

- `object.AsMaybe();` - convert a reference type.
- `Maybe<int>.From(7);` - works with value types.
- `Maybe<T>.Empty` - a maybe without a value.

`Maybe<T>` has a `ToString()` method so you can write it to a stream or use in a string interpolation, and you will get the serialized value or "`{no value}`".

Try returning a `Maybe<T>` whenever you have a situation wehere there may be no value, instead of `null`.
You may find that your bug count tends to diminish.

### Bit Manipulation ###

#### Octet ####

An `Octet` is an immutable type that represents 8 bits, or a byte.
In most cases, it can be directly used in place of a `byte` as there are implicit conversions to and from a `byte`.
There are also explicit conversions to and from `int` and `uint`.
The latter are explicit because there is potentially data loss, so use with care.

In an `Octet`, each bit position is directly addressable.
You can access `octet[0]` through `octet[7]`.

You can set a bit with `octet.WithBitSetTo(n, state)`.
Remember `Octet` is immutable so this gives you a new `Octet` and leaves the original unchanged.

You can perform logical bitwise operations using the `&` anf `|` operators.

### Diagnostics ###

#### ASCII Mnemonic Expansion ####

When dealing with streams of ASCII-encoded data, it is often helpful to be able to see non-printing and white space characters.
This is especially useful when logging.
The `ExpandAscii()` extension method makes this simple.
Us `string.ExpansAscii()` and cahacters such as carriage return, for example, will be rendered as `<CR>` instead of causing an ugly line break in your log output.

`ExpandAscii()` uses the mnemonics defined in the `AsciiSymbols` enumerated type.

#### Display Equivalence for Enumerated Types ####

The `[DisplayEquivalent("text")]` Attribute works with the `EnumExtensions.DisplayEquivalent()` extension method.
This can be useful for building drop-down lists and Combo box contents for enumerated types.
You can always get the equivalent human-readable display text for an enumerated value using `value.DisplayEquivalent()`.
This will return the display text if it has been set, or the name of the enum value otherwise.
Set the display text by dropping a `[DisplayEquivalent("text")]` attribute on each field of the enum.

### Asynchrony and Threading ###

#### ConfigureAwait ####

There is an extension method in .NET used to configure awaitable tasks, called `ConfigureAwait(bool)`.
THe method affects how the task awaiter schedules its continuation.
With `ConfigureAwait(true)` the tasks continues on the current synchronization context.
That usually means on the same thread, and is particularly relevant when the awaiter is a user interface thread.
Conversely, `ConfigureAwait(false)` means that continuation can happen on any thread, and usually that will be a thread pool worker thread.
The implications are quite profound. Consider the following method:

``` lang=cs
public async Task SomeMethod()
    {
	Console.WriteLine("Starting on thread {0}", Thread.CurrentThread.ManagedThreadId);
	await Task.Delay(1000).ConfigureAwait(false);
	Console.WriteLine("Continuing on thread {0}", Thread.CurrentThread.ManagedThreadId);
    }
```

When you run this, you may get something like

> Starting on thread 14  
> Continuing on thread 11

But it is not at all ovious how `ConfigureAwait()` should be used.
What if you don't specifiy?
Is the await configured or unconfigured?
Does `ConfigureAwait(false)` mean you don't want to configure it, or that you want to configure it not to do something?
It's just horrible, you can't read the code and instantly understand what it does, and that violates the _Principle of Least Astonishment_.

So we made some extension methods that essentially do the same thing, but make more sense. Our aync method now becomes:

``` lang=cs
public async Task SomeMethod()
    {
	Console.WriteLine("Starting on thread {0}", Thread.CurrentThread.ManagedThreadId);
	await Task.Delay(1000).ContinueOnAnyThread();
	Console.WriteLine("Continuing on thread {0}", Thread.CurrentThread.ManagedThreadId);
    }
```

and we get

> Starting on thread 15  
> Continuing on thread 13

Alternatively:

``` lang=cs
public async Task SomeMethod()
    {
	Console.WriteLine("Starting on thread {0}", Thread.CurrentThread.ManagedThreadId);
	await Task.Delay(1000).ContinueInCurrentContext();
	Console.WriteLine("Continuing on thread {0}", Thread.CurrentThread.ManagedThreadId);
    }
```

The await captures the current `SynchronizationContext` and uses it to schedule the continuation.
What happens next depends on the application model and how it implements `SynchronizationContext`.
For a user interface application, the UI generally runs in a Single Threaded Apartment (STA thread).
In this model, asynchronous operations are posted to the message queue of the STA thread.
The continuation will then happen on the UI thread once teh thread is idel and the message pump runs.
In a free-threaded application model such as a console application, the continuation will likely
still happen on a different thread.

Here you can see the danger of this option.
If the continuation is queued in the message queue waiting for messages to be pumped, but the UI is blocked waiting for the task to complete, then the continuation may never get to run.
The task is prevented from completing and we are in deadlock.
Therefore, best practice for library writers is to always use `ContinueOnAnyThread()`.

#### Cancel Culture ####

One final extension method is `Task.WithCancellation(token)`.
This takes a task that is not cancellable and adds cancellation to it.
Note that this doesn't stop the task from running and it may still run to completion,
but it means you don't have to wait for it.

#### Logging ####

The `TA.Utils.Core.Diagnostics` namespace defines a pair of interfaces, `ILog` and `IFluentLogBuilder`, that define an abstract logging service.

Libraries can perform logging through these interfaces without ever taking a dependency on any logging imnplementation.
The actual implementation can be injected at runtime, typically in a constructor parameter.
The policy decision about which logging engine to use can be taken in the top level composition root of the application.

The fluent interface defined in `IFluentLogBuilder` was modeled on the NLog fluent interface, so it is a very natural fit.
However, the interface has enough flexibility to adapt to other logging backends without too much trouble.]

A null implementation is provided in `DegenerateLoggerService` and `DegenerateLogBuilder`.
The two classes do essentially nothing and produce no output; they are a data sink.
Libraries can choose to use this as their default logging implementation, which is easier than checking
whether the logger is null every time it is used.

The interface supports semantic logging. You can use a simple format string like so:

``` lang=cs
log.Info().Message("Sending data {0}", data).Write();
log.Error().Message("Exception {0} occurred with error code {1}", ex.Message, errorCode).Write();
```

But this leaves useful information on the table. Extra rich information can be included like so:

``` lang=cs
log.Info().Message("Sending data {data}", data).Write();
log.Error()
    .Message("Exception {exception} occurred with error code {error}")
    .Property("exception", ex.Message)
    .Property("error", errorCode)
    .Exception(ex)
    .Write();
```

In both statements, we are adding property-value pairs to the log.
In the first `Log.Info()` statement this is implicit, whereas in the `Log.Error()` statement it is made explicit.
This extra information may or may not be used by the log renderer, but if its not there then it can't be used!
So if in doubt, include extra information where it is appropriate.

Again, this feature set is native to NLog so makes for a very lightweight adaptor.
When developing adaptors for other logging frameworks, every attempt shouldbe made to preserve as much of the information as possible.

### Two-stage Approach to Logging ###

Think of logging as occurring in two distinct stages.

1. You build the log entry using `IFluentLogBuilder`, adding all of the relevant information as _Properties_ of the log entry.
2. You send the log entry to the backend to be rendered.

The renderer may use none, some or all of the information you provided and it may even augment it with additional metadata.
As a library developer, you shouldn't be concerned with how the entry will be rendered, stored or how it will be formatted.
You should concentrate only on including as much relevant information as is appropriate.

Multiple renderers may be in use and different renderers will produce different output from the exact same log entry.
For example:

- A file renderer may include a timestamp and perform log file rotation so that a new file is created each day.
- A debug output stream may include the name of the class where the log entry originated and print only the message portion.
- A console logger may write different lines in different colours accoring to the severity level.
- A syslog rendere may include the host name of the originating computer.
- A NoSQL database renderer may write out all of the properties as a JSON document.

In most cases, the way in which log data is ultimately rendered is controlled by the application, often using a configuration file.
As a library developer, you must accept that you have little to no control over this.
Just concentrate in including appropriate and useful information and don't think about formatting or storage.

[mit]: https://tigra.mit-license.org "Tigra MIT License"
[semver]: https://semver.org/ "the rules of semantic versioning"
[gitversion]: https://gitversion.net/docs/ "GitVersion documentation"
[nuget]: https://www.nuget.org/ "NuGet gallery"
[myget]: https://www.myget.org/feed/Packages/tigra-astronomy "Tigra Astronomy public package feed"
[yt-gitversion]: https://www.youtube.com/watch?v=8WKDk8yPMUA "Automatically versioning your code based on Git commit history"
[yt-gitversion-arduino]: https://www.youtube.com/watch?v=P4B6PTP6aAk "Automatic version in Arduino code with GitVersion"
[yt-oss]: https://www.youtube.com/watch?v=kloweL2fw7Q "Set your software free"
[coffee]: https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=ARU8ANQKU2SN2&source=url "Support our open source projects with a donation"
