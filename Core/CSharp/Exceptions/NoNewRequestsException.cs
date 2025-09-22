using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{
    [DataContract]
    public class NoNewRequestsException : Exception
    {
        public NoNewRequestsException(string message) : base(message)
        {

        }
        public NoNewRequestsException() : base()
        {
        }

    }
}
