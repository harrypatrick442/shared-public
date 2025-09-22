using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Collections
{
    public sealed class StagedDictionaryBuffer<TKey, TValue>
    {
        private Dictionary<TKey, TValue>[] _Dictionaries;
        public StagedDictionaryBuffer(int nStages) {
            if (nStages < 2) throw new ArgumentException($"{nameof(nStages)} must be 2 or greater");
            _Dictionaries = new Dictionary<TKey, TValue>[nStages];
            for (int i = _Dictionaries.Length - 1; i >= 0; i--)
            {
                _Dictionaries[i] = new Dictionary<TKey, TValue>();
            }
        }
        public bool TryGetValue(TKey key, out TValue value)
        {
            for(int i=_Dictionaries.Length-1; i>=0; i--)
            {
                if(_Dictionaries[i].TryGetValue(key, out value))
                    return true;
            }
            value = default(TValue);
            return false;
        }
        public void AddIntoLatest(TKey key, TValue value)
        {
            _Dictionaries[_Dictionaries.Length - 1][key] = value;
        }
        public void AddOrReplaceIntoLatest(TKey key, TValue value)
        {
            for (int i = 0; i < _Dictionaries.Length - 1; i++)
            {
                _Dictionaries[i].Remove(key);
            }
            _Dictionaries[_Dictionaries.Length - 1][key] = value;
        }
        public bool ContainsKey(TKey key)
        {
            for (int i = 0; i < _Dictionaries.Length; i++)
            {
                if (_Dictionaries[i].ContainsKey(key))
                    return true;
            }
            return false;
        }
        public void Remove(TKey key)
        {
            for (int i = 0; i < _Dictionaries.Length; i++)
            {
                _Dictionaries[i].Remove(key);
            }
        }
        public void Switch() { 
            for(int i=0; i<_Dictionaries.Length-1; i++) {
                _Dictionaries[i] = _Dictionaries[i + 1];
            }
        }
    }
}