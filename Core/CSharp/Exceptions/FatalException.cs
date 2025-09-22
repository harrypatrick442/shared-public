using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{
    [DataContract]
    public class FatalException : Exception
    {
        protected FatalException() : base() { }
        public FatalException(string message) : base(message) { }
        public FatalException(string message, Exception innerException) : base(message, innerException) { }
    }
}
