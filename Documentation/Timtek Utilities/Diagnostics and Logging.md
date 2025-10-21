# Diagnostics and Logging

## `ConsoleLoggerService` implements `ILog`

One step up from the `DegenerateLoggerService`, enables easy rudimentatary logging to the console
without depending on any logging framework or configurations files. This is meant to be a light-weight
stop-gap logging solution for projects that never get large enough to warrant full structured logging.

`ConsoleLoggerService` is fully compatible with and interchangeable with any logging back-end built on the `ILog` interface.
The _Liskov Subsitution Principle_ is observed, so it remains easy to switch logging back-ends just by changing a binding in your
IOC container. Start yoru console app with a `ConsoleLoggingService`, then when it becomes a limitation, simply plug-in a full-blown
logger such as the NLog implementation founf in `TA.Utils.Logging.NLog`.

## Logging

Logging is a big deal. It is an essential part of debugging during development,
but can also be really useful or even essential in production.
It needs to be easy to use, or developer's won't use it.
It needs to not re-invent any wheels. There are plenty of good logging services out there.

Our approach to logging is this:
1. No dependency on any particular logging framework.
   We define an abstract interface that can be adapted to any back-end logging engine.
2. Follow the KISS principle: "Keep it simple, stupid".
   Logging should be easy enough that people will use it, but have enough flexibility to be useful in the real world.

We provide an abstract fluent builder pattern for easily constructing log entries and which provides an extensibility point
for creating extension methods.

Our fluent builder interface supports _semantic logging_ which enables the creation of rich logging data.

Item (1) notwithstanding, we have based our fluent builder API loosely on the one used by NLog.
We think it is the best balance of simplicity and flexibility. However, we do not depend on NLog
and have made our own abstract interfaces that can target any logging framework.

The `TA.Utils.Core.Diagnostics` namespace defines a pair of interfaces, `ILog` and `IFluentLogBuilder`,
that define an abstract logging service which does not depend on any particular back-end.

Libraries can perform logging through these interfaces without ever taking a dependency on any logging imnplementation.
The actual implementation can be injected at runtime, typically in a constructor parameter.
The policy decision about which logging engine to use can be taken in the top level composition root of the application.

The fluent interface defined in `IFluentLogBuilder` was modeled on the NLog fluent interface, so it is a very natural fit.
However, the interface has enough flexibility to adapt to other logging backends without too much trouble.]

A null implementation is provided in `DegenerateLoggerService` and `DegenerateLogBuilder`.
The two classes do essentially nothing and produce no output; they are a data sink.
Libraries can choose to use this as their default logging implementation, which is easier than checking
whether the logger is null every time it is used.

