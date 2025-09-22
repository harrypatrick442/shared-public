using System;

namespace Core.Maths.Values
{
    public class MinValueHistoryOverflowing
    {
        private int _Length;
        private int _Index;
        private double[] _Values;
        private double _CurrentValue;
        private int _RoundsCountForBackupRecomputeMin;
        public double Value
        {
            get { return _CurrentValue; }
        }

        public MinValueHistoryOverflowing(int length)
        {
            if(length < 1) {
                throw new ArgumentException(nameof(length));
            }
            _Length = length;
            _Values = new double[length];
            _CurrentValue = 0;
        }

        public void Add(double value)
        {
            if(value<0) { 
                throw new ArgumentException($"An value should be greater or equal to zero. \"value\" was {value}");
            }
            if (value < _CurrentValue) {
                AddValueLessThanCurrentMin(value);
                return;
            }
            if (value > _CurrentValue)
            {
                AddValueGreaterThanCurrentMin(value);
                return;
            }
            AddValueEqualToCurrentMin(value);
        }
        private void AddValueLessThanCurrentMin(double value) {

            _CurrentValue = value;
            _Values[_Index++] = value;
            if (_Index >= _Length)
            {
                _Index = 0;
            }
            _RoundsCountForBackupRecomputeMin = 0;
        }
        private void AddValueGreaterThanCurrentMin(double value) {
            double beingRemoved = _Values[_Index];
            _Values[_Index++] = value;
            if (beingRemoved <= _CurrentValue)
            {
                RecomputeMin();
                _RoundsCountForBackupRecomputeMin = 0;
                if (_Index >= _Length)
                {
                    _Index = 0;
                }
                return;
            }
            if (_Index >= _Length)
            {
                _Index = 0;
                if (_RoundsCountForBackupRecomputeMin >= 100)
                {
                    _RoundsCountForBackupRecomputeMin = 0;
                    RecomputeMin();
                    return;
                }
                _RoundsCountForBackupRecomputeMin++;
            }
        }
        private void AddValueEqualToCurrentMin(double value) {
            _Values[_Index++] = value;
            if (_Index >= _Length)
            {
                _Index = 0;
                if (_RoundsCountForBackupRecomputeMin >= 100)
                {
                    _RoundsCountForBackupRecomputeMin = 0;
                    RecomputeMin();
                    return;
                }
                _RoundsCountForBackupRecomputeMin++;
            }
        }
        public void Clear()
        {
            for (int i = 0; i < _Length; i++)
            {
                _Values[i] = 0;
            }
            _CurrentValue = 0;
            _RoundsCountForBackupRecomputeMin = 0;
        }
        private void RecomputeMin()
        {
            _CurrentValue = _Values[0];
            int i = 1;
            do
            {
                double value = _Values[i++];
                if (value > _CurrentValue)
                {
                    _CurrentValue = value;
                }
            }
            while (i < _Length);
        }
    }
}
