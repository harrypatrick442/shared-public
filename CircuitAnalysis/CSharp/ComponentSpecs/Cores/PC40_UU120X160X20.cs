using CircuitAnalysis.ComponentSpecs.Cores.CrossSection;
using Core.Maths.Tensors;
using System.Drawing;

namespace CircuitAnalysis.ComponentSpecs
{
    public class PC40_UU120X160X20 : CoreSpecs
    {
        public override double A_L => 120;
        public override double A_L_tollerance => 0.25;

        public override double A_e => 600/ Math.Pow(10, 6);

        public override double B_sat => 0.380;/*at 100 degrees c*/

        public override double RelativePermeability => 2000;
        public override CoreCrossSection CrossSection => new RectangularCoreCrossSection(0.03, 0.02);

        public override string DataSheetUrl => throw new NotImplementedException();
    }
}
