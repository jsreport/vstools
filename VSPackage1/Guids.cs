// Guids.cs
// MUST match guids.h
using System;

namespace JanBlaha.VSPackage1
{
    static class GuidList
    {
        public const string guidVSPackage1PkgString = "8fa74bbf-9b41-4e9c-ad4f-305c22281354";
        public const string guidVSPackage1CmdSetString = "5ec01479-95d3-4b90-8af5-ec66fc0353aa";
        public const string guidToolWindowPersistanceString = "de5e8b16-3898-44c5-8117-83e6a5eb0458";

        public static readonly Guid guidVSPackage1CmdSet = new Guid(guidVSPackage1CmdSetString);
    };
}