using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Snippets.Core.ImageProcessing
{
    [DataContract]
    [KnownType(typeof(DynamicImageEntry))]
    public class DynamicImageEntry {
        private int _Width;
        [JsonPropertyName("width")]
        [JsonInclude]
        [DataMember(Name = "width")]
        public int Width { get { return _Width; } protected set{_Width = value;} }
        private int _Height;
        [JsonPropertyName("height")]
        [JsonInclude]
        [DataMember(Name = "height")]
        public int Height { get { return _Height; } protected set { _Height = value; } }
        private string _RelativePath;
        [JsonPropertyName("relativePath")]
        [JsonInclude]
        [DataMember(Name = "relativePath")]
        public string RelativePath { get { return _RelativePath; } protected set { _RelativePath = value; } }
        public DynamicImageEntry(string relativePath, int width, int height) {
            _RelativePath = relativePath?.Replace("\\", "/");
            _Width = width;
            _Height = height;
        }
    }
}
