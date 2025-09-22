using Core.Maths.Matrices;
using InfernoDispatcher;
using InfernoDispatcher.Tasks;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Math = System.Math;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
namespace Core.Maths
{
    public partial class MatrixHelper
    {

        public static double[][] Create(int rows, int cols)
        {
            double[][] result = new double[rows][];
            for (int i = 0; i < rows; ++i)
                result[i] = new double[cols];
            return result;
        }

        public static double[][] Scale(double[][] a, double scale)
        {
            double[][] b = new double[a.Length][];
            int nRows = a.Length;
            int nCols = a[0].Length;
            for (int i = 0; i < nRows; i++)
            {
                double[] bRow = new double[nCols];
                b[i] = bRow;
                for (int j = 0; j < nCols; j++)
                {
                    bRow[j] = a[i][j] * scale;
                }
            }
            return b;
        }
        public static double[][] Identity(int n)
        {
            // return an n x n Identity matrix
            double[][] result = Create(n, n);
            for (int i = 0; i < n; ++i)
                result[i][i] = 1.0;

            return result;
        }
        public static double[][] Add(double[][] a, double[][] b)
        {
            int nRows = a.Length;
            int nColumns = a[0].Length;
            double[][] c = new double[nColumns][];
            for (int row = 0; row < nRows; row++)
            {
                double[] aRow = a[row];
                double[] bRow = b[row];
                double[] cRow = new double[nColumns];
                c[row] = cRow;
                for (int column = 0; column < nColumns; column++)
                {
                    cRow[column] = aRow[column] + bRow[column];
                }
            }
            return c;
        }
        /// <summary>
        /// Multiplies two matrices.
        /// </summary>
        /// <param name="matrixA">The first matrix.</param>
        /// <param name="matrixB">The second matrix.</param>
        /// <returns>The result of the matrix multiplication.</returns>
        /// <exception cref="ArgumentException">Thrown when the matrices cannot be multiplied.</exception>
        public static double[][] Multiply(double[][] matrixA, double[][] matrixB)
        {
            int rowsA = matrixA.Length;
            int colsA = matrixA[0].Length;
            int rowsB = matrixB.Length;
            int colsB = matrixB[0].Length;

            // Check if the number of columns in matrixA equals the number of rows in matrixB
            if (colsA != rowsB)
            {
                throw new ArgumentException("Number of columns in matrixA must be equal to the number of rows in matrixB.");
            }

            // Initialize result matrix with zeros
            double[][] result = new double[rowsA][];
            for (int i = 0; i < rowsA; i++)
            {
                result[i] = new double[colsB];
            }

            // Perform matrix multiplication
            for (int i = 0; i < rowsA; i++)
            {
                for (int j = 0; j < colsB; j++)
                {
                    result[i][j] = 0;
                    for (int k = 0; k < colsA; k++)
                    {
                        result[i][j] += matrixA[i][k] * matrixB[k][j];
                    }
                }
            }
            return result;
        }
        public static long EstimateMemoryForMultiply(long rowsA, long colsA, long rowsB, long colsB)
        {
            // Validate the dimensions
            if (colsA != rowsB)
                throw new ArgumentException("Number of columns in _Matrix A must equal the number of rows in _Matrix B.");

            // Memory for matrix A
            long matrixAMemory = rowsA * colsA * sizeof(double);

            // Memory for matrix B
            long matrixBMemory = rowsB * colsB * sizeof(double);

            // Memory for the result matrix
            long resultMatrixMemory = rowsA * colsB * sizeof(double);

            // Total memory required for the MatrixMultiply method
            long totalMemory = matrixAMemory + matrixBMemory + resultMatrixMemory;

            return totalMemory;
        }

