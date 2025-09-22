namespace CoreTests
{
    using NUnit.Framework;
    using Core.Maths;
    using Core.FileSystem;
    using Core.Maths.BlockOperationMatrices;
    using InfernoDispatcher;
    using System.Diagnostics;
    using Core.Cleanup;
    using Core.Maths.CUBLAS;

    namespace CoreTests.Maths
    {
        [TestFixture]
        public class MatrixHelper_CUBLAS_Tests
        {
            [Test]
            public void TestMatrixMultiplyWithGPUColumnMajor()
            {
                using (CudaContextAssignedThreadPool cudaThreadPool = new CudaContextAssignedThreadPool(1))
                {
                    // Arrange: Known input matrices
                    double[] matrixA = {
                1, 2, 3,
                4, 5, 6,
                7, 8, 9
            }; // 3x3 matrix
                    double[] matrixB = {
                9, 8, 7,
                6, 5, 4,
                3, 2, 1
            }; // 3x3 matrix

                    int rowsA = 3, colsA = 3, colsB = 3;

                    // Expected result of multiplying matrixA * matrixB
                    double[] expectedMatrixC = {
                30, 24, 18,
                84, 69, 54,
                138, 114, 90
            };

                    // Act: Perform matrix multiplication
                    double[] result = MatrixHelper.MatrixMultiplyWithGPUColumnMajor(matrixA, matrixB, rowsA, colsA, colsB, cudaThreadPool).Wait();

                    // Assert: Check if the result matches the expected matrix
                    Assert.AreEqual(expectedMatrixC, result);
                }
            }

            [Test]
            public void TestMatrixInverseWithGPUColumnMajor()
            {
                using (var threadPool = new CusolverContextAssignedThreadPool(1))
                {
                    // Arrange: A known 2x2 matrix
                    double[] matrix = {
                4, 7,
                2, 6
            };
                    int size = 2;

                    // The expected inverse of the matrix
                    double[] expectedInverse = {
                0.6, -0.7,
                -0.2, 0.4
            };

                    // Act: Perform matrix inversion
                    double[] result = MatrixHelper.MatrixInverseWithGPUColumnMajor(matrix, size, threadPool).Wait();

                    // Assert: Check if the result matches the expected inverse
                    Assert.That(result, Is.EqualTo(expectedInverse).Within(1e-5), "Matrix inversion result did not match expected.");
                }
            }

            [Test]
            public void TestMatrixMultiplyWithInvalidDimensions_ThrowsException()
            {
                using (CudaContextAssignedThreadPool cudaThreadPool = new CudaContextAssignedThreadPool(1))
                {
                    // Arrange: Mismatched matrices
                    double[] matrixA = {
                1, 2, 3,
                4, 5, 6
            }; // 2x3 matrix
                    double[] matrixB = {
                1, 2,
                3, 4
            }; // 2x2 matrix

                    int rowsA = 2, colsA = 3, colsB = 2;

                    // Act & Assert: Expect an exception due to incompatible matrix dimensions
                    Assert.Throws<Exception>(() =>
                        MatrixHelper.MatrixMultiplyWithGPUColumnMajor(matrixA, matrixB, rowsA, colsA, colsB, cudaThreadPool).Wait(),
                        "Expected an exception due to dimension mismatch, but none was thrown."
                    );
                }
            }

            [Test]
            public void TestMatrixInverseWithNonSquareMatrix_ThrowsException()
            {
                // Arrange: Non-square matrix (3x2)
                using (var threadPool = new CusolverContextAssignedThreadPool(1))
                {
                    double[] matrix = {
                1, 2,
                3, 4,
                5, 6
            };
                    int size = 2; // Incorrect size for a non-square matrix

                    // Act & Assert: Expect an exception because the matrix is not square
                    Assert.Throws<Exception>(() =>
                        MatrixHelper.MatrixInverseWithGPUColumnMajor(matrix, size, threadPool).Wait(),
                        "Expected an exception due to non-square matrix, but none was thrown."
                    );
                }
            }
        }
    }
}
