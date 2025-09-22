namespace SnippetsCore.Lists
{
    public class LinkedList<TEntry>
    {
        private int _Count = 0;
        public int Count { get { return _Count; } }
        private LinkedListEntry<TEntry> _FirstEntry;
        public LinkedListEntry<TEntry> FirstEntry { get { return _FirstEntry; } }
        private LinkedListEntry<TEntry> _LastEntry;
        public LinkedListEntry<TEntry> LastEntry { get { return _LastEntry; } }
        public LinkedListEntry<TEntry> Append(TEntry entry) {
            if (_LastEntry == null)
            {
                _FirstEntry = new LinkedListEntry<TEntry>(null, entry);
                _LastEntry = _FirstEntry;
                _Count++;
                return _FirstEntry;
            }
            LinkedListEntry<TEntry> previousLastEntry = _LastEntry;
            _LastEntry = new LinkedListEntry<TEntry>(previousLastEntry, entry);
            _Count++;
            return _LastEntry;
        }
        public void RemoveLast()
        {
            if (_LastEntry == null) return;
            if (_LastEntry.Previous != null)
            {
                _LastEntry.Previous.Next = null;
                _LastEntry = _LastEntry.Previous;
            }
            else
            {
                _LastEntry = null;
                _FirstEntry = null;
            }
            _Count--;
        }
        public void Remove(LinkedListEntry<TEntry> entry) {
            if (entry.Previous != null)
            {
                if (entry.Next != null)
                {
                    entry.Next.Previous = entry.Previous;
                    entry.Previous.Next = entry.Next;
                }
                else
                {
                    _LastEntry = entry.Previous;
                    entry.Previous.Next = null;
                }
            }
            else
            {
                _FirstEntry = entry.Next;
                entry.Next.Previous = null;
            }
            _Count--;
        }

    }
}