```csharp
public class MyClassThatUsesLogging
{
    private ILog Log;

    // Construct an instance and optionally inject the logging service implementation.
    public MyClassThatUsesLogging(ILog logService = null)
    {
        // Use the supplied logging service, or fall back to the degenerate logger.
        this.Log = logService ?? new DegenerateLoggerService();
    }

    public void MethodThatGeneratesLogEntries()
    {
        Log.Info()
            .Message("I am loosely coupled. I do not depend on any logging back-end.")
            .Write();
    }
}
```
The interface supports semantic logging. You can use a simple format string like so:
```csharp
log.Info().Message("Sending data {0}", data).Write();
log.Error().Message("Exception {0} occurred with error code {1}", ex.Message, errorCode).Write();
```
But this leaves useful information on the table. Extra rich information can be included like so:
```csharp
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

## Two-stage Approach to Logging

Think of logging as occurring in two distinct stages.

1. You build the log entry using `IFluentLogBuilder`, adding all of the relevant information as _Properties_ of the log entry.
2. You send the log entry to the back-end to be rendered on one or more _Targets_.

Each target may use none, some or all of the information you provided and it may even augment it with additional metadata.
As a library developer, you shouldn't be concerned with how the entry will be rendered, stored or how it will be formatted.
You should concentrate only on including as much relevant information as is appropriate in your log entries.

Multiple targets may be in use and different targets will produce different output from the exact same log entry.
For example:

- A file target may include a timestamp and perform log file rotation so that a new file is created each day.
- A debug output stream may include the name of the class where the log entry originated and print only the message portion.
- A console logger may write different lines in different colours accoring to the severity level.
- A syslog target may include the host name of the originating computer.
- A NoSQL database renderer may write out all of the properties as a JSON document.

In most cases, the way in which log data is ultimately rendered is outside of application control.
Typically, a configuration file is used. The configuration file may be added or changed post-deployment.
As a library developer, you must accept that you have little to no control over this.
Just concentrate on including appropriate and useful information and don't think about formatting or storage.

## A Note on Semantic Logging

If you have always thought about logging as `Console.WriteLine()` statements, then you have probably focussed
on formatting your output and given little thought to the content.
You might struggle to see the point of semantic logging and you might be due for a paradigm shift.
Forget about how your log output _looks_ and focus on what data it _contains_.
Your responsibility as the log entry creator is to include as much relevant data as possible.
Assume that formatting (rendering) and filtering will be done elsewhere and is outside your control.

How useful would it be, for example, if when you logged an exception,
it included all the exception metadata, any inner exceptions, and a full stack trace?
You might struggle to achieve this using `Console.WriteLine()`.
In our paradigm, that is as simple as adding `.Exception(ex)` to your log builder statement.

Having done that, you might think "so what?". The log file produced still only shows the exception message,
so what was the point? We struggled with this oursleves. You put the data in, but it doesn't easily come out
in a meaningful way.
Then one day we were "red-pilled" by [Seq][seq].
We discovered the truth that flat files are an inadequate solution for rendering log output.

We had our "Aha!" moment the first time we logged and exception to Seq and were able to view the full stack trace.
There is so much more to Seq, but that was the moment we understood semantic logging.
Seq unlocks the full usefulness of all that data and will change the way you write log entries.
Once you see the truth, Neo, you cannot go back. You cannot "unsee" Seq.
We realised that merely by changing a configuration file, i.e. with zero code impact,
we could send our log entries across the network to a log server that could store them in a SQL database.
We could then use our web browser to log into that server to view the log data, in real time,
and be able to view, search, filter and query based on the full data that we put into our log entries.

Seq can be used with our logging abstraction and the NLog adapter, and using the NLog.Targets.Seq NuGet package.
You can then configure a Seq target for NLog in your NLog.config file (there is no special code needed).

## Seq Special Considerations

We highly recommend Seq. It's free for a single user and can be set up in a few moments using Docker. This section contains some of our explorations with Seq.

### TL;DR

- Add and ambient property called `CorrelationId` to all log entries and set it to a new `Guid` each time the program starts. This helps you find all the log entries relating to a particular run of the program.
- Register a _Last Chance Exception Handler_ as soon as your program starts; this will let you catch and log any program crashes as they happen. Have your handler log the exception and display a message to the user with the `CorrelationId` that they should use in any bug reports. This will make it trivial to find the error in the logs.
- When reporting a `CorrelationId` to the user, you can use just the last few digits (we use 6) of the `CorrelationId`, this is usually enough to uniquely identify a log session.
- Use _dependency injection_ and have your IOC container create loggers for injection into class constructors.

This method may be useful. It was written for use with Ninject, but you can distill out the approach  of looking at the stack frame to work out the calling type and set the logger name from that.
```csharp
/// <summary>
///     Get an instance of a service from the dependency injection kernel.
///     Special handling for logging services.
/// </summary>
/// <typeparam name="TService">The type of service requested.</typeparam>
/// <returns>An instance of the requested service.</returns>
public static TService Get<TService>()
{
   if (typeof(ILog).IsAssignableFrom(typeof(TService)))
   {
       // Special handling for request for ILog.
       // Try to determine the calling type by examining the stack, and pass it to the kernel as a binding parameter.
       var callerStackFrame = new StackFrame(1);
       var callingMethod = callerStackFrame.GetMethod();
       // MethodBase.ReflectedType is more reliable than the direct Type property and less likely to return an "un-utterable name".
       var callerType = callingMethod.ReflectedType;
       var callerTypeName = callerType?.Name ?? string.Empty;
       if (!string.IsNullOrEmpty(callerTypeName))
       {
           var logServiceNameParameter = new Parameter(LogSourceParameterName, callerTypeName, false);
           return Kernel.Get<TService>(logServiceNameParameter);
       }
   }

   // For all other requests, simply request the type from the DI kernel.
    return Kernel.Get<TService>();
}
```

### Log Correlation

We use a global static readonly GUID called something like `CorrelationId`.
We initialize this with a new `Guid` as early as possible in the program execution, usually in a static initializer, so that it has a new value for each run of the program. We then add this to every instance of `ILog` as an Ambient Property.

This way, every log entry we write contains a `CorrelationId` value which is unique for each run of the program. In Seq, you can Expand a log entry, find the `CorrelationId` property, and click the checkmark next to it, then select "Find". This will find all the log entries for one run of the program.

![Correlation of log entries](Assets/Find-CorrelationId.png)

### Custom Severity Levels

NLog has fice severity levels: `Trace`, `Debug`, `Info`, `Warning`, `Error` and a pseudo-level `Fatal` which actually causes the program to exit, so can't really be used as a normal severity level.

We find this a bit limiting and would like to be able to create our own levels, such as `Note` and `Important`.

The `NLog.Targets.Seq` logging target has support for this, and we also support it in our `ILog` interface via the `ILog.Level(string levelName)` method.

The way this works is to create an additional log event property, by default named "CustomLevel", containing the level name. The Seq target then uses this property as the level when it posts the data to the Seq server. If the default property name is no good for some reason, it can be changed using a `LogServiceOptions` instance and setting `LogServiceOptions.CustomSeverityPropertyName` property to the preferred name, like so:
```csharp
var options = LogServiceOptions.DefaultOptions.CustomSeverityPropertyName("SeqLevel");
var log = new LoggingService(options);
```
A small bit of configuration is needed to wire this up, in the Seq target in the `NLog.config` file, like so:
```xml
      \\<target xsi:type="Seq" name="seq" 
      serverUrl="http://your-server-url:5341"
      apiKey="your-seq-api-key"
      seqLevel="${event-properties:CustomLevel:whenEmpty=${level}}"\\>
```
The magic is in `seqLevel="${event-properties:CustomLevel:whenEmpty=${level}}`

This uses the value of `CustomLevel` as the Seq level, unless it is empty or missing, in which case it defaults to the NLog level.

In targets other than Seq, this will just appear as yet another log event property.

[seq]: https://datalust.co/seq "Seq semantic logging service"
