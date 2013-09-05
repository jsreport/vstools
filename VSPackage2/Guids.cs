// Guids.cs
// MUST match guids.h
using System;

namespace Company.VSPackage2
{
    static class GuidList
    {
        public const string guidVSPackage2PkgString = "8afc0d46-5bb3-44a0-8c0f-b51a83d8f42e";
        public const string guidVSPackage2CmdSetString = "c05dac12-0470-48d4-8edb-abe559d4c854";
        public const string guidVSPackage2EditorFactoryString = "c3d3c71b-ed61-44a8-aae5-849f0bab2651";

        public static readonly Guid guidVSPackage2CmdSet = new Guid(guidVSPackage2CmdSetString);
        public static readonly Guid guidVSPackage2EditorFactory = new Guid(guidVSPackage2EditorFactoryString);
    };
}