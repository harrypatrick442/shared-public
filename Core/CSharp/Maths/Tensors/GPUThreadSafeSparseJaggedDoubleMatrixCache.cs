using Core.Maths.CUBLAS;
using Core.Maths.Tensors.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Value = Core.Maths.Tensors.ThreadSafeSparseJaggedDoubleMatrixCache_Value;
namespace Core.Maths.Tensors
{
    public class GPUThreadSafeSparseJaggedDoubleMatrixCache: ThreadSafeSparseJaggedDoubleMatrixCache
    {
        private const int DEFAULT_INITIAL_BUFFER_SIZE = 64;
        protected DynamicResizingRowsColumnsValuesBuffer _RowsColumnsValuesBuffer;
        protected GPUDynamicResizingRowsColumnsValuesBuffer _GPUBuffers;
        public GPUThreadSafeSparseJaggedDoubleMatrixCache(
            CudaContextWithThread fixedThreadInvokableCudaContext,
            int initialBufferSize = DEFAULT_INITIAL_BUFFER_SIZE) 
            :base()
        {
            _RowsColumnsValuesBuffer = new DynamicResizingRowsColumnsValuesBuffer(initialBufferSize);
            _GPUBuffers = new GPUDynamicResizingRowsColumnsValuesBuffer(fixedThreadInvokableCudaContext, initialBufferSize);
        }
        public void ChangedValuesToGPUBuffer()
        {
            lock (_MapRowColumnToValue)
            {
                _RowsColumnsValuesBuffer.Clear();
                _RowsColumnsValuesBuffer.AddRange(_Values.Where(v => v.Changed));
                _GPUBuffers.TransferSync(_RowsColumnsValuesBuffer);
            }
        }
    }
}
