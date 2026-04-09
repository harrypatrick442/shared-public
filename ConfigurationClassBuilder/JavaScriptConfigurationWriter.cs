using System.Reflection;
using System.Text;
using Checksums;
using Core.Atomics;
namespace ConfigurationClassBuilder
{
    public class JavaScriptConfigurationWriter
    {
        public static void Write<TConfigurationStruct>(
            string filePath,
            TConfigurationStruct configurationStruct,
            AlreadyWroteWatcher alreadyWroteWatcher
        ) where TConfigurationStruct: unmanaged
        {
            string structName = typeof(TConfigurationStruct).Name;
            StringBuilder sb = new StringBuilder();
            sb.Append("const ");
            sb.Append(structName);
            sb.Append(" = {");
            bool isFirst = true;
            foreach (var field in typeof(TConfigurationStruct).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                Type fieldType = field.FieldType;
                string fieldName = field.Name;
                object? value = field.GetValue(configurationStruct);
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sb.AppendLine(",");
                }
                //sb.Append($"    .{StringHelper fieldName} = {value}");
            }
            //alreadyWroteWatcher.ImGoingToWrite(projectSpecificConfigurationFilePath);
            //Directory.CreateDirectory(Path.GetDirectoryName(projectSpecificConfigurationFilePath)!);
            //File.WriteAllText(projectSpecificConfigurationFilePath, sb.ToString());
        }
        private static string GetCPlusPlusTypeName(Type fieldType)
        {
            if (fieldType == typeof(byte)) return "uint8_t";
            if (fieldType == typeof(sbyte)) return "int8_t";
            if (fieldType == typeof(short)) return "int16_t";
            if (fieldType == typeof(ushort)) return "uint16_t";
            if (fieldType == typeof(int)) return "int32_t";
            if (fieldType == typeof(uint)) return "uint32_t";
            if (fieldType == typeof(long)) return "int64_t";
            if (fieldType == typeof(ulong)) return "uint64_t";

            if (fieldType == typeof(float)) return "float";
            if (fieldType == typeof(double)) return "double";
            if (fieldType == typeof(decimal)) return "double"; // no native decimal in C++, map to double

            throw new NotSupportedException($"Unsupported type: {fieldType.FullName}");
        }
        private static bool RequiresCstdint(Type type)
        {
            return type == typeof(byte)
                || type == typeof(sbyte)
                || type == typeof(short)
                || type == typeof(ushort)
                || type == typeof(int)
                || type == typeof(uint)
                || type == typeof(long)
                || type == typeof(ulong);
        }
    }
}
