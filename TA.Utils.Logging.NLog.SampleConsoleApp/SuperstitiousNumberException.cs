// This file is part of the TA.Utils project
// Copyright © 2016-2020 Tigra Astronomy, all rights reserved.
// File: SuperstitiousNumberException.cs  Last modified: 2020-07-16@20:38 by Tim Long

using System;

namespace TA.Utils.Logging.NLog.SampleConsoleApp
    {
    [Serializable]
    public class SuperstitiousNumberException : Exception
        {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public SuperstitiousNumberException() { }

        public SuperstitiousNumberException(string message) : base(message) { }

        public SuperstitiousNumberException(string message, Exception inner) : base(message, inner) { }
        }
    }