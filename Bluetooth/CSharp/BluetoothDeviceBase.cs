using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using Tmds.DBus;

namespace Bluetooth
{
    public abstract class BluetoothDeviceBase
    {
        public string Name{ get;}
        public long Address { get; }
        protected BluetoothDeviceBase(string name, long address) {
            Name = name;
            Address = address;
        }
    }
}
