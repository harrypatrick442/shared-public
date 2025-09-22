using Logging;
using Core.Maths;
using Timer = System.Timers.Timer;

namespace Core.LoadBalancing
{
    public abstract class ReceivingLoadBalancerBase:INodeIdAndLoadFactorHandler
    {
        private bool _AlreadyDoingUpdate = false;
        private bool _LoadFactorsChangedSinceLastUpdate = false;
        private Dictionary<int, NodeIdAndLoadFactor> _MapNodeIdToNodeIdAndLoadFactor =
            new Dictionary<int, NodeIdAndLoadFactor>();
        private Timer _TimerScheduledUpdate;
        private double _MaxDesirableVariationInProportion;
        private int _MaxLength;

        public LoadFactorType LoadFactorType { get; }

        public ReceivingLoadBalancerBase(LoadFactorType loadFactorType, int intervalUpdateMilliseconds, double maxDesirableVariationInProportion = 0.01, int maxLength = 40)
        {
            LoadFactorType = loadFactorType;
            _TimerScheduledUpdate = new Timer(intervalUpdateMilliseconds);
            _TimerScheduledUpdate.Elapsed += UpdateSequenceToHandOut;
            _TimerScheduledUpdate.AutoReset = false;
            _TimerScheduledUpdate.Enabled = true;
        }
        public void GotNodeIdAndLoadFactor(NodeIdAndLoadFactor nodeIdAndLoadFactor)
        {
            lock (_MapNodeIdToNodeIdAndLoadFactor)
            {
                _LoadFactorsChangedSinceLastUpdate = true;
                _MapNodeIdToNodeIdAndLoadFactor[nodeIdAndLoadFactor.NodeId] = nodeIdAndLoadFactor;
            }
            ScheduleUpdate();
        }
        public void EndpointWentOffline(int nodeId)
        {
            lock (_MapNodeIdToNodeIdAndLoadFactor)
            {
                _MapNodeIdToNodeIdAndLoadFactor.Remove(nodeId);
            }
            CancelScheduledUpdate();
            UpdateSequenceToHandOut();
        }
        private void ScheduleUpdate()
        {
            _TimerScheduledUpdate.Start();
        }
        private void CancelScheduledUpdate()
        {
            _TimerScheduledUpdate.Stop();
        }
        private void UpdateSequenceToHandOut(object sender, EventArgs e)
        {
            UpdateSequenceToHandOut();
        }
        protected abstract void NewNodeIdsEstablished(int[] nodeIds);
        public void UpdateSequenceToHandOut()
        {
            NodeIdAndLoadFactor[] nodeIdAndLoadFactors;
            lock (_MapNodeIdToNodeIdAndLoadFactor)
            {
                if (!_LoadFactorsChangedSinceLastUpdate) return;
                _LoadFactorsChangedSinceLastUpdate = false;
                if (_AlreadyDoingUpdate) return;
                _AlreadyDoingUpdate = true;//Also means no locking required to access _MapNodeIdToNodeIdAndLoadFactor 
                nodeIdAndLoadFactors = _MapNodeIdToNodeIdAndLoadFactor.Values.ToArray();
            }
            try
            {
                double[] inverseLoads = nodeIdAndLoadFactors.Select(n => n.LoadFactor <= 0 ? 1 : (1 / (double)n.LoadFactor)).ToArray();
                int[] nodeIds = PrpoortionalRepresentationArray.Create(inverseLoads, _MaxDesirableVariationInProportion, _MaxLength)
                    .Select(p => nodeIdAndLoadFactors[p].NodeId)
                    .ToArray();
                NewNodeIdsEstablished(nodeIds);
                _TimerScheduledUpdate.Start();
            }
            catch (Exception ex)
            {
                Logs.Default.Error(ex);
            }
            finally
            {
                lock (_MapNodeIdToNodeIdAndLoadFactor)
                {
                    _AlreadyDoingUpdate = false;
                }
            }
        }
        public void Dispose()
        {
            _TimerScheduledUpdate.Dispose();
        }
    }
}