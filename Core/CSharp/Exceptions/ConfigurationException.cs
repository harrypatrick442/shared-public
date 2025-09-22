using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{
    [DataContract]
    public class ConfigurationException:Exception
    {
        protected ConfigurationException() : base() { }
        public ConfigurationException(string message) : base(message) { }
        public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
