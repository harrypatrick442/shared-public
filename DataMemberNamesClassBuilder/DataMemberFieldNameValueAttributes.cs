using MessageTypes.Attributes;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace DataMemberNamesClassBuilder
{
    public class DataMemberFieldNameValueAttributes
    {
        public string Name { get; }
        public string Value { get; }
        public Attribute[] Attributes { get; }
        public Type ForType { get; }
        public Type? TypeFromActualUse { get; }
        public Type ConsumingType { get; }
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
            Type? typeFromActualUse,
            Type? consumingType) {
            Name = name;
            Value = value;
            Attributes = attributes;
            ForType = forType;
            TypeFromActualUse = typeFromActualUse;
            ConsumingType = consumingType;
        }
    }
}
