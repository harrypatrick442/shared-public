using Core.Pool;
using Microsoft.Data.Sqlite;
using System.Text;
namespace Database
{
    public class DatabasePathsHelper
    {
        public static string SplitIdentifierIntoHundredsPathSegments(long identifier) {
            string str = identifier.ToString();
            string[] strs = new string[(str.Length+1) / 2];
            int i = 0, j = 0;
            int lengthMinusOne = str.Length - 1;
            while (i<lengthMinusOne)
            {
                strs[j++] =  ""+str[i++]+str[i++];
            }
            if (i <= lengthMinusOne)
            {
                strs[j] = ""+str[i];
            }
            return '/'+string.Join(Path.DirectorySeparatorChar, strs);
        }
    }
}
