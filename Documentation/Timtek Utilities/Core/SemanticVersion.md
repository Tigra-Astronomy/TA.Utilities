# SemanticVersion

Encapsulates parsing, validation, display formatting, equality, and sorting of SemVer strings. Pre-release and build metadata are handled per SemVer 2.0.0 rules.

- Parse: `new SemanticVersion("1.0.0-alpha.1+meta")`
- Validate: `SemanticVersion.IsValid("1.0.0")`
- Compare/sort: compares major/minor/patch; pre-releases sort before releases; numeric identifiers sort numerically.

Examples (from specifications):
- `new("1.0.1-alpha.9") < new("1.0.1-alpha.10")`
- Build metadata is ignored for equality.