        public static InfernoTaskWithResultBase<double[][]> MultiplyMultipleThreads(double[][] matrixA, double[][] matrixB)
        {
            int rowsA = matrixA.Length;
            int colsA = matrixA[0].Length;
            int rowsB = matrixB.Length;
            int colsB = matrixB[0].Length;
            int nThreads = Environment.ProcessorCount;
            // Check if the number of columns in matrixA equals the number of rows in matrixB
            if (colsA != rowsB)
            {
                throw new ArgumentException("Number of columns in matrixA must be equal to the number of rows in matrixB.");
            }

            // Initialize result matrix with zeros
            double[][] result = new double[rowsA][];
            // Perform matrix multiplication
            int rowSizePerThread = (int)Math.Floor((double)rowsA / nThreads);
            if (rowSizePerThread < 1) {
                return Dispatcher.Instance.Run(() => Multiply(matrixA, matrixB));
            }

            int rowFrom = 0;
            int rowTo = rowSizePerThread;
            var tasks = new List<InfernoTask>(nThreads);
            object lockObject = new object();
            var multiplyPartition = (int rowFrom, int rowToExclusive) =>
            {
                return Dispatcher.Instance.Run(() =>
                {
                    int nRowsThisPartition = rowToExclusive - rowFrom;
                    double[][] rowsForThisPartition = new double[nRowsThisPartition][];
                    int l = 0;
                    for (int i = rowFrom; i < rowToExclusive; i++)
                    {
                        double[] row = new double[colsB];
                        rowsForThisPartition[l++] = row;
                        for (int j = 0; j < colsB; j++)
                        {
                            for (int k = 0; k < colsA; k++)
                            {
                                row[j] += matrixA[i][k] * matrixB[k][j];
                            }
                        }
                    }
                    lock (lockObject) {
                        Array.Copy(rowsForThisPartition, 0, result, rowFrom, nRowsThisPartition);
                    }
                });
            };
            int toMinusOnePartition = rowsA - rowSizePerThread;
            while (rowFrom < toMinusOnePartition)
            {
                tasks.Add(multiplyPartition(rowFrom, rowTo));

                rowFrom = rowTo;
                rowTo += rowSizePerThread;
            }
            tasks.Add(multiplyPartition(rowFrom, rowsA));
            return InfernoTask.Join(tasks).Then(() =>
            {
                lock (lockObject)
                {
                    return result;
                }
            });
        }
        public static long EstimateMemoryForMultiplyMultipleThreads(long rowsA, long colsA, long rowsB, long colsB, long numThreads)
        {
            // Validate the dimensions
            if (colsA != rowsB)
                throw new ArgumentException("Number of columns in _Matrix A must equal the number of rows in _Matrix B.");

            // Memory for matrix A
            long matrixAMemory = rowsA * colsA * sizeof(double);

            // Memory for matrix B
            long matrixBMemory = rowsB * colsB * sizeof(double);

            // Memory for the result matrix
            long resultMatrixMemory = rowsA * colsB * sizeof(double);

            // Memory for each thread (each thread may require a portion of the result matrix in memory)
            long memoryPerThread = (rowsA / numThreads) * colsB * sizeof(double);

            // Total memory required for the MatrixMultiplyMultipleThreads method
            long totalMemory = matrixAMemory + matrixBMemory + resultMatrixMemory + (numThreads * memoryPerThread);

            return totalMemory;
        }


        public static void FillWithZeros(double[][] a)
        {
            int nRows = a.Length;
            int nColumns = a[0].Length;
            for (int rowIndex = 0; rowIndex < nRows; rowIndex++)
            {
                double[] row = a[rowIndex];
                for (int columnIndex = 0; columnIndex < nColumns; columnIndex++)
                {
                    row[columnIndex] = 0;
                }
            }
        }

        public static double[][] Transpose(double[][] a)
        {
            int aNRows = a.Length;
            int aNColumns = a[0].Length;
            double[][] b = new double[aNColumns][];
            for (int j = 0; j < aNColumns; j++)
            {
                double[] bRow = new double[aNRows];
                b[j] = bRow;
                for (int i = 0; i < aNRows; ++i)
                {
                    bRow[i] = a[i][j];
                }
            }
            return b;
        }
        public static void ReplaceInfsWithMaxs(double[][] a)
        {
            int nRows = a.Length;
            int nColumns = a[0].Length;
            for (int rowIndex = 0; rowIndex < nRows; rowIndex++)
            {
                double[] row = a[rowIndex];
                for (int columnIndex = 0; columnIndex < nColumns; columnIndex++)
                {
                    double value = row[columnIndex];
                    if (double.IsInfinity(value))
                    {
                        value = value > 0 ? double.MaxValue : double.MinValue;
                        row[columnIndex] = value;
                    }
                }
            }
        }

