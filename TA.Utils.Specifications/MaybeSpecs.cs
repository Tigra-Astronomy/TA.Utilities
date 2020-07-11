// This file is part of the TA.Utils project
//
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
//
// File: MaybeSpecs.cs  Last modified: 2020-07-11@18:47 by Tim Long

using System.Linq;
using Machine.Specifications;
using TA.Utils.Core;

namespace TA.Utils.Specifications
    {
    [Subject(typeof(Maybe<>), "Creation")]
    internal class when_creating_explicitly
        {
        It should_be_empty_if_source_is_null = () => Maybe<object>.From(null).ShouldEqual(Maybe<object>.Empty);
        It should_have_content_if_not_null = () => Maybe<object>.From(new object()).Any().ShouldBeTrue();
        It should_not_have_content_if_null = () => Maybe<object>.From(null).Any().ShouldBeFalse();
        }

    [Subject(typeof(MaybeExtensions), "extension methods")]
    public class when_creating_a_maybe_from_an_object_using_extension_methods
        {
        static object source = new object();
        Because of = () => maybe = source.AsMaybe();
        It should_have_content = () => maybe.Any().ShouldBeTrue();
        It should_not_be_empty = () => maybe.None.ShouldBeFalse();
        It should_contain_the_source_object = () => maybe.Single().ShouldBeTheSameAs(source);
        static Maybe<object> maybe;
        }
    }