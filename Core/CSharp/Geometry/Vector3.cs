using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Geometry
{
	[DataContract]
	public class Vector3<TValue> where TValue : struct
	{
		private TValue _X, _Y, _Z;

		[JsonPropertyName("x")]
		[JsonInclude]
		[DataMember(Name = "x")]
		public TValue X { get { return _X; } protected set { _X = value; } }

		[JsonPropertyName("y")]
		[JsonInclude]
		[DataMember(Name = "y")]
		public TValue Y { get { return _Y; } protected set { _Y = value; } }

		[JsonPropertyName("z")]
		[JsonInclude]
		[DataMember(Name = "z")]
		public TValue Z { get { return _Z; } protected set { _Z = value; } }

		public Vector3(TValue x, TValue y, TValue z)
		{
			_X = x;
			_Y = y;
			_Z = z;
        }
        protected Vector3() { }
        public override string ToString()
		{
			return $"{{x:{_X}, y:{_Y}, z:{_Z}}}";
		}
		public static Vector3<TValue> operator *(Matrix3X3<TValue> m, Vector3<TValue> v)
		{
			dynamic vd = v;
			dynamic md = m;
			TValue x = (m.A * vd.X) + (m.B * vd.Y) + (m.C * vd.Z);
			TValue y = (m.D * vd.X) + (m.E * vd.Y) + (m.F * vd.Z);
			TValue z = (m.G * vd.X) + (m.H * vd.Y) + (m.I * vd.Z);
			return new Vector3<TValue>(x, y, z);
		}
		public static Vector3<TValue> operator *(Vector3<TValue> a, Vector3<TValue> b)
		{
			dynamic ad = a;
			dynamic bd = b;
			TValue x = (ad.X * bd.X);
			TValue y = (ad.Y * bd.X);
			TValue z = (ad.Z * bd.X);
			return new Vector3<TValue>(x, y, z);
		}
	}
}
