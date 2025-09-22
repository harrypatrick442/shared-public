using System;

namespace Core.Maths.CUBLAS
{
    public enum CudaError
    {
        cudaSuccess = 0,
        cudaErrorMissingConfiguration = 1,
        cudaErrorMemoryAllocation = 2,
        cudaErrorInitializationError = 3,
        cudaErrorLaunchFailure = 4,
        cudaErrorPriorLaunchFailure = 5,
        cudaErrorLaunchTimeout = 6,
        cudaErrorLaunchOutOfResources = 7,
        cudaErrorInvalidDeviceFunction = 8,
        cudaErrorInvalidConfiguration = 9,
        cudaErrorInvalidDevice = 10,
        cudaErrorInvalidValue = 11,
        cudaErrorInvalidPitchValue = 12,
        cudaErrorInvalidSymbol = 13,
        cudaErrorMapBufferObjectFailed = 14,
        cudaErrorUnmapBufferObjectFailed = 15,
        cudaErrorInvalidHostPointer = 16,
        cudaErrorInvalidDevicePointer = 17,
        cudaErrorInvalidTexture = 18,
        cudaErrorInvalidTextureBinding = 19,
        cudaErrorInvalidChannelDescriptor = 20,
        cudaErrorInvalidMemcpyDirection = 21,
        cudaErrorAddressOfConstant = 22,
        cudaErrorTextureFetchFailed = 23,
        cudaErrorTextureNotBound = 24,
        cudaErrorSynchronizationError = 25,
        cudaErrorInvalidFilterSetting = 26,
        cudaErrorInvalidNormSetting = 27,
        cudaErrorMixedDeviceExecution = 28,
        cudaErrorCudartUnloading = 29,
        cudaErrorUnknown = 30,
        cudaErrorNotYetImplemented = 31,
        cudaErrorMemoryValueTooLarge = 32,
        cudaErrorInvalidResourceHandle = 33,
        cudaErrorNotReady = 34,
        cudaErrorInsufficientDriver = 35,
        cudaErrorSetOnActiveProcess = 36,
        cudaError

    }
    public static class CudaErrorExtensions
    {
        /// <summary>
        /// Gets the name of the CudaError enum value.
        /// </summary>
        /// <param name="error">The CudaError enum value.</param>
        /// <returns>The name of the enum value as a string.</returns>
        public static string GetName(this CudaError error)
        {
            return Enum.GetName(typeof(CudaError), error) ?? "Unknown CudaError";
        }
    }
}