
using Machine.Specifications;
using TA.Utils.Core;

namespace TA.Utils.Specifications
    {
    [Subject(typeof(AsciiExtensions))]
    internal class when_expanding_individual_characters
        {
        It should_expand_cr = () => '\r'.ExpandAscii().ShouldEqual("<CR>");
        It should_expand_lf = () => '\n'.ExpandAscii().ShouldEqual("<LF>");
        It should_expand_ff = () => '\f'.ExpandAscii().ShouldEqual("<FF>");
        It should_expand_htab = () => '\t'.ExpandAscii().ShouldEqual("<HT>");
        It should_expand_vtab = () => '\v'.ExpandAscii().ShouldEqual("<VT>");
        It should_expand_bell = () => '\a'.ExpandAscii().ShouldEqual("<BELL>");
        It should_not_expand_space = () => ' '.ExpandAscii().ShouldEqual(" ");
        }

    [Subject(typeof(AsciiExtensions), "printing characters")]
    internal class when_expanding_a_string_containing_only_printing_characters
        {
        const string expected =
            @"1234567890-=qwertyuiop[]\asdfghjkl;'zxcvbnm,./!@##$%^&*()_+QWERTYUIOP{}|ASDFGHJKL:""ZXCVBNM<>?";
        It should_not_expand = () => expected.ExpandAscii().ShouldEqual(expected);
        }

    [Subject(typeof(AsciiExtensions), "whitespace characters")]
    internal class when_expanding_a_string_containing_nonprinting_characters
        {
        const string source = "The\tquick brown fox\r\njumps over the lazy dog\a";
        const string expected = "The<HT>quick brown fox<CR><LF>jumps over the lazy dog<BELL>";
        It should_replace_nonprinting_characters = () => source.ExpandAscii().ShouldEqual(expected);
        }
    }