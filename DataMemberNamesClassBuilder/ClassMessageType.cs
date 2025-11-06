using Core.Reflection;
using MessageTypes.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DataMemberNamesClassBuilder
{
    public static class ClassExportHelper
    {
        public static void Find(
            string namespaceMustContain,
            Type[] typeInEachNamespace, 
            out Type[] dataMemberNamesTypes,
            out DataMemberNamesClass[] toExports,
            out Func<Type, DataMemberNamesClass> getDataMemberNamesClass) {
            Func<FieldInfo, Type, Type> getTypeFromActualUse = Create_GetTypeFromActualUse();
            dataMemberNamesTypes = typeInEachNamespace
                .SelectMany(GetTypesInNamespace)
                .Where(t => t.Namespace.Contains(namespaceMustContain)).ToArray();
            DataMemberNamesClass[] toExportsInternal = dataMemberNamesTypes
                .Select(d=>
                {
                    return GetDataMemberNamesClass(d, getTypeFromActualUse);
                }).ToArray();
            toExports = toExportsInternal;
            getDataMemberNamesClass = 
                (type) => toExportsInternal
                .Where(d => type.Name == d.TypeName)
                .FirstOrDefault();

        }
        private static DataMemberNamesClass GetDataMemberNamesClass(
            Type dataMemberNamesType,
            Func<FieldInfo, Type, Type> getTypeFromActualUse)
        {

            List<Type> typeAndBaseTypes = new List<Type> { };
            Type t = dataMemberNamesType;
            while (t != null)
            {
                typeAndBaseTypes.Add(t);
                t = t.BaseType;
            }
            List<DataMemberFieldNameValueAttributes> dataMemberFieldNameValuePairs = new List<DataMemberFieldNameValueAttributes>();
            foreach (Type type in typeAndBaseTypes)
            {
                FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static);
                dataMemberFieldNameValuePairs.AddRange(
                    fieldInfos
                    .Select(fieldInfo => new DataMemberFieldNameValueAttributes(fieldInfo.Name,
                    (string)fieldInfo.GetValue(null), AttributesHelper.GetAttributes(fieldInfo), type,
                    typeFromActualUse:getTypeFromActualUse(fieldInfo, type))));
            }
            DataMemberFieldNameValueAttributes[][] duplicatedDataMemberFieldNameValuePairs =
                dataMemberFieldNameValuePairs.GroupBy(d => d.Value).Where(g => g.Count() > 1).Select(g => g.ToArray()).ToArray();
            if (duplicatedDataMemberFieldNameValuePairs.Length > 0)
                throw new Exception($"{dataMemberNamesType.Name}.{duplicatedDataMemberFieldNameValuePairs.First().First().Name} value \"{duplicatedDataMemberFieldNameValuePairs.First().First().Value}\" was shared by multiple fields");

            MessageTypeAttribute
                messageTypeAttribute = (MessageTypeAttribute)
                    dataMemberNamesType.GetCustomAttribute(typeof(MessageTypeAttribute), true);
            return new DataMemberNamesClass(dataMemberNamesType.Name,
                dataMemberFieldNameValuePairs.ToArray(), messageTypeAttribute);
        }
        private static Func<FieldInfo, Type, Type?> Create_GetTypeFromActualUse() { 
            return (fieldInfo, dataMemberNamesClassType) => {
                string actualClassName = dataMemberNamesClassType.Name;
                int indexOfDataMemberNamesString = actualClassName.LastIndexOf("DataMemberNames");
                if (indexOfDataMemberNamesString + 15 != actualClassName.Length) {
                    throw new Exception("\"DataMemberNames\" should be at the end of the class name");
                }
                string nameOfConsumingClassToSearchFor = actualClassName.Substring(0, indexOfDataMemberNamesString);
                string parentNamespace = dataMemberNamesClassType.Namespace.Split(".").FirstOrDefault();
                if (parentNamespace == null) { 
                    parentNamespace = dataMemberNamesClassType.Namespace;
                }
                Type[] consumingClassTypes = dataMemberNamesClassType.Assembly
                    .GetTypes()
                    .Where(t =>
                        t.IsClass&&
                        t.Name == nameOfConsumingClassToSearchFor)
                    .ToArray();
                string? obfuscatedName = (string?)fieldInfo.GetValue(null);
                Type[] consumingClassPropertyTypes = consumingClassTypes.SelectMany(
                    c=>
                c.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p=>new
                {
                    dataMemberAttribute = (DataMemberAttribute?)p.GetCustomAttribute(typeof(DataMemberAttribute), true),
                    jsonPropertyNameAttribute = (JsonPropertyNameAttribute?)p.GetCustomAttribute(typeof(JsonPropertyNameAttribute)),
                    propertyInfo = p
                }
                )
                .Where(o=>
                {
                    return (o.dataMemberAttribute != null && o.dataMemberAttribute.Name == obfuscatedName)
                    || (o.jsonPropertyNameAttribute != null && o.jsonPropertyNameAttribute.Name == obfuscatedName)
                    ;
                })
                .Select(o=>o.propertyInfo.PropertyType))
                .ToArray();
                if (consumingClassPropertyTypes.Any()) {
                    if (consumingClassPropertyTypes.GroupBy(t => t).Count() > 1) {
                        throw new Exception($"Not all property types used on the property named {fieldInfo.Name} for classes named {nameOfConsumingClassToSearchFor} matched. Types found were: {string.Join(',', consumingClassPropertyTypes.Select(t => t.Name))}");
                    }
                    return consumingClassPropertyTypes[0];
                }
                return null;
            };
        }
        private static Type[] GetTypesInNamespace(Type typeInNamespace)
        {
            string namespac = typeInNamespace.Namespace;
            int i = namespac.IndexOf("DataMemberNames.");
            if (i < 0)
            {
                if (namespac.IndexOf("DataMemberNames") == namespac.Length - 15)
                {

                }
                else
                {
                    return new Type[0];
                }
            }
            else
            {
                namespac = namespac.Substring(0, i + 15);
            }

            return ReflectionHelper.GetTypesInNamespace(
                        Assembly.GetAssembly(typeInNamespace),
                        namespac
                ).Where(t => t.Namespace.IndexOf(".Interserver") < 0).ToArray();
        }
    }
}
