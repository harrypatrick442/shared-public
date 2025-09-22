using System;

namespace Core.Maths.Matrices
{
    public partial class MatrixConditioning
    {
        private const double SmallValueThreshold = 1,
            LargeValueThreshold=1; // Minimum norm value to prevent instability

        public static double CalculateConditionNumber(
            double[][] matrix, double[][]? inverseMatrix = null)
        {
            if (inverseMatrix == null)
                inverseMatrix = MatrixHelper.Invert(matrix);

            // Validate input
            if (matrix == null || inverseMatrix == null)
                throw new ArgumentNullException("Matrix or its inverse cannot be null.");

            if (matrix.Length == 0 || matrix.Length != matrix[0].Length)
                throw new ArgumentException("Matrix must be square.");

            if (inverseMatrix.Length != matrix.Length || inverseMatrix[0].Length != matrix[0].Length)
                throw new ArgumentException("Invert _Matrix must have the same dimensions as the original _Matrix.");

            // Compute the norms of the matrix and its inverse
            double matrixNorm = MatrixHelper.InfinityNorm(matrix);
            double inverseMatrixNorm = MatrixHelper.InfinityNorm(inverseMatrix);

            // Compute the condition number
            return matrixNorm * inverseMatrixNorm;
        }
        public static ConditionedSystem ConditionMatrixWithScaling(
            double[][] A, double[]? b = null)
        {
            int n = A.Length;
            int m = A[0].Length;

            // Step 1: Compute row norms
            double[] rowNorms = new double[n];
            for (int i = 0; i < n; i++)
            {
                double rowNorm = 0;
                for (int j = 0; j < m; j++)
                {
                    rowNorm += Math.Abs(A[i][j]);
                }

                // Apply threshold to prevent very small norms
                rowNorms[i] = 1d;// Math.Min(Math.Max(rowNorm, SmallValueThreshold), LargeValueThreshold);
            }

            // Step 2: Create row scaling matrix S_R
            double[][] S_R = CreateDiagonalMatrix(rowNorms, invert: true);

            // Step 3: Scale rows
            double[][] A_scaled = MatrixHelper.Multiply(S_R, A);
            double[]? b_scaled = b != null ? MatrixHelper.MatrixMultiplyByVector(S_R, b) : null;

            // Step 4: Compute column norms
            double[] columnNorms = new double[m];
            for (int j = 0; j < m; j++)
            {
                double columnNorm = 0;
                for (int i = 0; i < n; i++)
                {
                    columnNorm += Math.Abs(A_scaled[i][j]);
                }

                // Apply threshold to prevent very small norms
                columnNorms[j] = 1d;// Math.Min(Math.Max(columnNorm, SmallValueThreshold), LargeValueThreshold);
            }

            // Step 5: Create column scaling matrix S_C
            double[][] S_C = CreateDiagonalMatrix(columnNorms, invert: true);

            // Step 6: Scale columns
            A_scaled = MatrixHelper.Multiply(A_scaled, S_C);

            return new ConditionedSystem(A_scaled, b_scaled, rowNorms, columnNorms, S_R, S_C);
        }

        private static double[][] CreateDiagonalMatrix(double[] norms, bool invert)
        {
            int size = norms.Length;
            double[][] diagonalMatrix = new double[size][];
            for (int i = 0; i < size; i++)
            {
                diagonalMatrix[i] = new double[size];
                diagonalMatrix[i][i] = invert ? 1.0 / norms[i] : norms[i];

                // Ensure scaling factor is within reasonable bounds
                if (double.IsInfinity(diagonalMatrix[i][i]) || double.IsNaN(diagonalMatrix[i][i]))
                {
                    throw new InvalidOperationException($"Scaling factor at index {i} is invalid: {diagonalMatrix[i][i]}");
                }
            }
            return diagonalMatrix;
        }
    }
}
