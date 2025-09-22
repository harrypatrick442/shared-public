using Core.Exceptions;
using System;
using System.Collections.Generic;
using Core.Parsing;

namespace Core.CSV
{
    public static class CSVParsingHelper
    {
        public static Func<string, Func<string[], string>> Get_Get_GetValueFromLineLexicalClosure(List<string> headingsNormalized)
        {
            return new Func<string, Func<string[], string>>((heading) =>
            {
                int index = GetIndexOfHeading(headingsNormalized, heading);
                return new Func<string[], string>((line) =>
                {
                    return line[index];
                });
            });
        }
        public static Func<string, Func<string[], int>> Get_Get_GetIntFromLineLexicalClosure(List<string>headingsNormalized)
        {
            return new Func<string, Func<string[], int>>((heading) =>
            {
                int index = GetIndexOfHeading(headingsNormalized, heading);
                return new Func<string[], int>((line) =>
                {
                    int value;
                    string lineValue = line[index];
                    if (!int.TryParse(lineValue, out value))
                        throw new ParseException($"Was unable to parse \"{heading}\" value \"{lineValue}\" to an int");
                    return value;
                });
            });
        }
        public static Func<string, Func<string[], bool>> Get_Get_GetBoolFromLineLexicalClosure(List<string> headingsNormalized)
        {
            return new Func<string, Func<string[], bool>>((heading) =>
            {
                int index = GetIndexOfHeading(headingsNormalized, heading);
                return new Func<string[], bool>((line) =>
                {
                    bool value;
                    string lineValue = line[index];
                    if (!FlexibleBoolParser.TryParse(lineValue, out value, emptyIs:false))
                        throw new ParseException($"Was unable to parse \"{heading}\" value \"{lineValue}\" to an bool");
                    return value;
                });
            });
        }
        public static int GetIndexOfHeading(List<string> headingsNormalized, string heading) {
            string headingNormalized = NormalizeHeading(heading);
            int index = headingsNormalized.IndexOf(headingNormalized);
            if (index < 0)
                throw new ParseException($"Could not find the heading \"{heading}\" in the csv file");
            return index;
        }
        public static string NormalizeHeading(string heading) {
            return heading.ToLower().Replace("-", "").Replace("_", "").Replace(" ", "");
        }
    }
}