        public static double[][] Product(double[][] matrixA, double[][] matrixB)
        {
            int aRows = matrixA.Length; int aCols = matrixA[0].Length;
            int bRows = matrixB.Length; int bCols = matrixB[0].Length;
            if (aCols != bRows)
                throw new Exception("Non-conformable matrices in Product");

            double[][] result = Create(aRows, bCols);

            for (int i = 0; i < aRows; ++i) // each row of A
                for (int j = 0; j < bCols; ++j) // each col of B
                    for (int k = 0; k < aCols; ++k) // could use k less-than bRows
                        result[i][j] += matrixA[i][k] * matrixB[k][j];

            return result;
        }
        public static double[] MatrixMultiplyByVector(double[][] a, double[] b)
        {
            int aRows = a.Length; int aCols = a[0].Length;
            double bRows = b.Length;
            if (aCols != bRows)
                throw new Exception($"{nameof(aCols)} was not equal to {nameof(bRows)}");

            double[] c = new double[aRows];

            for (int i = 0; i < aRows; ++i)
            {
                double sumOfProductsForARow = 0;
                double[] aRow = a[i];
                for (int j = 0; j < bRows; ++j)
                {
                    sumOfProductsForARow += aRow[j] * b[j];
                }
                c[i] = sumOfProductsForARow;
            }
            return c;
        }
        /// <summary>
        /// Segmented or block matrix multiplication by a vector. Multiply a matrix by a vector where the matrix columns are an integer multiple of the vector size. The method will check if the number of columns is a whole integer multiple of the vector size and throw an exception if not.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static double[] BlockMatrixMultipliedByVector(double[][] matrix, double[] vector)
        {
            int numRows = matrix.Length;
            if (numRows == 0) throw new ArgumentException("Matrix cannot be empty.");

            int numCols = matrix[0].Length;
            int vectorSize = vector.Length;

            // Check if the number of columns in the matrix is a whole integer multiple of the vector size
            if (numCols % vectorSize != 0)
            {
                throw new ArgumentException("Number of columns in the _Matrix must be a whole integer multiple of the vector size.");
            }

            // Calculate the number of segments in the matrix that correspond to the vector size
            int numSegments = numCols / vectorSize;

            // Initialize the result vector
            double[] result = new double[numRows];

            // Perform matrix-vector multiplication
            for (int i = 0; i < numRows; i++)
            {
                double sum = 0;
                for (int s = 0; s < numSegments; s++)
                {
                    for (int j = 0; j < vectorSize; j++)
                    {
                        sum += matrix[i][s * vectorSize + j] * vector[j];
                    }
                }
                result[i] = sum;
            }

            return result;
        }
        public static double[][] Invert(IBigMatrix matrix, CancellationToken? cancellationToken = null)
        {
            return Invert(matrix.Data, cancellationToken);
        }
        public static double[][] Invert(double[][] matrix, CancellationToken? cancellationToken = null)
        {
            //bool useMultithreadingLocal =  matrix.Length >= 233;
            int n = matrix.Length;
            if (n == 0)
                throw new Exception("Matrix was empty");
            double[][] result = Duplicate(matrix);

            int[] perm;
            int toggle;
            double[][] lum =
                Decompose(matrix, out perm, out toggle);
            if (lum == null)
                throw new Exception("Unable to compute inverse");
            // Precompute all b vectors
            double[][] bVectors = new double[n][];
            for (int i = 0; i < n; ++i)
            {
                bVectors[i] = new double[n];
                for (int j = 0; j < n; ++j)
                {
                    bVectors[i][j] = (i == perm[j]) ? 1.0 : 0.0;
                }
            }

            Action<int, int> forRangeOfI = (iFrom, iToExclusive) => {
                for (int i = iFrom; i < iToExclusive; ++i)
                {
                    if (cancellationToken!=null&&cancellationToken.Value.IsCancellationRequested) 
                        break;
                    double[] x = Invert_SolveLUSystem(lum, bVectors[i]);

                    for (int j = 0; j < n; ++j)
                    {
                        result[j][i] = x[j];
                    }
                }
            };

            if (matrix.Length >= 33)
            {
                int numberOfThreads = Environment.ProcessorCount; // Use the number of logical processors
                if (n >= 0)
                {
                    if (n < numberOfThreads)
                    {
                        numberOfThreads = n;
                    }
                    int iRangePerCore = (int)Math.Floor((double)n / (double)numberOfThreads);
                    Parallel.For(0, numberOfThreads, i =>
                    {
                        int iFromInclusive = iRangePerCore * i;
                        int iToExclusive = (i >= numberOfThreads - 1) ? n : iRangePerCore * (i + 1);
                        forRangeOfI(iFromInclusive, iToExclusive);
                    });
                }
                else
                {
                    forRangeOfI(0, n);
                }
            }
            else
            {
                forRangeOfI(0, n);
            }
            if (cancellationToken!=null&&cancellationToken.Value.IsCancellationRequested)
                return new double[0][];
            return result;
        }

