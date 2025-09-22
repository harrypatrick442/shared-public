using System;

namespace Core.Events
{
    public class ItemEventArgs<TItem>: EventArgs
    {
        private TItem _Item;
        public TItem Item { get { return _Item; } }
        public ItemEventArgs(TItem item) {
            _Item = item;
        }
    }
}