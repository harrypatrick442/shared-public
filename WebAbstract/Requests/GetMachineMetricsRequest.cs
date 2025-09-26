using Core.Messages.Messages;
using System.Runtime.Serialization;

namespace WebAbstract.Requests
{
    [DataContract]
    public class GetMachineMetricsRequest : TicketedMessageBase
    {
        public GetMachineMetricsRequest() : base(InterserverMessageTypes.GetMachineMetricsRequest)
        { 
            
        }
    }
}
