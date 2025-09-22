using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{
    [DataContract]
    public class RejectedClientException : Exception
    {
        protected RejectedClientException() : base() { }
        public RejectedClientException(string message) : base(message) { }
    }
}
