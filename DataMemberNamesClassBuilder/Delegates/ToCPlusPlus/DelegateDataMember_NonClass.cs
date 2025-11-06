namespace DataMemberNamesClassBuilder.Delegates.ToCPlusPlus
{
    internal delegate void DelegateDataMember_NonClass(
        CPlusPlusType cPlusPlusType,
        bool isArray,
        string key,
        string variableName,
        string underscoredVariableName
    );
}
