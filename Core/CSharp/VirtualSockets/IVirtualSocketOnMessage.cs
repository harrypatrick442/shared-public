using System;
using VirtualSockets;

namespace Core.VirtualSockets
{
    public interface IVirtualSocketOnMessage
    {
        event EventHandler<VirtualSocketMessageEventArgs> OnMessage; 
    }
}
