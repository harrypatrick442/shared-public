using System.Drawing;

namespace CircuitAnalysis.ComponentSpecs
{
    public abstract class WireSpecs : SpecsBase
    {
        public abstract double FullDiameterMeters { get; }
        public abstract double ConductingDiameterMeters { get; }
        public abstract double Resistivity { get; }
        protected WireSpecs() { }
    }
}
