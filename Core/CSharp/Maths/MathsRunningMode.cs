using Core.FileSystem;
using Core.Maths.Core.Maths;
using Core.MemoryManagement;
using Shutdown;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Core.Maths
{
    public enum MathsRunningMode
    {
        GpuOnly,
        CpuOnly,
        Whatever
    }
}
