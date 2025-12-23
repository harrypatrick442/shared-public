using MessageTypes.Attributes;
using System;
using System.Reflection;

namespace DataMemberNamesClassBuilder.Delegates.ToCPlusPlus
{/// <summary>
/// 
/// </summary>
/// <param name="fieldInfo"></param>
/// <param name="dataMemberNamesType"></param>
/// <param name="actualContainingClassType"></param>
/// <returns>The actual type of the property</returns>
    internal delegate Type? DelegateGetTypeAndContainingTypeFromActualUse(
        FieldInfo fieldInfo, Type dataMemberNamesType, out Type? actualContainingClassType);
}
