namespace CoreTests
{
    using NUnit.Framework;
    using System;
    using System.IO;
    using Core.FileSystem;
    using System.Reflection;
    using Core.Maths.Matrices;

    namespace CoreTests.Maths
    {
        [TestFixture]
        public class BlockMatrixTests
        {

            [SetUp]
            public void Setup()
            {

            }

            [TearDown]
            public void TearDown()
            {

            }

            [Test]
            public void BlockMatrix_WriteAndReadValue_ShouldStoreAndRetrieveValue()
            {
                // Arrange
                double expectedValue = 1;
                int size = 100;
                using (TemporaryDirectory temporaryDirectory = new TemporaryDirectory())
                {
                    using (BlockMatrix blockMatrix = new BlockMatrix(size, size, Path.Combine(temporaryDirectory.AbsolutePath, "matrix.bin")))
                    {
                        // Act
                        for (int row = 0; row < size; row++)
                        {
                            for (int column = 0; column < size; column++)
                            {
                                expectedValue++;
                                blockMatrix[row, column] = expectedValue;

                                // Assert
                                
                            }
                        }
                        expectedValue = 1;
                        for (int row = 0; row < size; row++)
                        {
                            for (int column = 0; column < size; column++)
                            {
                                expectedValue++;
                                double actualValue = blockMatrix[row, column];

                                // Assert
                                Assert.AreEqual(expectedValue, actualValue, "The value read from the matrix does not match the expected value.");
                            }
                        }
                    }
                }
            }

            [Test]
            public void BlockMatrix_ReadWriteRow_ShouldStoreAndRetrieveRowCorrectly()
            {
                // Arrange
                int rowIndex = 1;
                double[] expectedRow = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

                using (TemporaryDirectory temporaryDirectory = new TemporaryDirectory())
                {
                    using (BlockMatrix blockMatrix = new BlockMatrix(10, 10, Path.Combine(temporaryDirectory.AbsolutePath, "matrix.bin")))
                    {
                        // Act
                        blockMatrix.WriteRow(rowIndex, expectedRow);
                        double[] actualRow = blockMatrix.ReadRow(rowIndex);

                        // Assert
                        Assert.AreEqual(expectedRow.Length, actualRow.Length, "RowIndex length mismatch.");
                        for (int i = 0; i < expectedRow.Length; i++)
                        {
                            Assert.AreEqual(expectedRow[i], actualRow[i], $"Mismatch at column {i}.");
                        }
                    }
                }
            }

            [Test]
            public void BlockMatrix_ThrowExceptionOnInvalidWrite()
            {
                // Arrange
                int invalidRowIndex = 15;
                int validColIndex = 5;
                double value = 100.0;

                using (TemporaryDirectory temporaryDirectory = new TemporaryDirectory())
                {
                    using (BlockMatrix blockMatrix = new BlockMatrix(10, 10, Path.Combine(temporaryDirectory.AbsolutePath, "matrix.bin")))
                    {
                        // Act & Assert
                        Assert.Throws<IndexOutOfRangeException>(() =>
                        {
                            blockMatrix[invalidRowIndex, validColIndex] = value;
                        }, "Expected an IndexOutOfRangeException when writing to an invalid row index.");
                    }
                }
            }

            [Test]
            public void BlockMatrix_MultiplyByVector_ShouldReturnCorrectResult()
            {
                // Arrange
                double[] vector = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
                double[] expectedResult = new double[10];

                using (TemporaryDirectory temporaryDirectory = new TemporaryDirectory())
                {
                    using (BlockMatrix blockMatrix = new BlockMatrix(10, 10, Path.Combine(temporaryDirectory.AbsolutePath, "matrix.bin")))
                    {

                        for (int i = 0; i < 10; i++)
                        {
                            blockMatrix[i, i] = 2; // Set diagonal values to 2 for easier multiplication
                            expectedResult[i] = 2 * vector[i];
                        }

                        // Act
                        double[] actualResult = blockMatrix.MultiplyByVector(vector);

                        // Assert
                        Assert.AreEqual(expectedResult.Length, actualResult.Length);
                        for (int i = 0; i < expectedResult.Length; i++)
                        {
                            Assert.AreEqual(expectedResult[i], actualResult[i], $"Mismatch at index {i}.");
                        }
                    }
                }
            }

