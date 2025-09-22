using Core.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace Snippets.NativeExtensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> Combine<TKey, TValue>(this IDictionary<TKey, TValue> dictionaryMe,
            IDictionary<TKey, TValue> otherDictionary, bool throwOnKeyAlreadyExists = false)
        {
            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
            dictionaryMe.ToList().ForEach(x => dictionary[x.Key] = x.Value);
            dictionary.AddRange(otherDictionary);
            return dictionary;
        }
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionaryMe, IDictionary<TKey, TValue> otherDictionary, bool throwOnKeyAlreadyExists = false)
        {
            otherDictionary.ToList().ForEach(x =>
            {
                if (throwOnKeyAlreadyExists && dictionaryMe.ContainsKey(x.Key)) throw new DuplicateKeyException(x.Key);
                dictionaryMe[x.Key] = x.Value;
            });
        }
    }
}
