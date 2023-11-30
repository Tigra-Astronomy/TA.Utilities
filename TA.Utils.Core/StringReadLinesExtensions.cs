// This file is part of the TA.Utils project
// Copyright © 2015-2023 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: StringReadLinesExtensions.cs  Last modified: 2023-11-13@17:19 by Tim Long

using System.Collections.Generic;
using System.IO;

namespace TA.Utils.Core;

public static class StringReadLinesExtension
    {
    public static IEnumerable<string> GetLines(this string text)
        {
        return GetLines(new StringReader(text));
        }

    public static IEnumerable<string> GetLines(this Stream stm)
        {
        return GetLines(new StreamReader(stm));
        }

    public static IEnumerable<string> GetLines(this TextReader reader)
        {
        string line;
        while ((line = reader.ReadLine()) != null)
            yield return line;
        reader.Dispose();
        }
    }