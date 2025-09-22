using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.DataMemberNames;
namespace Core.Messages.Responses
{
    [DataContract]
    [KnownType(typeof(GenericTicketedResponse))]
    public class GenericTicketedResponse : TicketedResponseMessageBase
    {
        private string _ErrorMessage;
        [JsonPropertyName(GenericTicketedResponseDataMemberNames.ErrorMessage)]
        [JsonInclude]
        [DataMember(Name = GenericTicketedResponseDataMemberNames.ErrorMessage)]
        public string ErrorMessage { get { return _ErrorMessage; } protected set { _ErrorMessage = value; } }
        private bool _Successful;
        [JsonPropertyName(GenericTicketedResponseDataMemberNames.Successful)]
        [JsonInclude]
        [DataMember(Name = GenericTicketedResponseDataMemberNames.Successful)]
        public bool Successful { get { return _Successful; } protected set { _Successful = value; } }
        public GenericTicketedResponse(long ticket,
             string errorMessage, bool successful) : base(ticket)
        {
            _ErrorMessage = errorMessage;
            _Successful = successful;
        }
        protected GenericTicketedResponse() : base(-1) { }
        public static GenericTicketedResponse<TPayload> Success<TPayload>(TPayload payload, long ticket) where TPayload : class
        {
            return new GenericTicketedResponse<TPayload>(ticket, null, true, payload);
        }
        public static GenericTicketedResponse Success(long ticket)
        {
            return new GenericTicketedResponse(ticket, null, true);
        }
        public static GenericTicketedResponse Failure(string errorMessage, long ticket)
        {
            return new GenericTicketedResponse(ticket, errorMessage, false);
        }
    }
    [DataContract]
    public class GenericTicketedResponse<TPayload> : GenericTicketedResponse
        where TPayload : class
    {
        private TPayload _Payload;
        [JsonPropertyName(GenericTicketedResponseDataMemberNames.Payload)]
        [JsonInclude]
        [DataMember(Name = GenericTicketedResponseDataMemberNames.Payload)]
        public TPayload Payload { get { return _Payload; } protected set { _Payload = value; } }
        public GenericTicketedResponse(long ticket,
            string errorMessage, bool successful, TPayload payload) : base(ticket, errorMessage, successful)
        {
            _Payload = payload;
        }
        protected GenericTicketedResponse() { }
    }
}
