using System;

namespace Core.Events
{
    public class DataEventArgs : EventArgs
    {
        private byte[] _Data;
        public byte[] Data { get { return _Data; } }
        public DataEventArgs(byte[] data)
        {
            _Data = data;
        }
    }
}