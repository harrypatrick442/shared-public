using System; 
namespace DataMemberNamesClassBuilder
{
    public static class CPlusPlusHelper
    {
        public static CPlusPlusType TranslateTypeForCPlusPlus(Type type)
        {
            if (type == null) return CPlusPlusType.Unknown;
            if (type == typeof(string))
                return CPlusPlusType.CharPointer;
            if (type == typeof(sbyte))
                return CPlusPlusType.Int8;
            if (type == typeof(byte))
                return CPlusPlusType.UInt8;
            if (type == typeof(ushort))
                return CPlusPlusType.UInt16;
            if (type == typeof(uint))
                return CPlusPlusType.UInt32;
            if (type == typeof(int))
                return CPlusPlusType.Int32;
            if (type == typeof(long))
                return CPlusPlusType.Int64;
            if (type == typeof(ulong))
                return CPlusPlusType.UInt64;
            if (type == typeof(double))
                return CPlusPlusType.Double;
            if (type == typeof(bool))
                return CPlusPlusType.Bool;
            if (type == typeof(Int16))
                return CPlusPlusType.Int16;
            if (type == typeof(Int32))
                return CPlusPlusType.Int32;
            if (type == typeof(Int64))
                return CPlusPlusType.Int64;
            if (type == typeof(UInt16))
                return CPlusPlusType.UInt16;
            if (type == typeof(UInt32))
                return CPlusPlusType.UInt32;
            if (type == typeof(UInt64))
                return CPlusPlusType.UInt64;
            bool isNullable = type.IsGenericType 
                && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (isNullable)
            {
                Type innerType = Nullable.GetUnderlyingType(type)!;
                if (innerType == typeof(string))
                    return CPlusPlusType.NullableCharPointer;
                if (innerType == typeof(sbyte))
                    return CPlusPlusType.NullableInt8;
                if (innerType == typeof(byte))
                    return CPlusPlusType.NullableUInt8;
                if (innerType == typeof(ushort))
                    return CPlusPlusType.NullableUInt16;
                if (innerType == typeof(uint))
                    return CPlusPlusType.NullableUInt32;
                if (innerType == typeof(int))
                    return CPlusPlusType.NullableInt32;
                if (innerType == typeof(long))
                    return CPlusPlusType.NullableInt64;
                if (innerType == typeof(double))
                    return CPlusPlusType.NullableDouble;
                if (innerType == typeof(bool))
                    return CPlusPlusType.NullableBool;
                if (innerType == typeof(Int16))
                    return CPlusPlusType.NullableInt16;
                if (innerType == typeof(Int32))
                    return CPlusPlusType.NullableInt32;
                if (innerType == typeof(Int64))
                    return CPlusPlusType.NullableInt64;
                if (innerType == typeof(UInt16))
                    return CPlusPlusType.NullableUInt16;
                if (innerType == typeof(UInt32))
                    return CPlusPlusType.NullableUInt32;
                if (innerType == typeof(UInt64))
                    return CPlusPlusType.NullableUInt64;
                if (innerType.IsEnum)
                {
                    return CPlusPlusType.NullableInt32;
                }
                if (!innerType.IsPrimitive && innerType.IsClass)
                {
                    if (innerType.Equals(typeof(object)))
                    {
                        return CPlusPlusType.Unknown;
                    }
                    return CPlusPlusType.Class;
                }
            }
            if (type.IsEnum) {
                return CPlusPlusType.Int32;
            }
            if (!type.IsPrimitive && type.IsClass) {
                if (type.Equals(typeof(object)))
                {
                    return CPlusPlusType.Unknown;
                }
                return CPlusPlusType.Class;
            }
            throw new NotSupportedException($"Unsupported type: {type.FullName}");
        }

    }
}
