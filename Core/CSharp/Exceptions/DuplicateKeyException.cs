using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{
    [DataContract]
    public class DuplicateKeyException : Exception
    {
        protected DuplicateKeyException() : base() { }
        public DuplicateKeyException(string message) : base(message) { }
        public DuplicateKeyException(object key) : base($"Duplicate key {key}") { }
    }
}
