using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Core.Reflection;
using System.Collections.Generic;
using System.IO;
using GlobalConstants;

namespace DataMemberNamesClassBuilder
{
    public class JavaScriptConstantsBuilderHelper
    {

        public static void Run(string outputDirectory, string assemblyPath,
            Type[] typeInEachNamespace)
        {
            Type[] constantsClassTypes = typeInEachNamespace.SelectMany(
                GetTypesInNamespace)
                .GroupBy(t=>t)
                .Select(g=>g.First())
                .ToArray();
            ConstantsClass[] constantsClasses = GetConstantsClasses(constantsClassTypes);
            string generatedConstantsJavaScriptFilePath = 
                Path.Combine(outputDirectory, "Generated.js");
            try { File.Delete(generatedConstantsJavaScriptFilePath); } catch { };
            Directory.CreateDirectory(outputDirectory);
            StringBuilder sb = new StringBuilder();
            sb.Append("const Generated = {");
            bool firstConstantsClass = true;
            foreach (ConstantsClass constantsClass in constantsClasses)
            {
                if (firstConstantsClass) firstConstantsClass = false;
                else sb.Append(",");
                sb.AppendLine();
                sb.Append("\t");
                sb.Append(constantsClass.ClassName);
                sb.Append(":{");
                bool firstMember = true; 
                foreach (ConstantsClassMember constantsClassMember in constantsClass.Members) {
                    if (firstMember) firstMember = false;
                    else sb.Append(",");
                    sb.AppendLine();
                    sb.Append("\t\t");
                    sb.Append(constantsClassMember.Name);
                    sb.Append(":");
                    Type valueType = constantsClassMember.Value.GetType();
                    if (typeof(string).IsAssignableFrom(valueType))
                    {
                        sb.Append("\"");
                        sb.Append((string)constantsClassMember.Value);
                        sb.Append("\"");
                    }
                    else if (typeof(int).IsAssignableFrom(valueType))
                    {
                        sb.Append((int)constantsClassMember.Value);
                    }
                    else if (typeof(long).IsAssignableFrom(valueType))
                    {
                        sb.Append((long)constantsClassMember.Value);
                    }
                    else if (typeof(bool?).IsAssignableFrom(valueType))
                    {
                        sb.Append((bool?)constantsClassMember.Value switch
                        {
                            true => "true",
                            false => "false",
                            null => "null"
                        });
                    }
                    else if (typeof(bool).IsAssignableFrom(valueType))
                    {
                        sb.Append((bool)constantsClassMember.Value?"true":"false");
                    }
                }
                sb.AppendLine();
                sb.Append("\t}");
            }
            sb.AppendLine();
            sb.AppendLine("}");
            sb.Append("export default Generated;");
            File.WriteAllText(generatedConstantsJavaScriptFilePath, sb.ToString());
        }
        private static ConstantsClass[] GetConstantsClasses(Type[] constantsClassTypes) {
            List<ConstantsClass> constantsClasses = new List<ConstantsClass>();
            foreach (Type constantsClassType in constantsClassTypes) {
                bool wholeClass = constantsClassType.GetCustomAttribute(typeof(ExportToJavaScriptAttribute)) != null;
                FieldInfo[] fieldInfos = constantsClassType.GetFields(BindingFlags.Public | BindingFlags.Static);
                FieldInfo[] fieldsToExport = wholeClass?fieldInfos:fieldInfos.Where(f => IsExportToJavaScript(f)).ToArray();
                if (fieldsToExport.Length <= 0) continue;
                ConstantsClassMember[] constantsClassMembers = fieldsToExport
                    .Select(f => new ConstantsClassMember(f.Name, f.GetValue(null)))
                    .ToArray();
                constantsClasses.Add(new ConstantsClass(constantsClassType.Name, constantsClassMembers));
            }
            return constantsClasses.ToArray();
        }
        private static bool IsExportToJavaScript(FieldInfo fieldInfo)
        {
            return fieldInfo.GetCustomAttributes(typeof(ExportToJavaScriptAttribute), true).Any();
        }
        private static Type[] GetTypesInNamespace(Type typeInNamespace)
        {
            string namespac = typeInNamespace.Namespace;
            int i = namespac.IndexOf("Constants.");
            if (i < 0)
            {
                if (namespac.IndexOf("Constants") == namespac.Length - 9)
                {

                }
                else
                {
                    return new Type[0];
                }
            }
            else
            {
                namespac = namespac.Substring(0, i + 9);
            }

            return ReflectionHelper.GetTypesInNamespace(
                        Assembly.GetAssembly(typeInNamespace),
                        namespac
                ).ToArray();
        }
    }
}
