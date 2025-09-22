using JSON;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using WebAPI.Emailing;
using WebAPI.Emailing.Configuration;
using System.IO;
using MessageTypes.Attributes;
using Core.Strings;
using Core.Attributes;

[DataContract]
public class EmailingConfiguration
{
    private int _Id;
    [JsonPropertyName("id")]
    [JsonInclude]
    [DataMember(Name = "id")]
    [PrimaryKey]
    public int Id { get { return _Id; } set { _Id = value; } }
    private string _FromEmail;
    [JsonPropertyName("fromEmail")]
    [JsonInclude]
    [DataMember(Name = "fromEmail")]
    public string FromEmail { get { return _FromEmail; } protected set { _FromEmail = value; } }
    private string _ToEmail;
    [JsonPropertyName("toEmail")]
    [JsonInclude]
    [DataMember(Name = "toEmail")]
    public string ToEmail { get { return _ToEmail; } protected set { _ToEmail = value; } }
    private string _ToEmailsDevelopment;
    [JsonPropertyName("toEmailsDevelopment")]
    [JsonInclude]
    [DataMember(Name = "toEmailsDevelopment")]
    public string ToEmailsDevelopment { get { return _ToEmailsDevelopment; } protected set { _ToEmailsDevelopment = value; } }
    public string[] GetToEmailsDevelopmentArray() {
        return _ToEmailsDevelopment==null?null:StringHelper.MultipleSplit(new char[] { ' ', ',', '\t'}, _ToEmailsDevelopment);
    }
    private int _SmtpServerConfigurationId;
    [JsonPropertyName("smtpServerConfigurationId")]
    [JsonInclude]
    [DataMember(Name = "smtpServerConfigurationId")]
    public int SmtpServerConfigurationId { get { return _SmtpServerConfigurationId; } protected set { _SmtpServerConfigurationId = value; } }
    protected EmailingConfiguration() { }
}
