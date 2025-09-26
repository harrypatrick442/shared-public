using Core;
using Core.LoadBalancing;
using Logging;
using Shutdown;
using WebAbstract.MachineMetricsMesh;

namespace WebAbstract.LoadBalancing
{
    public class RequestingLoadBalancerBase
    {
        private int[] _NodeIds;
        private int _TimeoutMillisecondsGetMachineMetrics;
        private int _SubUpdateDelayMilliseconds;
        private int _NSubUpdateDelays;
        private NodeIdAndOnline[] _NodeIdAndOnlines;
        private CountdownLatch _CountdownLatchWaitForUpdate;
        private CancellationTokenSource _CancellationTokenSourceDisposed = new CancellationTokenSource();
        private const int MAX_SUB_UPDATE_DELAY_MILLISECONDS= 2000;
        protected RequestingLoadBalancerBase(int[] nodeIds, LoadFactorType loadFactorType,  int timeoutMillisecondsGetMachineMetrics, 
            int delayUpdateMilliseconds)
        {
            _NodeIds = nodeIds;
            _TimeoutMillisecondsGetMachineMetrics = timeoutMillisecondsGetMachineMetrics;
            if (delayUpdateMilliseconds <= MAX_SUB_UPDATE_DELAY_MILLISECONDS)
            {
                _NSubUpdateDelays = 1;
                _SubUpdateDelayMilliseconds = delayUpdateMilliseconds;
            }
            else
            {
                double nUpdateDelays = delayUpdateMilliseconds / MAX_SUB_UPDATE_DELAY_MILLISECONDS;
                nUpdateDelays = Math.Ceiling(nUpdateDelays);
                _NSubUpdateDelays = (int)nUpdateDelays;
                _SubUpdateDelayMilliseconds = delayUpdateMilliseconds / _NSubUpdateDelays;

            }
            _CountdownLatchWaitForUpdate = new CountdownLatch(1);
            ShutdownManager.Instance.Add(Dispose, ShutdownOrder.LoadBalancer);
            StartUpdateLoop_LoadFactor(loadFactorType);
        }
        public int[] GetMostToLeastAppropriateNodes(int nOptionsDesired, int timeoutMilliseconds)
        {
            return GetMostToLeastAppropriate(nOptionsDesired, timeoutMilliseconds).Select(n => n.NodeId).ToArray();
        }
        public NodeIdAndOnline[] GetMostToLeastAppropriate(int nOptionsDesired, int timeoutMilliseconds)
        {
            lock (this)
            {
                if (_NodeIdAndOnlines != null)
                    return _NodeIdAndOnlines.Take(nOptionsDesired).ToArray();
            }
            CountdownLatch countdownLatchWaitForUpdate;
            lock (this)
            {
                if (_CancellationTokenSourceDisposed.IsCancellationRequested)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }
                countdownLatchWaitForUpdate = _CountdownLatchWaitForUpdate;
            }
            countdownLatchWaitForUpdate.Wait(timeoutMilliseconds);
            lock (this)
            {
                return _NodeIdAndOnlines.Take(nOptionsDesired).ToArray();
            }

        }
        private void StartUpdateLoop_LoadFactor(LoadFactorType loadFactorType) {
            Loop(() =>
            {
                NodeIdAndLoadFactor[] NodeIdAndLoadFactors = MachineMetricsMesh.MachineMetricsMesh.Instance.GetLoadFactors(_NodeIds, loadFactorType, _TimeoutMillisecondsGetMachineMetrics);
                return OrderNodesMostToLeastDesirable(NodeIdAndLoadFactors);
            });
        }
        /*
        private void StartUpdateLoopMemoryProcessor() {
            Loop(() =>
            {
                NodeMachineMetrics[] nodeMachineMetrics = MachineMetricsMesh.Instance
                    .GetMachineMetrics(_NodeIds, _TimeoutMillisecondsGetMachineMetrics);
                return OrderNodesMostToLeastDesirable(nodeMachineMetrics, _FreeMemoryWeightingFactorRelativeToIdleProcess);
            });         
        }*/
        private void Loop(Func<NodeIdAndOnline[]> getNodeIdAndOnlinesMostToLeastDesirable)
        {
            new Thread(() =>
            {
                while (true)
                {
                    lock (this)
                    {
                        if (_CancellationTokenSourceDisposed.IsCancellationRequested)
                            return;
                    }
                    try
                    {
                        NodeIdAndOnline[] nodeIdAndOnlines = getNodeIdAndOnlinesMostToLeastDesirable();
                        lock (this)
                        {
                            _NodeIdAndOnlines = nodeIdAndOnlines;
                            _CountdownLatchWaitForUpdate.Signal();
                            if (_CancellationTokenSourceDisposed.IsCancellationRequested)
                                return;
                            _CountdownLatchWaitForUpdate = new CountdownLatch(1);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logs.Default.Error(ex);
                    }
                    for (int i = 0; i < _NSubUpdateDelays; i++)
                    {
                        Thread.Sleep(_SubUpdateDelayMilliseconds);
                        lock (this)
                        {
                            if (_CancellationTokenSourceDisposed.IsCancellationRequested)
                                return;
                        }
                    }
                }
            }).Start();
        }
        
        private static NodeIdAndOnline[] OrderNodesMostToLeastDesirable(NodeIdAndLoadFactor[] nodeIdAndLoadFactors)
        {
            List<int> nodesWithNoLoadFactors = new List<int>();
            List<NodeIdAndLoadFactor> nodesWithLoadFactor= new List<NodeIdAndLoadFactor>();
            foreach (NodeIdAndLoadFactor nodeIdAndLoadFactor in nodeIdAndLoadFactors)
            {
                if (nodeIdAndLoadFactor.LoadFactor!= null) nodesWithLoadFactor.Add(nodeIdAndLoadFactor);
                else nodesWithNoLoadFactors.Add(nodeIdAndLoadFactor.NodeId);
            }
            return nodesWithLoadFactor.OrderBy(n => (double)n.LoadFactor).Select(n => new NodeIdAndOnline(n.NodeId, true, null))
                .Concat(nodesWithNoLoadFactors.Select(n => new NodeIdAndOnline(n, false, null))).ToArray();
        }
        /*
        private static NodeIdAndOnline[] OrderNodesMostToLeastDesirable(
            NodeMachineMetrics[] NodeMachineMetricss, double freeMemoryWeightingFactorRelativeToIdleProcessor)
        {
            List<long> nodesWithNoReturnedMetrics = new List<long>();
            List<NodeMachineMetrics> nodesWithMetrics = new List<NodeMachineMetrics>();
            foreach (NodeMachineMetrics nodeMachineMetrics in NodeMachineMetricss)
            {
                if (nodeMachineMetrics.MachineMetrics != null) nodesWithMetrics.Add(nodeMachineMetrics);
                else nodesWithNoReturnedMetrics.Add(nodeMachineMetrics.NodeId);
            }
            long maxFreeMemory = nodesWithMetrics.Max(n => n.MachineMetrics.Memory.FreeMb);
            double maxIdleProcessorPercent = 100 - nodesWithMetrics.Min(n => n.MachineMetrics.Processor.PercentCpuUsageByAllProcesses);
            Func<NodeMachineMetrics, double> getFreeMemoryFactor = maxFreeMemory <= 0
                ? (nodeMachineMetrics) => 1
                : (nodeMachineMetrics) => nodeMachineMetrics.MachineMetrics.Memory.FreeMb / maxFreeMemory;
            Func<NodeMachineMetrics, double> getIdleProcessorFactor = maxIdleProcessorPercent <= 0
                ? (nodeMachineMetrics) => 1
                : (nodeMachineMetrics) => freeMemoryWeightingFactorRelativeToIdleProcessor * 
                    (100 - nodeMachineMetrics.MachineMetrics.Processor.PercentCpuUsageByAllProcesses)
                    / maxIdleProcessorPercent;

            return nodesWithMetrics.Select(n => new
            {
                freeMemoryFactor = getFreeMemoryFactor(n),
                idleProcessorFactor = getIdleProcessorFactor(n),
                nodeMachineMetrics = n
            })
            .OrderByDescending(o => o.freeMemoryFactor < o.idleProcessorFactor
                ? o.freeMemoryFactor
                : o.idleProcessorFactor)
            .Select(o => new NodeIdAndOnline(o.nodeMachineMetrics.NodeId, true, o.nodeMachineMetrics.MachineMetrics))
            .Concat(nodesWithNoReturnedMetrics.Select(n => new NodeIdAndOnline(n, false, null))).ToArray();
        }*/
        private void Dispose()
        {
            lock (this)
            {
                _CancellationTokenSourceDisposed.Cancel();
                _CountdownLatchWaitForUpdate?.Signal();
            }
        }
    }
}