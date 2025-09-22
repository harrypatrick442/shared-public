using Core.Maths.Matrices;
using Core.Timing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Maths
{
    public interface IMatrixInverter
    {
        public IBigMatrix Invert(IBigMatrix bigMatrix);
    }
}