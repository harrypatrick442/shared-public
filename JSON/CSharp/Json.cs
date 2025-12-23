
namespace JSON
{
	public class Json
	{
		private static readonly IJsonParser _Instance = new SystemTextJsonParser();
		public static IJsonParser Instance { get { return _Instance; } }	
		public static TType Deserialize<TType>(string json)
		{
			return _Instance.Deserialize<TType>(json);
		}

        public static T Deserialize<T>(object value)
        {
            throw new NotImplementedException();
        }

        public static string Serialize<TType>(TType instance, bool prettify = false)
		{
			string serialized =  _Instance.Serialize(instance, prettify);
			return serialized;
		}
	}
}
