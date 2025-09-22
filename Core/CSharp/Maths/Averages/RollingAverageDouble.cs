using ManagedCuda;
using ManagedCuda.BasicTypes;
using System;
using System.Runtime.InteropServices;
namespace Core.Maths.Averages
{
    public class RollingAverageDouble
    {
        private int _HistorySize;
        private int _HistorySizeMinusOne;
        private double _RollingAverage = 0;
        private int _NLogged = 0;
        private bool _Filled = false;
        public double Value => _RollingAverage;
        public RollingAverageDouble(int historySize) { 
            _HistorySize = historySize;
            _HistorySizeMinusOne = historySize - 1;
        }
        public double Update(double value) {
            if (_Filled)
            {
                _RollingAverage = ((_RollingAverage * (_HistorySizeMinusOne)) + value) / _NLogged;
                return _RollingAverage;
            }
            if (_NLogged < 1)
            {
                _RollingAverage = value;
                _NLogged++;
                return _RollingAverage;
            }
            if (_NLogged < _HistorySize)
            {
                _RollingAverage = ((_NLogged++ * _RollingAverage) + value)
                    / _NLogged;
                return _RollingAverage;
            }
            _RollingAverage = ((_RollingAverage * (_NLogged - 1)) + value) / _NLogged;
            _Filled = true;
            return _RollingAverage;
        }
    }
}