using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{
    [DataContract]
    public class MustWaitToRetryException : AuthenticationException
    {
        private int _SecondsDelay;
        public int SecondssDelay { get { return _SecondsDelay; } }
        public MustWaitToRetryException(string message, int secondsDelay) : base(message) {
            _SecondsDelay = secondsDelay;
        }
        protected MustWaitToRetryException() { }

    }
}
