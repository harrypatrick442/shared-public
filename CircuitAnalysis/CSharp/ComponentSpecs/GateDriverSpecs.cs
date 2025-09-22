using Core.Maths.Tensors;
using System.Drawing;

namespace CircuitAnalysis.ComponentSpecs
{
    public abstract class GateDriverSpecs : SpecsBase
    {
        // Maximum Supply Voltage (V)
        public abstract double VddMax { get; set; }

        // Minimum Supply Voltage (V)
        public abstract double VddMin { get; set; }

        // Input High Voltage Threshold (V)
        public abstract double Vih { get; set; }

        // Input Low Voltage Threshold (V)
        public abstract double Vil { get; set; }

        // Maximum Peak Output Current (A)
        public abstract double Ipk { get; set; }

        // Output Resistance (Ohms)
        public abstract double Ro { get; set; }

        // Rise Time (s)
        public abstract double Tr { get; set; }

        // Fall Time (s)
        public abstract double Tf { get; set; }

        // Propagation Delay Time (s)
        public abstract double Td1 { get; set; }

        // Propagation Delay Time (s) (second delay measurement)
        public abstract double Td2 { get; set; }

        // Quiescent Current at High Input (A)
        public abstract double IqHigh { get; set; }

        // Quiescent Current at Low Input (A)
        public abstract double IqLow { get; set; }

        // Maximum Junction Temperature (°C)
        public abstract double TjMax { get; set; }

        // Thermal Resistance, Junction-to-Ambient (°C/W)
        public abstract double RthJa { get; set; }

        // Latch-Up Protection Current (A)
        public abstract double LatchUpCurrent { get; set; }
    }
}
