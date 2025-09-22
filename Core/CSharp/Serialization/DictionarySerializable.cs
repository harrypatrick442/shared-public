using System;
using System.Collections.Generic;
using System.Reflection;
using JSON;

namespace Core.Serialization
{
    /// <summary>
    /// This is designed to be a means to deserialize a complex object without dropping off any properties
    /// It is not designed to always return the latest state of the thing it comes from. To do that create a new instance each time you read it then save it back.
    /// </summary>
    public abstract class DictionarySerializable
    {

        private object _LockObjectGetSetMember = new object();
        private IJsonParser _JSONParser;
        protected Dictionary<string, string> _MapNameToSerializedObject;
        protected Dictionary<string, object> _MapNameToDeserializedObject = new Dictionary<string, object>();
        protected TObject Get<TObject>(string name)
        {
            lock (_LockObjectGetSetMember)
            {
                if (_MapNameToDeserializedObject.ContainsKey(name))
                    return (TObject)_MapNameToDeserializedObject[name];
                if (!_MapNameToSerializedObject.ContainsKey(name)) return default(TObject);
                string str = _MapNameToSerializedObject[name];
                object obj = _JSONParser.Deserialize<TObject>(str);
                _MapNameToDeserializedObject[name] = obj;
                return (TObject)obj;
            }
        }
        protected void Set<T>(T t, string name) where T : class
        {
            lock (_LockObjectGetSetMember)
            {
                _MapNameToDeserializedObject[name] = t;
                _MapNameToSerializedObject[name] = _JSONParser.Serialize(t, true);
            }
        }
        public void SerializeObjectsToMap()
        {
            MethodInfo methodInfoSerialize = typeof(IJsonParser).GetMethod(nameof(IJsonParser.Serialize));
            foreach (string name in _MapNameToDeserializedObject.Keys)
            {
                object value = _MapNameToDeserializedObject[name];
                if (value == null)
                    _MapNameToSerializedObject[name] = null;
                else
                {
                    string serialized = (string)methodInfoSerialize.MakeGenericMethod(value.GetType()).Invoke(_JSONParser, new object[] { value, false });
                    _MapNameToSerializedObject[name] = serialized;
                }
            }
        }
        protected DictionarySerializable(IJsonParser jsonParser, Dictionary<string, string> mapNameToSerializedObject)
        {
            _JSONParser = jsonParser;
            _MapNameToSerializedObject = mapNameToSerializedObject;
        }
    }
}