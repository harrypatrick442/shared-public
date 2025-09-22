using System;

namespace Core.Exceptions
{
    public class OutOfResourcesException : Exception
    {
        public OutOfResourcesException(string message) : base(message)
        {

        }
        public OutOfResourcesException(string message, Exception ex) : base(message, ex)
        {

        }
    }
}
