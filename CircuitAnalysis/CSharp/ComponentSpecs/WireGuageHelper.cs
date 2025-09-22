using System;
using System.Collections.Generic;
using Core.Enums;

namespace CircuitAnalysis.ComponentSpecs
{
    public static class WireGaugeHelper
    {
        // Conversion factors
        private const double MmToMeters = 0.001;
        private const double UmToMeters = 0.000001;

        private static readonly Dictionary<int, (double Diameter, double EnamelThickness)> AwgSpecs =
            new Dictionary<int, (double Diameter, double EnamelThickness)>
            {
                { 50, (0.025, 0.002) }, { 49, (0.028, 0.002) },
                { 48, (0.032, 0.002) }, { 47, (0.036, 0.002) },
                { 46, (0.04, 0.002) },  { 45, (0.045, 0.003) },
                { 44, (0.05, 0.003) },  { 43, (0.056, 0.003) },
                { 42, (0.063, 0.003) }, { 41, (0.071, 0.003) },
                { 40, (0.08, 0.003) },  { 39, (0.09, 0.004) },
                { 38, (0.1, 0.004) },   { 37, (0.112, 0.004) },
                { 36, (0.127, 0.004) }, { 35, (0.142, 0.004) },
                { 34, (0.16, 0.004) },  { 33, (0.18, 0.004) },
                { 32, (0.202, 0.004) }, { 31, (0.227, 0.005) },
                { 30, (0.254, 0.005) }, { 29, (0.286, 0.005) },
                { 28, (0.321, 0.005) }, { 27, (0.361, 0.006) },
                { 26, (0.405, 0.006) }, { 25, (0.455, 0.006) },
                { 24, (0.511, 0.006) }, { 23, (0.573, 0.008) },
                { 22, (0.644, 0.008) }, { 21, (0.723, 0.01) },
                { 20, (0.812, 0.01) },  { 19, (0.912, 0.01) },
                { 18, (1.024, 0.01) },  { 17, (1.15, 0.012) },
                { 16, (1.291, 0.012) }, { 15, (1.45, 0.015) },
                { 14, (1.628, 0.015) }, { 13, (1.828, 0.015) },
                { 12, (2.053, 0.015) }, { 11, (2.304, 0.02) },
                { 10, (2.588, 0.02) },  { 9, (2.906, 0.02) },
                { 8, (3.264, 0.025) },  { 7, (3.665, 0.025) },
                { 6, (4.115, 0.025) },  { 5, (4.621, 0.03) },
                { 4, (5.189, 0.03) },   { 3, (5.827, 0.03) },
                { 2, (6.544, 0.035) },  { 1, (7.348, 0.04) },
                { 0, (8.251, 0.04) }
            };

        // Method to get conducting diameter with a specified unit (default: meters)
        public static double GetConductingDiameter(int awg, Units unit = Units.Meters)
        {
            if (!AwgSpecs.TryGetValue(awg, out var specs))
                throw new ArgumentOutOfRangeException(nameof(awg), "AWG size not found in specification table.");

            return ConvertToUnit(specs.Diameter, unit);
        }

        // Method to get enamel thickness with a specified unit (default: meters)
        public static double GetEnamelThickness(int awg, Units unit = Units.Meters)
        {
            if (!AwgSpecs.TryGetValue(awg, out var specs))
                throw new ArgumentOutOfRangeException(nameof(awg), "AWG size not found in specification table.");

            return ConvertToUnit(specs.EnamelThickness, unit);
        }

        // Converts a value in millimeters to the specified unit
        private static double ConvertToUnit(double valueInMm, Units unit)
        {
            return unit switch
            {
                Units.Meters => valueInMm * MmToMeters,
                Units.Millimeters => valueInMm,
                Units.Micrometers => valueInMm / MmToMeters * UmToMeters,
                _ => throw new ArgumentException("Unsupported unit type", nameof(unit)),
            };
        }
    }
}
