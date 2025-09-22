
using System.Collections.Generic;
namespace Core {
    public class MappedLists<TKey, TListValue>
    {
        private Dictionary<TKey, List<TListValue>> _MapKeyToListOfValues = new Dictionary<TKey, List<TListValue>>();
        public bool AddIfDoesntContain(TKey key, TListValue value) {
            if (!_MapKeyToListOfValues.ContainsKey(key)) {
                _MapKeyToListOfValues.Add(key, new List<TListValue> { value});
                return true;
            }
            List<TListValue> list = _MapKeyToListOfValues[key];
            if (list.Contains(value)) return false;
            list.Add(value);
            return true;
        }
        public bool RemoveIfContains(TKey key, TListValue value) {
            if (!_MapKeyToListOfValues.ContainsKey(key)) return false;
            List<TListValue> list = _MapKeyToListOfValues[key];
            if (!list.Contains(value)) return false;
            list.Remove(value);
            return true;
        }
        public bool ContainsKey(TKey key) {
            return _MapKeyToListOfValues.ContainsKey(key);
        }
        public TListValue[] GetArray(TKey key) {
            return _MapKeyToListOfValues[key].ToArray();
        }
    }
}
