namespace CircuitAnalysis.ComponentSpecs
{
    public class _1N4619 : ZenerDiodeSpecs
    {
        public override double VzNom { get; set; } = 3.0;      // Nominal Zener Voltage (V)
        public override double VzMin { get; set; } = 2.850;    // Min Zener Voltage (V)
        public override double VzMax { get; set; } = 3.150;    // Max Zener Voltage (V)
        public override double Izt { get; set; } = 250e-6;     // Test Current (A)
        public override double Zzt { get; set; } = 1600;       // Max Zener Impedance (Ω)
        public override double IrMax { get; set; } = 0.8e-6;   // Max Reverse Leakage Current (A)
        public override double Vr { get; set; } = 1.0;         // Reverse Voltage (V)
        public override double Izm { get; set; } = 85e-3;      // Max Zener Current (A)
        public override double NoiseDensity { get; set; } = 1.0e-6; // Noise Density (V/√Hz)
        public override double PdMax { get; set; } = 0.250;    // Max Power Dissipation (W)
        public override double TjMax { get; set; } = 200;      // Max Junction Temperature (°C)

        public override string DataSheetUrl => throw new NotImplementedException();
    }
}
