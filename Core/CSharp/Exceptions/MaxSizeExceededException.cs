using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{
    public class MaxSizeExceededException : Exception
    {
        public MaxSizeExceededException(string message, Exception innerParsingException) : base(message, innerParsingException)
        {

        }
        public MaxSizeExceededException(string message) : base(message)
        {

        }
        public MaxSizeExceededException() { 
        
        }
    }
}
