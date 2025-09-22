using InfernoDispatcher.Tasks;
using ManagedCuda;
using System;
using System.Threading;
namespace Core.Maths.CUBLAS
{

    public class CudaSynchronizeHelper
    {

        public static bool IsStreamSynchronized(CudaStream stream)
        {
            int status = CudartInterop.cudaStreamQuery(stream.Stream.Pointer);
            if (status == 0) // cudaSuccess
            {
                return true; // Stream is synchronized
            }
            if (status == 1||status==600) // cudaErrorNotReady
            {
                return false; // Stream is still executing
            }
            throw new Exception($"CUDA stream error: {status}");
        }
        public static InfernoTaskNoResultBase WhenSynchronizedRunOnCudaThread(
            ICudaContextWithThread contextWithThread, CudaStream stream, 
            CancellationToken cancellationToken, Action<CudaContextHandles> callback) {
            InactiveInfernoTaskNoResultArgument<CudaContextHandles> inactiveInfernoTaskNoResult = new InactiveInfernoTaskNoResultArgument<CudaContextHandles>(callback);
            Action? check = null;
            check = () => {
                try
                {
                    contextWithThread.UsingContext((handles) =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            inactiveInfernoTaskNoResult.Cancel();
                            return;
                        }
                        try
                        {
                            if (IsStreamSynchronized(stream))
                            {
                                inactiveInfernoTaskNoResult.Run(new object[] { handles });
                                return;
                            }
                            check!();
                        }
                        catch (Exception ex)
                        {
                            inactiveInfernoTaskNoResult.Fail(ex);
                        }
                    });
                }
                catch (Exception ex)
                {
                    inactiveInfernoTaskNoResult.Fail(ex);
                }
            };
            check();
            return inactiveInfernoTaskNoResult;
        }
    }
}