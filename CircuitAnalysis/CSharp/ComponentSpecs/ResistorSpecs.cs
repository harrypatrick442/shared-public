namespace CircuitAnalysis.ComponentSpecs
{
    public abstract class ResistorSpecs : SpecsBase {
        public abstract string Name { get; }
        public abstract double Resistance { get; }
        public abstract double Tolerance { get; }
        public abstract double PowerRating { get; }
        public double MinimumResistance => (1 - Tolerance) * Resistance;
        public double MaximumResistance => (1 + Tolerance) * Resistance;
        public abstract double ParasiticCapacitance { get; } 
        // Represents the small inherent capacitance of the resistor
    }
}
