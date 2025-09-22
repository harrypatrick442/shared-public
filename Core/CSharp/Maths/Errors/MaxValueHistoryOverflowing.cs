using System;

namespace Core.Maths.Values
{
    public class MaxValueHistoryOverflowing
    {
        private int _Length;
        private int _Index;
        private double[] _Values;
        private double _CurrentMax;
        private int _RoundsCountForBackupRecomputeMax;
        public double Value
        {
            get { return _CurrentMax; }
        }

        public MaxValueHistoryOverflowing(int length)
        {
            if(length < 1) {
                throw new ArgumentException(nameof(length));
            }
            _Length = length;
            _Values = new double[length];
            _CurrentMax = 0;
        }

        public void Add(double value)
        {
            if(value<0) { 
                throw new ArgumentException($"An value should be greater or equal to zero. \"value\" was {value}");
            }
            if (value > _CurrentMax)
            {
                AddValueGreaterThanCurrentMax(value);
                return;
            }
            if (value < _CurrentMax) {
                AddValueLessThanCurrentMax(value);
                return;
            }
            AddValueEqualToCurrentMax(value);
        }
        private void AddValueGreaterThanCurrentMax(double value) {

            _CurrentMax = value;
            _Values[_Index++] = value;
            if (_Index >= _Length)
            {
                _Index = 0;
            }
            _RoundsCountForBackupRecomputeMax = 0;
        }
        private void AddValueLessThanCurrentMax(double value) {
            double beingRemoved = _Values[_Index];
            _Values[_Index++] = value;
            if (beingRemoved >= _CurrentMax)
            {
                RecomputeMax();
                _RoundsCountForBackupRecomputeMax = 0;
                if (_Index >= _Length)
                {
                    _Index = 0;
                }
                return;
            }
            if (_Index >= _Length)
            {
                _Index = 0;
                if (_RoundsCountForBackupRecomputeMax >= 100)
                {
                    _RoundsCountForBackupRecomputeMax = 0;
                    RecomputeMax();
                    return;
                }
                _RoundsCountForBackupRecomputeMax++;
            }
        }
        private void AddValueEqualToCurrentMax(double value) {
            _Values[_Index++] = value;
            if (_Index >= _Length)
            {
                _Index = 0;
                if (_RoundsCountForBackupRecomputeMax >= 100)
                {
                    _RoundsCountForBackupRecomputeMax = 0;
                    RecomputeMax();
                    return;
                }
                _RoundsCountForBackupRecomputeMax++;
            }
        }
        public void Clear()
        {
            for (int i = 0; i < _Length; i++)
            {
                _Values[i] = 0;
            }
            _CurrentMax = 0;
            _RoundsCountForBackupRecomputeMax = 0;
        }
        private void RecomputeMax()
        {
            _CurrentMax = _Values[0];
            int i = 1;
            do
            {
                double value = _Values[i++];
                if (value > _CurrentMax)
                {
                    _CurrentMax = value;
                }
            }
            while (i < _Length);
        }
    }
}
