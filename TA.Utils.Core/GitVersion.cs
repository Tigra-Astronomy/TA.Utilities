// This file is part of the TA.Utils project
//
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
//
// File: GitVersionExtensions.cs  Last modified: 2020-07-11@19:58 by Tim Long

using System;
using System.Linq;
using System.Reflection;

namespace TA.Utils.Core
    {
    /// <summary>
    ///     Provides a set of read-only properties for accessing version information that was injected by
    ///     GitVersion as part of the build process.
    /// </summary>
    public static class GitVersion
        {
        private static readonly Type InjectedVersion = ReflectInjectedGitVersionType();

        private static string GitVersionField(this Type gitVersionInformationType, string fieldName)
            {
            var versionField = gitVersionInformationType?.GetField(fieldName);
            return versionField?.GetValue(null).ToString() ?? "undefined";
            }

        private static Type ReflectInjectedGitVersionType()
            {
            var assembly = Assembly.GetExecutingAssembly();
            var type = assembly.GetTypes().SingleOrDefault(t => t.Name == "GitVersionInformation");
            return type;
            }

        public static string GitInformationalVersion => InjectedVersion.GitVersionField("InformationalVersion");

        public static string GitCommitSha => InjectedVersion.GitVersionField("Sha");

        public static string GitCommitShortSha => InjectedVersion.GitVersionField("ShortSha");

        public static string GitCommitDate => InjectedVersion.GitVersionField("CommitDate");

        public static string GitSemVer => InjectedVersion.GitVersionField("SemVer");

        public static string GitFullSemVer => InjectedVersion.GitVersionField("FullSemVer");

        public static string GitBuildMetadata => InjectedVersion.GitVersionField("FullBuildMetaData");

        public static string GitMajorVersion => InjectedVersion.GitVersionField("Major");

        public static string GitMinorVersion => InjectedVersion.GitVersionField("Minor");

        public static string GitPatchVersion => InjectedVersion.GitVersionField("Patch");
        }
    }