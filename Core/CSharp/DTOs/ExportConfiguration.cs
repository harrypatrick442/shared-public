using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MessageTypes.Attributes;
using Core.Attributes;

namespace SnippetsDatabase.DTOs
{
    [DataContract]
    public class ExportConfiguration
    {
        private int _Id;
        [JsonPropertyName("id")]
        [JsonInclude]
        [DataMember(Name = "id")]
        [PrimaryKey]
        public int Id { get { return _Id; } set { _Id = value; } }
        private string _Name;
        [JsonPropertyName("name")]
        [JsonInclude]
        [DataMember(Name = "name")]
        public string Name { get { return _Name; } protected set { _Name = value; } }

        private string _SiteDataUrl;
        [JsonPropertyName("siteDataUrl")]
        [JsonInclude]
        [DataMember(Name = "siteDataUrl")]
        public string SiteDataUrl { get { return _SiteDataUrl; } 
            protected set{_SiteDataUrl = value ;} }

        private string _ExportPathRelativeFromRoot;
        [JsonPropertyName("exportPathRelativeFromRoot")]
        [JsonInclude]
        [DataMember(Name = "exportPathRelativeFromRoot")]
        public string ExportPathRelativeFromRoot
            { get { return _ExportPathRelativeFromRoot; }
            protected set{ _ExportPathRelativeFromRoot = value; } }

        private string _RootCacheDirectoryPathFromRoot;
        [JsonPropertyName("rootCacheDirectoryPathFromRoot")]
        [JsonInclude]
        [DataMember(Name = "rootCacheDirectoryPathFromRoot")]
        public string RootCacheDirectoryPathFromRoot { get { return _RootCacheDirectoryPathFromRoot; } protected set { _RootCacheDirectoryPathFromRoot = value; } }

        private string _HouseTypeInfoUrl;
        [JsonPropertyName("houseTypeInfoUrl")]
        [JsonInclude]
        [DataMember(Name = "houseTypeInfoUrl")]
        public string HouseTypeInfoUrl { get { return _HouseTypeInfoUrl; } protected set { _HouseTypeInfoUrl = value; } }

        private string _PlotRendersSourceDirectoryAbsolutePath;
        [JsonPropertyName("plotRendersSourceDirectoryAbsolutePath")]
        [JsonInclude]
        [DataMember(Name = "plotRendersSourceDirectoryAbsolutePath")]
        public string PlotRendersSourceDirectoryAbsolutePath { get { return _PlotRendersSourceDirectoryAbsolutePath; } protected set { _PlotRendersSourceDirectoryAbsolutePath = value; } }

        private string _GallerySourceDirectoryAbsolutePath;
        [JsonPropertyName("gallerySourceDirectoryAbsolutePath")]
        [JsonInclude]
        [DataMember(Name = "gallerySourceDirectoryAbsolutePath")]
        public string GallerySourceDirectoryAbsolutePath { get { return _GallerySourceDirectoryAbsolutePath; } protected set { _GallerySourceDirectoryAbsolutePath = value; } }

        private string _FloorPlansSourceDirectoryAbsolutePath;
        [JsonPropertyName("floorPlansSourceDirectoryAbsolutePath")]
        [JsonInclude]
        [DataMember(Name = "floorPlansSourceDirectoryAbsolutePath")]
        public string FloorPlansSourceDirectoryAbsolutePath { get { return _FloorPlansSourceDirectoryAbsolutePath; } protected set { _FloorPlansSourceDirectoryAbsolutePath = value; } }

        private string _AerialImagesSourceDirectoryAbsolutePath;
        [JsonPropertyName("aerialImagesSourceDirectoryAbsolutePath")]
        [JsonInclude]
        [DataMember(Name = "aerialImagesSourceDirectoryAbsolutePath")]
        public string AerialImagesSourceDirectoryAbsolutePath { get { return _AerialImagesSourceDirectoryAbsolutePath; } protected set { _AerialImagesSourceDirectoryAbsolutePath = value; } }

        private string _Outside360PanoramasSourceDirectoryAbsolutePath;
        [JsonPropertyName("outside360PanoramasSourceDirectoryAbsolutePath")]
        [JsonInclude]
        [DataMember(Name = "outside360PanoramasSourceDirectoryAbsolutePath")]
        public string Outside360PanoramasSourceDirectoryAbsolutePath { get { return _Outside360PanoramasSourceDirectoryAbsolutePath; } protected set { _Outside360PanoramasSourceDirectoryAbsolutePath = value; } }

        private string _StaticAssetsSourceDirectoryAbsolutePath;
        [JsonPropertyName("staticAssetsSourceDirectoryAbsolutePath")]
        [JsonInclude]
        [DataMember(Name = "staticAssetsSourceDirectoryAbsolutePath")]
        public string StaticAssetsSourceDirectoryAbsolutePath { get { return _StaticAssetsSourceDirectoryAbsolutePath; } protected set { _StaticAssetsSourceDirectoryAbsolutePath = value; } }

        private int? _AerialGlowWidth;
        [JsonPropertyName("aerialGlowWidth")]
        [JsonInclude]
        [DataMember(Name = "aerialGlowWidth")]
        public int? AerialGlowWidth
        {
            get { return _AerialGlowWidth; }
            protected set { _AerialGlowWidth = value; }
        }
        protected ExportConfiguration() { }
        public static ExportConfiguration NewDefault()
        {
            return new ExportConfiguration();
        }
    }
}
