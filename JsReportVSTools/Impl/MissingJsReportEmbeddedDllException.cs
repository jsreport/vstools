using System;

namespace JsReportVSTools.Impl
{
    public class MissingJsReportEmbeddedDllException : Exception
    {
        public MissingJsReportEmbeddedDllException(string message) : base(message)
        {
        }
    }
}