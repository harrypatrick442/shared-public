using System;
using System.IO;
using System.Reflection;

namespace Core.Serialization
{
    public static class BinaryReflectionSerializer
    {
        public static void Serialize<TObject>(TObject obj, string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                SerializeObject(obj, writer);
            }
        }

        private static void SerializeObject(object obj, BinaryWriter writer)
        {
            Type objType = obj.GetType();

            // Write type information
            writer.Write(objType.AssemblyQualifiedName);

            // Check if it's an array
            if (objType.IsArray)
            {
                Array array = (Array)obj;
                writer.Write(array.Length);  // Write the length of the array

                foreach (var element in array)
                {
                    SerializeFieldOrProperty(writer, element, element.GetType());
                }
            }
            else
            {
                // Serialize each field and property for a non-array object
                foreach (var field in objType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    SerializeFieldOrProperty(writer, field.GetValue(obj), field.FieldType);
                }

                foreach (var property in objType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (property.CanRead)
                    {
                        SerializeFieldOrProperty(writer, property.GetValue(obj), property.PropertyType);
                    }
                }
            }
        }

        private static void SerializeFieldOrProperty(BinaryWriter writer, object value, Type type)
        {
            if (type == typeof(int))
            {
                writer.Write((int)value);
            }
            else if (type == typeof(double))
            {
                writer.Write((double)value);
            }
            else if (type == typeof(float))
            {
                writer.Write((float)value);
            }
            else if (type == typeof(long))
            {
                writer.Write((long)value);
            }
            else if (type == typeof(short))
            {
                writer.Write((short)value);
            }
            else if (type == typeof(byte))
            {
                writer.Write((byte)value);
            }
            else if (type == typeof(char))
            {
                writer.Write((char)value);
            }
            else if (type == typeof(decimal))
            {
                writer.Write((decimal)value);
            }
            else if (type == typeof(sbyte))
            {
                writer.Write((sbyte)value);
            }
            else if (type == typeof(ushort))
            {
                writer.Write((ushort)value);
            }
            else if (type == typeof(uint))
            {
                writer.Write((uint)value);
            }
            else if (type == typeof(ulong))
            {
                writer.Write((ulong)value);
            }
            else if (type == typeof(string))
            {
                writer.Write((string)value ?? string.Empty);
            }
            else if (type == typeof(bool))
            {
                writer.Write((bool)value);
            }
            else if (type == typeof(DateTime))
            {
                // Serialize DateTime as a binary value for compactness
                writer.Write(((DateTime)value).ToBinary());
            }
            else if (type.IsArray)
            {
                Array array = (Array)value;
                writer.Write(array.Length);  // Write array length
                foreach (var item in array)
                {
                    SerializeFieldOrProperty(writer, item, item.GetType());
                }
            }
            else
            {
                // Recursively serialize complex objects
                SerializeObject(value, writer);
            }
        }
    }
}
