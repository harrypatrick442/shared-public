namespace CircuitAnalysis.ComponentSpecs
{
    public abstract class CapacitorSpecs : SpecsBase
    {
        public abstract double Capacitance { get; }
        public abstract double VoltageRating { get; }
        public abstract double Tolerance { get; }
        public double MinimumCapacitance => (1d - Tolerance) * Capacitance;
        public double MaximumCapacitance => (1d + Tolerance) * Capacitance;
    }
}
