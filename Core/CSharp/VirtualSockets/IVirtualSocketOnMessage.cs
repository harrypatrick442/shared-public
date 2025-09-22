using System;

namespace Core.VirtualSockets
{
    public interface IVirtualSocketOnMessage
    {
        event EventHandler<VirtualSocketMessageEventArgs> OnMessage; 
    }
}
