using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.Exceptions;
namespace Core.Geometry
{
	[DataContract]
	public enum CubeFace
	{
		Back=0, Front=1, Right=2, Left=3, Up=4, Down=5
	}
	public static class CubeFaceHelper {
		public static CubeFace[] AllInBRFLUDOrder
		{
			get
			{
				return new CubeFace[] {
			CubeFace.Back,
			CubeFace.Front,
			CubeFace.Right,
			CubeFace.Left,
			CubeFace.Up,
			CubeFace.Down
		};
			}
		}
        public static CubeFace Parse(string str) {
			str = str.ToLower();
			switch (str) {
				case "b":
				case "back":
					return CubeFace.Back;
				case "f":
				case "front":
					return CubeFace.Front;
				case "r":
				case "right":
					return CubeFace.Right;
				case "l":
				case "left":
					return CubeFace.Left;
				case "u":
				case "up":
					return CubeFace.Up;
				case "d":
				case "down":
					return CubeFace.Down;
			}
			throw new ParseException($"Could not parse \"{str}\" to {nameof(CubeFace)}");
		}
	}
	public static class CubeFaceExtensions {
		public static char GetFirstLetter(this CubeFace cubeFace) {
			switch (cubeFace) {
				case CubeFace.Back:
					return 'b';
				case CubeFace.Front:
					return 'f';
				case CubeFace.Right:
					return 'r';
				case CubeFace.Left:
					return 'l';
				case CubeFace.Up:
					return 'u';
				case CubeFace.Down:
					return 'd';
			}
			throw new NotImplementedException();
		}
	}
}
