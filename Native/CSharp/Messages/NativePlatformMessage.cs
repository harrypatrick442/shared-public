using Core.Messages.Messages;
using Native.DataMemberNames.Messages;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Platform = Core.Enums.Platform;
namespace Native.Messages
{
    [DataContract]
	public class NativePlatformMessage : TypedMessageBase
    {
        [JsonPropertyName(NativePlatformMessageDataMemberNames.Platform)]
        [JsonInclude]
        [DataMember(Name = NativePlatformMessageDataMemberNames.Platform)]
        public Platform Platform { get; protected set; }
        public NativePlatformMessage(Platform platform)
        {
            Platform = platform;
            _Type = MessageTypes.NativePlatform;
        }
        protected NativePlatformMessage() { 
            
        }
    }
}
