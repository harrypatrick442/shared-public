using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{
    [DataContract]
    public class PermissionException : Exception
    {
        public PermissionException(string message) : base(message) {

        }
        protected PermissionException() { }

    }
}
