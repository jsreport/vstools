using System;
using System.Runtime.Serialization;

namespace JsReportVSTools.Impl
{
    [Serializable]
    public class MissingJsReportDllException : Exception
    {

        public MissingJsReportDllException()
        {
        }


        public MissingJsReportDllException(string message) : base(message)
        {
        }

        public MissingJsReportDllException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MissingJsReportDllException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}