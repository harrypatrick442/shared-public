using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Modelling
{
	[DataContract]
	public class NormalTexture : Texture
	{
		private float _BumpValue;
		[JsonPropertyName("bumpValue")]
		[JsonInclude]
		[DataMember(Name="bumpValue", EmitDefaultValue = true)]
		public float BumpValue { get { return _BumpValue; } protected set { _BumpValue = value; } }
		public NormalTexture(string name, float bumpValue) : base(name)
		{
			_BumpValue = bumpValue;
        }
        protected NormalTexture() { }
    }
}
