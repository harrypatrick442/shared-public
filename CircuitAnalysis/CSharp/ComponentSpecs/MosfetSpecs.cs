namespace CircuitAnalysis.ComponentSpecs
{
    public abstract class MosfetSpecs : SpecsBase
    {
        // Maximum Drain-Source Voltage (V)
        public abstract double VdsMax { get; }

        // Maximum Gate-Source Voltage (V)
        public abstract double VgsMax { get; }

        // Threshold Voltage (V) - Min, Typ, Max
        public abstract double VgsThresholdMin { get; } // Volts
        public abstract double VgsThresholdTyp { get; } // Volts
        public abstract double VgsThresholdMax { get; } // Volts
        // Maximum Continuous Drain Current (A)
        public abstract double IdMax { get; }

        // Maximum Pulsed Drain Current (A)
        public abstract double IdPulsedMax { get; }

        // Drain-Source On Resistance (Ohms)
        public abstract double RdsOn { get; }

        // Total Gate Charge (nC)
        public abstract double TotalGateCharge { get; }

        // Gate-Source Charge (nC)
        public abstract double GateSourceCharge { get; }

        // Gate-Drain Charge (nC)
        public abstract double GateDrainCharge { get; }

        // Input Capacitance (pF)
        public abstract double Ciss { get; }

        // Output Capacitance (pF)
        public abstract double Coss { get; }

        // Reverse Transfer Capacitance (pF)
        public abstract double Crss { get; }

        // Turn-on Delay Time (ns)
        public abstract double TurnOnDelayTime { get; }

        // Rise Time (ns)
        public abstract double RiseTime { get; }

        // Turn-off Delay Time (ns)
        public abstract double TurnOffDelayTime { get; }

        // Fall Time (ns)
        public abstract double FallTime { get; }

        // Maximum Junction Temperature (°C)
        public abstract double TjMax { get; }

        // Thermal Resistance, Junction-to-Case (°C/W)
        public abstract double RthJc { get; }

        // Thermal Resistance, Junction-to-Ambient (°C/W)
        public abstract double RthJa { get; }
    }

}
