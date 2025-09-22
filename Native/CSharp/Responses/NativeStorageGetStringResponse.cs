using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.DataMemberNames;
using Core.Messages.Messages;
using Native.DataMemberNames.Responses;

namespace Native.Responses
{
    [DataContract]
    public class NativeStorageGetStringResponse:TicketedMessageBase
    {
		[JsonPropertyName(NativeStorageGetStringResponseDataMemberNames.Value)]
		[JsonInclude]
		[DataMember(Name=NativeStorageGetStringResponseDataMemberNames.Value)]
		public string Value{get;protected set;}
		public NativeStorageGetStringResponse(string value, long ticket):base(TicketedMessageType.Ticketed)
        {
			Value = value;
			_Ticket = ticket;
		}
		protected NativeStorageGetStringResponse():base(TicketedMessageType.Ticketed) { 
			
		}
    }
}
