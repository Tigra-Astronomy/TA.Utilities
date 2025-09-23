# GitVersion: benefits over manual versioning

GitVersion automates semantic versioning by deriving a version from your Git history and branch conventions. Compared to hand‑maintained versions, it provides consistency, removes busywork, and eliminates whole classes of release mistakes.

Key benefits
- Zero manual bumps
  - No more “remember to bump” PRs; the version is computed from commits/branches/tags.
- Consistent SemVer across all builds
  - CI, local dev, and release builds resolve the same version for the same commit.
- First‑class pre‑releases without ad‑hoc suffixes
  - Branches map to pre‑release channels (e.g., develop → alpha, release/* → beta) with incremental identifiers.
- Traceability and provenance
  - InformationalVersion embeds branch and commit (e.g., +Branch.develop.Sha.XXXX), making binaries self‑describing.
- Works with Git Flow
  - Feature → pre‑release while developing; release branches stabilize as beta; tagging on main/master promotes to a stable version.
- Deterministic packages and assemblies
  - Packages built from the same commit get the same version, avoiding “mystery rebuilds” that overwrite artifacts.
- Cleaner PRs and histories
  - No noise commits solely to bump versions; reduces merge conflicts in project files.
- Policy as configuration
  - Version rules live in configuration rather than tribal knowledge; easy to review and change as a team policy.

How it helps in this repository
- We already reference GitVersion.MsBuild in the packable libraries, so assemblies and NuGet packages receive the computed version automatically at build time.
- The docs site deployments also use the GitVersion SemVer as the mike version for develop and release/* builds, so your published documentation versions align with your code versions.
- Release tagging uses the raw semantic tag (no leading “v”), which becomes the published stable version and moves the latest alias in documentation.

Manual versioning pitfalls avoided
- Forgotten or inconsistent bumps between projects/solutions in a repo.
- Conflicting edits to the same version fields during parallel work/merges.
- Drift between “About” dialogs, assembly attributes, package versions, and documentation.
- Ambiguous nightly/CI builds where two different commits share the same nominal version.

See also
- [[Versioning|Versioning (GitVersion + SemVer)]]