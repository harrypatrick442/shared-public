using Snippets.Core.Geometry;
using Snippets.Core.ImageProcessing;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Snippets.Core.Json;
namespace CloudRenderFileNameConverter
{
    [DataContract]
    [KnownType(typeof(DynamicImageExportProfileSize))]
    public class DynamicImageExportProfileSize
    {
        private int? _Width;
        [JsonPropertyName("width")]
        [JsonInclude]
        [DataMember(Name = "width", EmitDefaultValue=true)]
        public int? Width { get { return _Width; } set { _Width = value; } }

        private int? _Height;
        [JsonPropertyName("height")]
        [JsonInclude]
        [DataMember(Name = "height", EmitDefaultValue =true)]
        public int? Height { get { return _Height; } set { _Height = value; } }
        public DynamicImageExportProfileSize(int? width = null, int? height = null) {
            _Width = width;
            _Height = height;
        }
        public override string ToString()
        {
            return new NativeJsonParser().Serialize(this);
        }

    }
}
