namespace Bluetooth
{
    public class DeviceDiscoveryArgs
    {
        public bool Authenticated { get; }
        public bool Remembered { get; }
        public bool Unknowns { get; }
        public bool Connected { get; }
        public DeviceDiscoveryArgs(bool authenticated, bool remembered, 
            bool unknowns, bool connected) { 
            Authenticated = authenticated;
            Remembered = remembered;
            Unknowns = unknowns;
            Connected = connected;
        }
    }
}
