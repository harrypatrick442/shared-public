using System.IO;
using System;
using Core.Maths.BlockOperationMatrices;
using System.Collections.Generic;
using System.Collections.Concurrent;


namespace Core.Maths.BlockOperationMatrices
{
    public delegate byte[] DelegateReadBlockMatrixDataBytes(
        int nRows, int nColumns, int offsetTop, int offsetLeft);
}
