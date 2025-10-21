# Code Contract Assertions

A set of extension methods for making runtime assertions that can help to catch code contract violations.

```csharp
void AddUserToDatabase(User user)
{
    // Check that the user is not null and that the age is greater than 18.
    user.ContractAssertNotNull();
    user.Age.ContractAssert(p => p > 18, "Age must be greater than 18");
}
```
Any assertion failure will result in a `ContractViolationException` being thrown. It is recommended not to catch these exceptions, but to let them bubble up to the application root where they can be logged and the application terminated cleanly. A `ContractViolationException` is an unequivocal indication of a bug in the code, so it should never be caught and handled. It is a failure of the code contract, not a runtime error that can be recovered from.
