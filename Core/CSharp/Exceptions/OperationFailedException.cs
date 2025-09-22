using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{
    [DataContract]
    public class OperationFailedException:InvalidOperationException
    {
        protected OperationFailedException() : base() { }
        public OperationFailedException(string message) : base(message) { }
        public OperationFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
