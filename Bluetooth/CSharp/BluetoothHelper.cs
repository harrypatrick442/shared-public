using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using InTheHand.Net;
using System.Reactive.Linq;
using Bluetooth.Messages;
using Native.Messaging;
namespace Bluetooth
{
    public static class BluetoothHelper
    {

        public static BluetoothDevice[] Scan(Func<BluetoothDevice, bool>? match)
        {
            // Initialize Bluetooth client
            BluetoothClient bluetoothClient = new BluetoothClient();

            // Discover nearby Bluetooth devices
            BluetoothDeviceInfo[] devices = bluetoothClient.DiscoverDevices();
            return devices
                .Select(d => 
                    new BluetoothDevice(
                        d.DeviceName, 
                        d.DeviceAddress.ToString()))
                .Where(d => match == null || match!(d))
                .ToArray();

        }
        public static ConnectedBluetoothDeviceHandle Connect(string bluetoothAddress,
            Func<RegistrationMessageHandler> getRegistrationMessageHandler) {
                Guid serviceClass = BluetoothService.SerialPort;
                var address = BluetoothAddress.Parse(bluetoothAddress);
            BluetoothClient client = new BluetoothClient();
            var devices = client.DiscoverDevices(255, false, true, false);
            // no scan, just known devices
            var device = devices.FirstOrDefault(d => d.DeviceAddress.ToString() == bluetoothAddress);
            if (device == null || !device.Authenticated)
            {
                if (BluetoothSecurity.PairRequest(address, null))
                {
                    throw new BluetoothException(BluetoothFailedReason.Pairing);
                }
            }
            BluetoothEndPoint endPoint = new BluetoothEndPoint(address, serviceClass);
            BluetoothClient serialClient = new BluetoothClient();
            serialClient.Connect(endPoint);
            return new ConnectedBluetoothDeviceHandle(serialClient, getRegistrationMessageHandler());

        }
    }
}
