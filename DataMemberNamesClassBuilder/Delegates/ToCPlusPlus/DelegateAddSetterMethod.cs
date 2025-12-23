using MessageTypes.Attributes;
using System;

namespace DataMemberNamesClassBuilder.Delegates.ToCPlusPlus
{
    internal delegate void DelegateAddSetterMethod(
        string typeString, string variableName, string underscoredVariableName
    );
}
