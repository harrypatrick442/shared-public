using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MessageTypes.Attributes;
using Core.Attributes;

namespace SnippetsDatabase.DTOs
{
    [DataContract]
    public class Currency
    {
        private int _Id;
        [JsonPropertyName("id")]
        [JsonInclude]
        [DataMember(Name ="id")]
        [PrimaryKey]
        public int Id { get { return _Id; } set { _Id = value; } }
        private string _Name;
        [JsonPropertyName("name")]
        [JsonInclude]
        [DataMember(Name = "name")]
        public string Name { get { return _Name; } protected set { _Name = value; } }
        private string _Description;
        [JsonPropertyName("description")]
        [JsonInclude]
        [DataMember(Name = "description")]
        public string Description { get { return _Description; } protected set { _Description = value; } }
        private string _prefix;
        [JsonPropertyName("prefix")]
        [JsonInclude]
        [DataMember(Name= "prefix")]
        public string Prefix { get { return _prefix; } protected set { _prefix = value; } }
        private string _Suffix;
        [JsonPropertyName("suffix")]
        [JsonInclude]
        [DataMember(Name = "suffix")]
        public string Suffix { get { return _Suffix; } protected set { _Suffix = value; } }
        public static Currency NewDefault()
        {
            return new Currency();
        }
        protected Currency() { }
    }
}
