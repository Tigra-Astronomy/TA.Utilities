# Release Notes

2.10.0
 - Improved how code contracts handle recording the contract data in the `CodeContractViolationException`.  
 - Added the ability to configure logging levels in ConsoleLoggerOptions. The default is to log everything.
   - Specific severity levels can be included using RenderSeverityLevels(). Once configured with any value, the default no longer applies so all levels to be logged must be included.
   - Specific levels may also be ignored using IgnoreSeverityLevels(). The ignore list takes precedence over the include list.

2.9.0
- Added Code Contract assertions that can be used for enforcing code contracts at runtime.
  Contract failures result in a `CodeContractViolationException` being thrown.
  The exception records the value being tested, any predicate expression that was used in the test,
  and the message that was passed to the assertion method. Any occurrence of a `CodeContractViolationException`
  should be treated as an unambiguous bug in the code and not caught or handled, except to write it out to a log.

2.8.1
- Fixed an issue that caused a custom severity property name option to be ignored. 

2.8.0
- Fixed a formatting bug in `Octet.ToString()`

2.7.0
- Added support for custom severity levels in the `ILog` abstraction and NLog implementation.
- Used the "official" regular expression to validate semantic version strings. Note: some strings that were previously accepted, such as "01.02.03" will no longer be accepted as valid.