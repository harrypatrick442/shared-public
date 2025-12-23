using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
namespace DataMemberNamesClassBuilder
{
    public static class ClassMemberIsNullableHelper
    {
        public static bool IsNullable(MemberInfo member)
        {
            Type memberType = member switch
            {
                PropertyInfo p => p.PropertyType,
                FieldInfo f => f.FieldType,
                _ => throw new ArgumentException("Member must be a field or property")
            };

            // 1. Nullable value type (int?, double?, etc)
            if (Nullable.GetUnderlyingType(memberType) != null)
                return true;

            // 2. Reference type nullability (MyClass?)
            // Look for [Nullable(2)] on the member
            if (HasNullableAttribute(member))
                return true;

            // 3. Look for [NullableContext(2)] on declaring type or module
            if (IsNullableViaContext(member))
                return true;

            return false;
        }

        static bool HasNullableAttribute(MemberInfo member)
        {
            return member.CustomAttributes.Any(a =>
                a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute" &&
                a.ConstructorArguments.Count == 1 &&
                ExtractFlag(a.ConstructorArguments[0]) == 2
            );
        }

        static bool IsNullableViaContext(MemberInfo member)
        {
            // Check type
            var type = member.DeclaringType;
            if (type != null)
            {
                if (type.CustomAttributes.Any(a =>
                    a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute" &&
                    (byte)a.ConstructorArguments[0].Value! == 2))
                    return true;
            }

            // Check module
            var module = member.Module;
            if (module.CustomAttributes.Any(a =>
                a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute" &&
                (byte)a.ConstructorArguments[0].Value! == 2))
                return true;

            return false;
        }

        static byte ExtractFlag(CustomAttributeTypedArgument arg)
        {
            if (arg.ArgumentType == typeof(byte))
                return (byte)arg.Value!;

            if (arg.ArgumentType == typeof(byte[]))
                return (byte)((ReadOnlyCollection<CustomAttributeTypedArgument>)arg.Value!)?[0].Value!;

            return 0;
        }

    }
}
