using System;
using System.IO;
using System.Reflection;

namespace Core.Serialization
{
    public static class BinaryReflectionDeserializer
    {
        public static TObject Deserialize<TObject>(string filePath)
        {
            return (TObject)Deserialize(filePath);
        }

        public static object Deserialize(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                return DeserializeObject(reader);
            }
        }

        private static object DeserializeObject(BinaryReader reader)
        {
            // Read the type information
            string typeName = reader.ReadString();
            Type objType = Type.GetType(typeName);

            // Check if the type is an array
            if (objType.IsArray)
            {
                Type elementType = objType.GetElementType();
                int arrayLength = reader.ReadInt32();  // Read array length
                Array array = Array.CreateInstance(elementType, arrayLength);

                for (int i = 0; i < arrayLength; i++)
                {
                    array.SetValue(DeserializeFieldOrProperty(reader, elementType), i);
                }

                return array;
            }
            else
            {
                // Create an instance even if the constructor is private
                object obj = Activator.CreateInstance(objType, BindingFlags.Instance | BindingFlags.NonPublic, null, null, null);

                foreach (var field in objType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    object value = DeserializeFieldOrProperty(reader, field.FieldType);
                    field.SetValue(obj, value);
                }

                foreach (var property in objType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (property.CanWrite)
                    {
                        object value = DeserializeFieldOrProperty(reader, property.PropertyType);
                        property.SetValue(obj, value);
                    }
                }

                return obj;
            }
        }

        private static object DeserializeFieldOrProperty(BinaryReader reader, Type type)
        {
            if (type == typeof(int))
            {
                return reader.ReadInt32();
            }
            else if (type == typeof(double))
            {
                return reader.ReadDouble();
            }
            else if (type == typeof(float))
            {
                return reader.ReadSingle();
            }
            else if (type == typeof(long))
            {
                return reader.ReadInt64();
            }
            else if (type == typeof(short))
            {
                return reader.ReadInt16();
            }
            else if (type == typeof(byte))
            {
                return reader.ReadByte();
            }
            else if (type == typeof(char))
            {
                return reader.ReadChar();
            }
            else if (type == typeof(decimal))
            {
                return reader.ReadDecimal();
            }
            else if (type == typeof(sbyte))
            {
                return reader.ReadSByte();
            }
            else if (type == typeof(ushort))
            {
                return reader.ReadUInt16();
            }
            else if (type == typeof(uint))
            {
                return reader.ReadUInt32();
            }
            else if (type == typeof(ulong))
            {
                return reader.ReadUInt64();
            }
            else if (type == typeof(string))
            {
                return reader.ReadString();
            }
            else if (type == typeof(bool))
            {
                return reader.ReadBoolean();
            }
            else if (type == typeof(DateTime))
            {
                // Deserialize DateTime from binary value
                return DateTime.FromBinary(reader.ReadInt64());
            }
            else if (type.IsArray)
            {
                int length = reader.ReadInt32();  // Read array length
                Type elementType = type.GetElementType();
                Array array = Array.CreateInstance(elementType, length);

                for (int i = 0; i < length; i++)
                {
                    array.SetValue(DeserializeFieldOrProperty(reader, elementType), i);
                }

                return array;
            }
            else
            {
                // Recursively deserialize complex objects
                return DeserializeObject(reader);
            }
        }
    }
}
