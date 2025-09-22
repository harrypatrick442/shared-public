using Logging;
using System;

namespace Core.VirtualSockets
{
    public class VirtualSocketsBridge<TVirtualSocket> where TVirtualSocket : VirtualSocket,
        IVirtualSocketOnMessage, IVirtualSocketSendString
    {
        private delegate void DelegateEventHandler(object sender, VirtualSocketMessageEventArgs e);
        private TVirtualSocket _VirtualSocketA;
        private TVirtualSocket _VirtualSocketB;
        public event EventHandler OnDispose;
        private bool _Disposed = false;
        public bool Disposed { get { lock (this) { return _Disposed; } } }
        public VirtualSocketsBridge(TVirtualSocket virtualSocketA, TVirtualSocket virtualSocketB)
        {
            _VirtualSocketA = virtualSocketA;
            _VirtualSocketB = virtualSocketB;
            _VirtualSocketA.OnMessage += _HandleMessageFromA;
            _VirtualSocketB.OnMessage += _HandleMessageFromB;
            _VirtualSocketA.OnDispose += _HandleDispose;
            _VirtualSocketB.OnDispose += _HandleDispose;
        }
        private void _HandleMessageFromA(object sender, VirtualSocketMessageEventArgs e)
        {
            _VirtualSocketB.SendString(e.Type, e.Payload);
        }
        private void _HandleMessageFromB(object sender, VirtualSocketMessageEventArgs e)
        {
            _VirtualSocketA.SendString(e.Type, e.Payload);
        }
        public void Dispose()
        {
            lock (this)
            {
                if (_Disposed) return;
                _Disposed = true;
            }
            _VirtualSocketA.OnMessage -= _HandleMessageFromA;
            _VirtualSocketB.OnMessage -= _HandleMessageFromB;
            _VirtualSocketA.OnDispose -= _HandleDispose;
            _VirtualSocketB.OnDispose -= _HandleDispose;
            _VirtualSocketA.Dispose();
            _VirtualSocketB.Dispose();
            try
            {
                EventHandler onDispose = OnDispose;
                onDispose?.Invoke(this, new EventArgs());
            }
            catch (Exception ex) {
                Logs.Default.Error(ex);
            }
        }
        private void _HandleDispose(object sender, EventArgs e){
            Dispose();
        }

    }
}
