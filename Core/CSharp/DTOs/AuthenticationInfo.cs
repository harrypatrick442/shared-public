using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.DTOs
{
    [DataContract]
    [KnownType(typeof(AuthenticationInfo))]
    public class AuthenticationInfo
    {
        [JsonPropertyName("uI")]
        [JsonInclude]
        [DataMember(Name = "uI")]
        public long UserId { get; set; }
        [JsonPropertyName("e")]
        [JsonInclude]
        [DataMember(Name = "e")]
        public string Email { get; protected set; }
        [JsonPropertyName("p")]
        [JsonInclude]
        [DataMember(Name = "p")]
        public string Phone { get; protected set; }
        /*
        [JsonPropertyName("u")]
        [JsonInclude]
        [DataMember(Name = "u")]
        public string Username { get; protected set; }
        */
        [JsonPropertyName("h")]
        [JsonInclude]
        [DataMember(Name = "h")]
        public string Hash { get; protected set; }
        public AuthenticationInfo(long userId, string hash, string email,/* string username,*/ string phone) {
            UserId = userId;
            Hash = hash;
            Email = email;
            //Username = username;
            Phone = phone;
        }
        protected AuthenticationInfo() { }
    }
}
