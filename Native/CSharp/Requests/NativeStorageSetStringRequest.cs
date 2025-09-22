using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.Messages.Messages;
using Native.DataMemberNames.Requests;

namespace Native.Requests
{
    [DataContract]
    public class NativeStorageSetStringRequest:TicketedMessageBase
    {
        [JsonPropertyName(NativeStorageSetStringRequestDataMemberNames.Key)]
        [JsonInclude]
        [DataMember(Name = NativeStorageSetStringRequestDataMemberNames.Key)]
        public string Key { get; protected set; }
        [JsonPropertyName(NativeStorageSetStringRequestDataMemberNames.Value)]
        [JsonInclude]
        [DataMember(Name = NativeStorageSetStringRequestDataMemberNames.Value)]
        public string Value { get; protected set; }
        public NativeStorageSetStringRequest(string key, string value) : base(global::MessageTypes.MessageTypes.NativeStorageSetString) {
            Key = key;
            Value = value;
        }
        protected NativeStorageSetStringRequest() : base(global::MessageTypes.MessageTypes.NativeStorageSetString)
        {

        }
    }
}
