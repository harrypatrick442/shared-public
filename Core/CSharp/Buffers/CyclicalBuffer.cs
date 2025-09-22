using System;

namespace Core.Configuration
{
    public class CyclicalBuffer<TEntry>
    {
        private readonly TEntry?[] _Entries;
        private int _NextIndex;
        public bool IsBufferFull { get; private set; } // Tracks if the buffer has wrapped around

        public CyclicalBuffer(int length)
        {
            if (length <= 0)
                throw new ArgumentException("Buffer length must be greater than 0.", nameof(length));

            _Entries = new TEntry[length];
            _NextIndex = 0;
            IsBufferFull = false;
        }

        public void Add(TEntry entry)
        {
            _Entries[_NextIndex++] = entry;
            if (_NextIndex >= _Entries.Length)
            {
                _NextIndex = 0;
                IsBufferFull = true; // Buffer is now full
            }
        }

        public TEntry? GetOldest()
        {
            if (!IsBufferFull && _NextIndex == 0)
                return default(TEntry?); // Buffer is empty

            // Determine the oldest index
            int oldestIndex = IsBufferFull ? _NextIndex : 0;
            return _Entries[oldestIndex];
        }

        public TEntry? GetLatest()
        {
            if (!IsBufferFull && _NextIndex == 0)
                return default(TEntry?); // Buffer is empty

            int index = _NextIndex - 1;
            if (index < 0)
                index = _Entries.Length - 1;
            return _Entries[index];
        }
    }
}
