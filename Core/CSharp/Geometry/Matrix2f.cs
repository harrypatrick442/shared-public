using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Geometry
{

	[DataContract]
	public class Matrix2X2f
	{
		private float _A, _B, _C, _D;

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

		public Matrix2X2f(float a, float b, float c
			, float d)
		{
			_A = a;
			_B = b;
			_C = c;
			_D = d;
        }
        protected Matrix2X2f() { }
        public static Vector2f operator *(Matrix2X2f matrix, Vector2f vector) {
			return new Vector2f((matrix.A * vector.X) + (matrix.B * vector.Y), (matrix.C * vector.X) + (matrix.D * vector.Y));
		} 
		public override string ToString()
		{
			return $"{{\"a\":{_A}, \"b\":{_B}, \"c\":{_C}, \"d\":{_D}}}";
		}
	}
}
