using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Geometry
{
	[DataContract]
	public class Vector2<T>
	{
		private T _X, _Y;

		[JsonPropertyName("x")]
		[JsonInclude]
		[DataMember(Name = "x")]
		public T X { get { return _X; } protected set { _X = value; } }

		[JsonPropertyName("y")]
		[JsonInclude]
		[DataMember(Name = "y")]
		public T Y { get { return _Y; } protected set { _Y = value; } }

		public Vector2(T x, T y)
		{
			_X = x;
			_Y = y;
        }
        protected Vector2() { }

        public override bool Equals(object obj)
        {
            return obj is Vector2<T> vector &&
                   EqualityComparer<T>.Default.Equals(_X, vector._X) &&
                   EqualityComparer<T>.Default.Equals(_Y, vector._Y);
        }
    }
}
