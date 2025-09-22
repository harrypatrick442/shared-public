using CircuitAnalysis.ComponentSpecs.Cores.CrossSection;
using Core.Maths.Tensors;
using System.Drawing;

namespace CircuitAnalysis.ComponentSpecs
{
    public abstract class CoreSpecs : SpecsBase
    {
        /// <summary>
        /// inductance per square turn in nano henries
        /// </summary>
        public abstract double A_L { get; }
        public abstract double A_L_tollerance { get; }
        public abstract double A_e { get; }
        public abstract double B_sat { get; }
        public abstract double RelativePermeability { get; }
        public abstract CoreCrossSection CrossSection { get; }
    }
}
