using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Exceptions
{

    [DataContract]
    public class IllegalCharacterException : Exception
    {
        private char _Character;
        public char Character { get { return _Character; } }
        protected IllegalCharacterException() { }
        public IllegalCharacterException(char c):base(""+c) {
            _Character = c;
        }
    }
}
