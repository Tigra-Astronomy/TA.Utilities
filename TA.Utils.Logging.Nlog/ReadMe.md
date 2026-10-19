# NLog Logging Service #

This is a logging service implementation that uses *NLog* as the back-end.

The fluent interface defined in `TA.Utils.Diagnostics.IFluentLogBuilder` was modeled on the NLog fluent interface,
so it is a very natural fit. However, the interface has enough flexibility to adapt to other logging backends
without too much trouble.

NLog supports semantic logging. You can use a simple format string like so:

``` lang=cs
log.Info().Message("Sending data {0}", data).Write();
log.Error().Message("Exception {0} occurred with error code {1}", ex.Message, errorCode).Write();
```

But this leaves some functionality on the table. Extra rich information can be included like so:

``` lang=cs
log.Info().Message("Sending data {data}", data).Write();
log.Error()
    .Message("Exception {exception} occurred with error code {error}")
    .Property("exception", ex.Message)
    .Property("error", errorCode)
    .Exception(ex)
    .Write();
```

Named message-template holes are filled from positional arguments first, in template order.
Remaining named holes are filled from matching properties when `Write()` or `WriteIf()` writes the event.
Properties may be supplied before or after `Message()`, and remain available to structured targets.
Unmatched holes are left unchanged. NLog applies format specifiers, alignment and the `@`/`$` capture operators.

When using NLog, you never need to be concerned about output formatting or where the log messages will go.
This is all handled by NLog and the `NLog.config` file in the application directory.
There is a lot of flexibility in this approach.
As the developer, just concentrate on putting rich information into your log entries.

By default, the log name (source) is set to the file name of the source file where the log entry is created.
This is fine for most purposes.
This behaviour can be everridden either by specifying the log name along with the level:

``` lang=cs
log.Debug("Custom Source").Message("Hello log").Write();
```

or by adding an explicit name entry when building the log entry:

``` lang=cs
log.Debug().Message("Hello log").LoggerName("Custom Source").Write();
```
