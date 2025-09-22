using JSON;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using WebAPI.Emailing;
using WebAPI.Emailing.Configuration;
using System.IO;
using MessageTypes.Attributes;

[DataContract]
public class _Font
{
    private int _Id;
    [JsonPropertyName("id")]
    [JsonInclude]
    [DataMember(Name = "id")]
    public int Id { get { return _Id; } set { _Id = value; } }
    private int? _FontCollectionId;
    [JsonPropertyName("fontCollectionId")]
    [JsonInclude]
    [DataMember(Name = "fontCollectionId")]
    public int? FontCollectionId { get { return _FontCollectionId; } protected set { _FontCollectionId = value; } }
    private string _FileName;
    [JsonPropertyName("fileName")]
    [JsonInclude]
    [DataMember(Name = "fileName")]
    public string FileName { get { return _FileName; } protected set { _FileName = value; } }
    private string _FontFamily;
    [JsonPropertyName("fontFamily")]
    [JsonInclude]
    [DataMember(Name = "fontFamily")]
    public string FontFamily { get { return _FontFamily; } protected set { _FontFamily = value; } }
    private string _FontWeight;
    [JsonPropertyName("fontWeight")]
    [JsonInclude]
    [DataMember(Name = "fontWeight")]
    public string FontWeight { get { return _FontWeight; } protected set { _FontWeight = value; } }
    protected _Font() { }
}
