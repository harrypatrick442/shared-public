using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.Geometry;
namespace Core.Modelling
{
	[DataContract]
	public class AlbedoTexture : Texture
	{
		private RGBAColour _Colours;
		private Vector2f _Tiling;
		private string _RelativeFilePath;

		[JsonPropertyName("colours")]
		[JsonInclude]
		[DataMember(Name = "colours", EmitDefaultValue = false)]
		public RGBAColour Colours { get { return _Colours; } protected set { _Colours = value; } }

		[JsonPropertyName("tiling")]
		[JsonInclude]
		[DataMember(Name = "tiling", EmitDefaultValue = false)]
		public Vector2f Tiling { get { return _Tiling; } protected set { _Tiling = value; } }

		[JsonPropertyName("relativeFilePath")]
		[JsonInclude]
		[DataMember(Name = "relativeFilePath", EmitDefaultValue = false)]
		public string RelativeFilePath { get { return _RelativeFilePath; } protected set { _RelativeFilePath = value; } }

		public AlbedoTexture(string name, RGBAColour colours, Vector2f tiling, string relativeFilePath) : base(name)
		{
			_Colours = colours;
			_Tiling = tiling;
			_RelativeFilePath = relativeFilePath!=null? relativeFilePath.Replace("\\", "/"):null;
        }
        protected AlbedoTexture() { }
    }
}
