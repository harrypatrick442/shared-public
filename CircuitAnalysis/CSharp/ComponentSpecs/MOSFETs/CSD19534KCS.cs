namespace CircuitAnalysis.ComponentSpecs
{
    using CircuitAnalysis.ComponentSpecs;

    public class CSD19534KCS : MosfetSpecs
    {
        public override string DataSheetUrl => throw new NotImplementedException();
        public override double VdsMax { get; } = 100;  // Volts
        public override double VgsMax { get; } = 20;  // Volts
        public override double VgsThresholdMin { get; } = 2.4; // Volts
        public override double VgsThresholdTyp { get; } = 2.8; // Volts
        public override double VgsThresholdMax { get; } = 3.4; // Volts
        public override double IdMax { get; } = 54; // Amps (Silicon limited, TC = 25°C)
        public override double IdPulsedMax { get; } = 138; // Amps (Pulsed Drain Current)
        public override double RdsOn { get; } = 1.37e-2; // Ohms (at VGS = 10V)
        public override double TotalGateCharge { get; } = 1.71e-8; // Coulombs (17.1 nC)
        public override double GateSourceCharge { get; } = 5.1e-9; // Coulombs (5.1 nC)
        public override double GateDrainCharge { get; } = 3.2e-9; // Coulombs (3.2 nC)
        public override double Ciss { get; } = 1.29e-9; // Farads (1290 pF)
        public override double Coss { get; } = 2.57e-10; // Farads (257 pF)
        public override double Crss { get; } = 5.7e-12; // Farads (5.7 pF)
        public override double TurnOnDelayTime { get; } = 6e-9; // Seconds (6 ns)
        public override double RiseTime { get; } = 2e-9; // Seconds (2 ns)
        public override double TurnOffDelayTime { get; } = 9e-9; // Seconds (9 ns)
        public override double FallTime { get; } = 1e-9; // Seconds (1 ns)
        public override double TjMax { get; } = 175; // Degrees Celsius
        public override double RthJc { get; } = 1.3; // °C/W
        public override double RthJa { get; } = 62; // °C/W
    }


}
