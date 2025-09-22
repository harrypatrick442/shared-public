using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Modelling
{
	[DataContract]
	public class RGBAColour
	{
		private float _R, _G, _B, _Opacity=1;
		[JsonPropertyName("r")]
		[JsonInclude]
		[DataMember(Name = "r")]
		public float R { get { return _R; } protected set { _R = value; } }
		[JsonPropertyName("g")]
		[JsonInclude]
		[DataMember(Name = "g")]
		public float G { get { return _G; } protected set { _G = value; } }
		[JsonPropertyName("b")]
		[JsonInclude]
		[DataMember(Name = "b")]
		public float B { get { return _B; } protected set { _B = value; } }
		[JsonPropertyName("opacity")]
		[JsonInclude]
		[DataMember(Name = "opacity")]
		public float Opacity { get { return _Opacity; } set { _Opacity = value; } }

		public RGBAColour(float r, float g, float b, float opacity)
		{
			_R = r;
			_G = g;
			_B = b;
			_Opacity = opacity;
        }
        protected RGBAColour() { }
    }
}
