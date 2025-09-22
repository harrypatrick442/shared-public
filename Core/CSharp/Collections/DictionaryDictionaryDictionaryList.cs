using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;

namespace Core.Collections
{
    public sealed class DictionaryDictionaryDictionaryList<TKey, TValue>
    {
        private Dictionary<TKey, Dictionary<TKey, Dictionary<TKey,
            List<TValue>>>> _Dictionary
                        = new Dictionary<TKey, Dictionary<TKey, Dictionary<TKey, List<TValue>>>>();
        public void Map(TKey[] keys, TValue element)
        {
            Map(keys[0], keys[1], keys[2], element);
        }
        public void Map(TKey a, TKey b, TKey c, TValue element){
            if (!_Dictionary.TryGetValue(a, out Dictionary<TKey, Dictionary<TKey, List<TValue>>>? bC))
            {
                bC = new Dictionary<TKey, Dictionary<TKey, List<TValue>>> { { b,
                                new Dictionary<TKey, List<TValue>>{{ c, new List<TValue> { element } } } } };
                _Dictionary[a] = bC;
                return;
            }
            if (!bC.TryGetValue(b, out Dictionary<TKey, List<TValue>>? cE))
            {
                cE = new Dictionary<TKey, List<TValue>> { { c, new List<TValue> { element } } };
                bC[b] = cE;
                return;
            }
            if (!cE.TryGetValue(c, out List<TValue>? es))
            {
                es = new List<TValue> { element };
                cE[c] = es;
                return;
            }
            es.Add(element);
        }
        public List<TValue> QueryNoChecks(TKey a, TKey b, TKey c) {
            var bC = _Dictionary[a];
            var cE = bC[b];
            return cE[c];
        } 
    }
}