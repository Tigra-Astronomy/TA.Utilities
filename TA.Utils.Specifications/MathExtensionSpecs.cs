
using Machine.Specifications;
using TA.Utils.Core;

namespace TA.Utils.Specifications
    {
    class with_clip_test_context
        {
        protected static SemanticVersion MinimumVersion = new SemanticVersion("3.2.1");
        protected static SemanticVersion MaximumVersion = new SemanticVersion("9.8.7");
        }

    [Subject(typeof(MathExtensions))]
    internal class when_clipping_a_value_above_maximum : with_clip_test_context
        {
        It should_clip_at_maximum = () => testCase.Clip(MinimumVersion,MaximumVersion).ShouldEqual(MaximumVersion);
        static SemanticVersion testCase = new SemanticVersion("9.8.8");
        }

    [Subject(typeof(MathExtensions))]
    internal class when_clipping_a_value_below_minimum : with_clip_test_context
        {
        It should_clip_at_minimum = () => testCase.Clip(MinimumVersion, MaximumVersion).ShouldEqual(MinimumVersion);
        static SemanticVersion testCase = new SemanticVersion("1.0.0");
        }

    [Subject(typeof(MathExtensions))]
    internal class when_clipping_a_value_within_range : with_clip_test_context
        {
        It should_not_clip = () => testCase.Clip(MinimumVersion, MaximumVersion).ShouldEqual(testCase);
        static SemanticVersion testCase = new SemanticVersion("4.5.6");
        }

    }