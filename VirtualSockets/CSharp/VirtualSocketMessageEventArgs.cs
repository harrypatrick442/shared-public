using System;

namespace VirtualSockets
{
    public class VirtualSocketMessageEventArgs : EventArgs
    {
        private string _Type;
        public string Type { get { return _Type; } }
        private string _Payload;
        public string Payload { get { return _Payload; } }
        public VirtualSocketMessageEventArgs(string type, string payload) :base()
        {
            _Type = type;
            _Payload = payload;
        }
    }
}