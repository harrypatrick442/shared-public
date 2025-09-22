using Core.Maths.CUBLAS;
using InfernoDispatcher.Locking;
using ManagedCuda;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Core.Maths
{
    public class SignificantFiguresHelper
    {
        public static double RoundToSignificantFigures(double value, int significantFigures = 10)
        {
            // Step 1: Handle edge case where value is zero (logarithm undefined for zero)
            if (value == 0)
            {
                return 0; // Zero rounded to any precision is still zero
            }

            // Step 2: Find the order of magnitude of the absolute value
            double magnitude = Math.Floor(Math.Log10(Math.Abs(value)));

            // Step 3: Calculate the scaling factor based on the desired significant figures
            double scale = Math.Pow(10, magnitude - (significantFigures - 1));

            // Step 4: Round the value to the adjusted significant figures
            double roundedValue = Math.Round(value / scale) * scale;

            // Step 5: Return the rounded value
            return roundedValue;
        }
        public static (double roundedLarger, double roundedSmaller) RoundTwoValues(double a, double b, int significantFigures = 10)
        {
            double largerValue, smallerValue;
            if (a > b)
            {
                largerValue = a;
                smallerValue = b;
            }
            else {
                largerValue = b;
                smallerValue = a;
            }
            // Step 1: Handle edge case where either value is zero
            if (largerValue == 0 && smallerValue == 0)
            {
                return (0, 0); // Both are zero
            }
            if (largerValue == 0)
            {
                return (0, RoundToSignificantFigures(smallerValue, significantFigures));
            }
            if (smallerValue == 0)
            {
                return (RoundToSignificantFigures(largerValue, significantFigures), 0);
            }

            // Step 2: Determine the order of magnitude of both values
            double magnitudeLarger = Math.Floor(Math.Log10(Math.Abs(largerValue)));
            double magnitudeSmaller = Math.Floor(Math.Log10(Math.Abs(smallerValue)));

            // Step 3: Choose the smaller magnitude as the rounding scale reference
            double referenceMagnitude = Math.Min(magnitudeLarger, magnitudeSmaller);
            double scale = Math.Pow(10, referenceMagnitude - (significantFigures - 1));

            // Step 4: Round both values using the same scale
            double roundedLarger = Math.Round(largerValue / scale) * scale;
            double roundedSmaller = Math.Round(smallerValue / scale) * scale;

            // Step 5: Return the rounded values as a tuple
            return (roundedLarger, roundedSmaller);
        }


    }
}
