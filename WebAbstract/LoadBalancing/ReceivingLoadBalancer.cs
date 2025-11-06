using Logging;
using Core.Exceptions;
using InterserverComs;
using WebAbstract.LoadBalancing;
using Initialization.Exceptions;

namespace WebAbstract.LoadBalancing
{
    public class ReceivingLoadBalancer
    {
        private static ReceivingLoadBalancer _Instance;
        public static ReceivingLoadBalancer Initialize() {
            if (_Instance != null) throw new AlreadyInitializedException(nameof(ReceivingLoadBalancer));
            _Instance = new ReceivingLoadBalancer();
            return _Instance;
        }
        public static ReceivingLoadBalancer Instance { 
            get {
                if (_Instance == null)
                    throw new NotInitializedException(nameof(ReceivingLoadBalancer));
                return _Instance; 
            } 
        }
        private Dictionary<LoadFactorType, HashSet<INodeIdAndLoadFactorHandler>> _MapLoadFactorTypeToHandlers;
        private HashSet<INodeIdAndLoadFactorHandler> _AllHandlers;
        private ReceivingLoadBalancer()
        {
            _MapLoadFactorTypeToHandlers
                = new Dictionary<LoadFactorType, HashSet<INodeIdAndLoadFactorHandler>>();
            _AllHandlers = new HashSet<INodeIdAndLoadFactorHandler>();
            InterserverPort.Instance.InterserverEndpoints.InterserverConnectionClosed += InterserverConnectionClosed;
        }
        private void InterserverConnectionClosed(object sender, NodeEndpointEventArgs e)
        {
            int nodeId = e.NodeEndpoint.NodeId;
            foreach (INodeIdAndLoadFactorHandler handler in GetAllHandlers())
            {
                handler.EndpointWentOffline(nodeId);
            }
        }
        public void Received(BroadcastNodeLoadingMessage broadcastNodeLoadingInfo) {
            LoadFactorType loadFactorType = broadcastNodeLoadingInfo.LoadFactorType;
            INodeIdAndLoadFactorHandler[] handlers = GetHandlers(loadFactorType);
            if (handlers == null) return;
            NodeIdAndLoadFactor nodeIdAndLoadFactor = new NodeIdAndLoadFactor(
                broadcastNodeLoadingInfo.NodeId, broadcastNodeLoadingInfo.LoadFactor);
            foreach (INodeIdAndLoadFactorHandler handler in handlers)
            {
                try
                {
                    handler.GotNodeIdAndLoadFactor(nodeIdAndLoadFactor);
                }
                catch (Exception ex)
                {
                    Logs.Default.Error(ex);
                }
            }
        }
        private INodeIdAndLoadFactorHandler[] GetAllHandlers()
        {
            lock (_MapLoadFactorTypeToHandlers)
            {
                return _AllHandlers.ToArray();
            }
        }
        private INodeIdAndLoadFactorHandler[] GetHandlers(LoadFactorType loadFactorType) {
            lock (_MapLoadFactorTypeToHandlers)
            {
                if (!_MapLoadFactorTypeToHandlers.TryGetValue(loadFactorType,
                    out HashSet<INodeIdAndLoadFactorHandler> handlersList))
                {
                    return null;
                }
                if (handlersList.Count < 1) return null;
                return handlersList.ToArray();
            }
        }
        public void AddHandler(INodeIdAndLoadFactorHandler handler)
        {
            LoadFactorType loadFactorType = handler.LoadFactorType;
            lock (_MapLoadFactorTypeToHandlers)
            {
                if (_MapLoadFactorTypeToHandlers.TryGetValue(loadFactorType, out HashSet<INodeIdAndLoadFactorHandler> handlersForLoadFactorType))
                {
                    if (handlersForLoadFactorType.Contains(handler))
                        return;
                    handlersForLoadFactorType.Add(handler);
                    _AllHandlers.Add(handler);
                    return;
                }
                handlersForLoadFactorType = new HashSet<INodeIdAndLoadFactorHandler> { handler };
                _MapLoadFactorTypeToHandlers.Add(loadFactorType, handlersForLoadFactorType);
                _AllHandlers.Add(handler);
            }
        }
        public void RemoveHandler(INodeIdAndLoadFactorHandler handler)
        {
            LoadFactorType loadFactorType = handler.LoadFactorType;
            lock (_MapLoadFactorTypeToHandlers)
            {
                if (!_MapLoadFactorTypeToHandlers.TryGetValue(loadFactorType, out HashSet<INodeIdAndLoadFactorHandler> handlersForLoadFactorType))
                {
                    return;
                }
                handlersForLoadFactorType.Remove(handler);
                _AllHandlers.Add(handler);
            }
        }
        private void Dispose()
        {

            lock (_MapLoadFactorTypeToHandlers)
            {
                _MapLoadFactorTypeToHandlers.Clear();
                _AllHandlers.Clear();
            }
        }

    }
}