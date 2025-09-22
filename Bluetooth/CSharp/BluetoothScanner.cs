using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using Tmds.DBus;
using InTheHand.Net;
using InfernoDispatcher;
using Timer = System.Timers.Timer;
using InfernoDispatcher.Tasks;
using Plugin.BluetoothLE;
using System.Reactive.Linq;
namespace Bluetooth
{
    public class BluetoothScanner
    {
        public static List<BluetoothDeviceBase> ScanSync(
            DeviceDiscoveryArgs args,
            Func<BluetoothDeviceBase, bool>? shouldStop,
            int? timeoutMilliseconds) {



            var bleAdaptor1 = Plugin.BluetoothLE.CrossBleAdapter.Current;
            foreach (IScanResult result in bleAdaptor1.ScanExtra()) {
                Console.WriteLine(result.Device.Name);
                foreach (Guid serviceUuid in result.AdvertisementData.ServiceUuids) {
                    Console.WriteLine($"Service uuid: {serviceUuid}");
                }
            }
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            List<BluetoothDeviceBase> foundDevices = new List<BluetoothDeviceBase>();
            Timer? timer = null;
            if (timeoutMilliseconds != null) {
                 timer = new Timer();
                timer.Elapsed +=(e, o)=>{
                    cancellationTokenSource.Cancel();
                };
                timer.AutoReset = false;
                timer.Interval = (int)timeoutMilliseconds;
                timer.Enabled = true;
                timer.Start();
                cancellationTokenSource.Token.Register(timer.Dispose);
            }
            DelegateDiscoveredDevice discoveredDevice = (BluetoothDeviceBase device) => {
                foundDevices.Add(device);
                Console.WriteLine($"Discovered device: \"{device.Name}\"");
                if (shouldStop != null)
                {
                    return shouldStop(device);
                }
                return false;
            };
            InfernoTaskNoResult task = ScanAsync(args, discoveredDevice, 
                cancellationTokenSource.Token);
            task.Wait();
            timer?.Dispose();
            return foundDevices;
        }
        public static InfernoTaskNoResult ScanAsync(
            DeviceDiscoveryArgs args,
            DelegateDiscoveredDevice discoveredDevice,
            CancellationToken cancellationToken,
            Action<Exception>? handleExceptions = null)
        {
            if (handleExceptions == null)
            {
                handleExceptions = DefaultHandleExceptions;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
            InactiveInfernoTaskNoResult task = new InactiveInfernoTaskNoResult(null);
                ScanWindowsAsync(
                    args,
                    discoveredDevice,
                    cancellationToken,
                    task);
                return task;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                throw new NotImplementedException();
            }
            throw new NotSupportedException($"{nameof(BluetoothScanner)} for this platform");
        }
        private static void ScanWindowsAsync(
            DeviceDiscoveryArgs args, 
            DelegateDiscoveredDevice discoveredDevice,
            CancellationToken cancellationToken,
            InactiveInfernoTaskNoResult task)
        {
            try
            {
                BluetoothClient client = new BluetoothClient();
                task.ThenWhatever( (ignore) => client.Dispose());
                var callback = Create_DeviceDiscoveredCallback(
                        args, discoveredDevice, cancellationToken,
                        task
                );
                client.BeginDiscoverDevices(255, args.Authenticated, 
                    args.Remembered, args.Unknowns, args.Connected,
                    callback, client);
            }
            catch (Exception ex)
            {
                task.Fail(
                    new BluetoothException(
                        $"Windows Bluetooth scan failed: {ex.Message}", ex
                        )
                );
            }
        }
        private static void DefaultHandleExceptions(Exception ex) {
            Console.WriteLine(ex);
        }
        private static AsyncCallback Create_DeviceDiscoveredCallback(
            DeviceDiscoveryArgs args, 
            DelegateDiscoveredDevice discoveredDevice,
            CancellationToken cancellationToken,
            InactiveInfernoTaskNoResult task)
        {
            HashSet<BluetoothAddress> alreadyDiscovered = new HashSet<BluetoothAddress>();
            AsyncCallback? callback = null;
            callback = (IAsyncResult ar) =>
            {
                try
                {
                    BluetoothClient client = (BluetoothClient)ar.AsyncState!;
                    var devices = client.EndDiscoverDevices(ar);

                    foreach (var device in devices)
                    {
                        if (alreadyDiscovered.Contains(device.DeviceAddress)) {
                            continue;
                        }
                        var bluetoothDevice = new BluetoothDeviceWindows(device.DeviceName, device.DeviceAddress.ToInt64());
                        discoveredDevice.Invoke(bluetoothDevice);
                    }
                    if (cancellationToken.IsCancellationRequested) {
                        task.Success();
                        return;
                    }
                    // Continue scanning
                    client.BeginDiscoverDevices(255, args.Authenticated, args.Remembered,
                        args.Unknowns, args.Connected, callback!, client);
                }
                catch (Exception ex)
                {
                    task.Fail(new BluetoothException($"Error in device discovery callback: {ex.Message}"));
                }
            };
            return callback;
        }
        private static async Task ScanLinux()
        {
            try
            {
                var systemBus = Connection.System;
                var adapterProxy = systemBus.CreateProxy<IBlueZAdapter>("org.bluez", "/org/bluez/hci0");

                await adapterProxy.SetDiscoveryFilterAsync(new Dictionary<string, object>
                {
                    { "Transport", "bredr" }  // Only scan for Classic Bluetooth (BR/EDR)
                });

                Console.WriteLine("Starting Linux Bluetooth scan...");
                await adapterProxy.StartDiscoveryAsync();

                await Task.Delay(5000); // Scan for 5 seconds

                var devices = await adapterProxy.GetManagedObjectsAsync();
                Console.WriteLine("\nDiscovered Bluetooth Devices:");
                foreach (var (path, props) in devices)
                {
                    if (props.TryGetValue("org.bluez.Device1", out var deviceProps))
                    {
                        var name = deviceProps.ContainsKey("Name") ? (string)deviceProps["Name"] : "Unknown";
                        var address = (string)deviceProps["Address"];
                        Console.WriteLine($"- {name} ({address})");
                    }
                }

                await adapterProxy.StopDiscoveryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Linux Bluetooth scan failed: {ex.Message}");
            }
        }
    }

    // ✅ Linux BlueZ D-Bus Interface for Bluetooth Adapter
    [DBusInterface("org.bluez.Adapter1")]
    interface IBlueZAdapter : IDBusObject
    {
        Task StartDiscoveryAsync();
        Task StopDiscoveryAsync();
        Task<IDictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>>> GetManagedObjectsAsync();
        Task SetDiscoveryFilterAsync(IDictionary<string, object> properties);
    }
}
