using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Modelling
{
	[DataContract]
	public class Texture
	{
		private string _Name;
		[JsonPropertyName("name")]
		[JsonInclude]
		[DataMember(Name = "name", EmitDefaultValue = false)]
		public string Name { get { return _Name; } protected set { _Name = value; } }
		public Texture(string name)
		{
			_Name = name;
        }
        protected Texture() { }
    }
}
