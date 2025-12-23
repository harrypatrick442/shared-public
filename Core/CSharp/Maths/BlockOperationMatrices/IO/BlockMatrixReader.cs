using System;
using System.Collections.Generic;
using System.IO;

namespace Core.Maths.BlockOperationMatrices
{
    public class BlockMatrixReader : IDisposable
    {
        private const int N_BYTES_PER_PAIR = sizeof(double) + sizeof(int);
        protected const int FILE_STREAM_BUFFER_SIZE = 4096;
        private bool _Disposed = false;
        private FileStream _FileStream;
        private double _CurrentValue;
        private int _NInstancesOfCurrent;
        private int _NReadsOfCurrent;
        public BlockMatrixReader(string filePath)
        {
            _FileStream = new FileStream(filePath,
                FileMode.OpenOrCreate, FileAccess.ReadWrite,
                FileShare.None, FILE_STREAM_BUFFER_SIZE);
            _CurrentValue = 0;
            _NInstancesOfCurrent = 0;
            _NReadsOfCurrent = 0;
        }
        public double[] Read(int count)
        {
            double[] values = new double[count];
            for(int i = 0; i < count; i++)
            {
                values[i] = ReadDouble();
            }
            return values;
        }
        public double ReadDouble() {
            if(_NReadsOfCurrent>=_NInstancesOfCurrent)
            {
                ReadNext();
                _NReadsOfCurrent = 1;
                return _CurrentValue;
            }
            _NReadsOfCurrent++;
            return _CurrentValue;
        }
        private void ReadNext()
        {
            byte[] buffer = new byte[N_BYTES_PER_PAIR];
            int nRead = _FileStream.Read(buffer, 0, N_BYTES_PER_PAIR);
            if (nRead != N_BYTES_PER_PAIR) {
                throw new IndexOutOfRangeException("Reached end");
            }
            _CurrentValue = BitConverter.ToDouble(buffer, 0);
            _NInstancesOfCurrent = BitConverter.ToInt32(buffer, sizeof(double));
            if (_NInstancesOfCurrent <= 0)
            {
                throw new InvalidDataException(
                    $"{nameof(_NInstancesOfCurrent)} was <= 0 at position {_FileStream.Position - N_BYTES_PER_PAIR}. " +
                    "Possible corruption or malformed file.");
            }
        }
        private void CheckNotDisposed()
        {
            if (_Disposed) throw new ObjectDisposedException(nameof(BlockMatrixWriter));
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed)
            {
                return;
            }
            if (!disposing)
            {
                throw new InvalidOperationException(
                    $"{nameof(BlockMatrixReader)} was finalized without being disposed. " +
                    $"Always call {nameof(Dispose)} explicitly to ensure file format integrity.");
            }
            _Disposed = true;
            _FileStream?.Flush();
            _FileStream?.Close();
            _FileStream?.Dispose();
        }
        ~BlockMatrixReader()
        {
            Dispose(false);
        }
    }
}
