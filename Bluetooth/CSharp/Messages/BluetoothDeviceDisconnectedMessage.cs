using Core.Messages.Messages;
using System.Runtime.Serialization;
namespace Bluetooth.Messages
{
    [DataContract]
	public class BluetoothDeviceDisconnectedMessage : TypedMessageBase
    {
        public BluetoothDeviceDisconnectedMessage()
        {
            _Type = MessageTypes.BluetoothDeviceDisconnected;
        }
    }
}
