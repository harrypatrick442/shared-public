using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Collections
{
    public sealed class OrderedDictionary<TKey, TValue>
    {
        private LinkedList<OrderedDictionaryEntry<TValue>> _LinkedList =new LinkedList<OrderedDictionaryEntry<TValue>>();
        private Dictionary<TKey, OrderedDictionaryEntry<TValue>> _Dictionary = new Dictionary<TKey, OrderedDictionaryEntry<TValue>>();
        private Func<TValue, TKey> _GetKeyFromValue;
        public int Length { get { return _Dictionary.Count; } }
        public OrderedDictionary(Func<TValue, TKey> getKeyFromValue, params TValue[] values) {
            _GetKeyFromValue = getKeyFromValue;
            foreach(TValue value in values)
                AppendOrMoveToLast(value);
        }
        public void AppendOrMoveToLast(TValue value) {
            TKey key = _GetKeyFromValue(value);
            if (_Dictionary.TryGetValue(key, out OrderedDictionaryEntry<TValue> existingEntry)) {
                _LinkedList.Remove(existingEntry.LinkedListNode);
                existingEntry.Value = value;
                existingEntry.LinkedListNode = _LinkedList.AddLast(existingEntry);
                return;
            }
            OrderedDictionaryEntry<TValue> entry = new OrderedDictionaryEntry<TValue>(value);
            _Dictionary.Add(key, entry);
            LinkedListNode<OrderedDictionaryEntry<TValue>> linkedListNode = _LinkedList.AddLast(entry);
            entry.LinkedListNode = linkedListNode;
        }
        public bool TryGetValue(TKey key, out TValue value) {
            if (_Dictionary.TryGetValue(key, out OrderedDictionaryEntry<TValue> entry)) {
                value = entry.Value;
                return true;
            }
            value = default(TValue);
            return false;
        }
        public bool Remove(TValue value)
        {
            TKey key = _GetKeyFromValue(value);
            return RemoveByKey(key);
        }
        public bool RemoveByKey(TKey key)
        {
            if (_Dictionary.TryGetValue(key, out OrderedDictionaryEntry<TValue> existingEntry))
            {
                _LinkedList.Remove(existingEntry.LinkedListNode);
                _Dictionary.Remove(key);
                return true;
            }
            return false;
        }
        public TValue First { 
            get {
                OrderedDictionaryEntry<TValue> linkedListNode =  _LinkedList.FirstOrDefault();
                if (linkedListNode == null)
                    return default(TValue);
                return linkedListNode.Value;
            }
        }
        public TValue Last
        {
            get
            {
                OrderedDictionaryEntry<TValue> linkedListNode = _LinkedList.LastOrDefault();
                if (linkedListNode == null)
                    return default(TValue);
                return linkedListNode.Value;
            }
        }
        public TValue[] ToArray() {
            return _LinkedList.Select(e => e.Value).ToArray();
        }
        //TODO fix if mismatch in linked list and dictionary count. First check how dictionary count and linked list count are stored. Use longest one for fix. Might not even be necessary.
        public IEnumerable<TValue> TakeFromFirstWhile(Func<TValue, bool> callbackWhile)
        {
            LinkedListNode<OrderedDictionaryEntry<TValue>> linkedListNode = _LinkedList.First;

            while (linkedListNode != null)
            {
                TValue value = linkedListNode.Value.Value;
                if (!callbackWhile(value)) break;
                LinkedListNode<OrderedDictionaryEntry<TValue>> nextLinkedListNode
                    = linkedListNode.Next;
                _Dictionary.Remove(this._GetKeyFromValue(value));
                _LinkedList.Remove(linkedListNode);
                linkedListNode = nextLinkedListNode;
                yield return value;
            }
        }
        public void RemoveNFromFirst(int n)
        {
            LinkedListNode<OrderedDictionaryEntry<TValue>> linkedListNode = _LinkedList.First;

            int i = 0;
            while (linkedListNode != null&&i<n)
            {
                TValue value = linkedListNode.Value.Value;
                LinkedListNode<OrderedDictionaryEntry<TValue>> nextLinkedListNode
                    = linkedListNode.Next;
                _Dictionary.Remove(this._GetKeyFromValue(value));
                _LinkedList.Remove(linkedListNode);
                linkedListNode = nextLinkedListNode;
                i++;
            }
        }
        public IEnumerable<TValue> GetNEntriesFromEnd(int n) {
            return _LinkedList.TakeLast(n).Select(e=>e.Value);
        }
    }
}