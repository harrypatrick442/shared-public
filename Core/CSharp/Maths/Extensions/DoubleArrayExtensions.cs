using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Maths
{
    public static class DoubleArrayExtensions
    {
        public static double[][] RoundToDecimalPlaces(this double[][] matrix, int decimalPlaces)
        {
            double[][] returns = new double[matrix.Length][];
            // Loop through each row
            for (int i = 0; i < matrix.Length; i++)
            {
                double[] row = matrix[i];
                double[] returnsRow = new double[row.Length];
                // Loop through each column in the current row
                for (int j = 0; j < row.Length; j++)
                {
                    // Round the element to the specified number of decimal places
                    returnsRow[j] = Math.Round(row[j], decimalPlaces);
                }
                returns[i]= returnsRow;
            }
            return returns;
        }
    }
}
