using Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.NativeExtensions
{
    public static class EnumExtensions
    {
        public static int ToInt(this Enum enumValue)
        {
            Type underlyingType = Enum.GetUnderlyingType(enumValue.GetType());

            // Convert the enum value based on its underlying type
            if (underlyingType == typeof(int))
            {
                return (int)(object)enumValue;
            }
            else if (underlyingType == typeof(byte))
            {
                return (byte)(object)enumValue;
            }
            else if (underlyingType == typeof(long))
            {
                return (int)(long)(object)enumValue;  // Convert long to int (if safe)
            }
            else if (underlyingType == typeof(short))
            {
                return (short)(object)enumValue;
            }
            else
            {
                throw new InvalidOperationException("Unsupported enum underlying type");
            }
        }
    }
}
