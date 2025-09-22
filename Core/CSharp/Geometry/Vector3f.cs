using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Geometry
{
	[DataContract]
	public class Vector3f
	{
		private float _X, _Y, _Z;

		[JsonPropertyName("x")]
		[JsonInclude]
		[DataMember(Name = "x")]
		public float X { get { return _X; } protected set { _X = value; } }

		[JsonPropertyName("y")]
		[JsonInclude]
		[DataMember(Name = "y")]
		public float Y { get { return _Y; } protected set { _Y = value; } }

		[JsonPropertyName("z")]
		[JsonInclude]
		[DataMember(Name = "z")]
		public float Z { get { return _Z; } protected set { _Z = value; } }

		public Vector3f(float x, float y, float z)
		{
			_X = x;
			_Y = y;
			_Z = z;
        }
        protected Vector3f() { }
        public override string ToString()
		{
			return $"{{x:{_X}, y:{_Y}, z:{_Z}}}";
		}
	}
}
