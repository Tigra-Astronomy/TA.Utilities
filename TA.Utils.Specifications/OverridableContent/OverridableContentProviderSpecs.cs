#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Machine.Specifications;
using TA.Utils.Core.Diagnostics;
using TA.Utils.Core.OverridableContentProvider;

namespace TA.Utils.Specifications.OverridableContent;

#region Context-Builder infrastructure

class OverridableContentProviderContext
{
    public IOverridableContentProvider   Provider            { get; init; }
    public IReadOnlyList<string>         OverrideDirectories { get; init; }
    public OverridableContentResolution  ResolveResult       { get; set; }
    public Stream                        OpenResult          { get; set; }
    public Exception                     CaughtException     { get; set; }
}

class OverridableContentProviderContextBuilder
{
    internal const string FirstOverrideDirectory  = @"C:\Users\tester\.ftng\HighPriority";
    internal const string SecondOverrideDirectory = @"C:\Users\tester\.ftng\LowPriority";
    internal const string EmbeddedRootNamespace    =
        "TA.Utils.Specifications.OverridableContent.EmbeddedTestData";

    readonly MockFileSystem mockFileSystem = new();
    readonly ILog log = new ConsoleLoggerService(ConsoleLoggerOptions.DefaultOptions.RenderProperties(false));
    IReadOnlyList<string> overrideDirectories = [FirstOverrideDirectory, SecondOverrideDirectory];

    internal OverridableContentProviderContextBuilder WithFileInFirstDirectory(string itemName, string content)
    {
        mockFileSystem.AddFile($@"{FirstOverrideDirectory}\{itemName}", new MockFileData(content));
        return this;
    }

    internal OverridableContentProviderContextBuilder WithFileInSecondDirectory(string itemName, string content)
    {
        mockFileSystem.AddFile($@"{SecondOverrideDirectory}\{itemName}", new MockFileData(content));
        return this;
    }

    internal OverridableContentProviderContextBuilder WithNoOverrideDirectories()
    {
        overrideDirectories = [];
        return this;
    }

    internal OverridableContentProviderContext Build()
    {
        var provider = new global::TA.Utils.Core.OverridableContentProvider.OverridableContentProvider(
            typeof(OverridableContentProviderContextBuilder).Assembly,
            EmbeddedRootNamespace,
            mockFileSystem,
            log);
        return new OverridableContentProviderContext { Provider = provider, OverrideDirectories = overrideDirectories };
    }
}

class with_OverridableContentProvider_context
{
    protected static OverridableContentProviderContext        Context;
    protected static OverridableContentProviderContextBuilder ContextBuilder;

    Establish context = () => ContextBuilder = new OverridableContentProviderContextBuilder();

    Cleanup after = () =>
    {
        Context?.OpenResult?.Dispose();
        Context = null;
        ContextBuilder = null;
    };
}

#endregion

[Subject(typeof(OverridableContentProvider), "resolving with an override present in the highest priority directory")]
[Tags("OverridableContentProvider")]
class when_resolving_and_override_exists_in_first_directory : with_OverridableContentProvider_context
{
    Establish context = () => Context = ContextBuilder
        .WithFileInFirstDirectory("item.txt", "first directory content")
        .Build();

    Because of = () => Context.ResolveResult = Context.Provider.Resolve(Context.OverrideDirectories, "item.txt");

    It should_return_override_source = () =>
        Context.ResolveResult.Source.ShouldEqual(OverridableContentSource.Override);
    It should_return_path_in_first_directory = () =>
        Context.ResolveResult.Path.ShouldContain("HighPriority");
}

[Subject(typeof(OverridableContentProvider), "resolving with overrides in more than one directory")]
[Tags("OverridableContentProvider")]
class when_resolving_and_override_exists_in_both_directories : with_OverridableContentProvider_context
{
    Establish context = () => Context = ContextBuilder
        .WithFileInFirstDirectory("item.txt", "first directory content")
        .WithFileInSecondDirectory("item.txt", "second directory content")
        .Build();

    Because of = () => Context.ResolveResult = Context.Provider.Resolve(Context.OverrideDirectories, "item.txt");

    It should_prefer_the_first_directory = () =>
        Context.ResolveResult.Path.ShouldContain("HighPriority");
}

[Subject(typeof(OverridableContentProvider), "resolving with an override only in the second-priority directory")]
[Tags("OverridableContentProvider")]
class when_resolving_and_override_exists_only_in_second_directory : with_OverridableContentProvider_context
{
    Establish context = () => Context = ContextBuilder
        .WithFileInSecondDirectory("item.txt", "second directory content")
        .Build();

    Because of = () => Context.ResolveResult = Context.Provider.Resolve(Context.OverrideDirectories, "item.txt");

    It should_return_override_source = () =>
        Context.ResolveResult.Source.ShouldEqual(OverridableContentSource.Override);
    It should_return_path_in_second_directory = () =>
        Context.ResolveResult.Path.ShouldContain("LowPriority");
}

