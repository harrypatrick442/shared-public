using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core.FileSystem
{
    public class RandomAccessFile : IList<double>
    {
        private readonly FileStream _fileStream;
        private const int DoubleSize = sizeof(double); // Each double takes 8 bytes.

        public RandomAccessFile(FileStream fileStream)
        {
            _fileStream = fileStream ?? throw new ArgumentNullException(nameof(fileStream));
        }

        public double this[int index]
        {
            get
            {
                if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                _fileStream.Position = index * DoubleSize;
                byte[] buffer = new byte[DoubleSize];
                _fileStream.Read(buffer, 0, DoubleSize);
                return BitConverter.ToDouble(buffer, 0);
            }
            set
            {
                if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                _fileStream.Position = index * DoubleSize;
                byte[] buffer = BitConverter.GetBytes(value);
                _fileStream.Write(buffer, 0, DoubleSize);
                _fileStream.Flush();
            }
        }

        public int Count
        {
            get { return (int)(_fileStream.Length / DoubleSize); }
        }

        public bool IsReadOnly => false;

        public void Add(double item)
        {
            _fileStream.Position = _fileStream.Length; // Move to end of file
            byte[] buffer = BitConverter.GetBytes(item);
            _fileStream.Write(buffer, 0, DoubleSize);
            _fileStream.Flush();
        }

        public void Clear()
        {
            _fileStream.SetLength(0); // Truncate the file to zero bytes
        }

        public bool Contains(double item)
        {
            throw new NotImplementedException();
            return this.Any(d => d.Equals(item));
        }

        public void CopyTo(double[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0 || arrayIndex + Count > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            for (int i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = this[i];
            }
        }

        public IEnumerator<double> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        public int IndexOf(double item)
        {
            throw new NotImplementedException();
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Equals(item))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, double item)
        {
            throw new NotSupportedException("RandomAccessFile does not support inserting at arbitrary positions.");
        }

        public bool Remove(double item)
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

            // Shift all subsequent elements up by one position
            for (int i = index; i < Count - 1; i++)
            {
                this[i] = this[i + 1];
            }

            // Truncate the last element
            _fileStream.SetLength(_fileStream.Length - DoubleSize);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
