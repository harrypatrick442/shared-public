using Core.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.NativeExtensions
{
    public static class TaskExtensions
    {
        public static T WaitResult<T>(this Task<T> task)
        {
            return task.GetAwaiter().GetResult();
        }
    }
}
