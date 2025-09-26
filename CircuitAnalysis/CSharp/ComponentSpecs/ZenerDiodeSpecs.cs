namespace CircuitAnalysis.ComponentSpecs
{
    public abstract class ZenerDiodeSpecs : SpecsBase
    {
        // Nominal Zener Voltage (V)
        public abstract double VzNom { get; set; }

        // Minimum Zener Voltage (V)
        public abstract double VzMin { get; set; }

        // Maximum Zener Voltage (V)
        public abstract double VzMax { get; set; }

        // Test Current for Zener Voltage (A)
        public abstract double Izt { get; set; }

        // Maximum Zener Impedance at Izt (Ω)
        public abstract double Zzt { get; set; }

        // Maximum Reverse Leakage Current (A)
        public abstract double IrMax { get; set; }

        // Reverse Voltage at which Ir is specified (V)
        public abstract double Vr { get; set; }

        // Maximum Zener Current (A)
        public abstract double Izm { get; set; }

        // Maximum Noise Density (V/√Hz)
        public abstract double NoiseDensity { get; set; }

        // Maximum Power Dissipation (W)
        public abstract double PdMax { get; set; }

        // Operating and Storage Temperature Range (°C)
        public abstract double TjMax { get; set; }
    }
}
