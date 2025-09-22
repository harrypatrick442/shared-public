using System;
using System.Collections.Generic;
using Core.MemoryManagement;
namespace Core.Maths.BlockOperationMatrices
{
    public interface IBlockOperationMatrixReadOnDemand
    {

        protected int _OffsetTop { get; set; }
        protected int _OffsetLeft { get; set; }
        protected DelegateReadBlockMatrixDataBytes? _ReadDataBytes { get; set; }
    }
}
