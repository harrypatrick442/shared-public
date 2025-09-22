using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Snippets.Core.ImageProcessing
{
    [DataContract]
    [KnownType(typeof(RGBABytes))]
    public class RGBABytes {
        private byte _R, _G, _B, _A;
        [JsonPropertyName("r")]
        [JsonInclude]
        [DataMember(Name ="r")]
        public byte R { get { return _R; } set { _R = value; } }
        [JsonPropertyName("g")]
        [JsonInclude]
        [DataMember(Name = "g")]
        public byte G { get { return _G; } set { _G = value; } }
        [JsonPropertyName("b")]
        [JsonInclude]
        [DataMember(Name = "b")]
        public byte B { get { return _B; } set { _B = value; } }
        [JsonPropertyName("a")]
        [JsonInclude]
        [DataMember(Name = "a")]
        public byte A { get { return _A; } set { _A = value; } }
        public RGBABytes(byte r, byte g, byte b, byte a) {
            _R = r;
            _G = g;
            _B = b;
            _A = a;
        }
        public static RGBABytes ForDefault(byte def) {
            return new RGBABytes(def, def, def, def);
        }
    }
}
