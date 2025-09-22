using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Snippets.Core.ImageProcessing
{
    [DataContract]
    [KnownType(typeof(RGBAValuePair))]
    public class RGBAValuePair
    {
        private object _Value;
        [JsonPropertyName("value")]
        [JsonInclude]
        [DataMember(Name ="value")]
        public object Value{get{return _Value;} protected set{_Value = value;}}
        private RGBABytes _RGBABytes;
        [JsonPropertyName("rgba")]
        [JsonInclude]
        [DataMember(Name="rgba")]
        public RGBABytes RGBABytes { get { return _RGBABytes; }protected set { _RGBABytes = value; } }
        public RGBAValuePair(RGBABytes rgbaBytes, object value) {
            _RGBABytes = rgbaBytes;
            _Value = value;
        }
    }
}
