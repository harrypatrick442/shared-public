using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using Tmds.DBus;

namespace Bluetooth.Windows
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct BLUETOOTH_DEVICE_INFO
    {
        public int dwSize;
        public long Address;
        public uint ulClassofDevice;
        [MarshalAs(UnmanagedType.Bool)] public bool fConnected;
        [MarshalAs(UnmanagedType.Bool)] public bool fRemembered;
        [MarshalAs(UnmanagedType.Bool)] public bool fAuthenticated;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 248)] public string szName;
    }

}
