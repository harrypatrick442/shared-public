using Core.Exceptions;

namespace Core.Parsing
{
	public class FlexibleBoolParser
	{
		public static bool TryParse(string value, out bool res, bool? emptyIs = null){
			res = false;
			if (value == null)
			{
				if (emptyIs != null) return (bool)emptyIs;
				return false;
			}
			value = value.ToLower();
			switch (value) {
				case "":
					if (emptyIs != null)
					{
						res = (bool)emptyIs;
						return true;
					}
					return false;
				case "yes": res = true; return true;
				case "no": res = false; return true;
				case "true": res = true; return true;
				case "false": res = false; return true;
				case "y": res = true; return true;
				case "n": res = false; return true;
				default:
					return false;
			}
		}
		private static string ThrowCouldNotParse(string value) { 
			throw new ParseException($"Could not parse the value \"{value}\" to a {typeof(bool).Name}");
		}
	}
}
