using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using Tmds.DBus;

namespace Bluetooth.Windows
{

    [StructLayout(LayoutKind.Sequential)]
    internal struct BLUETOOTH_DEVICE_SEARCH_PARAMS
    {
        public int dwSize;
        public bool fReturnAuthenticated;
        public bool fReturnRemembered;
        public bool fReturnUnknown;
        public bool fReturnConnected;
        public bool fIssueInquiry;
        public byte cTimeoutMultiplier;
        public IntPtr hRadio;
    }
}
