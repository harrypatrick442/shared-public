using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{
    [DataContract]
    public class BadCredentialsException : AuthenticationException
    {
        public BadCredentialsException(string message) : base(message)
        {
        }
        public BadCredentialsException() : base("Bad credentials")
        {
        }

    }
}
