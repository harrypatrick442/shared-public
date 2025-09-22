

using System;

namespace DataMemberNamesClassBuilder
{
    public enum CPlusPlusType
    {
        Unknown,
        CharPointer,
        Int8,
        UInt8,
        Int16,
        UInt16,
        UInt32,
        Int32,
        Int64,
        UInt64,
        Double,
        Bool,
        NullableCharPointer,
        NullableInt8,
        NullableUInt8,
        NullableInt16,
        NullableUInt16,
        NullableInt32,
        NullableUInt32,
        NullableInt64,
        NullableUInt64,
        NullableDouble,
        NullableBool,
        Class,
        NullableClass
    }
    public static class CPlusPlusTypeExtensions
    {
        public static bool IsClass(this CPlusPlusType type)
        {
            return type.Equals(CPlusPlusType.Class);
        }
        public static bool IsUnknown(this CPlusPlusType type)
        {
            return type.Equals(CPlusPlusType.Unknown);
        }
        public static string GetString(this CPlusPlusType type)
        {
            return type switch
            {
                CPlusPlusType.CharPointer => "const char*",
                CPlusPlusType.Int8 => "int8_t",
                CPlusPlusType.UInt8 => "uint8_t",
                CPlusPlusType.Int16 => "int16_t",
                CPlusPlusType.UInt16 => "uint16_t",
                CPlusPlusType.Int32 => "int32_t",
                CPlusPlusType.UInt32 => "uint32_t",
                CPlusPlusType.Int64 => "int64_t",
                CPlusPlusType.UInt64 => "uint64_t",
                CPlusPlusType.Double => "double",
                CPlusPlusType.Bool => "bool",
                CPlusPlusType.NullableCharPointer => "std::optional<const char*>",
                CPlusPlusType.NullableInt8 => "std::optional<int8_t>",
                CPlusPlusType.NullableUInt8 => "std::optional<uint8_t>",
                CPlusPlusType.NullableInt16 => "std::optional<int16_t>",
                CPlusPlusType.NullableUInt16 => "std::optional<uint16_t>",
                CPlusPlusType.NullableInt32 => "std::optional<int32_t>",
                CPlusPlusType.NullableUInt32 => "std::optional<uint32_t>",
                CPlusPlusType.NullableInt64 => "std::optional<int64_t>",
                CPlusPlusType.NullableUInt64 => "std::optional<uint64_t>",
                CPlusPlusType.NullableDouble => "std::optional<double>",
                CPlusPlusType.NullableBool => "std::optional<bool>",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }

}
