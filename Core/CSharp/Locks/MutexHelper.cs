using Core.Strings; 
namespace Core.Locks
{
    public static class MutexHelper
    {
        public static string GetCleanMutexName(string str) {
            return StringHelper.MultipleReplace(str, new string[] { " ", "\\", "/", "." }, "_");
        }
    }
}