using MessageTypes.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DataMemberNamesClassBuilder
{
    public class AttributesHelper
    {
        public static Attribute[] GetAttributes(FieldInfo fieldInfo)
        {
            MessageTypes.Attributes.DataMemberNamesClassAttribute
                dataMemberNamesClass = (MessageTypes.Attributes.DataMemberNamesClassAttribute)
                    fieldInfo.GetCustomAttributes(typeof(DataMemberNamesClassAttribute), true)
                .FirstOrDefault();
            MessageTypes.Attributes.DataMemberNamesIgnoreAttribute
                dataMemberNamesIgnore = (MessageTypes.Attributes.DataMemberNamesIgnoreAttribute)
                    fieldInfo.GetCustomAttributes(typeof(DataMemberNamesIgnoreAttribute), true)
                .FirstOrDefault();
            List<Attribute> attributes = new List<Attribute>();
            if (dataMemberNamesClass != null) attributes.Add(dataMemberNamesClass);
            if (dataMemberNamesIgnore != null) attributes.Add(dataMemberNamesIgnore);
            return attributes.ToArray();
        }
    }
}
