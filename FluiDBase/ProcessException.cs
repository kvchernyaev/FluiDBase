using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace FluiDBase
{
    public class ProcessException : Exception
    {
        public ProcessException(string message)
            : base(message) { }


        //[StringFormatMethod("message")]
        public ProcessException(string message, params object[] args)
            : this(string.Format(message, args)) { }


        public ProcessException(string message, Exception innerException)
            : base(message, innerException) { }


        protected ProcessException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}