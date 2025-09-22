using Snippets.Core.Geometry;
using Snippets.Core.ImageProcessing;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CloudRenderFileNameConverter
{
    [DataContract]
    [KnownType(typeof(DynamicImageExportProfile))]
    public class DynamicImageExportProfile
    {
        private bool _IncludeOriginalSize;
        [JsonPropertyName("includeOriginalSize")]
        [JsonInclude]
        [DataMember(Name = "includeOriginalSize")]
        public bool IncludeOriginalSize { get { return _IncludeOriginalSize; } set { _IncludeOriginalSize = value; } }
        private bool _AllowIncreaseSize;
        [JsonPropertyName("allowIncreaseSize")]
        [JsonInclude]
        [DataMember(Name = "allowIncreaseSize")]
        public bool AllowIncreaseSize { get { return _AllowIncreaseSize; } set { _AllowIncreaseSize = value; } }
        private bool _SkipLargerSizesThanSource;
        [JsonPropertyName("skipLargerSizesThanSource")]
        [JsonInclude]
        [DataMember(Name = "skipLargerSizesThanSource")]
        public bool SkipLargerSizesThanSource { get { return _SkipLargerSizesThanSource; } set { _SkipLargerSizesThanSource = value; } }
        private DynamicImageExportProfileSize[] _DesiredSizes;
        [JsonPropertyName("desiredSizes")]
        [JsonInclude]
        [DataMember(Name="desiredSizes")]
        public DynamicImageExportProfileSize[] DesiredSizes { get { return _DesiredSizes; } protected set { _DesiredSizes = value; } }
        public DynamicImageExportProfile(DynamicImageExportProfileSize[] desiredSizes, bool includeOriginalSize = false, bool allowIncreaseSize = false, bool skipLargerSizesThanSource=true) {
            _DesiredSizes = desiredSizes;
            _IncludeOriginalSize = includeOriginalSize;
            _AllowIncreaseSize = allowIncreaseSize;
            _SkipLargerSizesThanSource = skipLargerSizesThanSource;
        }
    }
}
