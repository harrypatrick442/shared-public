using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System;

namespace Core.Geometry
{
	[DataContract]
	public class Vector2f
	{
		private float _X, _Y;
		private float? _Magnitude;

		[JsonPropertyName("x")]
		[JsonInclude]
		[DataMember(Name = "x")]
		public float X { get { return _X; } protected set { _X = value; } }

		[JsonPropertyName("y")]
		[JsonInclude]
		[DataMember(Name = "y")]
		public float Y { get { return _Y; } protected set { _Y = value; } }

		public Vector2f(float x, float y)
		{
			_X = x;
			_Y = y;
        }
        protected Vector2f() { }
        public float Magnitude
		{
			get
			{
				if (_Magnitude == null)
				{
					_Magnitude = (float)Math.Sqrt(Math.Pow(_X, 2) + Math.Pow(_Y, 2));
				}
				return (float)_Magnitude;
			}
		}
	}
}
