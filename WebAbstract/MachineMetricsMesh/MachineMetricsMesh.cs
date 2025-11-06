using Core.Exceptions;
using Shutdown;
using Core.Messages.Messages;
using InterserverComs;
using WebAPI.Responses;
using Core.Machine;
using Logging;
using Core;
using JSON;
using WebAbstract.LoadBalancing;
using WebAbstract.Requests;
using Initialization.Exceptions;

namespace WebAbstract.MachineMetricsMesh
{
    public partial class MachineMetricsMesh
    {
        private static MachineMetricsMesh _Instance;
        private bool _LoggingEnabled;
        public static MachineMetricsMesh Initialize(bool loggingEnabled) {
            if (_Instance != null) throw new AlreadyInitializedException(nameof(MachineMetricsMesh));
            _Instance = new MachineMetricsMesh(loggingEnabled);
            return _Instance;
        }
        public static MachineMetricsMesh Instance { 
            get { 
                if (_Instance == null)
                    throw new NotInitializedException(nameof(MachineMetricsMesh));
                return _Instance;
            } 
        }
        private int _MyNodeId;
        private CancellationTokenSource _CancellationTokenSourceDisposed = new CancellationTokenSource();
        private MachineMetricsMesh(bool loggingEnabled) {
            _LoggingEnabled = loggingEnabled;
            _MyNodeId = Nodes.Nodes.Instance.MyId;
            Initialize_Server();
            ShutdownManager.Instance.Add(Dispose, ShutdownOrder.MachineMetricsMesh);
        }
        #region Methods
        #region Public
        public void BroadcastNodeLoading(int[] toNodeIds, LoadFactorType loadFactorType, double loadFactor)
        {
            int myNodeId = Nodes.Nodes.Instance.MyId;
            BroadcastNodeLoadingMessage broadcastNodeLoadingMessage = new BroadcastNodeLoadingMessage(myNodeId, loadFactor, loadFactorType);
            string json = Json.Serialize(broadcastNodeLoadingMessage);
            foreach (int toNodeId in toNodeIds)
            {
                if (toNodeId == myNodeId)
                {
                    BroadcastNodeLoading_Here(broadcastNodeLoadingMessage);
                    continue;
                }
                INodeEndpoint nodeEndpoint = InterserverPort.Instance.GetEndpointByNodeId(toNodeId);
                if (nodeEndpoint == null || (!nodeEndpoint.IsOpen))
                    continue;
                new Thread(() => {
                    try
                    {
                        nodeEndpoint.SendJSONString(json);
                    }
                    catch (Exception ex)
                    {
                        Logs.Default.Error(ex);
                    }
                }).Start();
            }
        }
        public NodeIdAndLoadFactor[] GetLoadFactors(int[] nodeIds, LoadFactorType loadFactorType, int timeoutMilliseconds)
        {
            List<NodeIdAndLoadFactor> nodeIdAndLoadFactors = new List<NodeIdAndLoadFactor>();
            CountdownLatch countdownLatch = new CountdownLatch(nodeIds.Length);
            foreach (int nodeId in nodeIds)
            {
                try
                {
                    new Thread(() =>
                    {
                        try
                        {
                            double? loadFactor = GetLoadFactor(nodeId, loadFactorType, timeoutMilliseconds);
                            lock (nodeIdAndLoadFactors)
                            {
                                nodeIdAndLoadFactors.Add(new NodeIdAndLoadFactor(nodeId, loadFactor));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (_LoggingEnabled)
                            {
                                Logs.Default.Error(ex);
                            }
                            lock (nodeIdAndLoadFactors)
                            {
                                nodeIdAndLoadFactors.Add(new NodeIdAndLoadFactor(nodeId, null));
                            }
                        }
                        finally
                        {
                            countdownLatch.Signal();
                        }
                    }).Start();
                }
                catch (Exception ex)
                {
                    lock (nodeIdAndLoadFactors)
                    {
                        nodeIdAndLoadFactors.Add(new NodeIdAndLoadFactor(nodeId, null));
                    }
                    countdownLatch.Signal();
                    if (_LoggingEnabled)
                    {
                        Logs.Default.Error(ex);
                    }
                }
            }
            countdownLatch.Wait();
            return nodeIdAndLoadFactors.ToArray();
        }
        public NodeMachineMetrics[] GetMachineMetrics(int[] nodeIds, int timeoutMilliseconds) {
            List<NodeMachineMetrics> nodeIdMachineMetricsPairs = new List<NodeMachineMetrics>();
            CountdownLatch countdownLatch = new CountdownLatch(nodeIds.Length);
            foreach (int nodeId in nodeIds) {
                try
                {
                    new Thread(() =>
                    {
                        try
                        {
                            MachineMetrics machineMetrics = GetMachineMetrics(nodeId, timeoutMilliseconds);
                            lock (nodeIdMachineMetricsPairs)
                            {
                                nodeIdMachineMetricsPairs.Add(new NodeMachineMetrics(nodeId, machineMetrics));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (_LoggingEnabled)
                            {
                                Logs.Default.Error(ex);
                            }
                            lock (nodeIdMachineMetricsPairs)
                            {
                                nodeIdMachineMetricsPairs.Add(new NodeMachineMetrics(nodeId, null));
                            }
                        }
                        finally
                        {
                            countdownLatch.Signal();
                        }
                    }).Start();
                }
                catch (Exception ex)
                {
                    lock (nodeIdMachineMetricsPairs)
                    {
                        nodeIdMachineMetricsPairs.Add(new NodeMachineMetrics(nodeId, null));
                    }
                    countdownLatch.Signal();
                    if (_LoggingEnabled)
                    {
                        Logs.Default.Error(ex);
                    }
                }
            }
            countdownLatch.Wait();
            return nodeIdMachineMetricsPairs.ToArray();
        }
        public MachineMetrics GetMachineMetrics(int nodeId, int timeoutMilliseconds)
        {
            MachineMetrics machineMetrics = null;
            _OperationRedirectedToNode<
                GetMachineMetricsRequest,
                GetMachineMetricsResponse>(nodeId,
                () =>
                {
                    machineMetrics = GetMachineMetrics_Here();
                },
                () => new GetMachineMetricsRequest(),
                (response) =>
                {
                    machineMetrics = response.MachineMetrics;
                },
                timeoutMilliseconds
            );
            return machineMetrics;
        }
        public double? GetLoadFactor(int nodeId, LoadFactorType loadFactorType, int timeoutMilliseconds)
        {
            double? value = 0;
            _OperationRedirectedToNode<
                GetLoadFactorRequest,
                GetLoadFactorResponse>(nodeId,
                () =>
                {
                    value = GetLoadFactor_Here(loadFactorType);
                },
                () => new GetLoadFactorRequest(loadFactorType),
                (response) =>
                {
                    value = response.LoadFactor;
                },
                timeoutMilliseconds
            );
            return value;
        }
        #endregion Public
        #region Private
        private void _OperationRedirectedToNode<TRemoteRequest, TRemoteResponse>(
            int nodeId, Action callbackDoHere,
            Func<TRemoteRequest> callbackCreateRequest,
            Action<TRemoteResponse> didRemotely, int timeoutMilliseconds)
            where TRemoteRequest : TicketedMessageBase where TRemoteResponse : TicketedMessageBase
        {
            if (nodeId == _MyNodeId)
            {
                callbackDoHere();
                return;
            }
            INodeEndpoint nodeEndpoint = InterserverPort.Instance.GetEndpointByNodeId(nodeId);
            if (nodeEndpoint == null)
                throw new OperationFailedException($"Failed to get {nameof(INodeEndpoint)} for node with id {nodeId}");
            TRemoteResponse removeAssociateResponse = InterserverTicketedSender.Send<TRemoteRequest, TRemoteResponse>(
                callbackCreateRequest(),
                timeoutMilliseconds, _CancellationTokenSourceDisposed.Token, nodeEndpoint.SendJSONString);
            didRemotely(removeAssociateResponse);
        }
        private void Dispose() {
            _CancellationTokenSourceDisposed.Cancel();
        }
        #endregion Private
        #endregion Methods
    }
}