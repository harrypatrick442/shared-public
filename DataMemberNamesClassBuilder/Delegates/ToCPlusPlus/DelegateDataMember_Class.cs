using MessageTypes.Attributes;
using System;

namespace DataMemberNamesClassBuilder.Delegates.ToCPlusPlus
{
    internal delegate void DelegateDataMember_Class(
        CPlusPlusType cPlusPlusType,
        bool isArray,
        string key,
        string variableName,
        string underscoredVariableName,
        string propertyClassName/*dataMemberPropertyNameValuePair.Nam*/,
        string propertyClassTypeName/*dataMemberPropertyNameValuePair.TypeFromActualUse.Name*/,
        DataMemberNamesClassAttribute dataMemberNamesClassAttribute
    );
}
