using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{
    public class BackedUpException : OutOfResourcesException
    {
        public BackedUpException(string message) : base(message)
        {

        }
        public BackedUpException(string message, Exception ex) : base(message, ex)
        {

        }

    }
}
