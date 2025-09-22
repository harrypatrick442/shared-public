using System;

namespace Core.Maths
{
    public static class Evaluate
    {

        /// <summary>
        /// Evaluates a polynomial using Horner's Method.
        /// </summary>
        /// <param name="x">The value to evaluate the polynomial at.</param>
        /// <param name="hornerCoefficients">Horner's method coefficients.</param>
        /// <returns>The evaluated polynomial value.</returns>
        public static double Polynomial(double x, double[] coefficients)
        {
            double result = 0;
            for (int i = 0; i < coefficients.Length; i++) {
                result += coefficients[i] * Math.Pow(x, i);
            }
            return result;/*
            if (coefficients == null || coefficients.Length == 0)
            {
                throw new ArgumentException("Coefficients array cannot be null or empty.");
            }

            double result = 0;
            // Process coefficients from highest degree to lowest (descending order)
            foreach (var coefficient in coefficients)
            {
                result = result * (x + coefficient);
            }

            return result;*/
        }
        /// <summary>
        /// Evaluates the derivative of a polynomial at a specific value of x.
        /// </summary>
        /// <param name="x">The value to evaluate the derivative at.</param>
        /// <param name="coefficients">The coefficients of the polynomial in ascending order (small-to-large powers).</param>
        /// <returns>The value of the derivative at x.</returns>
        public static double PolynomialDerivative(double x, double[] coefficients)
        {
            //BEEN CHECKED FUCKING WORKS NO NEED TO CHECK AGAIN
            if (coefficients == null || coefficients.Length <= 1)
            {
                // If there are no coefficients or only a constant term, derivative is 0
                return 0.0;
            }

            double result = 0.0;

            // Start from the highest degree term and work backward
            for (int i = coefficients.Length - 1; i > 0; i--)
            {
                result = result * x + i * coefficients[i];
            }

            return result;
        }


    }
}