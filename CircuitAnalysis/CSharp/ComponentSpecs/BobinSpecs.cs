using System.Drawing;

namespace CircuitAnalysis.ComponentSpecs
{
    public abstract class BobinSpecs : SpecsBase
    {
        public abstract double WidthToCore { get; }
        public abstract double LengthForWindings { get; }
        protected BobinSpecs() { 
            
        }
    }
}
