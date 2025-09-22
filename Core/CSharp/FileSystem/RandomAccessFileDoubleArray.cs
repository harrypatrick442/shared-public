using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core.FileSystem
{
    public class RandomAccessFileDoubleArray : IList<double[]>
    {
        private const int N_WRITES_BEFORE_FLUSH = 100;
        private readonly FileStream _fileStream;
        private readonly int _arrayLength; // The number of doubles in each entry
        private readonly int _entrySize; // The number of bytes per entry
        private int _CountdownToFlush = 0;
        public RandomAccessFileDoubleArray(FileStream fileStream, int arrayLength)
        {
            _fileStream = fileStream ?? throw new ArgumentNullException(nameof(fileStream));
            _arrayLength = arrayLength;
            _entrySize = arrayLength * sizeof(double);
        }

        public double[] this[int index]
        {
            get
            {
                if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                _fileStream.Position = (long)index * (long)_entrySize;
                byte[] buffer = new byte[_entrySize];
                _fileStream.Read(buffer, 0, _entrySize);
                return Enumerable.Range(0, _arrayLength)
                                 .Select(i => BitConverter.ToDouble(buffer, i * sizeof(double)))
                                 .ToArray();
            }
            set
            {
                if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                if (value.Length != _arrayLength) throw new ArgumentException($"Each array must have {_arrayLength} elements.");

                _fileStream.Position = index * _entrySize;
                foreach (double val in value)
                {
                    byte[] buffer = BitConverter.GetBytes(val);
                    _fileStream.Write(buffer, 0, buffer.Length);
                }
                if (--_CountdownToFlush <= 0)
                {
                    _fileStream.Flush();
                    _CountdownToFlush = N_WRITES_BEFORE_FLUSH;
                }
            }
        }

        public int Count
        {
            get { return (int)(_fileStream.Length / _entrySize); }
        }

        public bool IsReadOnly => false;

        public void Add(double[] item)
        {
            if (item.Length != _arrayLength) throw new ArgumentException($"Each array must have {_arrayLength} elements.");
            _fileStream.Position = _fileStream.Length; // Move to end of file
            foreach (double val in item)
            {
                byte[] buffer = BitConverter.GetBytes(val);
                _fileStream.Write(buffer, 0, buffer.Length);
            }
            _fileStream.Flush();
        }

        public void Clear()
        {
            _fileStream.SetLength(0); // Truncate the file to zero bytes
        }

        public bool Contains(double[] item)
        {
            throw new NotImplementedException();
            return this.Any(array => array.SequenceEqual(item));
        }

        public void CopyTo(double[][] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0 || arrayIndex + Count > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            for (int i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = this[i];
            }
        }

        public IEnumerator<double[]> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        public int IndexOf(double[] item)
        {
            throw new NotImplementedException();
            for (int i = 0; i < Count; i++)
            {
                if (this[i].SequenceEqual(item))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, double[] item)
        {
            throw new NotSupportedException("RandomAccessFile does not support inserting at arbitrary positions.");
        }

        public bool Remove(double[] item)
        {
            throw new NotImplementedException();
            int index = IndexOf(item);
            if (index == -1) return false;

            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));

            // Shift all subsequent entries up by one position
            for (int i = index; i < Count - 1; i++)
            {
                this[i] = this[i + 1];
            }

            // Truncate the last entry
            _fileStream.SetLength(_fileStream.Length - _entrySize);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public void RemoveFrom(int index)
        {
            if (index >= Count) return;

            // Calculate the new length of the file
            long newLength = index <= 0 ? 0 : index * _entrySize;

            // Truncate the file starting from the specified index
            _fileStream.SetLength(newLength);
        }
    }
}