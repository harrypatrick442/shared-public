using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using MessageTypes.Attributes;
using Core.Attributes;

namespace WebAPI.Emailing.Configuration
{
    [DataContract]
    public class CustomHeader
    {
        private int _Id;
        [JsonPropertyName("id")]
        [JsonInclude]
        [DataMember(Name = "id")]
        [PrimaryKey]
        public int Id { get { return _Id; } set { _Id = value; } }
        private int? _CustomHeadersCollectionId;
        [JsonPropertyName("customHeadersCollectionId")]
        [JsonInclude]
        [DataMember(Name = "customHeadersCollectionId")]
        public int? CustomHeadersCollectionId { get { return _CustomHeadersCollectionId; } protected set { _CustomHeadersCollectionId = value; } }
        private string _Key;
        [JsonPropertyName("key")]
        [JsonInclude]
        [DataMember(Name = "key")]
        public string Key { get { return _Key; } protected set { _Key = value; } }
        private string _ValuePlaceholder;
        [JsonPropertyName("valuePlaceholder")]
        [JsonInclude]
        [DataMember(Name = "valuePlaceholder")]
        public string ValuePlaceholder { get { return _ValuePlaceholder; } protected set { _ValuePlaceholder = value; } }
        protected CustomHeader() { }
    }
}
