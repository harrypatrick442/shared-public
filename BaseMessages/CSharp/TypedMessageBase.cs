using Core.DataMemberNames;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.Interfaces;

namespace Core.Messages.Messages
{
    [DataContract]
    [KnownType(typeof(TypedMessageBase))]
    public class TypedMessageBase:ITypedMessage
    {
        protected string _Type;
        [JsonPropertyName(MessageTypeDataMemberName.Value)]
        [JsonInclude]
        [DataMember(Name = MessageTypeDataMemberName.Value)]
        public string Type { get { return _Type; } protected set { _Type = value; } }
    }
}
