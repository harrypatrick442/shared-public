using System;
using Core.Exceptions;

namespace Core.Enums
{
    public enum PermissionEnum
    {
        ReadFromDatabase = 1,
        WriteToDatabase = 2,
        DeleteFromDatabase = 3,
        Accounts=4,
        PhysicalModules=5
    }
    public static class PermissionEnumHelper
    {
        public static PermissionEnum Parse(int value)
        {
            if (!Enum.IsDefined(typeof(PermissionEnum), value))
                throw new ParseException($"Failed to pass value {value} to {typeof(PermissionEnum)}");
            return (PermissionEnum)value;
        }
    }
}