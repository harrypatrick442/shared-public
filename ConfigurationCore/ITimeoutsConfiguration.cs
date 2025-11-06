using System.Threading.Tasks;

namespace ConfigurationCore
{
    public interface ITimeoutsConfiguration
    {
        public int ClientEndpointTimeoutHandlerIntervalDoTimeoutsMilliseconds { get; }
        public int CachedIndexHtmlExpiresAfterMilliseconds { get; }
        public int TimeoutRemoteOperation { get; }
        public int TimeoutRemoteLockingOperation { get; }
    }
}
