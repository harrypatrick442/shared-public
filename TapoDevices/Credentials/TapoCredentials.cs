using JSON;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TapoDevices.Credentials
{
    [DataContract]
    public class TapoCredentials
    {
        [JsonPropertyName(TapoCredentialsDataMemberNames.Username)]
        [JsonInclude]
        [DataMember(Name = TapoCredentialsDataMemberNames.Username)]
        public virtual string Username { get; protected set; }
        [JsonPropertyName(TapoCredentialsDataMemberNames.Password)]
        [JsonInclude]
        [DataMember(Name = TapoCredentialsDataMemberNames.Password)]
        public virtual string Password { get; protected set; }
        public TapoCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }
        protected TapoCredentials()
        {

        }
        public static TapoCredentials FromFile(string filePath)
        {
            string content = File.ReadAllText(filePath);
            return Json.Deserialize<TapoCredentials>(content);
        }
    }
}
