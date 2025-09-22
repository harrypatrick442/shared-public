using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Geometry
{
	[DataContract]

	[KnownType(typeof(Location))]
	public class Location
	{
		private float[] _Position;
		[JsonPropertyName("position")]
		[JsonInclude]
		[DataMember(Name = "position")]
		public float[] PositionArray { get { return _Position; } protected set { _Position = value; } }

		private float[] _Scaling;
		[JsonPropertyName("scaling")]
		[JsonInclude]
		[DataMember(Name = "scaling")]
		public float[] ScalingArray { get { return _Scaling; } protected set { _Scaling = value; } }
		private float[] _Rotation;
		[JsonPropertyName("rotation")]
		[JsonInclude]
		[DataMember(Name = "rotation")]
		public float[] RotationArray { get { return _Rotation; } protected set { _Rotation = value; } }
		public Location(float[] position, float[] scaling, float[] rotation) {
			_Position = position;
			_Rotation = rotation;
			_Scaling = scaling;
        }
        protected Location() { }
    }
}
