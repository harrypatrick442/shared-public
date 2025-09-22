using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{
    [DataContract]
    public class BusyException : Exception
    {
        public BusyException(string message) : base(message) {

        }
        protected BusyException() { }

    }
}
