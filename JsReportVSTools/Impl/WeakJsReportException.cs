using System;
using System.Runtime.Serialization;

namespace JsReportVSTools.Impl
{
    [Serializable]
    public class WeakJsReportException : Exception
    {

        public WeakJsReportException()
        {
        }


        public WeakJsReportException(string message) : base(message)
        {
        }

        public WeakJsReportException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WeakJsReportException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}