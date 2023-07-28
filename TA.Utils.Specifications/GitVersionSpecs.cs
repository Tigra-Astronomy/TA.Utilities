using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using TA.Utils.Core;

namespace TA.Utils.Specifications
    {
        class with_gitversion_null_entry_assembly
        {
            Establish context = () => GitVersion.UnitTestNullInjectedVersionType();
        }

        class when_retrieving_version_information : with_gitversion_null_entry_assembly
        {
            It should_report_default_full_semantic_version = () => GitVersion.GitFullSemVer.ShouldEqual("0.0.0-unversioned");
            It should_report_default_semantic_version = () => GitVersion.GitSemVer.ShouldEqual("0.0.0");
            It should_report_default_major_version = () => GitVersion.GitMajorVersion.ShouldEqual("0");
            It should_report_default_minor_version = () => GitVersion.GitMinorVersion.ShouldEqual("0");
            It should_report_default_patch_version = () => GitVersion.GitPatchVersion.ShouldEqual("0");
            It should_report_empty_branch_name = () => GitVersion.GitBranchName.ShouldBeEmpty();
            It should_report_empty_metadata = () => GitVersion.GitBuildMetadata.ShouldBeEmpty();
            It should_report_empty_commit_date = () => GitVersion.GitCommitDate.ShouldBeEmpty();
            It should_report_empty_commit_hash = () => GitVersion.GitCommitSha.ShouldBeEmpty();
            It should_report_empty_commit_short_hash = () => GitVersion.GitCommitShortSha.ShouldBeEmpty();
            It should_report_default_info_version = () => GitVersion.GitInformationalVersion.ShouldEqual(GitVersion.GitFullSemVer);
            
        }
    }
