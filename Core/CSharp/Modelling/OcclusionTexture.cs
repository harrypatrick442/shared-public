using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Modelling
{
	[DataContract]
	public class OcclusionTexture : Texture
	{
		private float _Value;
		[JsonPropertyName("value")]
		[JsonInclude]
		[DataMember(Name = "value")]
		public float Value { get { return _Value; } protected set { _Value = value; } }
		public OcclusionTexture(string name, float value) : base(name)
		{
			_Value = value;
        }
        protected OcclusionTexture() { }
    }
}
