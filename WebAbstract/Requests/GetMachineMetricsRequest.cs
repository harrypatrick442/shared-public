using Core.Messages.Messages;
using MessageTypes.Internal;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace WebAPI.Requests
{
    [DataContract]
    public class GetMachineMetricsRequest : TicketedMessageBase
    {
        public GetMachineMetricsRequest() : base(InterserverMessageTypes.GetMachineMetricsRequest)
        { 
            
        }
    }
}
