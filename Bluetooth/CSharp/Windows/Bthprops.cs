using System;
using System.Runtime.InteropServices;

namespace Bluetooth.Windows
{
    internal static class Bthprops
    {
        [DllImport("Bthprops.cpl", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr BluetoothFindFirstDevice(IntPtr searchParams, IntPtr deviceInfo);

        [DllImport("Bthprops.cpl", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool BluetoothFindNextDevice(IntPtr hFind, IntPtr deviceInfo);

        [DllImport("Bthprops.cpl", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool BluetoothFindDeviceClose(IntPtr hFind);

        [DllImport("Bthprops.cpl", SetLastError = true)]
        public static extern IntPtr BluetoothFindFirstRadio(IntPtr searchParams, out IntPtr hRadio);

        [DllImport("Bthprops.cpl", SetLastError = true)]
        public static extern bool BluetoothFindNextRadio(IntPtr hFind, out IntPtr hRadio);

        [DllImport("Bthprops.cpl", SetLastError = true)]
        public static extern bool BluetoothFindRadioClose(IntPtr hFind);

        [DllImport("Bthprops.cpl", SetLastError = true)]
        public static extern bool BluetoothEnableDiscovery(IntPtr hRadio, bool fEnabled);
    }
}
