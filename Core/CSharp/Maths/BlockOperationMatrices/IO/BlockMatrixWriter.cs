using Shutdown;
using System;
using System.Collections.Generic;
using System.IO;

namespace Core.Maths.BlockOperationMatrices
{
    public class BlockMatrixWriter: IDisposable
    {
        protected const int FILE_STREAM_BUFFER_SIZE = 4096;
        private bool _Disposed = false;
        private FileStream _FileStream;
        private double _CurrentValue;
        private int _NInstancesOfCurrent;
        public BlockMatrixWriter(string filePath) {
            _FileStream = new FileStream(filePath,
                FileMode.OpenOrCreate, FileAccess.ReadWrite,
                FileShare.None, FILE_STREAM_BUFFER_SIZE);
            _CurrentValue = 0;
            _NInstancesOfCurrent = 0;
        }

        public void Write(double value)
        {
            CheckNotDisposed();
            _Write(value);
        }
        public void Write(double[] values) {
            CheckNotDisposed();
            foreach (double value in values) {
                _Write(value);
            }
        }
        public void _Write(double value)
        {
            if (_CurrentValue == value)
            {
                _NInstancesOfCurrent++;
                return;
            }
            if (_NInstancesOfCurrent > 0)
            {
                WriteCurrentValue();
            }
            _CurrentValue = value;
            _NInstancesOfCurrent = 1;
        }
        private void WriteCurrentValue()
        {
            _FileStream.Write(BitConverter.GetBytes(_CurrentValue));
            _FileStream.Write(BitConverter.GetBytes(_NInstancesOfCurrent));
        }
        private void CheckNotDisposed() {
            if (_Disposed) throw new ObjectDisposedException(nameof(BlockMatrixWriter));
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed) {
                return;
            }
            if (!disposing)
            {
                throw new InvalidOperationException(
                    $"{nameof(BlockMatrixWriter)} was finalized without being disposed. " +
                    $"Always call {nameof(Dispose)} explicitly to ensure file format integrity.");
            }
            _Disposed = true;
            if (_NInstancesOfCurrent > 0)
            {
                WriteCurrentValue();
            }
            _FileStream?.Flush();
            _FileStream?.Close();
            _FileStream?.Dispose();
        }
        ~BlockMatrixWriter() {
            Dispose(false);
        }
    }
}
