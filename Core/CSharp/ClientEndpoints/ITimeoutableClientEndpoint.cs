using System;

namespace Core.ClientEndpoints
{
    public interface ITimeoutableClientEndpoint
    {
        long TimeoutAtMillisecondsUTC { get; }
        event EventHandler OnDisposed;
        void Dispose();
    }
}
