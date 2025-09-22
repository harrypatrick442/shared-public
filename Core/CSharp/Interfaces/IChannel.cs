using System;
namespace Core.Port
{
    public interface IChannel:IDisposable{
        void Send(IMessageToSend message);
        bool IsConnected { get; }
    }
}