        public static Matrix<double> ComputePseudoInverse(Matrix<double> A, double tolerance = 1e-10)
        {
            // Perform Singular Value Decomposition (SVD)
            var svd = A.Svd(true);
            var U = svd.U;
            var S = svd.S;
            var VT = svd.VT;

            // Create the pseudo-inverse of the diagonal matrix S
            var SInv = DenseMatrix.CreateDiagonal(S.Count, S.Count, i =>
                S[i] > tolerance ? 1.0 / S[i] : 0.0 // Invert nonzero singular values
            );

            // Compute A⁺ = V * S⁺ * Uᵀ
            return VT.Transpose() * SInv * U.Transpose();
        }

        public static double[][] ComputePseudoInverse(double[][] matrix)
        {
            // Example matrix (singular or non-square)
            var A = DenseMatrix.OfArray(ConvertJaggedTo2D(matrix));

            // Compute the pseudo-inverse
            var A_pseudoInv = ComputePseudoInverse(A);
            return MatrixToDoubleArray(A_pseudoInv);
        }
        public static double[,] ConvertJaggedTo2D(double[][] jaggedArray)
        {
            int rows = jaggedArray.Length;
            int cols = jaggedArray[0].Length; // Assume all rows have the same length

            double[,] result = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result[i, j] = jaggedArray[i][j];
                }
            }

