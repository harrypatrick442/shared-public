using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{
    [DataContract]
    public class DependencyException:Exception
    {
        protected DependencyException() : base() { }
        public DependencyException(string message) : base(message) { }
        public DependencyException(string message, Exception innerException) : base(message, innerException) { }
    }
}
