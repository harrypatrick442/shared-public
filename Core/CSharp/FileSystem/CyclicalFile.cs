using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Core.FileSystem
{
    public sealed class CyclicalFile:IDisposable
    {
        private const int
            //DO NOT TOUCH THESE CONSTANTS.
            HEADER_LENGTH = 128,
            CURRENT_ENTRY_INDEX_INDEX = 0,
            ENTRY_SIZE_INDEX =12;

        private readonly int _EntrySize;
        private readonly int _NEntries;
        private readonly Stream _Stream;
        private int _CurrentEntryIndex;
        private long _CurrentStreamPosition;
        private bool _Disposed = false;
        public CyclicalFile(string filePath, int entrySize, int nEntries)
        {
            _EntrySize = entrySize;
            _NEntries = nEntries;
            _Stream = File.Open(filePath, FileMode.OpenOrCreate);
            if (_Stream.Length >= HEADER_LENGTH)
            {
                int entrySizeFromHeader = _ReadEntrySize();
                if (entrySizeFromHeader != entrySize) 
                    throw new InvalidDataException($"{nameof(entrySize)} argument was {entrySize} but the file specified {nameof(entrySize)} of {entrySizeFromHeader}");
                _CurrentEntryIndex = _ReadCurrentEntryIndex();
                _CurrentStreamPosition = (_CurrentEntryIndex * _EntrySize)+HEADER_LENGTH;
            }
            else {
                _CurrentEntryIndex = 0;
                _CurrentStreamPosition = HEADER_LENGTH;
                _WriteCurrentEntryIndex(_CurrentEntryIndex);
                _WriteEntrySize(_EntrySize);
            }
            _SanityCheck();
        }
        public void Write(List<byte[]> bytess) {
            lock (this)
            {
                _CheckNotDisposed_NoLock();
                int nToSkip = bytess.Count - _NEntries;
                IEnumerable<byte[]> bytesCappedInLength = nToSkip > 0
                    ? bytess.Skip(nToSkip) : bytess;
                long streamPosition = _CurrentStreamPosition;
                foreach (byte[] bytes in bytesCappedInLength)
                {
                    _Stream.Position = streamPosition;
                    if (bytes.Length > _EntrySize)
                        throw new InvalidDataException($"{nameof(bytes)} had a length greater than {nameof(_EntrySize)} {_EntrySize}");
                    _Stream.Write(bytes, 0, bytes.Length);
                    _CurrentEntryIndex++;
                    if (_CurrentEntryIndex >= _NEntries)
                    {
                        streamPosition = HEADER_LENGTH;
                        _CurrentEntryIndex = 0;
                    }
                    else
                    {
                        streamPosition += _EntrySize;
                    }
                }
                _CurrentStreamPosition = streamPosition;
                _WriteCurrentEntryIndex(_CurrentEntryIndex);
            }
        }
        public byte[][] Read() {
            lock (this)
            {
                _CheckNotDisposed_NoLock();
                long streamPosition = _CurrentStreamPosition;
                long lengthMinusOnEntry = _Stream.Length - _EntrySize;
                int nEntriesToRead = (int)((_Stream.Length - HEADER_LENGTH) / _EntrySize);
                int entryIndex = 0;
                byte[][] entries = new byte[nEntriesToRead][];
                while (entryIndex < nEntriesToRead)
                {
                    if (streamPosition > lengthMinusOnEntry) {
                        streamPosition = HEADER_LENGTH;
                    }
                    byte[] entry = new byte[_EntrySize];
                    entries[entryIndex] = entry;
                    _Stream.Position = streamPosition;
                    _Stream.Read(entry, 0, _EntrySize);
                    streamPosition+=_EntrySize;
                    entryIndex++;
                }
                return entries;
            }
        }
        private void _CheckNotDisposed_NoLock() {
            if (_Disposed) throw new ObjectDisposedException(nameof(CyclicalFile));
        }
        private int _ReadCurrentEntryIndex()
        {
            return _ReadInt(CURRENT_ENTRY_INDEX_INDEX);
        }
        private void _WriteCurrentEntryIndex(int value)
        {
            _WriteInt(CURRENT_ENTRY_INDEX_INDEX, value);
        }
        private int _ReadEntrySize()
        {
            return _ReadInt(ENTRY_SIZE_INDEX);
        }
        private void _WriteEntrySize(int value)
        {
            _WriteInt(ENTRY_SIZE_INDEX, value);
        }
        private int _ReadInt(long index)
        {
            _Stream.Position = index;
            byte[] bytes = new byte[4];
            _Stream.Read(bytes, 0, 4);
            return BitConverter.ToInt32(bytes);
        }
        private void _WriteInt(long index, int value) {
            byte[] bytes = BitConverter.GetBytes(value);
            _Stream.Position = index;
            _Stream.Write(bytes);
        }
        private void _SanityCheck()
        {
            //TODO
        }
        public void Dispose() {
            lock (this)
            {
                if (_Disposed) return;
                _Disposed = true;
                _Stream.Dispose();
            }
        }
    }
}
