using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using JSON;

namespace Core.Serialization
{
    /// <summary>
    /// This is designed to be a means to deserialize a complex object without dropping off any properties
    /// It is not designed to always return the latest state of the thing it comes from. To do that create a new instance each time you read it then save it back.
    /// </summary>
    public static class BinaryParser
    {
        public static byte[] SerializeToByteArray<T>(this T obj) where T : class
        {
            if (obj == null)
            {
                return null;
            }
            using (var ms = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(ms, obj);
                return ms.ToArray();
            }
        }

        public static T Deserialize<T>(this byte[] byteArray) where T : class
        {
            if (byteArray == null)
            {
                return default(T);
            }
            using (var memStream = new MemoryStream(byteArray))
            {
                var serializer = new DataContractSerializer(typeof(T));
                var obj = (T)serializer.ReadObject(memStream);
                return obj;
            }
        }
    }
}
