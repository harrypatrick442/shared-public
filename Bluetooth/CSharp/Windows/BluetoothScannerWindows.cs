using InfernoDispatcher.Tasks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Timer = System.Timers.Timer;
using Core.PInvoke;
using Plugin.BluetoothLE;
using InTheHand.Net.Sockets;
using Bluetooth;
namespace Bluetooth.Windows
{
    internal class BluetoothScannerWindows
    {
        [DllImport("Bthprops.cpl", SetLastError = true)]
        private static extern IntPtr BluetoothFindFirstDevice(ref BLUETOOTH_DEVICE_SEARCH_PARAMS searchParams, ref BLUETOOTH_DEVICE_INFO deviceInfo);

        [DllImport("Bthprops.cpl", SetLastError = true)]
        private static extern bool BluetoothFindNextDevice(IntPtr hFind, ref BLUETOOTH_DEVICE_INFO deviceInfo);

        [DllImport("Bthprops.cpl", SetLastError = true)]
        private static extern bool BluetoothFindDeviceClose(IntPtr hFind);
        public static List<IntPtr> GetAllBluetoothRadios()
        {
            List<IntPtr> radios = new List<IntPtr>();

            IntPtr searchParamsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BLUETOOTH_FIND_RADIO_PARAMS)));
            BLUETOOTH_FIND_RADIO_PARAMS searchParams = new BLUETOOTH_FIND_RADIO_PARAMS
            {
                dwSize = Marshal.SizeOf(typeof(BLUETOOTH_FIND_RADIO_PARAMS))
            };
            Marshal.StructureToPtr(searchParams, searchParamsPtr, false);

            IntPtr hFind = Bthprops.BluetoothFindFirstRadio(searchParamsPtr, out IntPtr hRadio);
            if (hFind == IntPtr.Zero)
            {
                Console.WriteLine("No Bluetooth radios found.");
                Marshal.FreeHGlobal(searchParamsPtr);
                return radios;
            }

            do
            {
                Console.WriteLine($"Found Bluetooth Radio: {hRadio}");
                radios.Add(hRadio);
            } while (Bthprops.BluetoothFindNextRadio(hFind, out hRadio));

            Bthprops.BluetoothFindRadioClose(hFind);
            Marshal.FreeHGlobal(searchParamsPtr);
            return radios;
        }

        public static bool SetDiscoveryMode(IntPtr hRadio, bool enable)
        {
            bool result = Bthprops.BluetoothEnableDiscovery(hRadio, enable);
            if (!result)
            {
                int error = Marshal.GetLastWin32Error();
                throw new BluetoothException($"Failed to set discovery mode. Error: {error}");
            }
            else
            {
                Console.WriteLine($"Bluetooth discovery mode set to: {(enable ? "Enabled" : "Disabled")}");
            }
            return result;
        }
        public static InfernoTaskNoResult DiscoverBluetoothDevicesAsync(
            DeviceDiscoveryArgs args,
            DelegateDiscoveredDevice discoveredDevice,
            CancellationToken cancellationToken)
        {

            var radios = GetAllBluetoothRadios();
            if (radios.Count == 0)
            {
                throw new BluetoothException("No Bluetooth radios found.");
            }
            SetDiscoveryMode(radios[0], true);


            Console.WriteLine("Scanning for Bluetooth Devices...");

            using (BluetoothClient client = new BluetoothClient())
            {
                var devices = client.DiscoverDevices(255, true, true, true);

                if (devices.Length == 0)
                {
                    Console.WriteLine("No Bluetooth devices found.");
                    return null;
                }

                foreach (var device in devices)
                {
                    Console.WriteLine($"Found Device: {device.DeviceName} ({device.DeviceAddress})");
                }
            }

            Console.WriteLine("Bluetooth scan complete.");

            return null;
            Console.WriteLine("Scanning for Bluetooth devices...");

            var adapter = CrossBleAdapter.Current;
            adapter.Scan().Subscribe(scanResult =>
            {
                Console.WriteLine($"Found device: {scanResult.Device.Name} ({scanResult.Device.Uuid})");
            });

            Console.ReadLine(); // Keep app running
            return null;
            /*
            InactiveInfernoTaskNoResult task = new InactiveInfernoTaskNoResult(null);

            foreach (IntPtr radio in radios)
            {
                IntPtr searchParamsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BLUETOOTH_DEVICE_SEARCH_PARAMS)));
                BLUETOOTH_DEVICE_SEARCH_PARAMS searchParams = new BLUETOOTH_DEVICE_SEARCH_PARAMS
                {
                    dwSize = Marshal.SizeOf(typeof(BLUETOOTH_DEVICE_SEARCH_PARAMS)),
                    fReturnAuthenticated = args.Authenticated,
                    fReturnRemembered = args.Remembered,
                    fReturnUnknown = args.Unknowns,
                    fReturnConnected = args.Connected,
                    fIssueInquiry = true,  // Forces active scanning
                    cTimeoutMultiplier = 30,
                    hRadio = radio // ✅ Use the correct radio handle
                };
                Marshal.StructureToPtr(searchParams, searchParamsPtr, false);

                IntPtr deviceInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BLUETOOTH_DEVICE_INFO)));
                BLUETOOTH_DEVICE_INFO deviceInfo = new BLUETOOTH_DEVICE_INFO { dwSize = Marshal.SizeOf(typeof(BLUETOOTH_DEVICE_INFO)) };
                Marshal.StructureToPtr(deviceInfo, deviceInfoPtr, false);

                IntPtr hFind = Bthprops.BluetoothFindFirstDevice(searchParamsPtr, deviceInfoPtr);
                int errorCode = Kernel32.GetLastError();
                if (errorCode != 0)
                {
                    Marshal.FreeHGlobal(searchParamsPtr);
                    Marshal.FreeHGlobal(deviceInfoPtr);
                    task.Fail(new BluetoothException(Kernel32.GetErrorMessage(errorCode)));
                    return task;
                }

                Func<bool> checkFound = () =>
                {
                    int error = Kernel32.GetLastError();
                    if (error != 0) Console.WriteLine(Kernel32.GetErrorMessage(error));

                    if (hFind != IntPtr.Zero)
                    {
                        deviceInfo = Marshal.PtrToStructure<BLUETOOTH_DEVICE_INFO>(deviceInfoPtr);
                        BluetoothDeviceWindows foundDevice = new BluetoothDeviceWindows(
                            name: deviceInfo.szName,
                            address: deviceInfo.Address
                        );

                        if (discoveredDevice(foundDevice))
                        {
                            return true;
                        }
                    }
                    return cancellationToken.IsCancellationRequested;
                };

                Timer? timerCheck = null;
                Action cleanup = () =>
                {
                    timerCheck?.Dispose();
                    Bthprops.BluetoothFindDeviceClose(hFind);
                    Marshal.FreeHGlobal(searchParamsPtr);
                    Marshal.FreeHGlobal(deviceInfoPtr);
                };

                if (checkFound())
                {
                    cleanup();
                    task.Success();
                    return task;
                }

                timerCheck = new Timer
                {
                    Interval = 20000,
                    AutoReset = true
                };
                timerCheck.Elapsed += (o, e) =>
                {
                    bool found = Bthprops.BluetoothFindNextDevice(hFind, deviceInfoPtr);
                    if (found) Console.WriteLine("Found another device!");

                    if (checkFound())
                    {
                        cleanup();
                        task.Success();
                    }
                };
                timerCheck.Enabled = true;
                timerCheck.Start();

                cancellationToken.Register(() =>
                {
                    cleanup();
                    task.Success();
                });
            }

            return task;*/
        }
    }
}
