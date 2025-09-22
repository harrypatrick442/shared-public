using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Linq;
using System;
using System.Collections.Generic;
using Snippets.Core.Modelling;

namespace Snippets.Core.ImageProcessing
{

    [DataContract]
    [KnownType(typeof(PixelMappingsInt))]
    public class PixelMappingsInt
    {
        //private Dictionary<byte, Dictionary<byte, Dictionary<byte, Dictionary<byte, int>>>> _MapRToMapGToMapBToMapAToValue;

        [JsonPropertyName("mapRToMapGToMapBToMapAToValue")]
        [JsonInclude]
        //[DataMember(Name = "mapRToMapGToMapBToMapAToValue")]
        //protected Dictionary<byte, Dictionary<byte, Dictionary<byte, Dictionary<byte, int>>>> MapRToMapGToMapBToMapAToValue { get { return _MapRToMapGToMapBToMapAToValue; } set { _MapRToMapGToMapBToMapAToValue = value; } }
        private RGBAValuePair[] _RGBAValuePairs;
        [JsonPropertyName("rgbaValuePairs")]
        [JsonInclude]
        [DataMember(Name = "rgbaValuePairs")]
        public RGBAValuePair[] RGBAValuePairs { get { return _RGBAValuePairs; } protected set { _RGBAValuePairs = value; } }
        private string _ImageFileRelativePath;
        [JsonPropertyName("imageFileRelativePath")]
        [JsonInclude]
        [DataMember(Name = "imageFileRelativePath")]
        public string ImageFileRelativePath { get { return _ImageFileRelativePath; } protected set { _ImageFileRelativePath = value; } }
        public PixelMappingsInt(RGBAValuePair[] rGBAValuePairs, string imageRelativePath)
        {

            _RGBAValuePairs = rGBAValuePairs;
            _ImageFileRelativePath = imageRelativePath;
        }
    }
}
