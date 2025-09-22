namespace Core.Maths.Matrices
{
    using System;

    public class TensorEntryContributions
    {
        public double[][] Entries { get; set; }
    }

    public static class MatrixContributionAnalyzer
    {
        /// <summary>
        /// Computes the contribution of each element in the original matrix to each entry in its inverse.
        /// </summary>
        /// <param name="matrix">The original matrix (or its inverse).</param>
        /// <param name="isInverse">True if the input is the inverse, false if it's the original matrix.</param>
        /// <returns>An EntryContribution[][] array with contribution data.</returns>
        public static TensorEntryContributions[][] ComputeContributions(double[][] matrix,
            bool isInverse)
        {
            int n = matrix.Length;
            if (n == 0 || matrix[0].Length != n)
                throw new ArgumentException("Input _Matrix must be square.");

            // Compute the inverse if the provided matrix is not already the inverse
            double[][] originalMatrix = isInverse ? MatrixHelper.Invert(matrix) : matrix;
            double[][] inverseMatrix = isInverse ? matrix : MatrixHelper.Invert(matrix);

            // Create the result array
            TensorEntryContributions[][] contributions = new TensorEntryContributions[n][];
            for (int i = 0; i < n; i++)
            {
                contributions[i] = new TensorEntryContributions[n];
                for (int j = 0; j < n; j++)
                {
                    contributions[i][j] = new TensorEntryContributions
                    {
                        Entries = new double[n][]
                    };

                    for (int k = 0; k < n; k++)
                    {
                        contributions[i][j].Entries[k] = new double[n];
                    }
                }
            }

            // Compute contributions via perturbation
            double epsilon = 1e-5;
            for (int row = 0; row < n; row++)
            {
                for (int col = 0; col < n; col++)
                {
                    // Perturb the (row, col) element of the original matrix
                    double[][] perturbedMatrix = CopyMatrix(originalMatrix);
                    perturbedMatrix[row][col] += epsilon;

                    // Compute the inverse of the perturbed matrix
                    double[][] perturbedInverse = MatrixHelper.Invert(perturbedMatrix);

                    // Calculate the difference and populate the contributions
                    for (int i = 0; i < n; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            contributions[i][j].Entries[row][col] =
                                (perturbedInverse[i][j] - inverseMatrix[i][j]) / epsilon;
                        }
                    }
                }
            }

            return contributions;
        }

        /// <summary>
        /// Creates a deep copy of a matrix.
        /// </summary>
        private static double[][] CopyMatrix(double[][] matrix)
        {
            int n = matrix.Length;
            double[][] copy = new double[n][];
            for (int i = 0; i < n; i++)
            {
                copy[i] = new double[n];
                Array.Copy(matrix[i], copy[i], n);
            }
            return copy;
        }
    }
}