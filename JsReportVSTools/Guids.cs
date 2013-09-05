// Guids.cs
// MUST match guids.h
using System;

namespace JsReportVSTools
{
    static class GuidList
    {
        public const string guidJsReportVSToolsPkgString = "60546472-b12d-4ad6-b5ea-74401d44d9fc";
        public const string guidJsReportVSToolsCmdSetString = "6bc24cf1-a4b0-4ee3-9482-e5c199ef35df";
        public const string guidJsReportVSToolsEditorFactoryString = "d191ce00-f98f-4f4e-90d4-e5b43e22d757";

        public static readonly Guid guidJsReportVSToolsCmdSet = new Guid(guidJsReportVSToolsCmdSetString);
        public static readonly Guid guidJsReportVSToolsEditorFactory = new Guid(guidJsReportVSToolsEditorFactoryString);
    };
}