using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Linq;
using Core.Geometry;
using Core.FileSystem;
using System.IO;
using JSON;
namespace Core.Multimedias
{
    [DataContract]
    [KnownType(typeof(Multimedia))]
    public class Multimedia
	{

        private MultimediaType _MultimediaType;
        [JsonPropertyName("t")]
        [JsonInclude]
        [DataMember(Name ="t")]
        public MultimediaType MultimediaType { get { return _MultimediaType; } protected set { _MultimediaType = value; } }
        public int MultimediaTypeInt { get { return (int)_MultimediaType; } protected set { _MultimediaType = (MultimediaType)value; } }
        private int _Id;
        [JsonPropertyName("i")]
        [JsonInclude]
        [DataMember(Name = "i")]
        public int Id { get { return _Id; } protected set { _Id = value; } }
        private string _Url;
        [JsonPropertyName("u")]
        [JsonInclude]
        [DataMember(Name = "u")]
        public string Url { get { return _Url; } protected set { _Url = value; } }
        public Multimedia(int id, MultimediaType multimediaType, string url) {
            _Id = id;
            _MultimediaType = multimediaType;
            _Url = url;
        }
        protected Multimedia() { }
    }
}
