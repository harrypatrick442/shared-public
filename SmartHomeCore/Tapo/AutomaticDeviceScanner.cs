using Core.Exceptions;
using SmartHomeCore.Tapo.Events;
using System.Timers;
using Timer = System.Timers.Timer;
namespace SmartHomeCore.Tapo
{
    public sealed class AutomaticDevicesScanner
    {
        private int INTERVAL_MILLISECONDS = 30 * 1000;
        private static readonly object _LockObjectInstance = new object();
        private static AutomaticDevicesScanner? _Instance;
        public static AutomaticDevicesScanner Instance
        {
            get
            {
                lock (_LockObjectInstance)
                {
                    if (_Instance == null)
                    {
                        _Instance = new AutomaticDevicesScanner();
                    }
                    return _Instance;
                }
            }
        }
        private Timer _TimerScan;
        private HashSet<IDeviceConnectedStatus> _Devices;
        private static readonly object _LockObjectUpdateState = new object();
        private AutomaticDevicesScanner()
        {
            _Devices = new HashSet<IDeviceConnectedStatus>();
            _TimerScan = new Timer();
            _TimerScan.Interval = INTERVAL_MILLISECONDS;
            _TimerScan.Elapsed += TimerElapsed;
            _TimerScan.AutoReset = true;
            _TimerScan.Start();
        }
        public void AddScanTriggeringDevice(IDeviceConnectedStatus device)
        {
            lock (_LockObjectUpdateState)
            {
                if (!_Devices.Add(device)) return;
                device.ConnectedChanged += DeviceConnectedChanged;
                if (!device.Connected)
                {
                    _TimerScan.Start();
                }
            }
        }
        public void RemoveScanTriggeringDevice(IDeviceConnectedStatus device)
        {
            lock (_LockObjectUpdateState)
            {
                device.ConnectedChanged -= DeviceConnectedChanged;
                _Devices.Remove(device);
            }
        }
        private bool HasUnconnectedDevices()
        {
            return _Devices.Where(d => !d.Connected).Any();
        }
        private void DeviceConnectedChanged(object? sender, ConnectedChangedEventArgs? e)
        {
            if (e.Connected)
            {
                return;
            }
            lock (_LockObjectUpdateState)
            {
                _TimerScan.Start();
            }
        }
        private void TimerElapsed(object? sender, ElapsedEventArgs e)
        {
            TapoDevicesScanner.Instance.Scan();
            lock (_LockObjectUpdateState)
            {
                if (!HasUnconnectedDevices())
                {
                    _TimerScan.Stop();
                }
            }
        }
    }
}
