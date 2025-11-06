using MessageTypes.Attributes;
using System;

namespace DataMemberNamesClassBuilder.Delegates.ToCPlusPlus
{
    internal delegate void DelegateAddGetterMethodArray(
        string typeString, string variableName, string underscoredVariableName,
        string underscoredLengthVariableName
    );
}
