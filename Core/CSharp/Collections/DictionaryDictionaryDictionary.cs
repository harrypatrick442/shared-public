using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;

namespace Core.Collections
{
    public sealed class DictionaryDictionaryDictionary<TKey, TValue>
    {
        private Dictionary<TKey, Dictionary<TKey, Dictionary<TKey,
            TValue>>> _Dictionary
                        = new Dictionary<TKey, Dictionary<TKey, Dictionary<TKey, TValue>>>();

        public void Map(TKey[] keys, TValue value)
        {
            Map(keys[0], keys[1], keys[2], value);
        }
        public void Map(TKey a, TKey b, TKey c, TValue value)
        {
            if (!_Dictionary.TryGetValue(a, out Dictionary<TKey, Dictionary<TKey, TValue>>? bC))
            {
                bC = new Dictionary<TKey, Dictionary<TKey, TValue>> { { b,
                                new Dictionary<TKey, TValue>{{ c, value } } } };
                _Dictionary[a] = bC;
                return;
            }
            if (!bC.TryGetValue(b, out Dictionary<TKey, TValue>? cE))
            {
                cE = new Dictionary<TKey, TValue> { { c, value } };
                bC[b] = cE;
                return;
            }
            if (cE.ContainsKey(c))
            {
                throw new Exception("Attempted to duplicate entry");
            }
            cE[c] = value;
        }
        public TValue QueryNoChecks(TKey a, TKey b, TKey c) {
            return _Dictionary[a][b][c];
        }
        public bool ContainsKey(TKey a, TKey b, TKey c)
        {
            if (!_Dictionary.TryGetValue(a, out Dictionary<TKey, Dictionary<TKey, TValue>>? bC))
            {
                return false;
            }
            if (!bC.TryGetValue(b, out Dictionary<TKey, TValue>? cE))
            {
                return false;
            }
            return cE.ContainsKey(c);
        }
        public bool TryGetValue(TKey[] keys, out TValue value)
        {
            if (keys.Length!= 3) {
                throw new ArgumentException($"{nameof(keys)}.{nameof(keys.Length)} was not 3");
            }
            return TryGetValue(keys[0], keys[1], keys[2], out value);
        }
        public bool TryGetValue(TKey a, TKey b, TKey c, out TValue value)
        {
            if (!_Dictionary.TryGetValue(a, out Dictionary<TKey, Dictionary<TKey, TValue>>? bC)) {
                value = default(TValue);
                return false;
            }
            if (!bC.TryGetValue(b, out Dictionary<TKey, TValue>? cE))
            {
                value = default(TValue);
                return false;
            }
            return cE.TryGetValue(c, out value);
        }
    }
}