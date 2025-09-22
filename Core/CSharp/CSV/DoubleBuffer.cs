using System;
using System.IO;
using System.Net;

namespace Core.CSV
{

    internal class DoubleBuffer: ICSVBuffer
    {
        private Buffer _FirstBuffer;
        private Buffer _SecondBuffer;
        private Buffer _CurrentBuffer;
        private Buffer _NonCurrentBuffer;
        public DoubleBuffer(StreamReader streamReader)
        {
            _FirstBuffer = new Buffer(streamReader);
            _SecondBuffer = new Buffer(streamReader);
            _FirstBuffer.Load();
            _SecondBuffer.Load();
            _CurrentBuffer = _FirstBuffer;
            _NonCurrentBuffer = _SecondBuffer;
        }
        public bool HasNext
        {
            get
            {
                return _CurrentBuffer.HasNext || _NonCurrentBuffer.HasNext;
            }
        }
        public char Next
        {
            get
            {
                if (!_CurrentBuffer.HasNext)
                {
                    SwitchBufferAndLoadIntoCurrentBuffer();
                }
                if (!_CurrentBuffer.HasNext) throw new ArgumentOutOfRangeException("No more buffered data to read");
                return _CurrentBuffer.Next();
            }
        }
        public char Current { 
            get
            {
                return _CurrentBuffer.Current();
            } 
        }
        private void SwitchBufferAndLoadIntoCurrentBuffer()
        {
            var c = _CurrentBuffer;
            _CurrentBuffer = _NonCurrentBuffer;
            _NonCurrentBuffer = c;
            _NonCurrentBuffer.Load();
        }
        public char[] Advance(int nIndices)
        {
            char[] chars = new char[nIndices];
            for (int i = 0; i < nIndices; i++)
            {
                chars[i] = Next;
            }
            return chars;
        }
        public char LookAhead(int nIndices)
        {
            int nLeft = _CurrentBuffer.NLeft;
            if (nLeft < nIndices)
            {
                int nIndicesInNextBuffer = nIndices - nLeft;
                if (_NonCurrentBuffer.NLeft < nIndicesInNextBuffer) throw new IndexOutOfRangeException("Cant look ahead this far");
                return _NonCurrentBuffer.LookAhead(nIndicesInNextBuffer);
            }
            return _CurrentBuffer.LookAhead(nIndices);
        }
    }
}
