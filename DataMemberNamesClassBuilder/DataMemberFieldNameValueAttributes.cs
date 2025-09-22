using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Core.Reflection;
using Core.FileSystem;
using MessageTypes.Attributes;
using MessageTypes.Attributes;

namespace DataMemberNamesClassBuilder
{
    public class DataMemberFieldNameValueAttributes
    {
        public string Name { get; }
        public string Value { get; }
        public Attribute[] Attributes { get; }
        public Type ForType { get; }
        public Type? TypeFromActualUse { get; }
        public DataMemberNamesClassAttribute DataMemberNamesClassAttribute
        {
            get
            {

                return (MessageTypes.Attributes.DataMemberNamesClassAttribute)
                    Attributes.Where(a => typeof(MessageTypes.Attributes.DataMemberNamesClassAttribute).IsAssignableFrom(a.GetType())).FirstOrDefault();
            }
        }
        public DataMemberNamesIgnoreAttribute DataMemberNamesIgnoreAttribute
        {
            get
            {

                return (MessageTypes.Attributes.DataMemberNamesIgnoreAttribute)
                    Attributes.Where(a => typeof(MessageTypes.Attributes.DataMemberNamesIgnoreAttribute).IsAssignableFrom(a.GetType())).FirstOrDefault();
            }
        }
        public DataMemberFieldNameValueAttributes(
            string name, string value, Attribute[] attributes, Type forType,
            Type? typeFromActualUse) {
            Name = name;
            Value = value;
            Attributes = attributes;
            ForType = forType;
            TypeFromActualUse = typeFromActualUse;
        }
    }
}
