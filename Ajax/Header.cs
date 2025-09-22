using System;
namespace Ajax
{
    public class Header
    {
        private string[] _Values;
        public string[] Values { get { return _Values; } }
        public bool MultipleValues { get { return _Values.Length>1; } }
        private string _Name;
        public string Name { get { return _Name; } }
        public string Value { get { return _Values[0]; } }
        public Header(string name, params string[] values)
        {
            if (values==null) throw new ArgumentNullException(nameof(values));
            if (values.Length < 1) throw new ArgumentException("No values provided");
            _Name = name;
            _Values = values;
        }
    }
}
