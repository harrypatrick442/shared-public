using System;
using System.Collections.Generic;

namespace Core.Maths.BlockOperationMatrices.Interfaces
{
    public interface IBlockOperationMatrixPartitioned
    {
        public BlockOperationMatrix ChildTopLeftA { get; }
        public BlockOperationMatrix ChildTopRightB { get; }
        public BlockOperationMatrix ChildBottomLeftC { get; }
        public BlockOperationMatrix ChildBottomRightD { get; }
    }
}
