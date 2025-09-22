using MessageTypes.Attributes;

namespace Bluetooth.DataMemberNames.Messages
{
    [MessageType(MessageTypes.BluetoothDeviceDisconnected)]
    public static class BluetoothDeviceDisconnectedMessageDataMemberNames
    {
        public const string Address = "a";
    }
}