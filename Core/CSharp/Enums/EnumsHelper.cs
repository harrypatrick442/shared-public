
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Snippets.Enums
{
    public static class EnumsHelper
    {
        public static TEnum Parse<TEnum>(string value) where TEnum : struct, IConvertible
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException($"{nameof(TEnum)} must be an enumerated type");
            }
            if (!string.IsNullOrEmpty(value))
            {
                foreach (TEnum item in Enum.GetValues(typeof(TEnum)))
                {
                    if (item.ToString().ToLower().Equals(value.Trim().ToLower())) return item;
                }
            }
            return default(TEnum);
        }
        public static string GetString<TEnum>(TEnum @enum, bool toLowerCase = true) where TEnum : struct, IConvertible
        {
            string str = Enum.GetName(typeof(TEnum), @enum);
            if (toLowerCase)
                str = str.ToLowerInvariant();
            return str;
        }
    }
}