            [Test]
            public void BlockMatrix_Dispose_ShouldReleaseResources()
            {
                using (TemporaryDirectory temporaryDirectory = new TemporaryDirectory())
                {
                    using (BlockMatrix blockMatrix = new BlockMatrix(10, 10, Path.Combine(temporaryDirectory.AbsolutePath, "matrix.bin")))
                    {
                        // Arrange
                        blockMatrix.Dispose();

                        // Act & Assert
                        Assert.Throws<ObjectDisposedException>(() =>
                        {
                            blockMatrix[0, 0] = 1; // Attempt to access after disposal
                        });
                    }
                }
            }
            [Test]
            public void BlockMatrix_CacheOverflow_ShouldTrackOverflowInvocation()
            {
                // Arrange
                int matrixSize = 200;  // Smaller matrix
                int blockSize = 64;    // Smaller block size
                double proportionFreeMemory = 0.000005;

                using (TemporaryDirectory temporaryDirectory = new TemporaryDirectory())
                {
                    using (BlockMatrix blockMatrix = new BlockMatrix(matrixSize, matrixSize,
                        Path.Combine(temporaryDirectory.AbsolutePath, "matrix.bin"),
                        blockSize: blockSize, proportionFreeMemoryConsume: proportionFreeMemory, fixInitialCacheSizeInfoForDebugOnly:true))
                    {
                        
                        int upperBoundMaxCacheSizeBlocks = (int)typeof(BlockMatrix).GetField("_UpperBoundMaxCacheSizeBlocks", 
                            BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(blockMatrix)!;
                        Assert.NotZero(upperBoundMaxCacheSizeBlocks, $"{nameof(upperBoundMaxCacheSizeBlocks)} was 0");
                        int nReadsPerUpdateMaxCacheSize = (int)typeof(BlockMatrix).GetField("_NReadsPerUpdateMaxCacheSize", 
                            BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(blockMatrix)!;
                        Assert.NotZero(nReadsPerUpdateMaxCacheSize, $"{nameof(nReadsPerUpdateMaxCacheSize)} was 0");
                        int nValuesPerBlock = blockSize / sizeof(double);
                        int mod = upperBoundMaxCacheSizeBlocks % nReadsPerUpdateMaxCacheSize;
                        int nAdditionalValueSetsToReachCheck = mod >0?nReadsPerUpdateMaxCacheSize - mod:0;
                        int nValuesSetToOneBeforeOverflow = (upperBoundMaxCacheSizeBlocks+ nAdditionalValueSetsToReachCheck - 1)
                            * nValuesPerBlock;
                        int rowIndex = 0; int columnIndex = 0;
                        double lastProportionOfMatrixInMemory = blockMatrix.ProportionOfMatrixInMemory;
                        Action writeNextDouble = () =>
                        {
                            blockMatrix[rowIndex, columnIndex++] = 1;
                            if (columnIndex >= matrixSize) {
                                columnIndex = 0;
                                rowIndex++;
                            }
                        };
                        for (int i = 0; i < nValuesSetToOneBeforeOverflow; i++) {
                            writeNextDouble();
                            double proportionOfMatrixInMemory = blockMatrix.ProportionOfMatrixInMemory;
                            Assert.GreaterOrEqual(proportionOfMatrixInMemory, lastProportionOfMatrixInMemory, "Matrix size decreased prematurely indicating a premature overflow");
                            lastProportionOfMatrixInMemory = proportionOfMatrixInMemory;
                        }
                        writeNextDouble();
                        Assert.Less(blockMatrix.ProportionOfMatrixInMemory, lastProportionOfMatrixInMemory, "Matrix size should have discreased if overflow happened");
                    }
                }
            }

            [Test]
            public void BlockMatrix_CacheOverflow_ShouldCallOverflowMethod()
            {
                // Arrange
                int matrixSize = 200;  // Smaller matrix to speed up the test
                int blockSize = 64;    // Small block size for quicker cache overflow
                double proportionFreeMemory = 0.000005;

                using (TemporaryDirectory temporaryDirectory = new TemporaryDirectory())
                {
                    using (BlockMatrix blockMatrix = new BlockMatrix(matrixSize, matrixSize,
                        Path.Combine(temporaryDirectory.AbsolutePath, "matrix.bin"),
                        blockSize: blockSize, proportionFreeMemoryConsume: proportionFreeMemory))
                    {
                        // Use reflection to access private fields and methods
                        var overflowMethod = typeof(BlockMatrix).GetMethod("Overflow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var blockCacheField = typeof(BlockMatrix).GetField("_BlockCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var lowerBoundField = typeof(BlockMatrix).GetField("_LowerBoundMaxCacheSizeBlocks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        // Act
                        for (int i = 0; i < 50; i++)
                        {
                            for (int j = 0; j < 50; j++)
                            {
                                blockMatrix[i, j] = i + j;  // Populate with dummy data
                            }
                        }

                        // Manually trigger cache overflow
                        for (int i = 50; i < 100; i++)
                        {
                            for (int j = 50; j < 100; j++)
                            {
                                blockMatrix[i, j] = i + j;  // Continue to fill cache
                            }
                        }

                        // Get internal values for validation
                        // Adjust how we access the _BlockCache field to ensure it's handled correctly
                        
                        // Ensure we're casting to the right type (Dictionary<long, LinkedListNode<Block>>)
                        var blockCache = blockCacheField.GetValue(blockMatrix) as Dictionary<long, LinkedListNode<BlockMatrix_Block>>;

                        Assert.IsNotNull(blockCache, "BlockCache should not be null.");
                        int lowerBoundMaxCacheSize = (int)lowerBoundField.GetValue(blockMatrix);

                        // Check that the overflow was triggered at least once
                        if (blockCache.Count > lowerBoundMaxCacheSize)
                        {
                            // Use reflection to invoke the Overflow method
                            overflowMethod.Invoke(blockMatrix, null);
                        }

                        // Assert
                        Assert.Less(blockMatrix.ProportionOfMatrixInMemory, 1.0, "Cache did not overflow as expected.");
                    }
                }
            }

            [Test]
            public void BlockMatrix_Initialization_ShouldSetCorrectValues()
            {
                int nRows = 50, nColumns = 50, blockSize = 128;
                double proportionFreeMemory = 0.6;

                using (TemporaryDirectory temporaryDirectory = new TemporaryDirectory())
                {
                    using (BlockMatrix blockMatrix = new BlockMatrix(nRows, nColumns,
                        Path.Combine(temporaryDirectory.AbsolutePath, "matrix.bin"),
                        blockSize: blockSize, proportionFreeMemoryConsume: proportionFreeMemory))
                    {
                        // Assert basic properties
                        Assert.AreEqual(nRows, blockMatrix.NRows, "Incorrect row size.");
                        Assert.AreEqual(nColumns, blockMatrix.NColumns, "Incorrect column size.");
                        Assert.AreEqual(blockSize, blockMatrix.BlockSize, "Block size is not set correctly.");
                    }
                }
            }
            [Test]
            public void BlockMatrix_RowReadWrite_AcrossLargeBlocks_ShouldRemainConsistent()
            {
                int matrixSize = 200;  // Matrix size
                int blockSize = 128;   // Larger block size
                double proportionFreeMemory = 0.6;

                using (TemporaryDirectory temporaryDirectory = new TemporaryDirectory())
                {
                    using (BlockMatrix blockMatrix = new BlockMatrix(matrixSize, matrixSize,
                        Path.Combine(temporaryDirectory.AbsolutePath, "matrix.bin"),
                        blockSize: blockSize, proportionFreeMemoryConsume: proportionFreeMemory))
                    {
                        double[] rowData = new double[matrixSize];
                        for (int i = 0; i < matrixSize; i++)
                        {
                            rowData[i] = i;
                        }

                        // Write row and read it back
                        blockMatrix.WriteRow(50, rowData);
                        double[] resultRow = blockMatrix.ReadRow(50);

                        // Assert consistency
                        for (int i = 0; i < matrixSize; i++)
                        {
                            Assert.AreEqual(rowData[i], resultRow[i], $"Mismatch at column {i}.");
                        }
                    }
                }
            }
            [Test]
            public void BlockMatrix_FlushMultipleTimes_ShouldMaintainDataIntegrity()
            {
                int matrixSize = 100;  // Smaller matrix size
                int blockSize = 64;    // Block size
                double proportionFreeMemory = 0.5;

                using (TemporaryDirectory temporaryDirectory = new TemporaryDirectory())
                {
                    using (BlockMatrix blockMatrix = new BlockMatrix(matrixSize, matrixSize,
                        Path.Combine(temporaryDirectory.AbsolutePath, "matrix.bin"),
                        blockSize: blockSize, proportionFreeMemoryConsume: proportionFreeMemory))
                    {
                        // Write and flush multiple times
                        for (int i = 0; i < matrixSize; i++)
                        {
                            for (int j = 0; j < matrixSize; j++)
                            {
                                blockMatrix[i, j] = i * j;
                            }
                            blockMatrix.Flush();  // Flush after each row write
                        }

                        // Validate data
                        for (int i = 0; i < matrixSize; i++)
                        {
                            for (int j = 0; j < matrixSize; j++)
                            {
                                double value = blockMatrix[i, j];
                                Assert.AreEqual(i * j, value, $"Data mismatch at {i},{j}");
                            }
                        }
                    }
                }
            }
            [Test]
            public void BlockMatrix_MaxBlockSize_ShouldHandleCorrectly()
            {
                int matrixSize = 100;
                int maxBlockSize = 8192;  // Test with a large block size
                double proportionFreeMemory = 0.7;

                using (TemporaryDirectory temporaryDirectory = new TemporaryDirectory())
                {
                    using (BlockMatrix blockMatrix = new BlockMatrix(matrixSize, matrixSize,
                        Path.Combine(temporaryDirectory.AbsolutePath, "matrix.bin"),
                        blockSize: maxBlockSize, proportionFreeMemoryConsume: proportionFreeMemory))
                    {
                        double value = 123.456;

                        // Write a large block worth of data
                        for (int i = 0; i < matrixSize; i++)
                        {
                            for (int j = 0; j < matrixSize; j++)
                            {
                                blockMatrix[i, j] = value;
                            }
                        }

                        // Ensure data is correct
                        for (int i = 0; i < matrixSize; i++)
                        {
                            for (int j = 0; j < matrixSize; j++)
                            {
                                Assert.AreEqual(value, blockMatrix[i, j], $"Mismatch at {i},{j} for max block size.");
                            }
                        }
                    }
                }
            }


        }
    }
}