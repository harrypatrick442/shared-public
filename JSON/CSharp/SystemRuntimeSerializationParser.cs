using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Runtime.Serialization.Json;
namespace JSON
{
	public class SystemRuntimeSerializationParser : IJsonParser
    {
        //Serializing same object 10000 times

        //115ms  System.Runtime.Serialization.
        //84ms  System.Text.Json.
        //200ms Jil.JSON. 

        //Deserlializing same object 10000 times

        //500ms System.Runtime.Serialization 
        //210ms Jil.JSON
        //120ms System.Text.Json

		private Type[] _KnownTypes;
		private EmitTypeInformation _EmitTypeInformation;
		public SystemRuntimeSerializationParser(EmitTypeInformation emitTypeInformation = EmitTypeInformation.AsNeeded, Type[] knownTypes = null)
		{
			_EmitTypeInformation = emitTypeInformation;
			_KnownTypes = knownTypes;
		}
		public TType Deserialize<TType>(string json)
		{

            if (string.IsNullOrEmpty(json)) return default(TType);
			DataContractJsonSerializerSettings dataContractJsonSerializerSettings = new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true,
                EmitTypeInformation = _EmitTypeInformation
            };
            if (_KnownTypes != null)
                dataContractJsonSerializerSettings.KnownTypes = _KnownTypes;
            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
			{
				var serializer = new DataContractJsonSerializer(typeof(TType), dataContractJsonSerializerSettings);
				return (TType)serializer.ReadObject(stream);
			}
        }

		public string Serialize<TType>(TType instance, bool prettify = false)
		{
			if (instance == null) return null;
            DataContractJsonSerializerSettings dataContractJsonSerializerSettings = new DataContractJsonSerializerSettings
			{
				UseSimpleDictionaryFormat = true,
				EmitTypeInformation = _EmitTypeInformation
			};
			if (_KnownTypes != null)
				dataContractJsonSerializerSettings.KnownTypes = _KnownTypes;
			var serializer = new DataContractJsonSerializer(typeof(TType), dataContractJsonSerializerSettings);
            using (var stream = new MemoryStream())
			{
				if (!prettify)
				{

					serializer.WriteObject(stream, instance);
					return System.Text.Encoding.UTF8.GetString(stream.ToArray());
				}
				using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, System.Text.Encoding.UTF8, true, prettify))
				{
					serializer.WriteObject(writer, instance);
					writer.Flush();
					return System.Text.Encoding.UTF8.GetString(stream.ToArray());
				}
			}
		}
	}
}
