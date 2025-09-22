using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System;

namespace Core.Locality
{

    [DataContract]
    [KnownType(typeof(Location))]
    public class Location
    {
        private float _Lat;

        [JsonPropertyName("lat")]
        [JsonInclude]
        [DataMember(Name = "lat")]
        public float Lat { get { return _Lat; } protected set { _Lat = value; } }
        private float _Lng;

        [JsonPropertyName("lng")]
        [JsonInclude]
        [DataMember(Name="lng")]
        public float Lng { get { return _Lng; } protected set { _Lng = value; } }
        protected Location() { }
    }
}
