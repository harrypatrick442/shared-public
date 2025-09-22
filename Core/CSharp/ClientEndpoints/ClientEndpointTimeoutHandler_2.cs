using Shutdown;
using Core.Timing;
using System;
using Timer = System.Timers.Timer;
namespace Core.ClientEndpoints
{
    public sealed class ClientEndpointTimeoutHandler_2<TClientEndpoint> 
        where TClientEndpoint:ITimeoutableClientEndpoint
    {
        private Timer _Timer;
        private Func<TClientEndpoint[]> _GetClientEndpointsSnapshot;
        private Action<TClientEndpoint> _RemoveClientEndpoint;
        public ClientEndpointTimeoutHandler_2(Func<TClientEndpoint[]> getClientEndpointsSnapshot, Action<TClientEndpoint> removeClientEndpoint)
        {
            _GetClientEndpointsSnapshot = getClientEndpointsSnapshot;
            _RemoveClientEndpoint = removeClientEndpoint;
            StartTimeout();
            ShutdownManager.Instance.Add(Dispose, ShutdownOrder.ClientEndpointTimeoutHandler);
        }
        private void StartTimeout() {
            _Timer = new Timer(GlobalConstants.Timeouts.CLIENT_ENDPOINT_TIMEOUT_HANDLER_INTERVAL_DO_TIMEOUTS_MILLISECONDS);
            _Timer.Elapsed += DoTimeouts;
            _Timer.AutoReset = true;
            _Timer.Enabled = true;
            _Timer.Start();
        }
        private void DoTimeouts(object sender, EventArgs e) {
            TClientEndpoint[] clientEndpoints = _GetClientEndpointsSnapshot();
            long millisecondsUTCNow = TimeHelper.MillisecondsNow;
            foreach (TClientEndpoint clientEndpoint in clientEndpoints) {
                if (clientEndpoint.TimeoutAtMillisecondsUTC > millisecondsUTCNow)
                    continue;
                _RemoveClientEndpoint(clientEndpoint);
            }
        }
        public void Dispose() {
            _Timer.Stop();
            _Timer.Dispose();
        }
    }
}
