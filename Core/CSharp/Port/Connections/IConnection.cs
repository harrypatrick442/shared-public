using Snippets.ReadWrite;
using System;
namespace Core.Port.Connections
{
    public interface IConnection: IConnectionType, IReadConnection, IWrite, IDisposable
    {

    }
}
