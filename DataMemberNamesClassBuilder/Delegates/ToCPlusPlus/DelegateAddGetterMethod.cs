using MessageTypes.Attributes;
using System;

namespace DataMemberNamesClassBuilder.Delegates.ToCPlusPlus
{
    internal delegate void DelegateAddGetterMethod(
        string typeString, string variableName, string underscoredVariableName
    );
}
