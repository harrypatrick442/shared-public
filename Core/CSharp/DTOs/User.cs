using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using JSON;
using MessageTypes.Attributes;
using Core.Attributes;

namespace Core.DTOs
{
    [DataContract]
    [KnownType(typeof(User))]
    public class User
    {
        private int _Id;
        [JsonPropertyName("i")]
        [JsonInclude]
        [DataMember(Name ="i")]
        [PrimaryKey]
        public int Id { get { return _Id; } set { _Id = value; } }
        private string _Email;
        [JsonPropertyName("e")]
        [JsonInclude]
        [DataMember(Name = "e")]
        public string Email { get { return _Email; } protected set { _Email = value; } }
        private string _Phone;
        [JsonPropertyName("p")]
        [JsonInclude]
        [DataMember(Name = "p")]
        public string Phone { get { return _Phone; } protected set { _Phone = value; } }
        private string _Username;
        [JsonPropertyName("u")]
        [JsonInclude]
        [DataMember(Name = "u")]
        public string Username { get { return _Username; } protected set { _Username = value; } }
        private string _Title;
        [JsonPropertyName("t")]
        [JsonInclude]
        [DataMember(Name = "t")]
        public string Title { get { return _Title; } protected set { _Title = value; } }
        private long _Created;
        [JsonPropertyName("c")]
        [JsonInclude]
        [DataMember(Name = "c")]
        public long Created { get { return _Created; } protected set { _Created = value; } }
        protected User() { }
    }
}
