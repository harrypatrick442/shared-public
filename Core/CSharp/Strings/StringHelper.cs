using System.Collections.Generic;
using System.Linq;
namespace Core.Strings
{
    public static class StringHelper
    {
        public static string LowerCamelCase(string str)
        {
            if (str == null) return str;
            string[] camelCaseSplit = StringHelper.SplitCamelCase(str);
            return camelCaseSplit.First().ToLower() + string.Join("", camelCaseSplit.Skip(1));
        }
        public static string UpperCamelCase(string str)
        {
            if (str == null) return str;
            if (str.Length < 1) return str;
            string[] camelCaseSplit = StringHelper.SplitCamelCase(str);
            string firstSplit = camelCaseSplit.First();
            string secondPartFirstSplit = "";
            if (firstSplit.Length > 1) {
                secondPartFirstSplit = firstSplit.Substring(1, firstSplit.Length - 1);
            }
            return (firstSplit[0]+"").ToUpper() + secondPartFirstSplit + string.Join("", camelCaseSplit.Skip(1));
        }
        public static string[] MultipleSplit(char[] toSplitOns, string str)
        {
            string[] toSplits = new string[] { str };
            foreach (char toSplitOn in toSplitOns)
            {
                List<string> nextToSplits = new List<string>();
                foreach (string toSplit in toSplits)
                {
                    nextToSplits.AddRange(toSplit.Split(toSplitOn));
                }
                toSplits = nextToSplits.ToArray();
            }
            return toSplits;
        }
        public static string Format(string str, Dictionary<string, string> mapTagToCustomContent) {

            foreach (KeyValuePair<string, string> keyValuePair in mapTagToCustomContent)
            {
                if (keyValuePair.Value == null) continue;
                str = str.Replace($"{{{keyValuePair.Key}}}", keyValuePair.Value);
            }
            return str;
        }
        public static string MultipleReplace(string str, string[] toReplaces, string with)
        {
            
            foreach (string toReplace in toReplaces)
            {
                str = str.Replace(toReplace, with);
            }
            return str;
        }
        public static string[] SplitCamelCase(string[] words) {
            return words.Select(word => SplitCamelCase(word))
                .SelectMany(w=>w)
                .ToArray();
        }
        public static string[] SplitCamelCase(string str) {
            List<string> words = new List<string>();
            string currentWord = "";
            foreach (char c in str) {
                if (char.IsUpper(c)) {
                    if (currentWord.Length > 0)
                    {
                        words.Add(currentWord);
                        currentWord = "";
                    }
                }
                currentWord += c;
            }
            if (currentWord.Length > 0)
                words.Add(currentWord);
            return words.ToArray();
        }
        public static bool ExtractFirstIntFromString(string str, out int value) {
            value = 0;
            int i = 0;
            while (i < str.Length)
            {
                char c = str[i++];
                if (char.IsDigit(c))
                {
                    string numberString = "" + c;
                    while (i < str.Length)
                    {
                        c = str[i++];
                        if (!char.IsDigit(c))
                        {
                            value = int.Parse(numberString);
                            return true;
                        }
                        numberString += c;
                    }
                    value = int.Parse(numberString);
                    return true;
                }
            }
            return false;
        }
        public static bool ExtractFirstStandAloneIntFromString(string str, out int value)
        {
            value = -1;
            string[] words = MultipleSplit(new char[] { '_', '-', ' ' }, str);
            foreach (string word in words) {
                if (int.TryParse(word, out value)) return true;
            }
            return false;
        }
        public static bool ExtractFirstStandAloneIntFromStringAllowingTrailingCharacters(string str, out int value)
        {
            value = -1;
            string[] words = MultipleSplit(new char[] { '_', '-', ' ' }, str);
            foreach (string word in words)            {
                string numbers = "";
                foreach (char c in word) {
                    if (char.IsNumber(c))
                        numbers += c;
                    else
                    {
                        if (numbers.Length > 0)
                        {
                            if (int.TryParse(numbers, out value)) return true;
                            else
                                break;
                        }
                        else
                            break;
                    }

                }
                if(int.TryParse(word, out value)) return true;
            }
            return false;
        }
        public static bool ExtractFirstIntFromStringStartingAtEnd(string str, out int value)
        {
            value = 0;
            int i = str.Length-1;
            while (i >=0 )
            {
                char c = str[i--];
                if (char.IsDigit(c))
                {
                    string numberString = "" + c;
                    while (i >=0)
                    {
                        c = str[i--];
                        if (!char.IsDigit(c))
                        {
                            value = int.Parse(numberString);
                            return true;
                        }
                        numberString = c+ numberString;
                    }
                    value = int.Parse(numberString);
                    return true;
                }
            }
            return false;
        }
        public static string PadWithLeadingZeros(string str, int length) {

            while (str.Length < length)
                str = '0' + str;
            return str;
        }
    }
}
