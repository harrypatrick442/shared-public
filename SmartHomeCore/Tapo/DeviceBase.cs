using SmartHomeCore.Tapo.Events;
using TapoSharp.Clients;
namespace SmartHomeCore.Tapo
{
    public abstract class DeviceBase: IDeviceConnectedStatus
    {
        private readonly object _LockObjectDispose = new object();
        private bool _Disposed = false;
        public string DeviceId { get; }

        public bool Connected => true;

        private List<EventHandler<ConnectedChangedEventArgs>> _ConnectedChangedEventHandlers
            = new List<EventHandler<ConnectedChangedEventArgs>>();

        public event EventHandler<ConnectedChangedEventArgs> ConnectedChanged
        {
            add {
                lock (_ConnectedChangedEventHandlers)
                {
                    _ConnectedChangedEventHandlers.Add(value);
                }
            }
            remove
            {
                lock (_ConnectedChangedEventHandlers)
                {
                    _ConnectedChangedEventHandlers.Remove(value);
                }
            }
        }

        public DeviceBase(string deviceId) {
            if(string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException(nameof(deviceId));
            //_TapoClient = null;
            DeviceId = deviceId;
            TapoDevicesScanner.Instance.ScanComplete += ScanComplete;
            AutomaticDevicesScanner.Instance.AddScanTriggeringDevice(this);
        }
        private void ScanComplete(object? o, ScanCompleteEventArgs e) {
            //_TapoClient = e.TryGetByDeviceId(DeviceId);
        }
        public void Dispose() {
            lock (_LockObjectDispose)
            {
                if (_Disposed) return;
                TapoDevicesScanner.Instance.ScanComplete -= ScanComplete;
                AutomaticDevicesScanner.Instance.RemoveScanTriggeringDevice(this);
                _Disposed = true;
            }
        }
    }
}