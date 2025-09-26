using System.Collections.Generic;
    using System;
using JSON;
using Logging;
using Core.Messages;
using System.Linq;
namespace VirtualSockets
{
    public class VirtualSockets
    {
        private volatile bool _Disposed = false;
        private Dictionary<long, VirtualSocket> _MapIdToVirtualSocket = new Dictionary<long, VirtualSocket>();
        public void Add(VirtualSocket virtualSocket) {
            lock (_MapIdToVirtualSocket)
            {
                if (!_Disposed)
                _MapIdToVirtualSocket.Add(virtualSocket.Id, virtualSocket);
            }
            if (_Disposed)
                virtualSocket.Dispose();
        }
        public VirtualSocket Get(long id) {
            lock (_MapIdToVirtualSocket) {
                if (!_MapIdToVirtualSocket.ContainsKey(id)) return null;
                return _MapIdToVirtualSocket[id];
            }
        }
        public void HandleMessage(TypeTicketedAndWholePayload message) {
            HandleMessage(Json.Deserialize<VirtualSocketMessage>(message.JsonString));
        }
        public void HandleMessage(VirtualSocketMessage message) {
            VirtualSocket virtualSocket = Get(message.Id);
            virtualSocket?.HandleMessage(message);
        }
        public void Remove(VirtualSocket virtualSocket)
        {
            lock(_MapIdToVirtualSocket){
                _MapIdToVirtualSocket.Remove(virtualSocket.Id);
            }
        }
        public void Dispose()
        {
            VirtualSocket[] virtualSockets;
            lock (_MapIdToVirtualSocket)
            {

                if (_Disposed) return;
                _Disposed = true;
                virtualSockets = _MapIdToVirtualSocket.Values.ToArray();
            }
            foreach (VirtualSocket virtualSocket in virtualSockets)
            {
                try
                {
                    virtualSocket.Dispose();
                }
                catch (Exception ex)
                {
                    Logs.Default.Error(ex);
                }
            }
        }
    }
}