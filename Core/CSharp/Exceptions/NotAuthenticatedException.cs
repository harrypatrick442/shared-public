using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{
    [DataContract]
    public class NotAuthenticatedException : AuthenticationException
    {
        public NotAuthenticatedException(string message) : base(message)
        {
        }
        public NotAuthenticatedException() : base("Bad credentials")
        {
        }

    }
}
