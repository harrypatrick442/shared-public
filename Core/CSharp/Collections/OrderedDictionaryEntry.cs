
using System.Collections.Generic;

namespace Core.Collections
{
    public sealed class OrderedDictionaryEntry<TValue>
    {
        public TValue Value { get; set; }
        public LinkedListNode<OrderedDictionaryEntry<TValue>> LinkedListNode { get; set; }
        public OrderedDictionaryEntry(TValue value)
        {
            Value = value;
        }
    }
}