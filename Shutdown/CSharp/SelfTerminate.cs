
namespace Snippets.ThisSystem
{
    public class SelfTerminate
    {
        public static bool IfAlreadyRunningSelfTerminate() {
            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location);
            int nInstancesRunning = System.Diagnostics.Process.GetProcessesByName(fileNameWithoutExtension ).Count();
            if (nInstancesRunning <= 1) return false;
           // System.Diagnostics.Process.GetCurrentProcess().Kill();
            Environment.Exit(0);
            return true;
        }
    }
}