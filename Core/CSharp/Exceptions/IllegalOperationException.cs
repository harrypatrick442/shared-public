using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{

    [DataContract]
    public class IllegalOperationException : Exception
    {
        protected IllegalOperationException() { }
        public IllegalOperationException(string message):base(message) { 
        }
        public IllegalOperationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
