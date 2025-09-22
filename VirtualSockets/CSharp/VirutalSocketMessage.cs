using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.Messages.Messages;
using VirtualSockets.DataMemberNames;

namespace Core.VirtualSockets
{
    [DataContract]
    public class VirtualSocketMessage:TypedMessageBase
    {
        private long _Id;
        [JsonPropertyName(VirtualSocketMessageDataMemberNames.Id)]
        [JsonInclude]
        [DataMember(Name = VirtualSocketMessageDataMemberNames.Id)]
        public long Id { get { return _Id; } protected set { _Id = value; } }
        private string _InternalType;
        [JsonPropertyName(VirtualSocketMessageDataMemberNames.InternalType)]
        [JsonInclude]
        [DataMember(Name = VirtualSocketMessageDataMemberNames.InternalType)]
        public string InternalType { get { return _InternalType; } protected set { _InternalType = value; } }
        private string _Payload;
        [JsonPropertyName(VirtualSocketMessageDataMemberNames.Payload)]
        [JsonInclude]
        [DataMember(Name= VirtualSocketMessageDataMemberNames.Payload)]
        public string Payload { get { return _Payload; } protected set { _Payload = value; } }
        public VirtualSocketMessage(long id, string payload, string type = null) {
            _Id = id;
            _Payload = payload;
            _InternalType = type;
            _Type = global::MessageTypes.MessageTypes.VirtualSocketMessage;
        }
        protected VirtualSocketMessage() { }
    }
}
