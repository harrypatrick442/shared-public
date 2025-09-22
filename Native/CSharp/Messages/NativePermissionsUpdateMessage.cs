using Core.Messages.Messages;
using Native.DataMemberNames.Messages;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace FilesRelayNative.Messages
{
    [DataContract]
	public class NativePermissionsUpdateMessage : TypedMessageBase
    {
        [JsonPropertyName(NativePermissionsUpdateMessageDataMemberNames.HasAllRequired)]
        [JsonInclude]
        [DataMember(Name = NativePermissionsUpdateMessageDataMemberNames.HasAllRequired)]
        public bool HasAllRequired { get; protected set; }
        public NativePermissionsUpdateMessage(
            bool hasAllRequired)
        {
            HasAllRequired = hasAllRequired;
            _Type = MessageTypes.MessageTypes.NativePermissionsUpdate;
        }
        protected NativePermissionsUpdateMessage() { 
            
        }
    }
}
