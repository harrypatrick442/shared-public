using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Core.Maths.Matrices
{
    public class BlockMatrix_Block
    {
        public long Index { get; set; }
        public byte[] Data { get; set; }
        public bool IsDirty { get; set; }

        public BlockMatrix_Block(long index, byte[] data)
        {
            Index = index;
            Data = data;
            IsDirty = false;
        }
    }
}
