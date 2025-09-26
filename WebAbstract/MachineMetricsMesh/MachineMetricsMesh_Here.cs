using Core.LoadBalancing;
using Core.MemoryManagement;
using WebAbstract.LoadBalancing;
using MachineMetrics = Core.Machine.MachineMetrics;
namespace WebAbstract.MachineMetricsMesh
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