// This file is part of the TA.Utils project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: GitVersion.cs  Last modified: 2020-07-13@02:11 by Tim Long

using System;
using System.Linq;
using System.Reflection;

namespace TA.Utils.Core
{
    /// <summary>
    ///     Provides a set of read-only properties for accessing version information that was injected by
    ///     GitVersion as part of the build process.
    /// </summary>
    /// <seealso cref="SemanticVersion" />
    public static class GitVersion
    {
        /// <summary>The type injected by GitVersion during the build process, containing version information.</summary>
        private static readonly Type InjectedVersion = ReflectInjectedGitVersionType();

        /// <summary>Gets the git informational version.</summary>
        /// <value>The git informational version.</value>
        /// <seealso cref="SemanticVersion" />
        public static string GitInformationalVersion => InjectedVersion.GitVersionField("InformationalVersion");

        /// <summary>Gets the git commit SHA.</summary>
        /// <value>The git commit SHA.</value>
        public static string GitCommitSha => InjectedVersion.GitVersionField("Sha");

        /// <summary>Gets the git commit short SHA.</summary>
        /// <value>The git commit short SHA.</value>
        public static string GitCommitShortSha => InjectedVersion.GitVersionField("ShortSha");

        /// <summary>Gets the git commit date.</summary>
        /// <value>The git commit date.</value>
        public static string GitCommitDate => InjectedVersion.GitVersionField("CommitDate");

        /// <summary>Gets the git semantic version string.</summary>
        /// <value>The git semantic version string.</value>
        /// <seealso cref="SemanticVersion" />
        public static string GitSemVer => InjectedVersion.GitVersionField("SemVer");

        /// <summary>Gets the git full semantic version string.</summary>
        /// <value>The git full semantic version string.</value>
        /// <seealso cref="SemanticVersion" />
        public static string GitFullSemVer => InjectedVersion.GitVersionField("FullSemVer");

        /// <summary>Gets the git build metadata.</summary>
        /// <value>The git build metadata string.</value>
        /// <seealso cref="SemanticVersion" />
        public static string GitBuildMetadata => InjectedVersion.GitVersionField("FullBuildMetaData");

        /// <summary>Gets the git major version.</summary>
        /// <value>The git major version, as a string.</value>
        /// <seealso cref="SemanticVersion" />
        public static string GitMajorVersion => InjectedVersion.GitVersionField("Major");

        /// <summary>Gets the git minor version.</summary>
        /// <value>The git minor version, as a string.</value>
        /// <seealso cref="SemanticVersion" />
        public static string GitMinorVersion => InjectedVersion.GitVersionField("Minor");

        /// <summary>Gets the git patch version.</summary>
        /// <value>The git patch version, as a string.</value>
        /// <seealso cref="SemanticVersion" />
        public static string GitPatchVersion => InjectedVersion.GitVersionField("Patch");

        /// <summary>Gets the name of the git branch from which the assembly was built.</summary>
        /// <value>The git branch name, as a string.</value>
        /// <seealso cref="SemanticVersion" />
        public static string GitBranchName => InjectedVersion.GitVersionField("BranchName");

        /// <summary>Uses reflection to fetch the value of a member field of the injected version information.</summary>
        /// <param name="gitVersionInformationType">Type of the git version information.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>A string containing the field value, or "undefined".</returns>
        private static string GitVersionField(this Type gitVersionInformationType, string fieldName)
        {
            var versionField = gitVersionInformationType?.GetField(fieldName);
            return versionField?.GetValue(null).ToString() ?? "undefined";
        }

        /// <summary>Reflects the type of the injected git version information.</summary>
        /// <returns>Type.</returns>
        private static Type ReflectInjectedGitVersionType()
        {
            var assembly = Assembly.GetEntryAssembly();
            var type = assembly.GetTypes().SingleOrDefault(t => t.Name == "GitVersionInformation");
            return type;
        }
    }
}