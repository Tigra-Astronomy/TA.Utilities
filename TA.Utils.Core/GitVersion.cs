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
        private static Type injectedVersion = ReflectInjectedGitVersionType();

        /// <summary>Gets the git informational version.</summary>
        /// <value>The git informational version.</value>
        /// <seealso cref="SemanticVersion" />
        public static string GitInformationalVersion =>
            injectedVersion?.GitVersionField("InformationalVersion") ?? GitFullSemVer;

        /// <summary>Gets the git commit SHA.</summary>
        /// <value>The git commit SHA.</value>
        public static string GitCommitSha => injectedVersion?.GitVersionField("Sha") ?? string.Empty;

        /// <summary>Gets the git commit short SHA.</summary>
        /// <value>The git commit short SHA.</value>
        public static string GitCommitShortSha => injectedVersion?.GitVersionField("ShortSha") ?? string.Empty;

        /// <summary>Gets the git commit date.</summary>
        /// <value>The git commit date.</value>
        public static string GitCommitDate => injectedVersion?.GitVersionField("CommitDate") ?? string.Empty;

        /// <summary>Gets the git semantic version string.</summary>
        /// <value>The git semantic version string.</value>
        /// <seealso cref="SemanticVersion" />
        public static string GitSemVer => injectedVersion?.GitVersionField("SemVer") ?? "0.0.0";

        /// <summary>Gets the git full semantic version string.</summary>
        /// <value>The git full semantic version string.</value>
        /// <seealso cref="SemanticVersion" />
        public static string GitFullSemVer => injectedVersion?.GitVersionField("FullSemVer") ?? "0.0.0-unversioned";

        /// <summary>Gets the git build metadata.</summary>
        /// <value>The git build metadata string.</value>
        /// <seealso cref="SemanticVersion" />
        public static string GitBuildMetadata => injectedVersion?.GitVersionField("FullBuildMetaData") ?? string.Empty;

        /// <summary>Gets the git major version.</summary>
        /// <value>The git major version, as a string.</value>
        /// <seealso cref="SemanticVersion" />
        public static string GitMajorVersion => injectedVersion?.GitVersionField("Major") ?? "0";

        /// <summary>Gets the git minor version.</summary>
        /// <value>The git minor version, as a string.</value>
        /// <seealso cref="SemanticVersion" />
        public static string GitMinorVersion => injectedVersion?.GitVersionField("Minor") ?? "0";

        /// <summary>Gets the git patch version.</summary>
        /// <value>The git patch version, as a string.</value>
        /// <seealso cref="SemanticVersion" />
        public static string GitPatchVersion => injectedVersion?.GitVersionField("Patch") ?? "0";

        /// <summary>Gets the name of the git branch from which the assembly was built.</summary>
        /// <value>The git branch name, as a string.</value>
        /// <seealso cref="SemanticVersion" />
        public static string GitBranchName => injectedVersion?.GitVersionField("BranchName") ?? string.Empty;

        /// <summary>
        ///     A method intended to be used for unit testing to set the injected version type to null. [#2]
        /// </summary>
        internal static void UnitTestNullInjectedVersionType()
        {
            injectedVersion = null;
        }

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
            var assembly = Assembly.GetEntryAssembly()??Assembly.GetExecutingAssembly();
            var type = assembly.GetTypes().SingleOrDefault(t => t.Name == "GitVersionInformation");
            return type;
        }
    }
}