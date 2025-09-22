using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
namespace Core.Port.Connections
{
    public class EventArgsIConnection : EventArgs {
        private IConnection _IConnection;
        public IConnection IConnection{ 
            get
            {
                return _IConnection;
            } 
        }
        public EventArgsIConnection(IConnection iConnection) {
            _IConnection = iConnection;            
        }
    }
}
