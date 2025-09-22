using System;

namespace Core.Maths.Matrices
{
    public class GMRES
    {
        // GMRES Solver Method
        public static double[] Solve(double[][] A, double[] b, double[] x0, int maxIterations, double tolerance)
        {
            int n = A.Length;
            double[] r = VecSub(b, MatVecMult(A, x0));  // r = b - A * x0
            double beta = Norm(r);  // Initial residual norm
            if (beta < tolerance)  // Early exit if the initial guess is good enough
            {
                return x0;
            }

            double[] x = new double[n];
            Array.Copy(x0, x, n);

            // Initialize storage for GMRES
            double[][] V = new double[maxIterations + 1][];
            for (int i = 0; i <= maxIterations; i++)
            {
                V[i] = new double[n];
            }

            double[][] H = new double[maxIterations + 1][];
            for (int i = 0; i <= maxIterations; i++)
            {
                H[i] = new double[maxIterations];
            }

            double[] g = new double[maxIterations + 1];
            g[0] = beta;

            // Normalize the initial residual to start the Krylov space
            for (int i = 0; i < n; i++)
            {
                V[0][i] = r[i] / beta;
            }

            for (int j = 0; j < maxIterations; j++)
            {
                // Arnoldi Process
                double[] w = MatVecMult(A, V[j]);
                for (int i = 0; i <= j; i++)
                {
                    H[i][j] = DotProduct(w, V[i]);
                    for (int k = 0; k < n; k++)
                    {
                        w[k] -= H[i][j] * V[i][k];
                    }
                }

                H[j + 1][j] = Norm(w);
                if (H[j + 1][j] < tolerance)
                {
                    // The new vector w is small enough, so we can stop the Arnoldi process
                    break;
                }

                for (int i = 0; i < n; i++)
                {
                    V[j + 1][i] = w[i] / H[j + 1][j];
                }

                // Apply Givens rotations to H to form an upper triangular matrix
                ApplyGivensRotation(H, g, j);

                // Update the residual norm
                double rNorm = Math.Abs(g[j + 1]);

                if (rNorm < tolerance)
                {
                    // We've converged
                    break;
                }
            }

            // Solve the least squares problem to find y
            double[] y = SolveUpperTriangular(H, g, maxIterations);

            // Reconstruct the solution x
            for (int j = 0; j < maxIterations; j++)
            {
                for (int i = 0; i < n; i++)
                {
                    x[i] += V[j][i] * y[j];
                }
            }

            return x;
        }

        // Function to apply Givens rotations
        private static void ApplyGivensRotation(double[][] H, double[] g, int j)
        {
            for (int i = 0; i < j; i++)
            {
                double temp = H[i][j];
                H[i][j] = GivensCos(g[i], g[i + 1]) * H[i][j] - GivensSin(g[i], g[i + 1]) * H[i + 1][j];
                H[i + 1][j] = GivensSin(g[i], g[i + 1]) * temp + GivensCos(g[i], g[i + 1]) * H[i + 1][j];
            }
        }

        // Solve the least squares system (upper triangular system H * y = g)
        private static double[] SolveUpperTriangular(double[][] H, double[] g, int m)
        {
            double[] y = new double[m];
            for (int i = m - 1; i >= 0; i--)
            {
                y[i] = g[i];
                for (int j = i + 1; j < m; j++)
                {
                    y[i] -= H[i][j] * y[j];
                }
                y[i] /= H[i][i];
            }
            return y;
        }

        // Givens rotation (cosine)
        private static double GivensCos(double v1, double v2)
        {
            double t = Math.Sqrt(v1 * v1 + v2 * v2);
            return v1 / t;
        }

        // Givens rotation (sine)
        private static double GivensSin(double v1, double v2)
        {
            double t = Math.Sqrt(v1 * v1 + v2 * v2);
            return v2 / t;
        }

        // Matrix-vector multiplication: A * v
        private static double[] MatVecMult(double[][] A, double[] v)
        {
            int n = A.Length;
            double[] result = new double[n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result[i] += A[i][j] * v[j];
                }
            }
            return result;
        }

        // Vector subtraction: v1 - v2
        private static double[] VecSub(double[] v1, double[] v2)
        {
            int n = v1.Length;
            double[] result = new double[n];
            for (int i = 0; i < n; i++)
            {
                result[i] = v1[i] - v2[i];
            }
            return result;
        }

        // Dot product of two vectors
        private static double DotProduct(double[] v1, double[] v2)
        {
            double sum = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                sum += v1[i] * v2[i];
            }
            return sum;
        }

        // Norm of a vector
        private static double Norm(double[] v)
        {
            return Math.Sqrt(DotProduct(v, v));
        }
    }

}