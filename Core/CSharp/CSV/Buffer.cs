using System;
using System.IO;
using System.Net;

namespace Core.CSV
{

    internal class Buffer
    {
        private const int MAX_LENGTH_READ = 1024;
        char[] _Buffer = new char[MAX_LENGTH_READ];
        private int index = 0;
        private StreamReader _StreamReader;
        private int _NCharsLoaded;
        public Buffer(StreamReader streamReader)
        {
            _StreamReader = streamReader;
        }
        public int NLeft
        {
            get
            {
                return _NCharsLoaded - index;
            }
        }
        public bool HasNext
        {
            get
            {
                return index < _NCharsLoaded;
            }
        }
        public void Load()
        {
            _NCharsLoaded = _StreamReader.Read(_Buffer, 0, MAX_LENGTH_READ);
        }
        public char Next()
        {
            char c = _Buffer[index];
            index++;
            return c;
        }
        public char Current() {
            return _Buffer[index - 1];
        }
        public char LookAhead(int nIndices)
        {
            int position = index + nIndices;
            return _Buffer[position-1];
        }
    }
}
