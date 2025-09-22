using CircuitAnalysis.ComponentSpecs;
using CircuitAnalysis.ComponentSpecs.Cores.CrossSection;

namespace CircuitAnalysis
{
    public class FlybackDesignEquations
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="throwExceptions"></param>
        /// <param name="P_out">output power required from transformer</param>
        /// <param name="V_in_min">minimum input voltage that power supply could provide</param>
        /// <param name="V_in_max">maximuminput voltage that power supply could provide</param>
        /// <param name="f_s">switching frequency</param>
        /// <param name="A_L">inductance per square turn in nano henries</param>
        /// <param name="D_max">maximum duty cycle</param>
        /// <param name="A_e">effective cross-sectional area of the core in meters squared</param>
        /// <param name="B_sat">saturation flux density</param>
        /// <param name="isCCMElseDCM">saturation flux density</param>
        /// <param name="V_out">output voltage from flyback circuit with secondary diode and capacitors</param>
        public static BasicCalculationsFlybackSpecs Calculate(
            bool throwExceptions,
            double P_out,
        double V_in_min,
        double V_in_max,
        double f_s,
        double D_max,
        bool isCCMElseDCM,
        double V_out,
        double efficiency,
        CoreSpecs coreSpecs,
        BobinSpecs bobinSpecs,
        WireSpecs primaryWireSpecs,
        WireSpecs secondaryWireSpecs)
        {
            double pIn = P_out / efficiency;
            double I_p_avg = P_out / V_in_min;// it makes sense because designers often use the worst -case scenario for sizing components, ensuring the converter performs correctly even at the lowest input voltage.
            double I_p_peak_theoretical_efficient = 0;
            double I_p_peak = isCCMElseDCM ? CalculatePeakCurrentCCM(
                out I_p_peak_theoretical_efficient) : CalculatePeakCurrentDCM(
                P_out, efficiency, V_in_min, D_max, out I_p_peak_theoretical_efficient);
            double L_p = FlybackDesignEquations.CalculatePrimaryInductanceRequired(
                P_out, I_p_peak, f_s, efficiency);
            double maximumEnergyPerCycle = Math.Pow(I_p_peak, 2) * L_p / 2d;
            double N_p = FlybackDesignEquations.CalculateNumberPrimaryTurnsRequired(L_p, coreSpecs.A_L);
            double B_max = FlybackDesignEquations.CalculateBMax(V_in_max, D_max, N_p, coreSpecs.A_e, f_s);
            bool willSaturate = B_max >= coreSpecs.B_sat;
            double proportionSaturated = B_max / coreSpecs.B_sat;
            double n/*turns ratio*/ = isCCMElseDCM
                    ? FlybackDesignEquations.CalculateTurnsRatioForCCMSimple(V_out, V_in_min, D_max)
                    : FlybackDesignEquations.CalculateTurnsRatioForDCMSimple(V_out, V_in_min, D_max);
            double I_s_peak = I_p_peak / n;
            double I_s_peak_theoretical_efficient = I_p_peak_theoretical_efficient / n;
            double L_s_check = CalculateSecondaryInductanceRequired(P_out, I_s_peak, f_s, efficiency);
            double N_s = N_p * n;
            double L_s = CalculateSecondaryInductance(L_p, n);
            if ((L_s_check - L_s) / L_s > 0.01)
            {
                throw new Exception("Something went wrong...");
            }
            double expectedFluxLinkageSecondary = L_s * I_s_peak;
            double expectedMagneticFluxSecondary = expectedFluxLinkageSecondary / N_s;
            double expectedFluxDensityCore = expectedMagneticFluxSecondary / coreSpecs.A_e;
            double primaryWindingLength = CalculateTotalWindingLength(
                out int nLayersPrimary,
                coreSpecs.CrossSection,
                bobinSpecs.WidthToCore,
                bobinSpecs.LengthForWindings,
                N_p,
                primaryWireSpecs.FullDiameterMeters
            );
            double R_p = CalculateWindingResistance(
                primaryWindingLength,
                primaryWireSpecs.ConductingDiameterMeters,
                primaryWireSpecs.Resistivity
            );
            double secondaryWindingLength = CalculateTotalWindingLength(
                out int nLayersSecondary,
                coreSpecs.CrossSection,
                bobinSpecs.WidthToCore,
                bobinSpecs.LengthForWindings,
                N_s,
                secondaryWireSpecs.FullDiameterMeters
            );
            double R_s = CalculateWindingResistance(
                secondaryWindingLength,
                secondaryWireSpecs.ConductingDiameterMeters,
                secondaryWireSpecs.Resistivity
            );
            return new BasicCalculationsFlybackSpecs(
                I_p_avg, I_p_peak, L_p, L_s,
                maximumEnergyPerCycle, 
                N_p, N_s, B_max, willSaturate, proportionSaturated,
                expectedFluxLinkageSecondary,
                expectedMagneticFluxSecondary,
                expectedFluxDensityCore,
                primaryWindingLength,
                nLayersPrimary,
                R_p,
                secondaryWindingLength,
                nLayersSecondary,
                R_s);
        }
        public static double CalculatePrimaryInductanceRequired(
            double P_out, double I_p_peak, double f_sw, double efficiency)
        {
            return (2d * P_out) / (efficiency * Math.Pow(I_p_peak, 2) * f_sw);
        }
        public static double CalculateSecondaryInductanceRequired(
    double P_out, double I_s_peak, double f_sw, double efficiency)
        {
            return (2d * P_out) / (efficiency * Math.Pow(I_s_peak, 2) * f_sw);
        }
        public static double CalculateEnergyStoredInCoreWhenTurnOff(double L_p, double I_p_peak)
        {
            return 0.5d * L_p * Math.Pow(I_p_peak, 2);
        }
        public static double CalculatePeakCurrentCCM(
            out double I_p_peak_theoretical_efficient)
        {
            throw new NotImplementedException();

            //return  P_out* currentRippleFactor / (efficiency * V_in_min * D_max);
        }
        public static double CalculatePeakCurrentDCM(double P_out,
            double efficiency, double V_in_min, double D_max,
            out double I_p_peak_theoretical_efficient)
        {
            I_p_peak_theoretical_efficient = (2d * P_out) / (V_in_min * D_max);
            return (2d * P_out) / (efficiency * V_in_min * D_max);
        }
        public static double CalculateNumberPrimaryTurnsRequired(double primaryInductanceHenries, double aLNanoHenriesPerTurnSquared)
        {
            return Math.Sqrt(primaryInductanceHenries * Math.Pow(10, 9) / aLNanoHenriesPerTurnSquared);
        }
        public static double CalculateBMax(double V_in_max, double D_max, double N_p, double A_e, double f_s)
        {
            return (V_in_max * D_max) / (N_p * A_e * f_s);
        }
        public static double CalculateTurnsRatioForCCMSimple(double V_out, double V_in_min, double D_max)
        {
            return V_out * (1 - D_max) / (V_in_min * D_max);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="V_out"></param>
        /// <param name="V_in_min">typically the minimum input voltage as this requires the highest turns ratio Ns/Np</param>
        /// <param name="D_max"></param>
        /// <returns></returns>
        public static double CalculateTurnsRatioForDCMSimple(double V_out, double V_in_min, double D_max)
        {
            return V_out * (1 - D_max) / (V_in_min * D_max);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="D_max"> desired maximum duty cycle</param>
        /// <param name="f_sw">switching frequency</param>
        /// <param name="V_out">desired output voltage</param>
        /// <param name="V_d"></param>
        /// <param name="V_in_min">minimum input voltage</param>
        /// <param name="V_ds_on">fet voltage drop while on (semiconductor driving primary)</param>
        /// <param name="V_RS">current sense resistor voltage drop (on primary in series with fet)</param>
        /// <param name="x_minimumIdleTime"></param>
        /// <returns></returns>
        public static double CalculateTurnsRatioForDCM(double D_max, double f_sw, double V_out, double V_d, double V_in_min, double V_ds_on, double V_RS, double x_minimumIdleTime)
        {
            double t1 = D_max / f_sw;
            double n = (((1d / f_sw) * (1d - x_minimumIdleTime) - t1) * (V_out + V_d)) / ((V_in_min - V_ds_on - V_RS) * t1);
            return n;
        }
        private static double CalculateMagneticFluxBuiltUpInCoreBeforeTurnOff(double L_p, double I_p_peak, double N_p)
        {
            return L_p * I_p_peak / N_p;
        }
        private static double CalculateVs(double L_p, double I_p_peak, double N_p, double N_s, double deltaT)
        {
            double magneticFlux = CalculateMagneticFluxBuiltUpInCoreBeforeTurnOff(L_p, I_p_peak, N_p);
            return -N_s * magneticFlux / deltaT;
        }
        public static double CalculateSecondaryInductance(double L_p, double n)
        {
            return L_p / Math.Pow(1d / n, 2);
        }
        public static double CalculateTotalWindingLength(
    out int layers,
    CoreCrossSection coreCrossSection,
    double bobbinWidthToCore,
    double bobbinLengthForWindings,
    double totalTurns,
    double wireDiameter,
    double spacingFactor = 1.0) // Spacing factor with a default of 1.0 (no extra space)
        {
            // Adjust wire diameter to account for spacing between turns
            double effectiveDiameter = wireDiameter * spacingFactor;

            // Calculate turns per layer based on the effective diameter
            double turnsPerLayer = (int)Math.Ceiling(bobbinLengthForWindings / effectiveDiameter);

            // Calculate the number of layers required to accommodate the total turns
            layers = (int)Math.Ceiling(totalTurns / turnsPerLayer);

            double totalLength = 0.0;
            Type crossSectionType = coreCrossSection.GetType();
            if (typeof(RectangularCoreCrossSection).IsAssignableFrom(crossSectionType))
            {
                RectangularCoreCrossSection rectangularCoreCrossSection = (RectangularCoreCrossSection)coreCrossSection;
                // Sum the perimeter of each layer
                for (int n = 0; n < layers; n++)
                {
                    double distanceFromCore = bobbinWidthToCore + n * effectiveDiameter; // Distance of current layer from core
                    double perimeter = 2 * (rectangularCoreCrossSection.Width + 2 * distanceFromCore) + 2 * (rectangularCoreCrossSection.Thickness + 2 * distanceFromCore); // Perimeter for rectangular winding
                    double turnsInThisLayer = Math.Min(turnsPerLayer, totalTurns - n * turnsPerLayer); // Turns in current layer
                    totalLength += perimeter * turnsInThisLayer; // Add length for all turns in this layer
                }
            }
            else if (typeof(CircularCoreCrossSection).IsAssignableFrom(crossSectionType))
            {
                CircularCoreCrossSection circularCoreCrossSection = (CircularCoreCrossSection)coreCrossSection;
                for (int n = 0; n < layers; n++)
                {
                    double distanceFromCore = bobbinWidthToCore + n * effectiveDiameter; // Distance of current layer from core
                    double totalRadius = circularCoreCrossSection.Radius + distanceFromCore;
                    double perimeter = 2 * Math.PI * totalRadius;
                    double turnsInThisLayer = Math.Min(turnsPerLayer, totalTurns - n * turnsPerLayer); // Turns in current layer
                    totalLength += perimeter * turnsInThisLayer; // Add length for all turns in this layer
                }
            }
            else {
                throw new NotImplementedException($"Not implemented for {nameof(CoreCrossSection)} derived class {crossSectionType.Name}");
            }
            return totalLength;
        }



        public static double CalculateWindingResistance(double length, double diameter, double resistivity)
        {
            // Cross-sectional area of the wire (A = π * (d / 2)^2)
            double crossSectionalArea = Math.PI * Math.Pow(diameter / 2, 2);

            // Calculate the resistance (R = ρ * L / A)
            double resistance = resistivity * length / crossSectionalArea;

            return resistance;
        }
    }
}
