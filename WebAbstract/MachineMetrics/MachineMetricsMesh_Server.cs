using JSON;
using Core.Handlers;
using Logging;
using InterserverComs;
using MessageTypes.Internal;
using Core;
using Core.Machine;
using WebAPI.Requests;
using WebAPI.Responses;
using Core.LoadBalancing;

namespace FilesRelayCore.TransferServers
{
    public partial class MachineMetricsMesh
    {
        private InterserverMessageTypeMappingsHandler _MessageTypeMappingsHandler;
        protected void Initialize_Server()
        {
            _MessageTypeMappingsHandler = InterserverMessageTypeMappingsHandler.Instance;
            _MessageTypeMappingsHandler.AddRange(
                new TupleList<string, DelegateHandleMessageOfType<InterserverMessageEventArgs>> {
                    {InterserverMessageTypes.GetMachineMetricsRequest, HandleGetMachineMetrics},
                    {InterserverMessageTypes.GetLoadFactor, HandleGetLoadFactor},
                    {InterserverMessageTypes.BroadcastLoadFactor, HandleBroadcastNodeLoading}
                }
            );
        }
        private void HandleGetMachineMetrics(InterserverMessageEventArgs e)
        {
            GetMachineMetricsRequest request = e.Deserialize<GetMachineMetricsRequest>();
            GetMachineMetricsResponse response;
            try
            {
                MachineMetrics machineMetrics = GetMachineMetrics_Here();
                response = GetMachineMetricsResponse.Successful(machineMetrics, request.Ticket);
            }
            catch (Exception ex)
            {
                Logs.Default.Error(ex);
                response = GetMachineMetricsResponse.Failed(request.Ticket);
            }
            try
            {
                e.EndpointFrom.SendJSONString(Json.Serialize(response));
            }
            catch (Exception ex)
            {
                Logs.Default.Error(ex);
            }
        }
        private void HandleGetLoadFactor(InterserverMessageEventArgs e)
        {
            GetLoadFactorRequest request = e.Deserialize<GetLoadFactorRequest>();
            GetLoadFactorResponse response;
            try
            {
                double loadFactor = GetLoadFactor_Here(request.LoadFactorType);
                response = GetLoadFactorResponse.Successful(loadFactor, request.Ticket);
            }
            catch (Exception ex)
            {
                Logs.Default.Error(ex);
                response = GetLoadFactorResponse.Failed(request.Ticket);
            }
            try
            {
                e.EndpointFrom.SendJSONString(Json.Serialize(response));
            }
            catch (Exception ex)
            {
                Logs.Default.Error(ex);
            }
        }
        private void HandleBroadcastNodeLoading(InterserverMessageEventArgs e)
        {
            BroadcastNodeLoadingMessage message = e.Deserialize<BroadcastNodeLoadingMessage>();
            BroadcastNodeLoading_Here(message);
        }
    }
}