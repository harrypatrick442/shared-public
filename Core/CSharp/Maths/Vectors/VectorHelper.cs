using Core.Maths.Tensors;
using System;
using System.ComponentModel;

namespace Core.Maths.Vectors
{
    public static partial class VectorHelper
    {
        public static double[] CreateNewWithValue(double value, int length)
        {
            double[] a = new double[length];
            for (int i = 0; i < length; i++)
            {
                a[i] = value;
            }
            return a;
        }
        public static void FillWithZeros(double[] a)
        {
            int length = a.Length;
            for (int index = 0; index < length; index++)
            {
                a[index] = 0;
            }
        }
        public static double[] Clone(double[] a)
        {
            double[] b = new double[a.Length];
            Array.Copy(a, b, a.Length);
            return b;
        }
        public static double[] Abs(double[] a)
        {
            double[] b = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                b[i] = Math.Abs(a[i]);
            }
            return b;
        }
        public static double[] Max(double[] a, double epsilon)
        {
            double[] b = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                b[i] = Math.Max(a[i], epsilon);
            }
            return b;
        }
        public static double[] Scale(double[] a, double scale)
        {
            double[] b = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                b[i] = a[i] * scale;
            }
            return b;
        }
        public static double[] Subtract(double[] a, double[] b)
        {
            if (a.Length != b.Length) throw new Exception("Lengths different");
            double[] c = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                c[i] = a[i] - b[i];
            }
            return c;
        }
        public static double[] Divide(double[] a, double[] b)
        {
            if (a.Length != b.Length) throw new Exception("Lengths different");
            double[] c = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                c[i] = a[i] / b[i];
            }
            return c;
        }
        public static double[] Multiply(double[] a, double[] b)
        {
            if (a.Length != b.Length) throw new Exception("Lengths different");
            double[] c = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                c[i] = a[i] * b[i];
            }
            return c;
        }
        public static double[] Addition(double[] a, double b)
        {
            double[] c = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                c[i] = a[i] + b;
            }
            return c;
        }
        public static double[] Addition(double[] a, double[] b)
        {
            if (a.Length != b.Length) throw new Exception($"{nameof(a)}.{nameof(a.Length)} was not equal to {nameof(b)}.{nameof(b.Length)}");
            double[] c = new double[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                c[i] = a[i] + b[i];
            }
            return c;
        }
        public static void AddOntoFirstVector(double[] a, double[] b)
        {
            //throw new Exception("Mortal fucking sin using this. Yes it works but it can cause a world of problems so just avoid it ffs.");
            if (a.Length != b.Length) throw new Exception($"{nameof(a)}.{nameof(a.Length)} was not equal to {nameof(b)}.{nameof(b.Length)}");
            for (int i = 0; i < a.Length; i++)
            {
                a[i] += b[i];
            }
        }
        public static double DotProduct(double[] a, double[] b)
        {
            if (a.Length != b.Length) throw new Exception($"{nameof(a)}.{nameof(a.Length)} was not equal to {nameof(b)}.{nameof(b.Length)}");
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                sum += a[i] * b[i];
            }
            return sum;
        }
        public static double Laplacian_GradientOfDivergence(double[] a, double[] b)
        {
            throw new NotImplementedException();
        }
        public static double[] Curl(double[] a, double[] b)
        {

            throw new NotImplementedException();
        }
        /// <summary>
        /// Calculates the p-norm of a vector.
        /// </summary>
        /// <param name="vector">The vector to calculate the p-norm for.</param>
        /// <param name="p">The p value for the norm (1 for Manhattan norm, 2 for Euclidean norm, etc.).</param>
        /// <returns>The p-norm of the vector.</returns>
        public static double PNorm(double[] vector, double p)
        {
            // Handle the infinity norm case
            if (double.IsPositiveInfinity(p))
            {
                return InfinityNorm(vector);
            }

            // General p-norm calculation
            double sum = 0.0;
            foreach (double v in vector)
            {
                sum += Math.Pow(Math.Abs(v), p);
            }

            return Math.Pow(sum, 1.0 / p);
        }

        /// <summary>
        /// Calculates the infinity norm (maximum absolute value) of a vector.
        /// </summary>
        /// <param name="vector">The vector to calculate the infinity norm for.</param>
        /// <returns>The infinity norm of the vector.</returns>
        private static double InfinityNorm(double[] vector)
        {
            double maxVal = double.MinValue;
            foreach (double v in vector)
            {
                if (Math.Abs(v) > maxVal)
                {
                    maxVal = Math.Abs(v);
                }
            }
            return maxVal;
        }
        public static double EuclideanL2Norm(double[] vector)
        {
            if (vector == null || vector.Length == 0)
                throw new ArgumentException("Vector cannot be null or empty.", nameof(vector));

            double sumOfSquares = 0.0;

            foreach (var value in vector)
            {
                sumOfSquares += value * value;
            }

            return Math.Sqrt(sumOfSquares);
        }

        public static string ToString(double[] vector)
        {
            return $"[{string.Join(",", vector)}]";
        }
    }
}