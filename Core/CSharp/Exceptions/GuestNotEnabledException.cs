using System.Runtime.Serialization;

namespace Core.Exceptions
{
    [DataContract]
    public class GuestNotEnabledException : AuthenticationException
    {
        public GuestNotEnabledException(string message) : base(message)
        {
        }
        public GuestNotEnabledException() : base("Guest not enabled")
        {
        }

    }
}
