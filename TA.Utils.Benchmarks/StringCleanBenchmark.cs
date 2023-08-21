// This file is part of the TA.Utils project
// Copyright © 2016-2023 Timtek Systems Limited, all rights reserved.
// File: StringCleanBenchmark.cs  Last modified: 2023-08-14@02:34 by Tim Long

using System.Text;
using BenchmarkDotNet.Attributes;

namespace TA.Utils.Benchmarks;

public class StringCleanBenchmark
    {
    [Params("aeiou", ".,", "Lorem ipsum dolor sit amet")]
    public string clean;
    public string source = @"
Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam felis eros, mattis eget libero vel, congue convallis felis. Nam vel finibus sapien, a iaculis nunc. Sed iaculis interdum feugiat. Sed ut mollis augue. Pellentesque urna ante, suscipit vel vestibulum quis, maximus non tortor. Cras ipsum neque, molestie id justo in, lobortis varius mauris. Etiam mollis pulvinar ligula, ut porta lorem eleifend vel. Integer metus mi, elementum ac aliquam placerat, feugiat ac ex. Quisque cursus sapien sed augue elementum, vel tristique lectus aliquam. Ut nunc elit, tristique at odio vitae, euismod gravida elit. Aenean tellus ligula, eleifend nec nisl sed, laoreet dapibus lectus.

Interdum et malesuada fames ac ante ipsum primis in faucibus. Vivamus sit amet diam suscipit, placerat nisi vel, consectetur tellus. Sed pharetra magna eget nibh sollicitudin vehicula. Curabitur vel libero bibendum, lobortis est ut, pretium eros. Quisque ac condimentum massa. Suspendisse sit amet interdum metus, id hendrerit lorem. Nam id urna sagittis, rhoncus nisi sit amet, sodales neque. Nulla sit amet rutrum nibh. Nulla vel pharetra ipsum, ac gravida odio. Aliquam erat volutpat. In ut faucibus sapien. Aliquam accumsan consectetur ligula gravida feugiat. Nam dictum volutpat quam eget ullamcorper. Nam sollicitudin mauris quis lorem dictum, quis faucibus neque posuere. Aliquam erat volutpat.

In sit amet quam molestie, scelerisque nisi vel, semper nisl. Nam posuere quam odio, ut congue nulla venenatis vel. Proin eu lacus et justo luctus fringilla. In hac habitasse platea dictumst. Donec ante nisl, molestie sit amet justo nec, dapibus iaculis mi. Quisque congue, leo et elementum blandit, mauris nulla cursus enim, et bibendum quam metus eget turpis. Nam fringilla arcu magna, at lacinia quam ornare nec. Nam hendrerit varius egestas. Nam magna tellus, ultrices in nisl placerat, placerat ultrices eros. Cras tincidunt, quam nec tempor vehicula, sem eros fermentum ipsum, vitae laoreet augue felis in ante.

Morbi a lorem ultrices, tincidunt magna at, viverra nulla. Proin at lorem sit amet elit lacinia ullamcorper at id neque. Etiam vel mauris eget nisi egestas efficitur. Etiam lectus orci, vestibulum non dui ac, placerat molestie mi. Praesent laoreet quam sit amet augue sagittis sagittis. Maecenas sed porta lorem, at ornare ligula. Phasellus blandit ex id vulputate condimentum. Vestibulum et justo diam. Maecenas molestie a massa mattis ultrices. Quisque non velit nec turpis lobortis vulputate. Duis vitae elementum ipsum, a dignissim nisi.

Ut tincidunt tincidunt nibh at porttitor. In hac habitasse platea dictumst. Morbi nec lobortis nunc, porttitor tincidunt felis. Praesent accumsan, neque eu bibendum egestas, mauris odio congue massa, eu rutrum ipsum leo sit amet purus. Donec finibus porta tristique. Aliquam molestie arcu ante, quis mattis velit bibendum sit amet. Suspendisse viverra sodales pellentesque. Nulla lorem ligula, vestibulum gravida neque vel, consectetur rutrum lorem. Aliquam malesuada et nibh id feugiat. Nam eget aliquam purus. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Aliquam at porttitor mauris. Sed non dictum sem.
";

    [Benchmark]
    public string CleanWithHashMap()
        {
        if (string.IsNullOrEmpty(source))
            return string.Empty;
        if (string.IsNullOrEmpty(clean))
            return source;
        // HashSet.Contains is O(1) whereas string.Contains is O(n)
        var cleanSet = new HashSet<char>(clean);
        var cleanString = new StringBuilder(source.Length);
        foreach (var ch in source)
            if (!cleanSet.Contains(ch))
                cleanString.Append(ch);
        return cleanString.ToString();
        }

    [Benchmark]
    public string CleanWithContains()
        {
        if (string.IsNullOrEmpty(source))
            return string.Empty;
        if (string.IsNullOrEmpty(clean))
            return source;
        var cleanString = new StringBuilder(source.Length);
        foreach (var ch in source)
            if (!clean.Contains(ch))
                cleanString.Append(ch);
        return cleanString.ToString();
        }
    }