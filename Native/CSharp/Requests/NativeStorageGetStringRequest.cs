using Core.Messages.Messages;
using Native.DataMemberNames.Requests;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Native.Requests
{
    [DataContract]
    public class NativeStorageGetStringRequest:TicketedMessageBase
    {
        [JsonPropertyName(NativeStorageGetStringRequestDataMemberNames.Key)]
        [JsonInclude]
        [DataMember(Name = NativeStorageGetStringRequestDataMemberNames.Key)]
        public string Key { get; protected set; }
        public NativeStorageGetStringRequest(string key) : base(global::MessageTypes.MessageTypes.NativeStorageGetString)
        {
            Key = key;
        }
        protected NativeStorageGetStringRequest():base(global::MessageTypes.MessageTypes.NativeStorageGetString) { 
            
        }
    }
}
