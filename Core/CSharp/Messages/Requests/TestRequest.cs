using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.DataMemberNames;
using Core.DataMemberNames.Messages;
using Core.Messages.Messages;

namespace Core.Messages.Requests
{
    [DataContract]
    public class TestMessage : TicketedMessageBase
    {

        public TestMessage(string value):base(MessageTypes.Test)
        {
            _Value = value;
        }
        public TestMessage(string value, long ticket) : base(TicketedMessageType.Ticketed)
        {
            _Value = value;
            _Ticket = ticket;
        }
        protected TestMessage() : base(TicketedMessageType.Ticketed)
        { }
        private string _Value;

        [JsonPropertyName(TestMessageDataMemberNames.Value)]
        [JsonInclude]
        [DataMember(Name = TestMessageDataMemberNames.Value)]
        public string Value { get { return _Value; }protected set { _Value = value; } }
    }
}
