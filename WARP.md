# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

Repository overview
- Solution: TA.Utils.sln
- Projects:
  - TA.Utils.Core (netstandard2.0): core utilities, logging abstractions, semantic version helpers, async helpers, state machine, property binding, ASCII helpers.
  - TA.Utils.Logging.Nlog (netstandard2.0): NLog-based implementation of the core logging abstraction.
  - TA.Utils.Specifications (net4.8; net8.0): MSpec tests covering the core library and the NLog adapter.
  - TA.Utils.Logging.NLog.SampleConsoleApp (net6.0): sample console app demonstrating the logging abstraction with the NLog implementation.
- Central package management: Directory.Packages.props
- Versioning: GitVersion.MsBuild is used in packable libraries; versions are computed from Git history at build time.

Common commands (PowerShell, Windows)
- Restore and build (solution)
  - dotnet restore .\TA.Utils.sln
  - dotnet build .\TA.Utils.sln -c Debug
  - For NuGet package outputs, build in Release (GeneratePackageOnBuild=true for libraries):
    - dotnet build .\TA.Utils.sln -c Release

- Run tests (MSpec via Visual Studio test adapter)
  - All tests (both target frameworks as configured by the project):
    - dotnet test .\TA.Utils.Specifications -c Debug
  - Only net8.0 target:
    - dotnet test .\TA.Utils.Specifications -c Debug -f net8.0
  - Run a single test class (filter by fully qualified name substring):
    - dotnet test .\TA.Utils.Specifications -c Debug -f net8.0 --filter "FullyQualifiedName~TA.Utils.Specifications.SemanticVersionSpecs"
  - Run a specific spec by display name substring (works with the VS adapter):
    - dotnet test .\TA.Utils.Specifications -c Debug -f net8.0 --filter "DisplayName~when_parsing_a_semantic_version"

- Run the sample app
  - dotnet run --project .\TA.Utils.Logging.NLog.SampleConsoleApp\TA.Utils.Logging.NLog.SampleConsoleApp.csproj

- Format/lint check (no changes)
  - This repository uses .editorconfig for style. To verify formatting without changing files:
    - dotnet format --verify-no-changes

- Pack (explicit)
  - Although libraries are configured to generate packages on build, you can also pack explicitly into a common folder:
    - New-Item -ItemType Directory -Force -Path .\artifacts\nuget | Out-Null
    - dotnet pack .\TA.Utils.Core\TA.Utils.Core.csproj -c Release -o .\artifacts\nuget -p:ContinuousIntegrationBuild=true
    - dotnet pack .\TA.Utils.Logging.Nlog\TA.Utils.Logging.Nlog.csproj -c Release -o .\artifacts\nuget -p:ContinuousIntegrationBuild=true

- Publish packages
  - MyGet (script included in repo):
    - .\Push-Packages.ps1                # pushes Release .nupkg found under bin\Release (set -ApiKey if needed)
    - Example with API key: .\Push-Packages.ps1 -ApiKey $env:MYGET_API_KEY
  - NuGet.org (example push; do not run unless intended):
    - Prereq: $env:NUGET_API_KEY is set
    - dotnet nuget push .\artifacts\nuget\*.nupkg --source "https://api.nuget.org/v3/index.json" --api-key $env:NUGET_API_KEY --skip-duplicate

High-level architecture
- Logging abstraction and adapter
  - TA.Utils.Core.Diagnostics defines ILog and IFluentLogBuilder. Libraries depend only on these abstractions.
  - TA.Utils.Logging.Nlog adapts the abstraction to NLog, including support for semantic properties and optional Seq target via NLog.Targets.Seq.
  - The sample console app wires up the NLog implementation and demonstrates usage.

- Utilities in Core (selected highlights used across projects)
  - SemanticVersion and GitVersion helpers to parse/compare versions and read GitVersion-generated assembly metadata.
  - Maybe<T> to express optional values without nulls; LINQ-friendly and intention-revealing.
  - Async helpers (ContinueOnAnyThread / ContinueInCurrentContext) to make continuation intent explicit.
  - Property binding utilities (KeyValueReader/PropertyBinder) for binding key-value records to types.
  - Simple state machine interfaces and implementation (StateMachine/*) for small workflow/state needs.
  - ASCII helpers and DisplayEquivalent attribute/extension methods for readable output and enum display text.

- Test strategy
  - TA.Utils.Specifications contains Machine.Specifications-based BDD-style tests. It targets both net4.8 and net8.0.
  - The Visual Studio test adapter (Machine.Specifications.Runner.VisualStudio + Microsoft.NET.Test.Sdk) enables discovery via dotnet test, including filtering by FullyQualifiedName or DisplayName.

Notes
- Package versions are managed centrally via Directory.Packages.props.
- Building libraries in Release will emit .nupkg/.snupkg under each project’s bin\Release; use the provided script or dotnet nuget push as appropriate.
