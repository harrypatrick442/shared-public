using System;

namespace Core.Exceptions
{
    public class IllegalValueException : Exception
    {
        public IllegalValueException(string message) : base(message)
        {

        }
        public IllegalValueException(string message, Exception ex) : base(message, ex)
        {

        }

    }
}
