using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Geometry
{

	[DataContract]
	public class Matrix3X3f
	{
		private float _A, _B, _C, _D, _E, _F, _G, _H, _I;

		[JsonPropertyName("a")]
		[JsonInclude]
		[DataMember(Name = "a")]
		public float A { get { return _A; } protected set { _A = value; } }

		[JsonPropertyName("b")]
		[JsonInclude]
		[DataMember(Name = "b")]
		public float B { get { return _B; } protected set { _B = value; } }

		[JsonPropertyName("c")]
		[JsonInclude]
		[DataMember(Name = "c")]
		public float C { get { return _C; } protected set { _C = value; } }
		[JsonPropertyName("d")]
		[JsonInclude]
		[DataMember(Name = "d")]
		public float D { get { return _D; } protected set { _D = value; } }
		[JsonPropertyName("e")]
		[JsonInclude]
		[DataMember(Name = "e")]
		public float E { get { return _E; } protected set { _E = value; } }
		[JsonPropertyName("f")]
		[JsonInclude]
		[DataMember(Name = "f")]
		public float F { get { return _F; } protected set { _F = value; } }
		[JsonPropertyName("g")]
		[JsonInclude]
		[DataMember(Name = "g")]
		public float G { get { return _G; } protected set { _G = value; } }
		[JsonPropertyName("h")]
		[JsonInclude]
		[DataMember(Name = "h")]
		public float H { get { return _H; } protected set { _H = value; } }
		[JsonPropertyName("i")]
		[JsonInclude]
		[DataMember(Name = "i")]
		public float I { get { return _I; } protected set { _I = value; } }

		public Matrix3X3f(float a, float b, float c
			, float d, float e, float f
			, float g, float h, float i)
		{
			_A = a;
			_B = b;
			_C = c;
			_D = d;
			_E = e;
			_F = f;
			_G = g;
			_H = h;
			_I = i;
        }
        protected Matrix3X3f() { }
        public override string ToString()
		{
			return $"{{\"a\":{_A}, \"b\":{_B}, \"c\":{_C}, \"d\":{_D}, \"e\":{_E}, \"f\":{_F}, \"g\":{_G}, \"h\":{_H}, \"i\":{_I}}}";
		}
	}
}
