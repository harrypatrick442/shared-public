using Core.Reflection;
using DataMemberNamesClassBuilder.Delegates.ToCPlusPlus;
using MessageTypes.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            DelegateGetTypeAndContainingTypeFromActualUse getTypeFromActualUse = 
                Create_GetTypeFromActualUse();
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
            DelegateGetTypeAndContainingTypeFromActualUse getTypeFromActualUse)
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
                    .Select(fieldInfo =>
                    {
                        var typeFromActualUse = 
                            getTypeFromActualUse(fieldInfo, type, 
                                out Type? consumingType);
                        return new DataMemberFieldNameValueAttributes(fieldInfo.Name,
                        (string)fieldInfo.GetValue(null), AttributesHelper.GetAttributes(fieldInfo), type,
                        typeFromActualUse,
                        consumingType);
                    }));
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
        private static DelegateGetTypeAndContainingTypeFromActualUse Create_GetTypeFromActualUse() { 
            return (FieldInfo fieldInfo, Type dataMemberNamesClassType,
                out Type? consumingClassType) => {
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
                var consumingClassPropertyTypeAndClassType_s = consumingClassTypes.SelectMany(
                    c=>
                c.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p=>new
                {
                    dataMemberAttribute = (DataMemberAttribute?)p.GetCustomAttribute(typeof(DataMemberAttribute), true),
                    jsonPropertyNameAttribute = (JsonPropertyNameAttribute?)p.GetCustomAttribute(typeof(JsonPropertyNameAttribute)),
                    propertyInfo = p,
                    consumingClassType=c
                }
                )
                .Where(o=>
                {
                    return (o.dataMemberAttribute != null && o.dataMemberAttribute.Name == obfuscatedName)
                    || (o.jsonPropertyNameAttribute != null && o.jsonPropertyNameAttribute.Name == obfuscatedName)
                    ;
                })
                .Select(o=>new {
                        propertyType = o.propertyInfo.PropertyType,
                        containingType = o.consumingClassType
                    }))
                .ToArray();
                if (consumingClassPropertyTypeAndClassType_s.Any()) {
                    if (consumingClassPropertyTypeAndClassType_s.GroupBy(o => o.propertyType).Count() > 1) {
                        throw new Exception($"Not all property types used on the property named {fieldInfo.Name} for classes named {nameOfConsumingClassToSearchFor} matched. Types found were: {string.Join(',', consumingClassPropertyTypeAndClassType_s.Select(t => t.propertyType.Name))}");
                    }
                    consumingClassType = consumingClassPropertyTypeAndClassType_s[0].containingType;
                    return consumingClassPropertyTypeAndClassType_s[0].propertyType;
                }
                consumingClassType = null;
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
