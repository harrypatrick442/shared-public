using System;
using System.Collections.Generic;
using Logging;
using System.Linq;
using InfernoDispatcher.Tasks;
using System.Threading;
using Shutdown;
using Core.Threading.ContextAssigned;

namespace Core.Maths.CUBLAS
{

    public interface INonThreadSpecificTaskSource<THandle> {
        InfernoTask? TakeTaskElseListAwaken(ContextAssignedSingleThreadDispatcher<THandle> singleThreadDispatcher);

    }
}