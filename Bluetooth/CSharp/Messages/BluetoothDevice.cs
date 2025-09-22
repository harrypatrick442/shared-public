using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Bluetooth.DataMemberNames.Messages;

namespace Bluetooth.Messages
{
    [DataContract]
    public class BluetoothDevice
    {
        [JsonPropertyName(BluetoothDeviceDataMemberNames.Name)]
        [JsonInclude]
        [DataMember(Name = BluetoothDeviceDataMemberNames.Name)]
        public string Name { get; protected set; }
        [JsonPropertyName(BluetoothDeviceDataMemberNames.Address)]
        [JsonInclude]
        [DataMember(Name = BluetoothDeviceDataMemberNames.Address)]
        public string Address { get; protected set; }
        public BluetoothDevice(string name, string address)
        {
            Name = name;
            Address = address;
        }
        protected BluetoothDevice()
        {

        }
    }
}
