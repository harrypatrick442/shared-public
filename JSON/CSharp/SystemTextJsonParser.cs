using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Text.Json;
namespace JSON
{
	public class SystemTextJsonParser : IJsonParser
    {
        //Serializing same object 10000 times

        //115ms  System.Runtime.Serialization.
        //84ms  System.Text.Json.
        //200ms Jil.JSON. 

        //Deserlializing same object 10000 times

        //500ms System.Runtime.Serialization 
        //210ms Jil.JSON
        //120ms System.Text.Json
		public SystemTextJsonParser()
		{

		}
		public TType Deserialize<TType>(string json)
		{

            if (string.IsNullOrEmpty(json)) return default(TType);
            JsonSerializerOptions options = new()
            {
                TypeInfoResolver = new PrivateConstructorContractResolver(),
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            return System.Text.Json.JsonSerializer.Deserialize<TType>(json, options);
        }

		public string Serialize<TType>(TType instance, bool prettify = false)
		{
			if (instance == null) return null;
            JsonSerializerOptions options = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            return System.Text.Json.JsonSerializer.Serialize<TType>(instance, options);
		}
	}
}
