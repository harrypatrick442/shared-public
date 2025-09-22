using Core.Events;
using System;

namespace Core.Events
{
    public class TypedMessageEventArgs : MessageEventArgs
    {
        private string _Type;
        public string Type{ get { return _Type; } }
        public TypedMessageEventArgs(string type, string message):base(message)
        {
            _Type = type;
        }
    }
}