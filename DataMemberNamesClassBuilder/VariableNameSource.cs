using MessageTypes.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DataMemberNamesClassBuilder
{
    public class VariableNameSource<TKey>
    {
        private int _CharIndex = 0, _NOfChar = 0;
        private const string ALLOWED_CHARS = "abcdefghijiklmnopqrstuvwxyz";
        
        private Dictionary<TKey, string> _MapKeyToVariableName = new Dictionary<TKey, string>();
        private HashSet<string> _InUse = new HashSet<string>();
        public string GetVariableName(TKey key, string defaultString) {
            if (_MapKeyToVariableName.TryGetValue(key, out string variableName)) {
                return variableName;
            }
            variableName = defaultString;
            if (_InUse.Contains(variableName))
            {
                variableName = NextVariableName();
            }
            else {
                _InUse.Add(variableName);
            }
            _MapKeyToVariableName[key] = variableName;
            return variableName;
        }
        public string GetUnderscoredVariableName(TKey key, string defaultString) { 
            return "_"+GetVariableName(key, defaultString);
        }
        private string NextVariableName() {
            char c = ALLOWED_CHARS[_CharIndex];
            string value = c + (_NOfChar == 0 ? "" : _NOfChar.ToString());
            _CharIndex++;
            if(_CharIndex>= ALLOWED_CHARS.Length)
            {
                _CharIndex = 0;
                _NOfChar++;
            }
            return value;
        }

    }
}
