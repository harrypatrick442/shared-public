using System;
namespace Core.Port.Connections
{
    public interface IServer: IDisposable
    {
        event EventHandler<EventArgsIConnection> OnConnection;
    }
}
