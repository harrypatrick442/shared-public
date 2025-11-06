using Logging;
using Shutdown;
using Core.Exceptions;
using Core.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Initialization.Exceptions;
using DependencyManagement;
using ConfigurationCore;

namespace Core.ClientEndpoints
{
    public sealed class ClientEndpointTimeoutHandler
    {
        private static ClientEndpointTimeoutHandler _Instance;
        private HashSet<ITimeoutableClientEndpoint> _ClientEndpoints;
        private CancellationTokenSource _CancellationTokenSourceDisposed = new CancellationTokenSource();
        public static ClientEndpointTimeoutHandler Initialize() {
            if (_Instance != null) throw new AlreadyInitializedException(nameof(ClientEndpointTimeoutHandler));
            _Instance = new ClientEndpointTimeoutHandler();
            return _Instance;
        }
        public static ClientEndpointTimeoutHandler Instance { 
            get { 
                if (_Instance == null) throw new NotInitializedException(nameof(ClientEndpointTimeoutHandler));
                return _Instance;
            } 
        }
        private ClientEndpointTimeoutHandler()
        {
            _ClientEndpoints = new HashSet<ITimeoutableClientEndpoint>();
            ShutdownManager.Instance.Add(Dispose, ShutdownOrder.ClientEndpointTimeoutHandler);
            new Thread(_TimeoutLooper).Start();
        }
        public void Add(ITimeoutableClientEndpoint clientEndpoint) {
            lock (_ClientEndpoints)
            {
                if (_CancellationTokenSourceDisposed.IsCancellationRequested) return;
                _ClientEndpoints.Add(clientEndpoint);
            }
            clientEndpoint.OnDisposed += _HandleClientEndpointDisposed;
        }
        private void _HandleClientEndpointDisposed(object sender, EventArgs e) {
            ITimeoutableClientEndpoint clientEndpoint = (ITimeoutableClientEndpoint)sender;
            clientEndpoint.OnDisposed -= _HandleClientEndpointDisposed;
            lock (_ClientEndpoints)
            {
                _ClientEndpoints.Remove(clientEndpoint);
            }
        }
        private void _TimeoutLooper() {
            int sleepBetweenDisposedCheckInterval = 
                    DependencyManager.Get<IIntervalsConfiguration>().SleepBetweenDisposedCheckIntervals;
            while (true)
            {
                try
                {
                    if (_CancellationTokenSourceDisposed.IsCancellationRequested) return;
                    int n = DependencyManager.Get<ITimeoutsConfiguration>().ClientEndpointTimeoutHandlerIntervalDoTimeoutsMilliseconds
                        / sleepBetweenDisposedCheckInterval;
                    for (int i = 0; i < n; i++)
                    {
                        Thread.Sleep(sleepBetweenDisposedCheckInterval);
                        if (_CancellationTokenSourceDisposed.IsCancellationRequested) return;
                    }
                    _DoTimeouts();
                }
                catch (Exception ex) {
                    Logs.Default.Error(ex);
                }
            }
        }
        private void _DoTimeouts() {
            ITimeoutableClientEndpoint[] clientEndpoints;
            lock (_ClientEndpoints) {
                clientEndpoints = _ClientEndpoints.ToArray();
            }
            long millisecondsUTCNow = TimeHelper.MillisecondsNow;
            foreach (ITimeoutableClientEndpoint clientEndpoint in clientEndpoints) {
                if (clientEndpoint.TimeoutAtMillisecondsUTC > millisecondsUTCNow)
                    continue;
                clientEndpoint.OnDisposed -= _HandleClientEndpointDisposed;
                try
                {
                    clientEndpoint.Dispose();
                }
                catch (Exception ex) {
                    Logs.Default.Error(ex);
                }
                _ClientEndpoints.Remove(clientEndpoint);
            }
        }
        public void Dispose() {
            ITimeoutableClientEndpoint[] clientEndpoints;
            lock (_ClientEndpoints)
            {
                if (_CancellationTokenSourceDisposed.IsCancellationRequested) return;
                _CancellationTokenSourceDisposed.Cancel();
                clientEndpoints = _ClientEndpoints.ToArray();
                _ClientEndpoints.Clear();
            }
            foreach (ITimeoutableClientEndpoint clientEndpoint in clientEndpoints) {
                clientEndpoint.OnDisposed -= _HandleClientEndpointDisposed;
            }
        }
    }
}
