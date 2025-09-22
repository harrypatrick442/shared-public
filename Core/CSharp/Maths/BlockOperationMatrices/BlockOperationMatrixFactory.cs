using System.IO;
using System;
using Core.Threading;
using Core.FileSystem;
using Core.MemoryManagement;
using System.Diagnostics;
using Core.Pool;
using Core.Maths.Matrices;


namespace Core.Maths.BlockOperationMatrices
{
    public static class BlockOperationMatrixFactory
    {
        // Method to create a BlockOperationMatrixPartitioned directly from double[][]
        /*public static BlockOperationMatrixPartitioned CreatePartitionedMatrixFromData(
            double[][] fullMatrix,
            WorkingDirectoryManager workingDirectoryManager,
            string matrixName)
        {
            // Get the dimensions of the full matrix
            int numRows = fullMatrix.Length;
            int numCols = fullMatrix[0].Length;

            // Split into 4 submatrices (Top-Left, Top-Right, Bottom-Left, Bottom-Right)
            int midRow = numRows / 2;
            int midCol = numCols / 2;

            double[][] topLeft = ExtractSubMatrix(fullMatrix, 0, midRow, 0, midCol);
            double[][] topRight = ExtractSubMatrix(fullMatrix, 0, midRow, midCol, numCols);
            double[][] bottomLeft = ExtractSubMatrix(fullMatrix, midRow, numRows, 0, midCol);
            double[][] bottomRight = ExtractSubMatrix(fullMatrix, midRow, numRows, midCol, numCols);

            // Create BlockOperationMatrixNonPartitioned for each submatrix
            var blockMatrixTopLeft = new BlockOperationMatrixNonPartitionedFromData(topLeft.Length, topLeft[0].Length, workingDirectoryManager, topLeft);
            var blockMatrixTopRight = new BlockOperationMatrixNonPartitionedFromData(topRight.Length, topRight[0].Length, workingDirectoryManager, topRight);
            var blockMatrixBottomLeft = new BlockOperationMatrixNonPartitionedFromData(bottomLeft.Length, bottomLeft[0].Length, workingDirectoryManager, bottomLeft);
            var blockMatrixBottomRight = new BlockOperationMatrixNonPartitionedFromData(bottomRight.Length, bottomRight[0].Length, workingDirectoryManager, bottomRight);

            // Combine into a BlockOperationMatrixPartitioned
            var partitionedMatrix = new BlockOperationMatrixPartitioned(
                blockMatrixTopLeft,
                blockMatrixTopRight,
                blockMatrixBottomLeft,
                blockMatrixBottomRight,
                workingDirectoryManager,
                matrixName
            );

            return partitionedMatrix;
        }*/
        public static BlockOperationMatrix FromForInversion(
            IBigMatrix bigMatrix,
            double proportionFreeMemoryToUse,
            WorkingDirectoryManager workingDirectoryManager, out int padding, out int nPartitions,
            GPUMathsParameters? gpuMathsParameters,
            MathsRunningMode runningModeForInversion, 
            CompositeProgressHandler? parentProgressHandler,
            bool verbose = true)
        {
            StandardProgressHandler progressHandler = new StandardProgressHandler();
            parentProgressHandler?.AddChild(progressHandler);
            using (verbose ? progressHandler.RegisterPrintPercentSameLine("Creating block _Matrix progress: ") : null)
            {
                if (bigMatrix.NRows != bigMatrix.NColumns)
                    throw new Exception($"_Matrix was not square");
                if (proportionFreeMemoryToUse <= 0)
                    throw new Exception($"{nameof(proportionFreeMemoryToUse)} cannot be less than or equal to zero");
                if (proportionFreeMemoryToUse > 1) proportionFreeMemoryToUse = 1;
                MemoryMetrics memoryMetrics = MemoryHelper.GetMemoryMetricsNow();
                int size = bigMatrix.NRows;
                long memoryAvailableRam = (long)Math.Floor(((double)memoryMetrics.FreeMb) * 1000000d * proportionFreeMemoryToUse);
                long memoryAvailableGPU = -1;
                bool usingGPU = false;
                switch (runningModeForInversion)
                {
                    case MathsRunningMode.GpuOnly:
                    case MathsRunningMode.Whatever:
                        usingGPU = true;
                        memoryAvailableGPU = (long)(MemoryHelper.GetGPUMemoryMetrics().Free * gpuMathsParameters.MaxProportionMemoryCanUse);
                        break;
                    case MathsRunningMode.CpuOnly:
                        break;
                    default:
                        throw new UnreachableException();
                }
                nPartitions = 0;
                while (true)
                {
                    int partitionSize = (int)((double)size / Math.Pow(2, nPartitions));
                    bool partitionFitsIntoByteArray = partitionSize * partitionSize * sizeof(double) < int.MaxValue;
                    if (partitionFitsIntoByteArray)
                    {
                        if (usingGPU)
                        {
                            long cpuOrGPUMemoryRequired = EstimateCPUOrGPUMemoryRequiredForGPUFromPartitionSize(partitionSize);

                            if (cpuOrGPUMemoryRequired < memoryAvailableGPU
                                && cpuOrGPUMemoryRequired < memoryAvailableRam)
                            {
                                break;
                            }
                        }
                        else
                        {
                            long memoryRequiredForCPUOnly = EstimateMemoryRequiredForCPUFromPartitionSize(partitionSize);
                            if (memoryRequiredForCPUOnly < memoryAvailableRam)
                            {
                                break;
                            }
                        }
                    }
                    nPartitions++;
                }
                Console.WriteLine($"Using {nPartitions} partitions");
                //if (size>100&nPartitions < 3) nPartitions = 4;
                padding = CalculatePadding(size, nPartitions);
                if (((double)(size + padding)) / Math.Pow(2, nPartitions) <= padding)
                {
                    throw new Exception("Something went wrong. Padding cannot be greater or equal to the size of the smallest partition");
                }
                double nPartitionsSquared = Math.Pow(nPartitions, 3);
                int nPartitionsCreated = 0;
                Action createdPartition = () => progressHandler.Set((double)++nPartitionsCreated / nPartitionsSquared);
                var result =  BlockOperationMatrixFactory.CreateRecursively(
                    size + padding, padding, padding, 0, 0,
                    nPartitions, bigMatrix, workingDirectoryManager,
                    createdPartition
                );
                progressHandler.Set(1);
                return result;
            }
        }
        private static long EstimateCPUOrGPUMemoryRequiredForGPUFromPartitionSize(int partitionSize) {
            long matrixMultiplyGPU = MatrixHelper.EstimateGPUOrCPUMemoryForMatrixMultiplyWithGPUColumnMajor(partitionSize, partitionSize, partitionSize, true);
            long matrixInverseGPU = MatrixHelper.EstimateCPUOrGPUMemoryForMatrixInverseWithGPUColumnMajor(partitionSize, true);
            return (long)Math.Max(matrixMultiplyGPU, matrixInverseGPU);
        }
        private static long EstimateMemoryRequiredForCPUFromPartitionSize(int partitionSize) {
            long matrixMultiplyCPU = MatrixHelper.EstimateMemoryForMultiplyMultipleThreads(partitionSize, partitionSize, partitionSize, partitionSize, Environment.ProcessorCount);
            long matrixInverseCPU = MatrixHelper.EstimateMemoryForInvert(partitionSize);
            return (long)Math.Max(matrixInverseCPU, matrixMultiplyCPU);
        }
        public static BlockOperationMatrix CreateRecursively(int size,
            int paddingBottom, int paddingRight,
            int offsetTop, int offsetLeft, int nMorePartitions,
            IBigMatrix bigMatrix,
            WorkingDirectoryManager workingDirectoryManager,
            Action createdPartition)
        {
            bool partitioned = nMorePartitions > 0;
            if (!partitioned)
            {
                int originalRows = size - paddingBottom;
                int originalColumns = size - paddingRight;
                byte[] bytes = bigMatrix.ReadBlockBytes(originalRows, originalColumns, offsetTop, offsetLeft);
                if (paddingRight > 0||paddingBottom>0)
                {
                    bytes = CopyUnpaddedToPadded(bytes, originalRows, originalColumns, paddingRight, paddingBottom);
                }
               // double[][] doubles = ConvertByteArrayToDoubleJaggedArray(bytes, nRows, nColumns);
                var nonPartitioned =  new BlockOperationMatrixNonPartitioned(size, size,
                    workingDirectoryManager, bytes);
                createdPartition();
                return nonPartitioned;
            }
            nMorePartitions--;
            int halfSize = size / 2;
            if (halfSize < 1) throw new Exception("Something went very wrong");
            var childTopLeftA = BlockOperationMatrixFactory.CreateRecursively(
                halfSize, 0, 0, offsetTop, offsetLeft, nMorePartitions,
                bigMatrix, workingDirectoryManager,
                createdPartition
            );
            var childTopRightB = BlockOperationMatrixFactory.CreateRecursively(
                halfSize, 0, paddingRight,  offsetTop, offsetLeft + halfSize,
                nMorePartitions,
                bigMatrix, workingDirectoryManager,
                createdPartition
            );
            var childBottomLeftC = BlockOperationMatrixFactory.CreateRecursively(
                halfSize, paddingBottom, 0, offsetTop + halfSize, offsetLeft, nMorePartitions,
                bigMatrix, workingDirectoryManager,
                createdPartition
            );
            var childBottomRightD = BlockOperationMatrixFactory.CreateRecursively(
                halfSize, paddingBottom, paddingRight, offsetTop + halfSize,
                offsetLeft + halfSize, nMorePartitions,
                bigMatrix, workingDirectoryManager,
                createdPartition
            );
            var ret = new BlockOperationMatrixPartitioned(
                childTopLeftA,
                childTopRightB,
                childBottomLeftC,
                childBottomRightD,
                workingDirectoryManager
            );
            return ret;
        }
        public static byte[] CopyUnpaddedToPadded(byte[] unpaddedMatrix, int originalRows, int originalCols, int paddingRight, int paddingBottom)
        {
            // Calculate sizes
            int originalBytesPerRow = originalCols * sizeof(double);
            int paddedCols = originalCols + paddingRight;
            int paddedRows = originalRows + paddingBottom;
            int paddedBytesPerRow = paddedCols * sizeof(double);

            // Create the padded matrix byte array
            byte[] paddedMatrix = new byte[paddedRows * paddedBytesPerRow];

            // Copy each row from the unpadded matrix into the padded matrix
            for (int row = 0; row < originalRows; row++)
            {
                // Calculate the position in the unpadded matrix
                int unpaddedOffset = row * originalBytesPerRow;

                // Calculate the position in the padded matrix
                int paddedOffset = row * paddedBytesPerRow;

                // Copy the unpadded row into the padded matrix row (excluding paddingRight)
                Buffer.BlockCopy(unpaddedMatrix, unpaddedOffset, paddedMatrix, paddedOffset, originalBytesPerRow);
            }

            // At this point, the unpadded matrix has been copied into the top-left of the padded matrix
            // and the extra space on the right and bottom remains filled with zeros (default for new byte[])


            // Insert identity values (1s) on the diagonal in the padded region
            for (int i = 0; i < Math.Min(paddingBottom, paddingRight); i++)
            {
                int rowIndex = originalRows + i;
                int colIndex = originalCols + i;

                // Find the position in the byte array (rowIndex * paddedBytesPerRow + colIndex * sizeof(double))
                int paddedOffset = rowIndex * paddedBytesPerRow + colIndex * sizeof(double);

                // Insert the value '1' as a double in the byte array
                byte[] identityValue = BitConverter.GetBytes(1.0);
                Buffer.BlockCopy(identityValue, 0, paddedMatrix, paddedOffset, sizeof(double));
            }
            return paddedMatrix;
        }

