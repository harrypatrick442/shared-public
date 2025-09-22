using System;

namespace Core.VirtualSockets
{
    public interface IVirtualSocketSendString
    {
        void SendString(string type, string payload); 
    }
}
