using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Core.Reflection;
using System.Collections.Generic;
using System.IO;
using Core.Strings;

namespace DataMemberNamesClassBuilder
{
    public static class MessageTypesBuilderHelper
    {
        public static void GenerateMessageTypesJs(string outputDirectory, 
            Type[] typeInEachNamespace)
        {
            Type[] messageTypesTypes = typeInEachNamespace.SelectMany(
                GetTypesInNamespace).ToArray();
            List<DataMemberFieldNameValueAttributes> dataMemberFieldNameValuePairs = new List<DataMemberFieldNameValueAttributes>();
            foreach (Type messageTypesType in messageTypesTypes)
            {
                FieldInfo[] fieldInfos = messageTypesType
                    .GetFields(BindingFlags.Public | BindingFlags.Static);
                dataMemberFieldNameValuePairs.AddRange(
                    fieldInfos
                    .Select(fieldInfo =>
                        new DataMemberFieldNameValueAttributes(
                            fieldInfo.Name,
                            (string)fieldInfo.GetValue(null),
                            AttributesHelper.GetAttributes(fieldInfo),
                            messageTypesType, null
                        )
                    )
                );
            }
            StringBuilder sbErrors = null;
            CheckForDuplicates(ref sbErrors, dataMemberFieldNameValuePairs, true);
            CheckForDuplicates(ref sbErrors, dataMemberFieldNameValuePairs, false);
            if (sbErrors != null) { 
                throw new Exception(sbErrors.ToString());
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("const MessageTypes");
            sb.Append(" ={");
            bool first = true;
            foreach (DataMemberFieldNameValueAttributes dataMemberPropertyNameValuePair in dataMemberFieldNameValuePairs)
            {
                if (first) first = false;
                else sb.Append(",");
                sb.AppendLine("");
                sb.Append("     ");
                string lowerCamelCase = StringHelper.LowerCamelCase(dataMemberPropertyNameValuePair.Name);
                sb.Append(lowerCamelCase);
                sb.Append(":");
                sb.Append("\"");
                sb.Append(dataMemberPropertyNameValuePair.Value);
                sb.Append("\"");
            }
            sb.AppendLine("");
            sb.AppendLine("}");
            sb.Append("export default MessageTypes");
            sb.AppendLine(";");
            Console.WriteLine(outputDirectory);
            File.WriteAllText(Path.Combine(outputDirectory, "MessageTypes.js"), sb.ToString());
        }
        private static void CheckForDuplicates(ref StringBuilder sb, List<DataMemberFieldNameValueAttributes> dataMemberFieldNameValuePairs, bool nameElseValue)
        {
            DataMemberFieldNameValueAttributes[][]
                groupsOfDuplicates =
            dataMemberFieldNameValuePairs
            .GroupBy((d) => nameElseValue?d.Name:d.Value)
            .Where(g => g.Count() > 1)
            .Select(g => g.ToArray())
            .ToArray();
            if (!groupsOfDuplicates.Any()) return;
            if(sb==null)sb= new StringBuilder();
            bool first = true;
            foreach (DataMemberFieldNameValueAttributes[] duplicates in groupsOfDuplicates)
            {
                sb.Append($" message type ");
                sb.Append(nameElseValue ? "name" : "value");
                sb.Append("\"");
                sb.Append(duplicates.First().Name);
                sb.Append("\" was duplicated in the classes");
                foreach (DataMemberFieldNameValueAttributes duplicate in duplicates)
                {
                    if (first) { first = false; }
                    else sb.Append(", ");
                    sb.Append(" namespace: \"");
                    sb.Append(duplicate.ForType.Namespace);
                    sb.Append("\" class: \"");
                    sb.Append(duplicate.ForType.Name);
                }
            }
        }
        private static Type[] GetTypesInNamespace(Type typeInNamespace)
        {
            string namespac = typeInNamespace.Namespace;
            int i = namespac.IndexOf(".");
            string rootNamespace = i >= 0?namespac.Substring(0, i):namespac;

            return ReflectionHelper.GetTypesInNamespace(
                        Assembly.GetAssembly(typeInNamespace),
                        rootNamespace
                ).Where(t => t.Name.Equals("MessageTypes")).ToArray();
        }
    }
}
