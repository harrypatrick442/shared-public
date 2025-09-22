using System;
using System.Collections.Generic;

namespace Core.Collections
{
    public sealed class DictionaryDictionaryDictionaryDictionary<TKey, TValue>
    {
        private Dictionary<TKey, Dictionary<TKey, Dictionary<TKey, Dictionary<TKey, TValue>>>> _Dictionary
                        = new Dictionary<TKey, Dictionary<TKey, Dictionary<TKey, Dictionary<TKey, TValue>>>>();

        // Map method accepting an array of keys
        public void Map(TKey[] keys, TValue value)
        {
            if (keys.Length != 4)
            {
                throw new ArgumentException($"{nameof(keys)}.{nameof(keys.Length)} was not 4");
            }
            Map(keys[0], keys[1], keys[2], keys[3], value);
        }

        // Map method accepting four separate keys
        public void Map(TKey a, TKey b, TKey c, TKey d, TValue value)
        {
            if (!_Dictionary.TryGetValue(a, out Dictionary<TKey, Dictionary<TKey, Dictionary<TKey, TValue>>>? bC))
            {
                bC = new Dictionary<TKey, Dictionary<TKey, Dictionary<TKey, TValue>>>
                {
                    { b, new Dictionary<TKey, Dictionary<TKey, TValue>>
                        {
                            { c, new Dictionary<TKey, TValue>
                                {
                                    { d, value }
                                }
                            }
                        }
                    }
                };
                _Dictionary[a] = bC;
                return;
            }

            if (!bC.TryGetValue(b, out Dictionary<TKey, Dictionary<TKey, TValue>>? cE))
            {
                cE = new Dictionary<TKey, Dictionary<TKey, TValue>>
                {
                    { c, new Dictionary<TKey, TValue>
                        {
                            { d, value }
                        }
                    }
                };
                bC[b] = cE;
                return;
            }

            if (!cE.TryGetValue(c, out Dictionary<TKey, TValue>? dF))
            {
                dF = new Dictionary<TKey, TValue> { { d, value } };
                cE[c] = dF;
                return;
            }

            if (dF.ContainsKey(d))
            {
                throw new Exception("Attempted to duplicate entry");
            }

            dF[d] = value;
        }

        // Query with no checks
        public TValue QueryNoChecks(TKey a, TKey b, TKey c, TKey d)
        {
            return _Dictionary[a][b][c][d];
        }

        // ContainsKey check for four levels
        public bool ContainsKey(TKey[] keys) {
            if (keys.Length != 4) throw new ArgumentException($"The length of {keys} must be 4");
            return ContainsKey(keys[0], keys[1], keys[2], keys[3]);
        }
        public bool ContainsKey(TKey a, TKey b, TKey c, TKey d)
        {
            if (!_Dictionary.TryGetValue(a, out Dictionary<TKey, Dictionary<TKey, Dictionary<TKey, TValue>>>? bC))
            {
                return false;
            }

            if (!bC.TryGetValue(b, out Dictionary<TKey, Dictionary<TKey, TValue>>? cE))
            {
                return false;
            }

            if (!cE.TryGetValue(c, out Dictionary<TKey, TValue>? dF))
            {
                return false;
            }

            return dF.ContainsKey(d);
        }

        // TryGetValue with an array of keys
        public bool TryGetValue(TKey[] keys, out TValue value)
        {
            if (keys.Length != 4)
            {
                throw new ArgumentException($"{nameof(keys)}.{nameof(keys.Length)} was not 4");
            }

            return TryGetValue(keys[0], keys[1], keys[2], keys[3], out value);
        }

        // TryGetValue for four separate keys
        public bool TryGetValue(TKey a, TKey b, TKey c, TKey d, out TValue value)
        {
            if (!_Dictionary.TryGetValue(a, out Dictionary<TKey, Dictionary<TKey, Dictionary<TKey, TValue>>>? bC))
            {
                value = default(TValue);
                return false;
            }

            if (!bC.TryGetValue(b, out Dictionary<TKey, Dictionary<TKey, TValue>>? cE))
            {
                value = default(TValue);
                return false;
            }

            if (!cE.TryGetValue(c, out Dictionary<TKey, TValue>? dF))
            {
                value = default(TValue);
                return false;
            }

            return dF.TryGetValue(d, out value);
        }
    }
}
