using Core.Exceptions;
using Core.FileSystem;
using Logging;
using ManagedCuda.NVRTC;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Core.Maths
{
    public class NvidiaCompiler
    {
        private static string BaseCudaPath = @"C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA";

        private static readonly object _LockObject = new object();
        private static string? _FilePath;
        private static Action? _Cleanup;
        static NvidiaCompiler()
        {
            Shutdown.ShutdownManager.Instance.Add(Cleanup, Shutdown.ShutdownOrder.UnzippedResource);
        }
        /*
        public static string Unpack()
        {
            lock (_LockObject)
            {
                if (_FilePath != null)
                {
                    if (File.Exists(_FilePath))
                    {
                        return _FilePath;
                    }
                }
                _Cleanup?.Invoke();
                var temporaryFile = new TemporaryFile(".exe", useLocalApplicationData: true);
                try
                {
                    _Cleanup = temporaryFile.Dispose;
                    File.WriteAllBytes(
                        temporaryFile.FilePath,
                        Environment.Is64BitOperatingSystem
                        ? ResourcesClang.LLVM_19_1_0_win64
                        : ResourcesClang.LLVM_19_1_0_win32
                    );
                    var fileInfo = new FileInfo(temporaryFile.FilePath);
                    fileInfo.Attributes |= FileAttributes.Normal;
                    File.SetAttributes(temporaryFile.FilePath, FileAttributes.Normal);
                    return temporaryFile.FilePath;
                }
                catch
                {
                    try { File.Delete(temporaryFile.FilePath); } catch { }
                    throw;
                }
            }
        }*/
        public static TemporaryFile PrecompileCuToSingleFile(
            string mainInputFilePath, string[] directoryPathsToInclude)
        {
            if (!File.Exists(mainInputFilePath))
                throw new FileNotFoundException($"{nameof(mainInputFilePath)} with value \"{mainInputFilePath}\" did not exist");
            TemporaryFile outputTemporaryFile = new TemporaryFile(".cu");
            try
            {
                StringBuilder sbArguments = new StringBuilder($" -x cu -E");
                sbArguments.Append($" -o \"{outputTemporaryFile.FilePath}\" \"{mainInputFilePath}\"");
                foreach (string directoryPathToInclude in directoryPathsToInclude)
                {
                    if (!Directory.Exists(directoryPathToInclude))
                        throw new DirectoryNotFoundException($"Directory in {nameof(directoryPathsToInclude)} with path \"{directoryPathToInclude}\" did not exist");
                    sbArguments.Append(" -I\"");
                    sbArguments.Append(directoryPathToInclude);
                    sbArguments.Append("\"");
                }
                Console.WriteLine(sbArguments.ToString());
                string nvccFilePath = Path.Combine(FindCudaDirectoryPath(), "bin", "nvcc.exe");
                if (!File.Exists(nvccFilePath))
                    throw new FileNotFoundException($"nvcc.exe was not found. It was expected at path \"{nvccFilePath}\". Please install NVIDIA software");
                ProcessStartInfo processInfo = new ProcessStartInfo
                {
                    FileName = nvccFilePath,
                    Arguments = sbArguments.ToString(),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (Process process = new Process())
                {
                    process.StartInfo = processInfo;
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        throw new OperationFailedException($"Pecompilation failed. Output: {output}\nError: {error}");
                    }
                }
                return outputTemporaryFile;
            }
            catch {
                outputTemporaryFile.Dispose();
                throw;
            }
        }
        public static TemporaryFile CompileRawCuUsingNVCC(string moduleCode)
        {
            using (TemporaryFile temporaryCuFile = new TemporaryFile(".cu"))
            {
                File.WriteAllText(temporaryCuFile.FilePath, moduleCode);
                return CompileCuFileToPtx(temporaryCuFile.FilePath);
            }
        }
        public static TemporaryFile CompileRawCuToPtx(string moduleCode)
        {
            try
            {
                return CompileRawCuStringToPtxUsingNVRTC(moduleCode);
            }
            catch (Exception exNVRTC)
            {
                string? nvrtcVersionMessage = TryGetNvrtcVersionMessage();
                string? installedVersionsMessage = TryGetInstalledVersionsMessage();

                try
                {
                    var temporaryFile = CompileRawCuUsingNVCC(moduleCode);
                    Console.WriteLine($"NVRTC compilation failed but managed to use NVCC instead. {nvrtcVersionMessage} {installedVersionsMessage} NormalizedError: {exNVRTC.Message}");
                    return temporaryFile;
                }
                catch (Exception exNVCC)
                {
                    string nvrtcMessage = $"NVRTC Compilation NormalizedError: {exNVRTC.Message}.";
                    if (nvrtcVersionMessage != null)
                        nvrtcMessage += $"{nvrtcVersionMessage} {installedVersionsMessage}";

                    throw new AggregateException(
                        "Failed to compile CUDA code using NVRTC and NVCC. " +
                        "Ensure the CUDA environment is correctly configured and accessible.",
                        new Exception[]
                        {
                    new OperationFailedException(nvrtcMessage, exNVRTC),
                    new OperationFailedException("NVCC Compilation NormalizedError: " + exNVCC.Message, exNVCC)
                        });
                }
            }
        }

        private static string TryGetNvrtcVersionMessage()
        {
            try
            {
                Version nvrtcVersion = GetNvrtcVersion();
                return nvrtcVersion != null
                    ? $"NVRTC required version: {nvrtcVersion}."
                    : "NVRTC version required could not be determined.";
            }
            catch
            {
                return "Count not determine NVRTC version required.";
            }
        }

        private static string TryGetInstalledVersionsMessage()
        {
            try
            {
                Tuple<Version, string>[] cudaVersions = GetCudaVersions();
                return cudaVersions != null && cudaVersions.Length > 0
                    ? $"Installed CUDA versions: {FormatCudaVersions(cudaVersions)}."
                    : "No installed CUDA versions detected.";
            }
            catch
            {
                return "Could not determine installed CUDA versions.";
            }
        }

        public static TemporaryFile CompileRawCuStringToPtxUsingNVRTC(string moduleCode)
        {
            try
            {
                var nvrtcVersion = GetNvrtcVersion();
                string cudaDirectory = FindCudaDirectoryPath(version => version.Major == nvrtcVersion.Major);
                string cudaBinDirectory = Path.Combine(cudaDirectory, "bin");
                if (!Directory.Exists(cudaBinDirectory))
                    throw new DirectoryNotFoundException($"There was no bin folder containing the cuda directories inside directory \"{cudaDirectory}\"");
                SetDllDirectory(cudaBinDirectory);
            }
            catch { }
            try
            {
                using (var nvrtc = new CudaRuntimeCompiler(moduleCode, "module.cu"))
                {
                    nvrtc.Compile(new string[]
                    {
                "--use_fast_math",               // Enable fast math optimizations
                "-lineinfo"                      // Include line info for debugging
                    }); // Compile the kernel source to PTX
                    string ptx = nvrtc.GetPTXAsString(); // Retrieve the compiled PTX
                    TemporaryFile temporaryFile = new TemporaryFile(".ptx");
                    File.WriteAllText(temporaryFile.FilePath, ptx);
                    return temporaryFile;
                }
            }
            catch (NVRTCException nvrtcException)
            {
                Logs.Default.Error(nvrtcException.NVRTCError);
                throw;
            }
        }
        public static TemporaryFile CompileCuFileToPtx(string inputCuFilePath)
        {
            TemporaryFile temporaryFile = new TemporaryFile(".ptx");
            CompileCuFileToPtx(inputCuFilePath, temporaryFile.FilePath);
            return temporaryFile;

        }
        public static void CompileCuFileToPtx(string inputCuFilePath, string outputPtxFilePath)
        {
            if (Path.GetExtension(inputCuFilePath) != ".cu")
                throw new ArgumentException($"Expected {nameof(inputCuFilePath)} to be a .cu file. Instead received \"{inputCuFilePath}\"");
            if (Path.GetExtension(outputPtxFilePath) != ".ptx")
                throw new ArgumentException($"Expected {nameof(outputPtxFilePath)} to be a .ptx file. Instead received \"{outputPtxFilePath}\"");
            if (!File.Exists(inputCuFilePath))
                throw new FileNotFoundException(inputCuFilePath);
            string arguments = $"-ptx \"{inputCuFilePath}\" -o \"{outputPtxFilePath}\"";
            string nvccFilePath = Path.Combine(FindCudaDirectoryPath(), "bin", "nvcc.exe");
            if (!File.Exists(nvccFilePath))
                throw new FileNotFoundException($"nvcc.exe was not found. It was expected at path \"{nvccFilePath}\". Please install NVIDIA software");
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = nvccFilePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (Process process = new Process())
            {
                process.StartInfo = processInfo;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new OperationFailedException($"NVCC compilation failed. Output: {output}\nError: {error}");
                }
            }
        }
        private static void Cleanup()
        {
            lock (_LockObject)
            {
                _Cleanup?.Invoke();
                _Cleanup = null;
            }
        }
        /// <summary>
        /// Gets all valid CUDA versions and their paths.
        /// </summary>
        /// <returns>Array of tuples with version and corresponding directory path.</returns>
        public static Tuple<Version, string>[] GetCudaVersions()
        {
            // Ensure the base directory exists
            if (!Directory.Exists(BaseCudaPath))
            {
                throw new DirectoryNotFoundException($"CUDA base directory not found: {BaseCudaPath}");
            }

            // Scan child directories for versioned folders (e.g., v12.6, v11.8)
            return Directory.GetDirectories(BaseCudaPath)
                .Select(path => new
                {
                    Name = Path.GetFileName(path),
                    Path = path,
                    Version = GetVersionFromFolder(Path.GetFileName(path))
                })
                .Where(x => x.Version != null) // Keep only valid version folders
                .Select(x => Tuple.Create(x.Version, x.Path))
                .ToArray();
        }

        /// <summary>
        /// Extracts a valid Version object from a folder name (e.g., "v12.6").
        /// </summary>
        private static Version GetVersionFromFolder(string folderName)
        {
            if (folderName.StartsWith("v") && Version.TryParse(folderName.Substring(1), out var version))
            {
                return version;
            }
            return null;
        }
        /// <summary>
        /// Finds the CUDA directory path based on the provided predicate or selects the latest version by default.
        /// </summary>
        /// <param name="predicate">Optional predicate to filter versions. Defaults to selecting the latest version.</param>
        /// <returns>Path to the selected CUDA version directory.</returns>
        public static string FindCudaDirectoryPath(Func<Version, bool> predicate = null)
        {
            var cudaVersions = GetCudaVersions();

            if (cudaVersions.Length == 0)
            {
                throw new DirectoryNotFoundException($"No valid CUDA versions found in {BaseCudaPath}");
            }

            // Apply the predicate or pick the latest version
            var selectedVersion = predicate == null
                ? cudaVersions.OrderByDescending(tuple => tuple.Item1).FirstOrDefault()
                : cudaVersions.Where(tuple => predicate(tuple.Item1)).OrderByDescending(tuple => tuple.Item1).FirstOrDefault();

            if (selectedVersion == null)
            {
                throw new InvalidOperationException($"No CUDA version matches the specified criteria.");
            }

            return selectedVersion.Item2;
        }
        /// <summary>
        /// Formats the list of installed CUDA versions for display in logs or error messages.
        /// </summary>
        /// <param name="cudaVersions">Array of installed CUDA versions and their paths.</param>
        /// <returns>Formatted string representation of CUDA versions.</returns>
        private static string FormatCudaVersions(Tuple<Version, string>[] cudaVersions)
        {
            if (cudaVersions == null || cudaVersions.Length == 0)
            {
                return "No installed CUDA versions detected.";
            }

            return string.Join(", ", cudaVersions.Select(v => $"{v.Item1} ({v.Item2})"));
        }
        private static Version GetNvrtcVersion() { 
        
            Assembly nvrtcAssembly  = typeof(ManagedCuda.NVRTC.CudaRuntimeCompiler).Assembly;
            return nvrtcAssembly.GetName().Version!;
        }
        [DllImport("kernel32", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetLastError();
    }
}