            return result;
        }

        public static double[][] MatrixToDoubleArray(Matrix<double> matrix)
        {
            int rows = matrix.RowCount;
            int cols = matrix.ColumnCount;
            double[][] array = new double[rows][];

            for (int i = 0; i < rows; i++)
            {
                array[i] = new double[cols];
                for (int j = 0; j < cols; j++)
                {
                    array[i][j] = matrix[i, j];
                }
            }

            return array;
        }

        public static long EstimateMemoryForInvert(long size)
        {
            // Memory for the original matrix
            long matrixMemory = size * size * sizeof(double);

            // Memory for the pivot array (used in LU decomposition)
            long pivotArrayMemory = size * sizeof(int);

            // Memory for b vectors (identity vectors used in solving the system)
            long bVectorsMemory = size * size * sizeof(double);

            // Memory for the result matrix (the inverse)
            long resultMatrixMemory = size * size * sizeof(double);

            // Total memory required for the MatrixInverse method
            long totalMemory = matrixMemory + pivotArrayMemory + bVectorsMemory + resultMatrixMemory;

            return totalMemory;
        }



        // Helper method: Transpose the input matrix from row-major to column-major order
        public static double[] Transpose(double[] matrix, int size)
        {
            double[] transposed = new double[size * size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    transposed[j * size + i] = matrix[i * size + j];  // Transpose the elements
                }
            }
            return transposed;
        }

        static double[] Invert_SolveLUSystem(double[][] luMatrix, double[] b)
        {
            // before calling this helper, permute b using the perm array
            // from MatrixDecompose that generated luMatrix
            int n = luMatrix.Length;
            double[] x = new double[n];
            b.CopyTo(x, 0);

            for (int i = 1; i < n; ++i)
            {
                double sum = x[i];
                for (int j = 0; j < i; ++j)
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum;
            }

            x[n - 1] /= luMatrix[n - 1][n - 1];
            for (int i = n - 2; i >= 0; --i)
            {
                double sum = x[i];
                for (int j = i + 1; j < n; ++j)
                    sum -= luMatrix[i][j] * x[j];
                x[i] = sum / luMatrix[i][i];
            }

            return x;
        }
        public static double Determinant(double[][] matrix)
        {
            int[] perm;
            int toggle;
            double[][] lum = Decompose_SingleThreaded(matrix, out perm, out toggle);
            if (lum == null)
                throw new Exception("Unable to compute Determinant");
            double result = toggle;
            for (int i = 0; i < lum.GetLength(0); ++i)
                result *= lum[i][i];
            return result;
        }

        static double[][] Duplicate(double[][] matrix)
        {
            // allocates/creates a duplicate of a matrix.
            double[][] result = Create(matrix.Length, matrix[0].Length);
            for (int i = 0; i < matrix.Length; ++i) // copy the values
                for (int j = 0; j < matrix[i].Length; ++j)
                    result[i][j] = matrix[i][j];
            return result;
        }
        public static double[][] DotProduct(double[][] matrixA, double[][] matrixB)
        {
            // Get the dimensions of the input matrices
            int rowsA = matrixA.Length;
            int colsA = matrixA[0].Length;
            int rowsB = matrixB.Length;
            int colsB = matrixB[0].Length;

            // Ensure the matrices can be multiplied (i.e., colsA == rowsB)
            if (colsA != rowsB)
            {
                throw new InvalidOperationException("The number of columns in _Matrix A must equal the number of rows in _Matrix B.");
            }
            if (colsB != rowsA)
            {
                throw new InvalidOperationException("The number of columns in _Matrix B must equal the number of rows in _Matrix A.");
            }
            double[][] result = new double[rowsA][];
            for (int i = 0; i < rowsA; i++)
            {
                result[i] = new double[colsB];
            }
            for (int i = 0; i < rowsA; i++)
            {
                for (int j = 0; j < colsB; j++)
                {
                    for (int k = 0; k < colsA; k++)
                    {
                        result[i][j] += matrixA[i][k] * matrixB[k][j];
                    }
                }
            }
            return result;
        }
        public static double[][] Decompose(double[][] matrix, out int[] perm,
            out int toggle)
        {
            if (matrix.Length >= 428)
            {
                return Decompose_Multithreaded(matrix, out perm, out toggle);
            }
            return Decompose_SingleThreaded(matrix, out perm, out toggle);
        }
        public static double[][] Decompose_SingleThreaded(double[][] matrix, out int[] perm, out int toggle)
        {
            // Doolittle LUP decomposition with partial pivoting.
            // rerturns: result is L (with 1s on diagonal) and U;
            // perm holds row permutations; toggle is +1 or -1 (even or odd)
            int rows = matrix.Length;
            int cols = matrix[0].Length; // assume square
            if (rows != cols)
                throw new Exception("Attempt to decompose a non-square m");

            int n = rows; // convenience

            double[][] result = Duplicate(matrix);

            perm = new int[n]; // set up row permutation result
            for (int i = 0; i < n; ++i) { perm[i] = i; }

            toggle = 1; // toggle tracks row swaps.
                        // +1 -greater-than even, -1 -greater-than odd. used by MatrixDeterminant

            for (int j = 0; j < n - 1; ++j) // each column
            {
                double colMax = Math.Abs(result[j][j]); // find largest val in col
                int pRow = j;
                //for (int i = j + 1; i less-than n; ++i)
                //{
                //  if (result[i][j] greater-than colMax)
                //  {
                //    colMax = result[i][j];
                //    pRow = i;
                //  }
                //}

                // reader Matt V needed this:
                for (int i = j + 1; i < n; ++i)
                {
                    if (Math.Abs(result[i][j]) > colMax)
                    {
                        colMax = Math.Abs(result[i][j]);
                        pRow = i;
                    }
                }
                // Not sure if this approach is needed always, or not.

                if (pRow != j) // if largest value not on pivot, swap rows
                {
                    double[] rowPtr = result[pRow];
                    result[pRow] = result[j];
                    result[j] = rowPtr;

                    int tmp = perm[pRow]; // and swap perm info
                    perm[pRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle; // adjust the row-swap toggle
                }

                // --------------------------------------------------
                // This part added later (not in original)
                // and replaces the 'return null' below.
                // if there is a 0 on the diagonal, find a good row
                // from i = j+1 down that doesn't have
                // a 0 in column j, and swap that good row with row j
                // --------------------------------------------------

                if (result[j][j] == 0.0)
                {
                    // find a good row to swap
                    int goodRow = -1;
                    for (int row = j + 1; row < n; ++row)
                    {
                        if (result[row][j] != 0.0)
                            goodRow = row;
                    }

                    if (goodRow == -1)
                        throw new Exception("Cannot use Doolittle's method");

                    // swap rows so 0.0 no longer on diagonal
                    double[] rowPtr = result[goodRow];
                    result[goodRow] = result[j];
                    result[j] = rowPtr;

                    int tmp = perm[goodRow]; // and swap perm info
                    perm[goodRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle; // adjust the row-swap toggle
                }
                // --------------------------------------------------
                // if diagonal after swap is zero . .
                //if (Math.Abs(result[j][j]) less-than 1.0E-20) 
                //  return null; // consider a throw

                for (int i = j + 1; i < n; ++i)
                {
                    result[i][j] /= result[j][j];
                    for (int k = j + 1; k < n; ++k)
                    {
                        result[i][k] -= result[i][j] * result[j][k];
                    }
                }


            } // main j column loop

            return result;
        }
        public static double[][] Decompose_Multithreaded(double[][] matrix, out int[] perm, out int toggle)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length; // assume square
            if (rows != cols)
                if (rows != cols)
                    throw new Exception("Attempt to decompose a non-square m");
            int n = matrix.Length;
            double[][] result = Duplicate(matrix);
            perm = new int[n];
            object permLockObject = new object();
            toggle = 1;

            // Initialize the permutation array
            for (int i = 0; i < n; i++)
            {
                perm[i] = i;
            }

            // Determine the number of threads to use
            int maxThreads = Math.Min(Environment.ProcessorCount, n);

            for (int j = 0; j < n - 1; j++) // for each column
            {
                // Find the pivot (the largest element in the column)
                double colMax = Math.Abs(result[j][j]);
                int pRow = j;

                Parallel.For(j + 1, n, new ParallelOptions { MaxDegreeOfParallelism = maxThreads }, i =>
                {
                    double absVal = Math.Abs(result[i][j]);
                    if (absVal > colMax)
                    {
                        lock (permLockObject)
                        {
                            if (absVal > colMax)
                            {
                                colMax = absVal;
                                pRow = i;
                            }
                        }
                    }
                });

                // Swap rows if necessary
                if (pRow != j)
                {
                    SwapRows(result, j, pRow);
                    int temp = perm[j];
                    perm[j] = perm[pRow];
                    perm[pRow] = temp;
                    toggle = -toggle;
                }

                // If the diagonal element is zero, the matrix is singular
                if (result[j][j] == 0.0)
                {
                    throw new InvalidOperationException("Matrix is singular and cannot be decomposed.");
                }

                // Update the matrix using multithreading
                Parallel.For(j + 1, n, new ParallelOptions { MaxDegreeOfParallelism = maxThreads }, i =>
                {
                    result[i][j] /= result[j][j];
                    for (int k = j + 1; k < n; k++)
                    {
                        result[i][k] -= result[i][j] * result[j][k];
                    }
                });
            }

            return result; // result contains both L and U, with L in the lower part and U in the upper part
        }
        public static string ToString(double[][] matrix)
        {
            StringBuilder sb = new StringBuilder();
            int rows = matrix.Length;
            int cols = matrix[0].Length;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    sb.Append(matrix[i][j] + "\t");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }


        public static (double[][] L, double[][] U, int[] perm, int toggle) LU_Decompose_Multithreaded(double[][] matrix)
        {
            int n = matrix.Length;
            double[][] L = Create(n, n);
            double[][] U = Duplicate(matrix);
            int[] perm = new int[n];
            int toggle = 1;

            // Initialize permutation array
            for (int i = 0; i < n; i++)
            {
                perm[i] = i;
            }

            // Outer loop for each column
            for (int j = 0; j < n - 1; j++)
            {
                // Find pivot (largest element in the column)
                double colMax = Math.Abs(U[j][j]);
                int pRow = j;

                Parallel.For(j + 1, n, i =>
                {
                    if (Math.Abs(U[i][j]) > colMax)
                    {
                        lock (perm)
                        {
                            if (Math.Abs(U[i][j]) > colMax)
                            {
                                colMax = Math.Abs(U[i][j]);
                                pRow = i;
                            }
                        }
                    }
                });

                // Swap rows if needed
                if (pRow != j)
                {
                    SwapRows(U, j, pRow);
                    SwapRows(L, j, pRow);
                    int temp = perm[j];
                    perm[j] = perm[pRow];
                    perm[pRow] = temp;
                    toggle = -toggle;
                }

                // Update the U matrix and compute the multipliers for L
                Parallel.For(j + 1, n, i =>
                {
                    U[i][j] /= U[j][j];
                    for (int k = j + 1; k < n; k++)
                    {
                        U[i][k] -= U[i][j] * U[j][k];
                    }
                    L[i][j] = U[i][j];
                });

                L[j][j] = 1.0; // Set the diagonal elements of L to 1
            }

            // Set the final diagonal element of L
            L[n - 1][n - 1] = 1.0;

            return (L, U, perm, toggle);
        }

        // Swap two rows in a matrix
        private static void SwapRows(double[][] matrix, int row1, int row2)
        {
            double[] temp = matrix[row1];
            matrix[row1] = matrix[row2];
            matrix[row2] = temp;
        }
        public static double[][] CombineMatrices(double[][] topLeft, double[][] topRight, double[][] bottomLeft, double[][] bottomRight)
        {
            // Get the dimensions of each submatrix
            int topLeftRows = topLeft.Length;
            int topLeftCols = topLeft[0].Length;

            int topRightRows = topRight.Length;
            int topRightCols = topRight[0].Length;

            int bottomLeftRows = bottomLeft.Length;
            int bottomLeftCols = bottomLeft[0].Length;

            int bottomRightRows = bottomRight.Length;
            int bottomRightCols = bottomRight[0].Length;

            // Validate that the matrices align to form a rectangle
            if (topLeftRows != topRightRows || bottomLeftRows != bottomRightRows)
            {
                throw new ArgumentException("The top matrices and bottom matrices must have the same number of rows respectively.");
            }

            if (topLeftCols != bottomLeftCols || topRightCols != bottomRightCols)
            {
                throw new ArgumentException("The left matrices and right matrices must have the same number of columns respectively.");
            }

            // Determine the total number of rows and columns in the combined matrix
            int totalRows = topLeftRows + bottomLeftRows;
            int totalCols = topLeftCols + topRightCols;

            // Create the combined matrix with the total size
            double[][] combinedMatrix = new double[totalRows][];
            for (int i = 0; i < totalRows; i++)
            {
                combinedMatrix[i] = new double[totalCols];
            }

            // Copy top-left matrix into the combined matrix
            for (int i = 0; i < topLeftRows; i++)
            {
                for (int j = 0; j < topLeftCols; j++)
                {
                    combinedMatrix[i][j] = topLeft[i][j];
                }
            }

            // Copy top-right matrix into the combined matrix
            for (int i = 0; i < topRightRows; i++)
            {
                for (int j = 0; j < topRightCols; j++)
                {
                    combinedMatrix[i][j + topLeftCols] = topRight[i][j];
                }
            }

            // Copy bottom-left matrix into the combined matrix
            for (int i = 0; i < bottomLeftRows; i++)
            {
                for (int j = 0; j < bottomLeftCols; j++)
                {
                    combinedMatrix[i + topLeftRows][j] = bottomLeft[i][j];
                }
            }

            // Copy bottom-right matrix into the combined matrix
            for (int i = 0; i < bottomRightRows; i++)
            {
                for (int j = 0; j < bottomRightCols; j++)
                {
                    combinedMatrix[i + topRightRows][j + bottomLeftCols] = bottomRight[i][j];
                }
            }

            return combinedMatrix;
        }

        // Combines two matrices horizontally
        public static double[][] CombineMatricesHorizontal(double[][] leftMatrix, double[][] rightMatrix)
        {
            // Ensure both matrices have the same number of rows
            if (leftMatrix.Length != rightMatrix.Length)
            {
                throw new ArgumentException("Both matrices must have the same number of rows to be combined horizontally.");
            }

            int numRows = leftMatrix.Length;
            int numColsLeft = leftMatrix[0].Length;
            int numColsRight = rightMatrix[0].Length;

            // Create a combined matrix with the same number of rows but with more columns
            double[][] combinedMatrix = new double[numRows][];
            for (int i = 0; i < numRows; i++)
            {
                combinedMatrix[i] = new double[numColsLeft + numColsRight];

                // Copy the left matrix values into the new matrix
                for (int j = 0; j < numColsLeft; j++)
                {
                    combinedMatrix[i][j] = leftMatrix[i][j];
                }

                // Copy the right matrix values into the new matrix (after the left matrix values)
                for (int j = 0; j < numColsRight; j++)
                {
                    combinedMatrix[i][j + numColsLeft] = rightMatrix[i][j];
                }
            }

            return combinedMatrix;
        }

        // Combines two matrices vertically
        public static double[][] CombineMatricesVertical(double[][] topMatrix, double[][] bottomMatrix)
        {
            // Ensure both matrices have the same number of columns
            if (topMatrix[0].Length != bottomMatrix[0].Length)
            {
                throw new ArgumentException("Both matrices must have the same number of columns to be combined vertically.");
            }

            int numRowsTop = topMatrix.Length;
            int numRowsBottom = bottomMatrix.Length;
            int numCols = topMatrix[0].Length;

            // Create a combined matrix with more rows but the same number of columns
            double[][] combinedMatrix = new double[numRowsTop + numRowsBottom][];
            for (int i = 0; i < numRowsTop; i++)
            {
                combinedMatrix[i] = new double[numCols];

                // Copy the top matrix values into the new matrix
                for (int j = 0; j < numCols; j++)
                {
                    combinedMatrix[i][j] = topMatrix[i][j];
                }
            }

            for (int i = 0; i < numRowsBottom; i++)
            {
                combinedMatrix[i + numRowsTop] = new double[numCols];

                // Copy the bottom matrix values into the new matrix (below the top matrix values)
                for (int j = 0; j < numCols; j++)
                {
                    combinedMatrix[i + numRowsTop][j] = bottomMatrix[i][j];
                }
            }

            return combinedMatrix;
        }

        // Function to generate a random double[][] matrix
        public static double[][] GenerateRandomMatrixJagged(int rows, int cols)
        {
            Random rand = new Random();
            double[][] matrix = new double[rows][];

            for (int i = 0; i < rows; i++)
            {
                matrix[i] = new double[cols];
                for (int j = 0; j < cols; j++)
                {
                    matrix[i][j] = rand.NextDouble() * 10; // Random values between 0 and 10
                }
            }

            return matrix;
        }// Function to convert a double[][] matrix to a double[] matrix
        public static double[] ConvertTo1DArrayRowMajor(double[][] matrix)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            double[] result = new double[rows * cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result[i * cols + j] = matrix[i][j]; // Linear indexing
                }
            }

            return result;
        }
        public static double[] ConvertTo1DArrayColumnMajor(double[][] matrix)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            double[] result = new double[rows * cols];

            for (int j = 0; j < cols; j++) // Iterate through columns first
            {
                for (int i = 0; i < rows; i++)
                {
                    result[j * rows + i] = matrix[i][j]; // Column-major indexing
                }
            }

            return result;
        }
        public static double[][] ConvertToJaggedMatrixFromColumnMajor(double[] columnMajorArray, int rows, int cols)
        {
            double[][] result = new double[rows][];
            for (int i = 0; i < rows; i++)
            {
                result[i] = new double[cols];
            }

            for (int j = 0; j < cols; j++) // Iterate through columns first
            {
                for (int i = 0; i < rows; i++)
                {
                    result[i][j] = columnMajorArray[j * rows + i]; // Reverse column-major indexing
                }
            }
            return result;
        }

        public static long CalculateMatrixMultiplicationNOperations(int rowsA, int colsA, int colsB)
        {
            // Validate the dimensions
            if (colsA <= 0 || rowsA <= 0 || colsB <= 0)
                throw new ArgumentException("Matrix dimensions must be positive.");

            // The total operations: m * p * (2n - 1)
            long totalOperations = (long)rowsA * colsB * (2 * colsA - 1);
            return totalOperations;
        }
        public static double Min(double[][] m)
        {
            double min = m[0][0];
            for (int i = 0; i < m.Length; i++) {
                double[] row = m[i];
                for (int j = 0; j < row.Length; j++) {
                    double value = row[j];
                    if (value < min)
                        min = value;
                }
            }
            return min;
        }
        public static double Max(double[][] m)
        {
            double max = m[0][0];
            for (int i = 0; i < m.Length; i++)
            {
                double[] row = m[i];
                for (int j = 0; j < row.Length; j++)
                {
                    double value = row[j];
                    if (value > max)
                        max = value;
                }
            }
            return max;
        }

        public static double InfinityNorm(double[][] matrix)
        {
            double maxRowSum = 0;

            foreach (var row in matrix)
            {
                double rowSum = 0;
                foreach (var value in row)
                {
                    rowSum += Math.Abs(value);
                }
                maxRowSum = Math.Max(maxRowSum, rowSum);
            }

            return maxRowSum;
        }
    }
}