using Core.LoadBalancing;
using Core.Machine;
using Core.MemoryManagement;

namespace FilesRelayCore.TransferServers
{
    public partial class MachineMetricsMesh
    {
        public MachineMetrics GetMachineMetrics_Here()
        {
            return new MachineMetrics(MemoryHelper.GetMemoryMetricsCached(), ProcessorMetricsSource.Instance.GetProcessorMetricsCached());
        }
        public double GetLoadFactor_Here(LoadFactorType loadFactorType)
        {
            return LoadFactorsSource.Instance.GetLoadFactor(loadFactorType);
        }
        public void BroadcastNodeLoading_Here(BroadcastNodeLoadingMessage broadcastNodeLoadingMessage)
        {
            ReceivingLoadBalancer.Instance.Received(broadcastNodeLoadingMessage);
        }
    }
}