using System;
using System.Timers;
using Core.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Core
{
		public class Iterator<TList, TItem> where TList:IList<TItem>
	{
		private int _Index = 0, _Count;
		private TList _Items;
		public bool HasNext { get { return _Index < _Count; } }
		public TItem Next() {
			return _Items[_Index++];
		}
		public TItem RemoveNext() {
			TItem item = _Items[_Index];
			_Count--;
			_Items.RemoveAt(_Index);
			return item;
		}
		public void Append(TItem item) {
			_Items.Add(item);
			_Count++;
		}
		public void Remove() {
			_Index--;
			_Count--;
			_Items.RemoveAt(_Index);
		}
		public void Insert(TItem item) {
			_Items.Insert(_Index - 1, item);
			_Index++;
			_Count++;
		}
		public TList RemainingList() {
			return _Items;
		}
		public Iterator(TList items)
		{
			_Items = items;
			_Count = _Items.Count;
		}
	}
}
