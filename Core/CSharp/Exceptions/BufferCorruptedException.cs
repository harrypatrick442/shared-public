using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{
    public class BufferCorruptedException : Exception
    {
        public BufferCorruptedException(string message) : base(message) { }
        public BufferCorruptedException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
