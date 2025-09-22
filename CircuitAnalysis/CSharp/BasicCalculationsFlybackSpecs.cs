namespace CircuitAnalysis
{
    public class BasicCalculationsFlybackSpecs
    {
        public double I_p_avg { get; }
        public double I_p_peak { get; }
        public double I_s_peak
        {
            get { return I_p_peak *NpOverNs; }
        }
        public double L_p {get; }
        public double L_s { get; }
        public double N_p { get; }
        public double N_s { get; }
        public double NpOverNs { get { return N_p / N_s; } }
        public double NsOverNp { get { return N_s / N_p; } }
        public double B_max { get; }
        public double ProportionSaturated { get; }
        public bool WillSaturate { get; }

        public double ExpectedFluxLinkageSecondary { get; }
        public double ExpectedMagneticFluxSecondary { get; }
        public double ExpectedFluxDensityCore { get; }
        public double PrimaryWindingLength { get; }
        public double NLayersPrimary { get; }
        public double R_p { get; }
        public double SecondaryWindingLength { get; }
        public double NLayersSecondary { get; }
        public double MaximumEnergyPerCycle { get; }
        public double R_s { get; }
        public BasicCalculationsFlybackSpecs(
            double I_p_avg, double I_p_peak, double L_p, double L_s,
            double maximumEnergyPerCycle,
            double N_p, double N_s, double B_max, bool willSaturate,
            double proportionSaturated,
            double expectedFluxLinkageSecondary,
            double expectedMagneticFluxSecondary,
            double expectedFluxDensityCore,
            double primaryWindingLength,
            double nLayersPrimary,
            double R_p,
            double secondaryWindingLength,
            double nLayersSecondary,
            double R_s) { 
            this.I_p_avg = I_p_avg;
            this.I_p_peak = I_p_peak;
            this.L_p = L_p;
            this.L_s = L_s;
            MaximumEnergyPerCycle = maximumEnergyPerCycle;
            this.N_p = N_p;
            this.N_s = N_s;
            this.B_max = B_max;
            WillSaturate = willSaturate;
            ProportionSaturated = proportionSaturated;
            ExpectedFluxLinkageSecondary = expectedFluxLinkageSecondary;
            ExpectedMagneticFluxSecondary = expectedMagneticFluxSecondary;
            ExpectedFluxDensityCore = expectedFluxDensityCore;
            PrimaryWindingLength = primaryWindingLength;
            this.R_p = R_p;
            SecondaryWindingLength = secondaryWindingLength;
            this.R_s = R_s;
        }
    }
}
