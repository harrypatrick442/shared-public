using System.IO;
using System;
using Core.Maths.BlockOperationMatrices;
using System.Collections.Generic;
using System.Collections.Concurrent;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Core.Threading;
using System.Threading.Tasks;
using Core.FileSystem;


namespace Core.Maths.BlockOperationMatrices
{
    public enum BlockOperationMatrixType
    {
        ReadOnDemandFromSource,
        DataProvidedInConstructor,
        PartitionedBuiltFromBlocks

    }
}
