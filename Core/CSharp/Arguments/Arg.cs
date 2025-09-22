
using JSON;
using Core.NativeExtensions;
using Snippets.NativeExtensions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Text;

namespace Core.Arguments
{
    [DataContract]
    public class Arg{
        private string _KeyNoPoll;
        [JsonPropertyName("keyNoPoll")]
        [JsonInclude]
        [DataMember(Name="keyNoPoll")]
        public string KeyNoPoll { get { return _KeyNoPoll; } protected set { _KeyNoPoll = value; } }
        private string _RawKey;
        [JsonPropertyName("rawKey")]
        [JsonInclude]
        [DataMember(Name = "rawKey")]
        public string RawKey { get { return _RawKey; } protected set { _RawKey = value; } }
        private string _Value;
        [JsonPropertyName("rawValue")]
        [JsonInclude]
        [DataMember(Name = "rawValue")]
        public string RawValue { get { return _Value; } protected set { _Value = value; } }
        public T GetValue<T>() {
            if (_Value == null) {
                return default(T);
            }
            T t;
            _Value.TryCast(out t);
            return t;
        }
        protected Arg()
        {

        }
        public Arg(string key)
        {
            Initialize(key, null);
        }
        public Arg(string key, string value) {
            Initialize(key, value);
        }
        private void Initialize(string key, string value)
        {
            _RawKey = key;
            _KeyNoPoll = RemovePoll(key);
            _Value = value;
        }
        public static string RemovePoll (string rawKey) { 
            return rawKey.Replace("-", "").Replace("—", "");
        }
        public static Arg ParseValueToJSONAndEncodeAsBase64<TValue>(string key, TValue value) where TValue:class {
            return new Arg(key, Encoding.Base64.Encode(Json.Serialize<TValue>(value)));
        }
        public override string ToString() {
            string str = _RawKey;
            if (_Value != null) str += $"={_Value}";
            return str;
        }
    }
}
