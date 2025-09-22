using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Geometry
{

	[DataContract]
	public class Matrix3X3<TValue> where TValue : struct
	{
		private TValue _A, _B, _C, _D, _E, _F, _G, _H, _I;

		[JsonPropertyName("a")]
		[JsonInclude]
		[DataMember(Name = "a")]
		public TValue A { get { return _A; } protected set { _A = value; } }

		[JsonPropertyName("b")]
		[JsonInclude]
		[DataMember(Name = "b")]
		public TValue B { get { return _B; } protected set { _B = value; } }

		[JsonPropertyName("c")]
		[JsonInclude]
		[DataMember(Name = "c")]
		public TValue C { get { return _C; } protected set { _C = value; } }
		[JsonPropertyName("d")]
		[JsonInclude]
		[DataMember(Name = "d")]
		public TValue D { get { return _D; } protected set { _D = value; } }
		[JsonPropertyName("e")]
		[JsonInclude]
		[DataMember(Name = "e")]
		public TValue E { get { return _E; } protected set { _E = value; } }
		[JsonPropertyName("f")]
		[JsonInclude]
		[DataMember(Name = "f")]
		public TValue F { get { return _F; } protected set { _F = value; } }
		[JsonPropertyName("g")]
		[JsonInclude]
		[DataMember(Name = "g")]
		public TValue G { get { return _G; } protected set { _G = value; } }
		[JsonPropertyName("h")]
		[JsonInclude]
		[DataMember(Name = "h")]
		public TValue H { get { return _H; } protected set { _H = value; } }
		[JsonPropertyName("i")]
		[JsonInclude]
		[DataMember(Name = "i")]
		public TValue I { get { return _I; } protected set { _I = value; } }

		public Matrix3X3(TValue a, TValue b, TValue c
			, TValue d, TValue e, TValue f
			, TValue g, TValue h, TValue i)
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
        protected Matrix3X3() { }
        public override string ToString()
        {
			return $"{{\"a\":{_A}, \"b\":{_B}, \"c\":{_C}, \"d\":{_D}, \"e\":{_E}, \"f\":{_F}, \"g\":{_G}, \"h\":{_H}, \"i\":{_I}}}";
        }
    }
}
