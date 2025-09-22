using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using Tmds.DBus;

namespace Bluetooth
{
    public delegate bool DelegateDiscoveredDevice(BluetoothDeviceBase device);
}