[Subject(typeof(OverridableContentProvider), "resolving with no override present but an embedded resource exists")]
[Tags("OverridableContentProvider")]
class when_resolving_and_no_override_exists_but_embedded_resource_does : with_OverridableContentProvider_context
{
    Establish context = () => Context = ContextBuilder.Build();

    Because of = () => Context.ResolveResult = Context.Provider.Resolve(Context.OverrideDirectories, "embedded-item.txt");

    It should_return_embedded_resource_source = () =>
        Context.ResolveResult.Source.ShouldEqual(OverridableContentSource.EmbeddedResource);
    It should_return_a_null_path = () =>
        Context.ResolveResult.Path.ShouldBeNull();
}

[Subject(typeof(OverridableContentProvider), "resolving when nothing exists")]
[Tags("OverridableContentProvider")]
class when_resolving_and_neither_override_nor_embedded_resource_exists : with_OverridableContentProvider_context
{
    Establish context = () => Context = ContextBuilder.WithNoOverrideDirectories().Build();

    Because of = () => Context.ResolveResult = Context.Provider.Resolve(Context.OverrideDirectories, "does-not-exist.txt");

    It should_return_not_found_source = () =>
        Context.ResolveResult.Source.ShouldEqual(OverridableContentSource.NotFound);
    It should_return_a_null_path = () =>
        Context.ResolveResult.Path.ShouldBeNull();
}

[Subject(typeof(OverridableContentProvider), "resolving with an empty directory list, falling back to the embedded resource")]
[Tags("OverridableContentProvider")]
class when_resolving_with_an_empty_directory_list_and_embedded_resource_exists : with_OverridableContentProvider_context
{
    Establish context = () => Context = ContextBuilder.WithNoOverrideDirectories().Build();

    Because of = () => Context.ResolveResult = Context.Provider.Resolve(Context.OverrideDirectories, "embedded-item.txt");

    It should_return_embedded_resource_source = () =>
        Context.ResolveResult.Source.ShouldEqual(OverridableContentSource.EmbeddedResource);
}

[Subject(typeof(OverridableContentProvider), "opening content with an override present")]
[Tags("OverridableContentProvider")]
class when_opening_and_override_exists : with_OverridableContentProvider_context
{
    Establish context = () => Context = ContextBuilder
        .WithFileInFirstDirectory("item.txt", "override file content")
        .Build();

    Because of = () => Context.OpenResult = Context.Provider.Open(Context.OverrideDirectories, "item.txt");

    It should_return_the_override_content = () =>
    {
        using var reader = new StreamReader(Context.OpenResult);
        reader.ReadToEnd().ShouldEqual("override file content");
    };
}

[Subject(typeof(OverridableContentProvider), "opening content with no override, falling back to the embedded resource")]
[Tags("OverridableContentProvider")]
class when_opening_and_no_override_exists : with_OverridableContentProvider_context
{
    Establish context = () => Context = ContextBuilder.Build();

    Because of = () => Context.OpenResult = Context.Provider.Open(Context.OverrideDirectories, "embedded-item.txt");

    It should_return_the_embedded_resource_content = () =>
    {
        using var reader = new StreamReader(Context.OpenResult);
        reader.ReadToEnd().Trim().ShouldEqual("embedded default content");
    };
}

[Subject(typeof(OverridableContentProvider), "opening content that does not exist anywhere")]
[Tags("OverridableContentProvider")]
class when_opening_and_nothing_exists : with_OverridableContentProvider_context
{
    Establish context = () => Context = ContextBuilder.WithNoOverrideDirectories().Build();

    Because of = () => Context.CaughtException = Catch.Exception(() => Context.Provider.Open(Context.OverrideDirectories, "does-not-exist.txt"));

    It should_throw_an_overridable_content_not_found_exception = () =>
        Context.CaughtException.ShouldBeOfExactType<OverridableContentNotFoundException>();
    It should_mention_the_item_name_in_the_message = () =>
        Context.CaughtException.Message.ShouldContain("does-not-exist.txt");
}

[Subject(typeof(OverridableContentProvider), "resolving the same provider instance against two different directory lists")]
[Tags("OverridableContentProvider")]
class when_resolving_the_same_provider_instance_with_different_directory_lists : with_OverridableContentProvider_context
{
    Establish context = () => Context = ContextBuilder
        .WithFileInSecondDirectory("item.txt", "second directory content")
        .Build();

    Because of = () =>
        // Demonstrates that a single provider instance is reusable across calls with different directory lists -
        // the whole point of moving overrideDirectories from the constructor to the method parameter.
        Context.ResolveResult = Context.Provider.Resolve([OverridableContentProviderContextBuilder.FirstOverrideDirectory], "item.txt");

    It should_not_find_the_item_in_an_unrelated_directory = () =>
        Context.ResolveResult.Source.ShouldEqual(OverridableContentSource.NotFound);
}
