using Core.Exceptions;
using Core.Maths.CUBLAS;
using InfernoDispatcher.Tasks;
using Logging;
using ManagedCuda;
using ManagedCuda.BasicTypes;
using ManagedCuda.NPP.NPPsExtensions;
using System;
using System.Threading;

namespace Core.Maths.Tensors
{
    public class GPUDynamicResizingRowsColumnsValuesBuffer
    {
        private CUdeviceptr _CurrentLengthPointer;
        private CUdeviceptr _RowsPointer;
        private CUdeviceptr _ColumnsPointer;
        private CUdeviceptr _ValuesPointer;
        private volatile int _CurrentLength;
        private volatile bool _Corrupted = false;
        private CudaContextWithThread _CudaContextWithThread;
        private CudaDeviceVariable<int> _RowsVectorVariable;
        private CudaDeviceVariable<int> _ColumnsVectorVariable;
        private CudaDeviceVariable<double> _ValuesVectorVariable;
        private CudaDeviceVariable<int> _CurrentLengthVariable;
        private readonly object _LockObject = new object();
        public GPUDynamicResizingRowsColumnsValuesBuffer(
            CudaContextWithThread cudaContextWithThread, int initialLength)
        {
            _CudaContextWithThread = cudaContextWithThread;
            _CurrentLength = initialLength;
            _CudaContextWithThread.UsingContext((System.Action<CudaContextHandles>)((h) =>
            {
                _CurrentLengthVariable = new CudaDeviceVariable<int>(1);
                _RowsVectorVariable = new CudaDeviceVariable<int>(_CurrentLength);
                _ColumnsVectorVariable = new CudaDeviceVariable<int>(_CurrentLength);
                _ValuesVectorVariable = new CudaDeviceVariable<double>(_CurrentLength);
                lock (_LockObject)
                {
                    _RowsPointer = _RowsVectorVariable.DevicePointer;
                    _ColumnsPointer = _ColumnsVectorVariable.DevicePointer;
                    _ValuesPointer = _ValuesVectorVariable.DevicePointer;
                }
            })).Wait();
        }
        public void GetPointers(out CUdeviceptr rows, 
            out CUdeviceptr columns, out CUdeviceptr values)
        {
            if (Thread.CurrentThread.ManagedThreadId == _CudaContextWithThread.ThreadId)
            {
                rows = _RowsVectorVariable.DevicePointer;
                columns = _ColumnsVectorVariable.DevicePointer;
                values = _ValuesVectorVariable.DevicePointer;
            }
            else
            {
                lock (_LockObject)
                {
                    rows = _RowsPointer;
                    columns = _ColumnsPointer;
                    values = _ValuesPointer;
                }
            }
        }
        public void TransferSync(IIndicesValuesBuffer buffer,
            CancellationToken? cancellationToken = null)
        {
            if (Thread.CurrentThread.ManagedThreadId == _CudaContextWithThread.ThreadId)
            {
                _Transfer(buffer);
                return;
            }
            _CudaContextWithThread.UsingContext((h) => _Transfer(buffer)).Wait();
        }
        public InfernoTaskNoResultBase TransferAsync(IIndicesValuesBuffer buffer,
            CancellationToken? cancellationToken = null)
        {
            if (Thread.CurrentThread.ManagedThreadId == _CudaContextWithThread.ThreadId)
            {
                throw new InvalidOperationException($"Shouldnt be calling this from the Cuda thread. You can use {nameof(TransferSync)}");
            }
            return _CudaContextWithThread.UsingContext((h) => _Transfer(buffer));
        }
        private void _Transfer(IIndicesValuesBuffer buffer)
        {
            CheckNotCorrupted();
            int length = buffer.Length;
            if (length < 1) return;
            if (length > _CurrentLength)
            {
                IncreaseSize(length);
            }
            int intByteLength = length * sizeof(int);
            int doubleByteLength = length * sizeof(double);
            _RowsVectorVariable.CopyToDevice(buffer.Indices, 0, intByteLength, 1);
            _ColumnsVectorVariable.CopyToDevice(buffer.Indices, 0, intByteLength, 1);
            _ValuesVectorVariable.CopyToDevice(buffer.Values, 0, doubleByteLength, 1);
        }
        private void IncreaseSize(int length)
        {
            try
            {
                int newSize = _CurrentLength;
                while (newSize < length)
                {
                    newSize *= 2;
                }
                _RowsVectorVariable?.Dispose();
                _ValuesVectorVariable?.Dispose();
                _RowsVectorVariable = new CudaDeviceVariable<int>(newSize);
                _ValuesVectorVariable = new CudaDeviceVariable<double>(newSize);
                _CurrentLengthVariable.CopyToDevice(newSize);
                _CurrentLength = newSize;
                lock (_LockObject)
                {
                    _RowsPointer = _RowsVectorVariable.DevicePointer;
                    _ValuesPointer = _ValuesVectorVariable.DevicePointer;
                }
            }
            catch (Exception ex)
            {
                _Corrupted = true;
                Logs.Default.Error($"Buffer resizing failed. Current size: {_CurrentLength}, Requested size: {length}. Exception: {ex}");
                throw new BufferCorruptedException("The GPU buffer is corrupted and cannot be used. Check logs for details.", ex);
            }
        }
        private void CheckNotCorrupted()
        {
            if (_Corrupted)
                throw new BufferCorruptedException("Corrupted");
        }
    }
}