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
        public NativeStorageGetStringRequest(string key) : base(MessageTypes.NativeStorageGetString)
        {
            Key = key;
        }
        protected NativeStorageGetStringRequest():base(MessageTypes.NativeStorageGetString) { 
            
        }
    }
}
