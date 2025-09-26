using Logging;
using JSON;

namespace VirtualSockets
{
    public abstract class VirtualSocket:IDisposable
    {
        public event EventHandler OnDispose;
        private long _Id;
        public long Id { get { return _Id; } }
        private Action<VirtualSocketMessage> _Send;
        private VirtualSockets _VirtualSockets;
        private bool _Disposed = false;
        protected VirtualSocket(long id, Action<VirtualSocketMessage> send, VirtualSockets virtualSockets)
        {
            _VirtualSockets = virtualSockets;
            _Send = send;
            _Id = id;
            _VirtualSockets.Add(this);
        }
        protected VirtualSocket(Action<VirtualSocketMessage> send, VirtualSockets virtualSockets)
        {
            _VirtualSockets = virtualSockets;
            _Send = send;
            _Id = VirtualSocketIdentifierSource.    NextId();
            _VirtualSockets.Add(this);
        }
        protected abstract void HandleMessage(string type, string payload);
        public void HandleMessage(VirtualSocketMessage message)
        {
            try
            {
                HandleMessage(message.InternalType, message.Payload);
            }
            catch (Exception ex) {
                Logs.Default.Error(ex);
            }
            if (message.InternalType== MessageTypes.VirtualSocketDisposing) {
                Dispose();
            }
        }
        public void SendMessage<TMessage>(string type, TMessage message) where TMessage:class{
            SendString(type,  Json.Serialize(message));
        }
        protected void SendString(string type, string payload) {
            _Send(new VirtualSocketMessage(_Id, payload, type));
        }
        public void SendClosing()
        {
        }
        public void Dispose()
        {
            lock (this)
            {
                if (_Disposed) return;
                _Disposed = true;
            }
            _VirtualSockets.Remove(this);
            try
            {
                OnDispose?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                Logs.Default.Error(ex);
            }
            try
            {
                _Send(new VirtualSocketMessage(_Id, null, MessageTypes.VirtualSocketDisposing));
            }
            catch (Exception ex)
            {
                Logs.Default.Error(ex);
            }
        }
    }
}