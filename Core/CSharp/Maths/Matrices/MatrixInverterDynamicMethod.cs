using Core.Maths.Matrices;
using Core.Timing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Maths
{
    public class MatrixInverterDynamicMethod
    {
        private long INTERVAL_UPDATE_MILLISECONDS = 60;
        private long _NextUpdate = 0;
        private bool _CollectingUpdateInfo = false;
        private int _NSamples = 5;
        private Func<double[][], double[][]>? _Invert = null;
        private Dictionary<MatrixInversionMethod, List<long>> _MapMatrixInversionMethodToTimesTaken = new Dictionary<MatrixInversionMethod, List<long>>();
        public double[][] Invert(double[][] matrix)
        {
            if (TimeHelper.MillisecondsNow >= _NextUpdate)
            {
                _CollectingUpdateInfo = true;
            }
            double[][] result;
            if (_CollectingUpdateInfo)
            {
                int nInMemorySamples = _InMemorySamples.Count;
                if (nInMemorySamples < _GPUSamples.Count)
                {
                    long startTime = TimeHelper.MillisecondsNow;
                    result = InvertInMemory(matrix);
                    long now = TimeHelper.MillisecondsNow;
                    long duration = now - startTime;
                    _InMemorySamples.Add(duration);
                    if (++nInMemorySamples >= _NSamples)
                    {
                        long totalTimeInMemory = _InMemorySamples.Sum();
                        long totalTimeGPU = _GPUSamples.Sum();
                        _InMemorySamples.Clear();
                        _GPUSamples.Clear();
                        _CollectingUpdateInfo = false;
                        _Invert = totalTimeInMemory < totalTimeGPU ? InvertInMemory : InvertWithGPU;
                        _NextUpdate = now + INTERVAL_UPDATE_MILLISECONDS;
                    }
                }
                else
                {
                    long startTime = TimeHelper.MillisecondsNow;
                    result = InvertWithGPU(matrix);
                    long duration = TimeHelper.MillisecondsNow - startTime;
                    _GPUSamples.Add(duration);
                }
            }
            else {
                result = _Invert!(matrix);
            }
            return result;
        }
        private double[][] InvertInMemory(double[][] matrix) {
            return MatrixHelper.Invert(matrix);
        }
        private double[][] InvertWithGPU(double[][] matrix) { 

        }
    }
}