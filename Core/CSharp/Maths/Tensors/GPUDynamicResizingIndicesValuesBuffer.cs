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
    public class GPUDynamicResizingIndicesValuesBuffer
    {
        private CUdeviceptr _CurrentLengthPointer;
        private CUdeviceptr _IndicesPointer;
        private CUdeviceptr _ValuesPointer;
        private volatile int _CurrentBuffersLength;
        private volatile int _CurrentNValuesInBuffersToTransfer;
        private volatile bool _Corrupted = false;
        private ICudaContextWithThread _CudaContextWithThread;
        public CudaDeviceVariable<int> _IndicesVectorVariable;
        public CudaDeviceVariable<double> _ValuesVectorVariable;
        private readonly object _LockObject = new object();
        public GPUDynamicResizingIndicesValuesBuffer(
            ICudaContextWithThread cudaContextWithThread, int initialLength)
        {
            _CudaContextWithThread = cudaContextWithThread;  
            _CurrentBuffersLength = initialLength;
            _CudaContextWithThread.UsingContext((System.Action<CudaContextHandles>)((h) =>
            {
                _IndicesVectorVariable = new CudaDeviceVariable<int>(_CurrentBuffersLength);
                _ValuesVectorVariable = new CudaDeviceVariable<double>(_CurrentBuffersLength);
                lock (_LockObject)
                {
                    _IndicesPointer = _IndicesVectorVariable.DevicePointer;
                    _ValuesPointer = _ValuesVectorVariable.DevicePointer;
                }
            })).Wait();
        }
        public void GetPointers(out CUdeviceptr indices, out CUdeviceptr values, out int length)
        {
            if (Thread.CurrentThread.ManagedThreadId == _CudaContextWithThread.ThreadId)
            {
                indices = _IndicesVectorVariable.DevicePointer;
                values = _ValuesVectorVariable.DevicePointer;
                length = _CurrentNValuesInBuffersToTransfer;
            }
            else
            {
                lock (_LockObject)
                {
                    indices = _IndicesPointer;
                    values = _ValuesPointer;
                    length = _CurrentNValuesInBuffersToTransfer;
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
            if (length > _CurrentBuffersLength)
            {
                IncreaseSize(length);
            }
            int intByteLength = length * sizeof(int);
            int doubleByteLength = length * sizeof(double);
            _IndicesVectorVariable.CopyToDevice(buffer.Indices, 0, 0, intByteLength);
            _ValuesVectorVariable.CopyToDevice(buffer.Values, 0, 0, doubleByteLength);
            _CurrentNValuesInBuffersToTransfer = length;
        }
        private void IncreaseSize(int length)
        {
            try
            {
                int newSize = _CurrentBuffersLength;
                while (newSize < length)
                {
                    newSize *= 2;
                }
                _IndicesVectorVariable?.Dispose();
                _ValuesVectorVariable?.Dispose();
                _IndicesVectorVariable = new CudaDeviceVariable<int>(newSize);
                _ValuesVectorVariable = new CudaDeviceVariable<double>(newSize);
                _CurrentBuffersLength = newSize;
                lock (_LockObject)
                {
                    _IndicesPointer = _IndicesVectorVariable.DevicePointer;
                    _ValuesPointer = _ValuesVectorVariable.DevicePointer;
                }
            }
            catch (Exception ex) {
                _Corrupted = true; 
                Logs.Default.Error($"Buffer resizing failed. Current size: {_CurrentBuffersLength}, Requested size: {length}. Exception: {ex}");
                throw new BufferCorruptedException("The GPU buffer is corrupted and cannot be used. Check logs for details.", ex);
            }
        }
        private void CheckNotCorrupted() {
            if (_Corrupted)
                throw new BufferCorruptedException("Corrupted");
        }
    }
}