using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.IO;
namespace Core.Port.Connections
{
    public enum ConnectionType { 
        Socket,
        NamedPipes,
        SimulatedInternal
    }
}
