using System;

namespace Core.Maths
{
    public partial class GPTMatrixInverter
    {
        public static double[][] Invert(double[][] matrix)
        {
            int n = matrix.Length;

            // Convert to 1D array for performance
            double[] lu = To1DArray(matrix, n);
            int[] permutation = new int[n];
            int pivotSign = PerformLUDecompositionUnsafe(lu, permutation, n);

            // Solve for each column of the identity matrix sequentially
            double[][] inverse = new double[n][];
            for (int i = 0; i < n; i++)
            {
                double[] column = SolveLUSystemUnsafe(lu, CreateUnitVectorUnsafe(i, n), permutation, n);
                inverse[i] = column;
            }

            return TransposeUnsafe(inverse, n);
        }

        private static unsafe int PerformLUDecompositionUnsafe(double[] matrix, int[] permutation, int n)
        {
            double[] scales = new double[n];
            int pivotSign = 1;

            // Compute row scaling factors sequentially
            fixed (double* matrixPtr = matrix, scalesPtr = scales)
            {
                for (int i = 0; i < n; i++)
                {
                    double max = 0.0;
                    for (int j = 0; j < n; j++)
                        max = Math.Max(max, Math.Abs(matrixPtr[i * n + j]));
                    scalesPtr[i] = 1.0 / max;
                }
            }

            // Perform LU decomposition
            for (int k = 0; k < n; k++)
            {
                int pivot = k;
                double max = 0.0;

                fixed (double* matrixPtr = matrix, scalesPtr = scales)
                {
                    for (int i = k; i < n; i++)
                    {
                        double temp = scalesPtr[i] * Math.Abs(matrixPtr[i * n + k]);
                        if (temp > max)
                        {
                            max = temp;
                            pivot = i;
                        }
                    }

                    // Swap rows
                    if (pivot != k)
                    {
                        SwapRowsUnsafe(matrixPtr, k, pivot, n);
                        (permutation[k], permutation[pivot]) = (permutation[pivot], permutation[k]);
                        pivotSign = -pivotSign;
                    }

                    // Eliminate below pivot
                    for (int i = k + 1; i < n; i++)
                    {
                        matrixPtr[i * n + k] /= matrixPtr[k * n + k];
                        for (int j = k + 1; j < n; j++)
                            matrixPtr[i * n + j] -= matrixPtr[i * n + k] * matrixPtr[k * n + j];
                    }
                }
            }

            return pivotSign;
        }

        private static unsafe double[] SolveLUSystemUnsafe(double[] lu, double[] b, int[] permutation, int n)
        {
            double[] x = new double[n];
            fixed (double* luPtr = lu, bPtr = b, xPtr = x)
            {
                // Forward substitution
                for (int i = 0; i < n; i++)
                {
                    xPtr[i] = bPtr[permutation[i]];
                    for (int j = 0; j < i; j++)
                        xPtr[i] -= luPtr[i * n + j] * xPtr[j];
                }

                // Backward substitution
                for (int i = n - 1; i >= 0; i--)
                {
                    for (int j = i + 1; j < n; j++)
                        xPtr[i] -= luPtr[i * n + j] * xPtr[j];
                    xPtr[i] /= luPtr[i * n + i];
                }
            }
            return x;
        }

        private static unsafe double[][] TransposeUnsafe(double[][] matrix, int n)
        {
            double[][] transposed = new double[n][];
            for (int j = 0; j < n; j++)
                transposed[j] = new double[n];

            fixed (double* matrixPtr = &matrix[0][0])
            {
                for (int j = 0; j < n; j++)
                {
                    for (int i = 0; i < n; i++)
                        transposed[j][i] = matrix[i][j];
                }
            }
            return transposed;
        }

        private static unsafe void SwapRowsUnsafe(double* matrix, int row1, int row2, int n)
        {
            for (int i = 0; i < n; i++)
            {
                double temp = matrix[row1 * n + i];
                matrix[row1 * n + i] = matrix[row2 * n + i];
                matrix[row2 * n + i] = temp;
            }
        }

        private static unsafe double[] CreateUnitVectorUnsafe(int index, int n)
        {
            double[] unit = new double[n];
            fixed (double* unitPtr = unit)
            {
                unitPtr[index] = 1.0;
            }
            return unit;
        }

        private static unsafe double[] To1DArray(double[][] matrix, int n)
        {
            double[] result = new double[n * n];
            fixed (double* resultPtr = result)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                        resultPtr[i * n + j] = matrix[i][j];
                }
            }
            return result;
        }
    }
}
