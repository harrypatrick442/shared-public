using Initialization.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using TapoDevices.Credentials;
using TapoDevices.DeviceInfos;
namespace TapoDevices
{
    public sealed class TapoDevicesScanner
    {
        private const string BASE_IP = "192.168.0.";
        private static readonly object _InstanceLockObject = new object();
        private static TapoDevicesScanner? _Instance;
        public static TapoDevicesScanner Instance
        {
            get
            {
                lock (_InstanceLockObject)
                {
                    if (_Instance == null)
                    {
                        throw new NotInitializedException(nameof(TapoDevicesScanner));
                    }
                    return _Instance;
                }
            }
        }
        private TapoCredentials _Credentials;
        private List<DeviceInfo> _LatestScanResults;
        public static TapoDevicesScanner Initialize(TapoCredentials credentials) {
            lock (_InstanceLockObject) {
                if (_Instance != null) { 
                    throw new AlreadyInitializedException(nameof(TapoDevicesScanner));
                }
                _Instance = new TapoDevicesScanner(credentials);
                return _Instance;
            }
        }
        private readonly object _LockObjectScanning = new object();
        private Task<List<DeviceInfo>> _CurrentScanTask = null;
        private List<EventHandler<ScanCompleteEventArgs>>
            _ScanCompleteEventHandlers = new List<EventHandler<ScanCompleteEventArgs>>();
        public event EventHandler<ScanCompleteEventArgs> ScanComplete
        {
            add
            {
                lock (_ScanCompleteEventHandlers)
                {
                    _ScanCompleteEventHandlers.Add(value);
                }
            }
            remove
            {
                lock (_ScanCompleteEventHandlers)
                {
                    _ScanCompleteEventHandlers.Remove(value);
                }
            }
        }
        private void DispatchScanComplete(IEnumerable<DeviceInfo> clients)
        {
            var eventArgs = new ScanCompleteEventArgs(clients);
            EventHandler<ScanCompleteEventArgs>[] eventHandlers;
            lock (_ScanCompleteEventHandlers)
            {
                eventHandlers = _ScanCompleteEventHandlers.ToArray();
            }
            foreach (var eventHandler in eventHandlers)
            {
                eventHandler.Invoke(this, eventArgs);
            }
        }
        private TapoDevicesScanner(TapoCredentials credentials)
        {
            _Credentials = credentials;
            Task.Run(Scan);
        }
        public async Task<TapoPlug?> FindPlug(string? nickname) {
            return await FindDevice<TapoPlug>(nickname,
                (deviceInfo) => new TapoPlug(deviceInfo.IPAddress, _Credentials));
        }
        public async Task<TapoBulb?> FindBulb(string? nickname)
        {
            return await FindDevice<TapoBulb>(nickname,
                (deviceInfo) => new TapoBulb(deviceInfo.IPAddress, _Credentials));
        }
        public async Task<TDevice?> FindDevice<TDevice>(
            string? nickname, Func<DeviceInfo, TDevice> createDevice)
        where TDevice:class{
            TDevice? device;
            var matchDeviceInfo = Create_MatchDeviceInfo(nickname);
            lock (_LockObjectScanning) {
                device = matchDeviceInfo(_LatestScanResults)
                    ?.Select(createDevice)
                    .FirstOrDefault();
            }
            if (device != null) {
                return device;
            }
            return await Scan()
                .ContinueWith(r=>
                    matchDeviceInfo(r.Result)
                    ?.Select(createDevice)
                    .FirstOrDefault());

        }
        private Func<List<DeviceInfo>?, IEnumerable<DeviceInfo>?> Create_MatchDeviceInfo(string? nickname) {
            return (deviceInfos) => deviceInfos
                    ?.Where(d => d.Nickname == nickname);
        }
        public List<DeviceInfo> ScanSync() {
            var task = Scan();
            task.Wait();
            return task.Result;
        }
        public Task<List<DeviceInfo>> Scan()
        {
            lock (_LockObjectScanning)
            {
                if (_CurrentScanTask!=null)
                {
                    return _CurrentScanTask;
                }
                _CurrentScanTask = ScanForTapoDevicesAsync();
            }
            return _CurrentScanTask.ContinueWith(r=>
            {

                lock (_LockObjectScanning)
                {
                    if (!r.IsCompletedSuccessfully)
                        throw new AggregateException("Failed during scan for devices", r.Exception);
                    _LatestScanResults = r.Result;
                    _CurrentScanTask = null;
                    return _LatestScanResults;
                }
            });
        }
        public async Task<List<DeviceInfo>> ScanForTapoDevicesAsync()
        {
            List<DeviceInfo> devices = new List<DeviceInfo>();
            int maxThreads = 50;
            SemaphoreSlim semaphore = new SemaphoreSlim(maxThreads);

            Task[] tasks = new Task[254];
            for (int i = 1; i <= 254; i++)
            {
                Console.WriteLine(i);
                await semaphore.WaitAsync();
                tasks[i-1] = Task.Run(async () =>
                {
                    string ip = $"{BASE_IP}{i}";
                    try
                    {
                        bool success = await PingAddressAsync(ip);
                        if (success)
                        {
                            TapoDevice device = new TapoDevice(ip, _Credentials);
                            await device.ConnectAsync();
                            var info = await device.GetInfoAsync();
                            lock (devices)
                            {
                                devices.Add(info);
                            }
                        }
                    }
                    catch { }
                    finally
                    {
                        semaphore.Release();
                    }
                });
            }
            await Task.WhenAll(tasks);
            lock (devices)
            {
                return devices;
            }
        }

        static async Task<bool> PingAddressAsync(string address)
        {
            try
            {
                using Ping ping = new Ping();
                PingReply reply = await ping.SendPingAsync(address);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

    }
}
