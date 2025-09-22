using System;

namespace Core.Exceptions
{
    public class LazyMessageException : Exception
    {
        private readonly Func<string> _generateMessage;

        public LazyMessageException(Func<string> generateMessage)
            : base(null) // Base exception message is null; the message is generated lazily.
        {
            _generateMessage = generateMessage;
        }

        public LazyMessageException(Func<string> generateMessage, Exception innerException)
            : base(null, innerException) // Pass the inner exception to the base constructor.
        {
            _generateMessage = generateMessage;
        }

        public override string Message => _generateMessage?.Invoke() ?? "An error occurred.";
    }
}
