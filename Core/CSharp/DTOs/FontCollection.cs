using JSON;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using WebAPI.Emailing;
using WebAPI.Emailing.Configuration;
using System.IO;
using MessageTypes.Attributes;
using Core.Attributes;

[DataContract]
public class FontCollection
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
    private string _Description;
    [JsonPropertyName("description")]
    [JsonInclude]
    [DataMember(Name = "description")]
    public string Description { get { return _Description; } protected set { _Description = value; } }
    protected FontCollection() { }
}
