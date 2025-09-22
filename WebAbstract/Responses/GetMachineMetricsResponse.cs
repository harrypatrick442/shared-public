using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.Messages.Messages;
using Core.DataMemberNames;
using Core.Machine;
using WebAbstract.DataMemberNames.Interserver.Responses;

namespace WebAPI.Responses
{
    [DataContract]
    public class GetMachineMetricsResponse:TicketedMessageBase
    {
        [JsonPropertyName(GetMachineMetricsResponseDataMemberNames.MachineMetrics)]
        [JsonInclude]
        [DataMember(Name = GetMachineMetricsResponseDataMemberNames.MachineMetrics,
            EmitDefaultValue = false)]
        public MachineMetrics MachineMetrics { get;  protected set; }
        [JsonPropertyName(GetMachineMetricsResponseDataMemberNames.Success)]
        [JsonInclude]
        [DataMember(Name = GetMachineMetricsResponseDataMemberNames.Success)]
        public bool Success { get; protected set; }
        protected GetMachineMetricsResponse(MachineMetrics machineMetrics,
            bool success, long ticket):base(TicketedMessageType.Ticketed)
        {
            MachineMetrics = machineMetrics;
            Success = success;
            Ticket = ticket;
        }
        protected GetMachineMetricsResponse() : base(TicketedMessageType.Ticketed) { }

        public static GetMachineMetricsResponse Successful(MachineMetrics machineMetrics, long ticket)
        {
            return new GetMachineMetricsResponse(machineMetrics, true, ticket);
        }
        public static GetMachineMetricsResponse Failed(long ticket)
        {
            return new GetMachineMetricsResponse(null, false, ticket);
        }
    }
}
