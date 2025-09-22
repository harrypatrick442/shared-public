using Core.Port;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Port.Connections
{
    public class EventArgsConnectionRead:EventArgs {
        private string _Str;
        public string Str { get { return _Str; } }

        private IConnection _IConnection;
        public IConnection IConnection { get { return _IConnection; } }
        public EventArgsConnectionRead(string str, IConnection iConnection) {
            _Str = str;
            _IConnection = iConnection;
        }
    }
}