        public static int CalculatePadding(int matrixSize, int partitionLevels)
        {
            // Calculate the divisor based on the number of partition levels (2^L)
            int divisor = (int)Math.Pow(2, partitionLevels);

            // Find the next number that is divisible by divisor and greater than or equal to matrixSize
            int paddedSize = ((matrixSize + divisor - 1) / divisor) * divisor;

            // Calculate the padding required
            int paddingRequired = paddedSize - matrixSize;

            return paddingRequired;
        }

        private static double[][] ExtractSubMatrix(double[][] matrix, int rowStart, int rowEnd, int colStart, int colEnd)
        {
            int numRows = rowEnd - rowStart;
            int numCols = colEnd - colStart;

            double[][] subMatrix = new double[numRows][];

            for (int i = 0; i < numRows; i++)
            {
                subMatrix[i] = new double[numCols];
                Array.Copy(matrix[rowStart + i], colStart, subMatrix[i], 0, numCols);
            }

            return subMatrix;
        }
        public static double[][] ConvertByteArrayToDoubleJaggedArray(byte[] byteArray, int nRows, int nColumns)
        {
            // Initialize the jagged array with the specified number of rows and columns
            double[][] result = new double[nRows][];

            int byteIndex = 0;

            // Iterate through the rows and columns
            for (int i = 0; i < nRows; i++)
            {
                result[i] = new double[nColumns]; // Create the row

                for (int j = 0; j < nColumns; j++)
                {
                    // Convert 8 bytes from the byte array to a double
                    result[i][j] = BitConverter.ToDouble(byteArray, byteIndex);

                    // Move to the next set of bytes (since each double takes 8 bytes)
                    byteIndex += sizeof(double);
                }
            }

            return result;
        }
    }
}
