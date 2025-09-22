using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Port;

namespace Core.Port.Connections
{
    public interface IConnectionDisposed {
        event EventHandler<EventArgsIConnection> OnDisposed;
    }
}
