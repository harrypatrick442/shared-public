using System;
using System.Collections.Generic;
using System.Threading;

namespace Core.MemoryManagement
{
    public interface IMemoryManaged
    {
        void ReduceMemoryFootprintByProportion(float proportion, CancellationToken? cancellationToken);
    }
}