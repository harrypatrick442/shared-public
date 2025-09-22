using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Core.Maths {
    public static class PrpoortionalRepresentationArray
    {
        public static int[] Create(double[] values, double maxDesirableVariationInProportion, int maxLength) {
                int valuesLength = values.Length;
                double[] nonZeroValues = values.Where(v => v > 0).ToArray();
                if (nonZeroValues.Length <= 0)
                {
                    int[] def = new int[valuesLength];
                    for (var i = 0; i < valuesLength; i++)
                        def[i] = i;
                    return def;
                }
                double smallestNonZeroValue = nonZeroValues.Min();
                values = values.Select(v => v / smallestNonZeroValue).ToArray();
                double totalOfValues = values.Sum();
                double[] proportionShouldBes = values.Select(v => v / totalOfValues).ToArray();
                int[] nOfEach = new int[valuesLength];
                int currentValueIndex = 0;
                Func<double> nextValue = () => {
                    currentValueIndex++;
                    if (currentValueIndex >= values.Length)
                        currentValueIndex = 0;
                    double value = values[currentValueIndex];
                    return value;
                };
                double total = 0;
                int index = 0;
                double currentlyLowestSeenProportionVariation = 1;
                List<int> choices = new List<int>();
                Func<double> getMaximumVariationInProportion = () => {
                    double maximumVariationInProportion = 0;
                    for (int i = 0; i < valuesLength; i++)
                    {
                        double currentProportion = (double)nOfEach[i] / (double)choices.Count();
                        double variationInProportion = currentProportion - proportionShouldBes[i];
                        double variationInProportionPositive = (double)Math.Sign(variationInProportion) * variationInProportion;
                        if (variationInProportionPositive > maximumVariationInProportion)
                            maximumVariationInProportion = variationInProportionPositive;
                    }
                    return maximumVariationInProportion;
                };
                int[] choicesForLowestSeenProportionVariation = null;
                while (true)
                {
                    while (total < index + 1)
                    {
                        total += nextValue();
                    }
                    choices.Add(currentValueIndex);
                    nOfEach[currentValueIndex]++;
                    double maximumVariationInProportion = getMaximumVariationInProportion();
                    if (currentlyLowestSeenProportionVariation > maximumVariationInProportion)
                    {
                        currentlyLowestSeenProportionVariation = maximumVariationInProportion;
                        choicesForLowestSeenProportionVariation = choices.ToArray();
                    }
                    if (maximumVariationInProportion < maxDesirableVariationInProportion)
                        return choices.ToArray();
                    index++;
                    if (index >= maxLength)
                        return choicesForLowestSeenProportionVariation;
                }
            } 
    }
}