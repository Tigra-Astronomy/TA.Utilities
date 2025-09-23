# Versioning

Timtek has settled on an automated versioning strategy based on [Semantic Versioning 2.0.0][semver] and [[About GitVersion]].

We give all of our software a semantic version, which we display to the user in the About box and write out to log files on startup.
We use [GitVersion][gitversion] to [automatically assign a version number to every build][yt-gitversion] (even in [Arduino projects][yt-gitversion-arduino]).
We never manually set the version number, it happens as part of the build process.
So we can never forget to "bump the version" and we can never forget to set it.
Total automation.
If you examine one of our log files, you may well find something like this:

```text
21:16:59.2909|INFO |Server          |Git Commit ID: "229c1acc4a7bda494f78a8c7cc811c2a4d8e9132"
21:16:59.3069|INFO |Server          |Git Short ID: "229c1ac"
21:16:59.3069|INFO |Server          |Commit Date: "2020-07-11"
21:16:59.3069|INFO |Server          |Semantic version: "0.1.0-alpha.1"
21:16:59.3069|INFO |Server          |Full Semantic version: "0.1.0-alpha.1"
21:16:59.3069|INFO |Server          |Build metadata: "Branch.develop.Sha.229c1acc4a7bda494f78a8c7cc811c2a4d8e9132"
21:16:59.3069|INFO |Server          |Informational Version: "0.1.0-alpha.1+Branch.develop.Sha.229c1acc4a7bda494f78a8c7cc811c2a4d8e9132"
```
There's no mistaking where that build came from.

## `SemanticVersion` class

Since we rely heavily on semantic versioning, it is useful to have a class that encapsulates all the rules for parsing, outputting, comparing and sorting semantic versions. That is the function of the `SemanticVersion` class. With it you can:

- Parse a semantic version string;
- Validate a semantic version string;
- Format a version for display, logging or printing;
- Test the equality of two versions using the correct comparison rules;
- Sort versions using the correct collation rules.

## GitVersion Support

[GitVersion][gitversion] also injects a static class into the assembly containing all the versioning information it computed based on your Git commit history.
This information can be a little tricky to get at, because it doesn't exist at compile time so you can't easily reference it. You have to use Reflection to get at it.
Our `GitVersion` class contains static properties for getting your semantic version metadata at runtime. We use it to write the log entries as shown above.

[semver]: https://semver.org/ "the rules of semantic versioning"
[gitversion]: https://gitversion.net/docs/ "GitVersion documentation"
[yt-gitversion]: https://www.youtube.com/watch?v=8WKDk8yPMUA "Automatically versioning your code based on Git commit history"
[yt-gitversion-arduino]: https://www.youtube.com/watch?v=P4B6PTP6aAk "Automatic version in Arduino code with GitVersion"
