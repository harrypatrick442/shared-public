using System;
using System.IO;
using System.Net;

namespace Core.CSV
{
	public class StringBuffer : ICSVBuffer
	{
		private string _Content;
		private int _Index=-1;
		public StringBuffer(string content){
			_Content = content;
		}
		public bool HasNext{
			get{
				return _Index < _Content.Length-1;
			}
		}
		public char Next
		{
			get
			{
				return _Content[++_Index];
			}
		}
		public char Current {
			get{
			return _Content[_Index];
			}
		}
		public char[] Advance(int nIndices)
		{
			char[] chars = new char[nIndices];
			for (int i = 0; i < nIndices; i++)
			{
				chars[i] = Next;
			}
			return chars;
		}
		public char LookAhead(int nIndices)
		{
			int nLeft = _Content.Length - (1 + _Index);
			if (nLeft < nIndices) throw new CSVReaderException("Something went wrong. Not this many indices left");
			return _Content[ _Index + nIndices];
		}
	}
}