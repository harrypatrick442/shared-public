namespace CircuitAnalysis.ComponentSpecs
{
    public class TC1413N : GateDriverSpecs
    {
        public override double VddMax { get; set; } = 16; // Volts
        public override double VddMin { get; set; } = 4.5; // Volts
        public override double Vih { get; set; } = 2.0; // Volts (Logic '1' threshold)
        public override double Vil { get; set; } = 0.8; // Volts (Logic '0' threshold)
        public override double Ipk { get; set; } = 3.0; // Amps (Peak output current)
        public override double Ro { get; set; } = 2.7; // Ohms (Output resistance, typical)
        public override double Tr { get; set; } = 20e-9; // Seconds (20 ns rise time)
        public override double Tf { get; set; } = 20e-9; // Seconds (20 ns fall time)
        public override double Td1 { get; set; } = 35e-9; // Seconds (35 ns delay time)
        public override double Td2 { get; set; } = 35e-9; // Seconds (35 ns delay time)
        public override double IqHigh { get; set; } = 0.5e-3; // Amps (500 µA with logic '1' input)
        public override double IqLow { get; set; } = 0.1e-3; // Amps (100 µA with logic '0' input)
        public override double TjMax { get; set; } = 150; // °C (Maximum junction temperature)
        public override double RthJa { get; set; } = 211; // °C/W (Thermal resistance for MSOP package)
        public override double LatchUpCurrent { get; set; } = 0.5; // Amps (Latch-up protection)

        public override string DataSheetUrl => throw new NotImplementedException();
    }
}
