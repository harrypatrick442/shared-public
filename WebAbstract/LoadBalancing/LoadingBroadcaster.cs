using Logging;
using Shutdown;
using WebAbstract.MachineMetricsMesh;

namespace WebAbstract.LoadBalancing
{
    public class LoadingBroadcaster
    {
        private System.Timers.Timer _TimerBroadcastLoading;
        private double _PreviousLoadFactor;
        private LoadFactorType _LoadFactorType;
        private int[] _NodeIdsToBroadcastTo;
        private Func<double> _GetLoadFactor;
        private readonly object _LockObject = new object();
        public LoadingBroadcaster(int[] nodeIdsToBroadcastTo, 
            LoadFactorType loadFactorType, Func<double> getLoadFactor)
        {
            _NodeIdsToBroadcastTo = nodeIdsToBroadcastTo;
            _LoadFactorType = loadFactorType;
            _GetLoadFactor = getLoadFactor;
            _TimerBroadcastLoading = new System.Timers.Timer(
                Configurations.Intervals.BROADCAST_WEBSOCKET_LOADING);
            _TimerBroadcastLoading.Elapsed += BroadcastLoading;
            _TimerBroadcastLoading.AutoReset = true;
            _TimerBroadcastLoading.Enabled = true;
            _TimerBroadcastLoading.Start();
            ShutdownManager.Instance.CancellationToken.Register(_TimerBroadcastLoading.Dispose);
        }
        private void BroadcastLoading(object? sender, EventArgs e)
        {
            try
            {
                double loadFactor;
                lock (_LockObject)
                {
                    loadFactor = _GetLoadFactor();
                    if (loadFactor == _PreviousLoadFactor) return;
                }
                _PreviousLoadFactor = loadFactor;
                MachineMetricsMesh.MachineMetricsMesh.Instance.BroadcastNodeLoading(
                    _NodeIdsToBroadcastTo, _LoadFactorType, loadFactor);
            }
            catch (Exception ex)
            {
                Logs.Default.Error(ex);
            }
        }
    }
}
