using MessageTypes.Attributes;
using SharedMasterSlaveUARTMessages.DataMemberNames.Messages;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace SharedMasterSlaveUARTMessages
{
    [DataMemberNamesClass(typeof(HasBusMessageDataMemberNames))]
    public class HasBusMessage
    {
        [JsonPropertyName(HasBusMessageDataMemberNames.Target)]
        [JsonInclude]
        [DataMember(Name = HasBusMessageDataMemberNames.Target)]
        public int Target { get; protected set; }
    }
}
