
using System.Linq;
using Machine.Specifications;
using TA.Utils.Core;

namespace TA.Utils.Specifications
    {
    [Subject(typeof(SemanticVersion), "Parsing")]
    internal class when_parsing_a_semantic_version
        {
        Because of = () => version = new SemanticVersion("3.2.1-prerelease.5+extra.metadata");
        static SemanticVersion version;
        It should_have_major_version_3 = () => version.MajorVersion.ShouldEqual(3);
        It should_have_minor_version_2 = () => version.MinorVersion.ShouldEqual(2);
        It should_have_patch_version_1 = () => version.PatchVersion.ShouldEqual(1);
        It should_have_prerelease_tag = () => version.PrereleaseVersion.Single().ShouldEqual("prerelease.5");
        It should_have_build_metadata = () => version.BuildVersion.Single().ShouldEqual("extra.metadata");
        }

    [Subject(typeof(SemanticVersion), "validity")]
    public class when_testing_for_validity
        {
        It should_be_false_for_invalid_versions = () => SemanticVersion.IsValid("3.2.1.3").ShouldBeFalse();
        It should_be_true_for_valid_versions =
            () => SemanticVersion.IsValid("3.2.1-prerelease.5+extra.metadata").ShouldBeTrue();
        }

    [Subject(typeof(SemanticVersion), "sorting and comparison")]
    public class when_sorting_semantic_versions
        {
        static SemanticVersion v100 = new SemanticVersion("1.0.0");
        static SemanticVersion v101 = new SemanticVersion("1.0.1");
        static SemanticVersion v101pre9 = new SemanticVersion("1.0.1-alpha.9");
        static SemanticVersion v101pre10 = new SemanticVersion("1.0.1-alpha.10");
        It should_sort_lower_versions_first = () => v100.ShouldBeLessThan(v101);
        It should_sort_higher_versions_second = () => v101.ShouldBeGreaterThan(v100);
        It should_sort_prerelease_versions_before_releases = () => v101pre9.ShouldBeLessThan(v101);
        It should_prefer_numeric_order_over_lexical_order = () => v101pre9.ShouldBeLessThan(v101pre10);
        }
    }