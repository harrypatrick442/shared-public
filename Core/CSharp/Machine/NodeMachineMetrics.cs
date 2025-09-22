namespace Core.Machine
{
	public class NodeMachineMetrics
    {
        public int NodeId { get; protected set; }
        public MachineMetrics MachineMetrics { get; protected set; }
        public NodeMachineMetrics(int nodeId, MachineMetrics machineMetrics)
        {
            NodeId= nodeId;
            MachineMetrics = machineMetrics;
        }
    }
}
