using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Core.Processes
{
    public static class ProcessHelper
    {
        public static bool IsProcessRunning(int pid) {
            return GetByPid(pid)!=null;
        }
        public static Process GetByPid(int pid) {
            return Process.GetProcesses().Where(process => process.Id == pid).FirstOrDefault();
        }
        public static int GetMyPid()
        {
            return System.Diagnostics.Process.GetCurrentProcess().Id;
        }
        public static ProcessWrapper[] GetProcessesNow()
        {
            return System.Diagnostics.Process.GetProcesses().Select(p => new ProcessWrapper(p)).ToArray();
        }
        public static int GetMyProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }
    }
}
