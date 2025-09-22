using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{
    [DataContract]
    public abstract class AuthenticationException : Exception
    {
        public AuthenticationException(string message) : base(message)
        {

        }
        public AuthenticationException(string message, Exception ex) : base(message, ex)
        {

        }
        protected AuthenticationException() { }

    }
}
