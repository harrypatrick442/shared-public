using CircuitAnalysis.ComponentSpecs.Cores.CrossSection;

namespace CircuitAnalysis.ComponentSpecs
{
    public class PC40_UY30_MnZn_U : CoreSpecs
    {
        public override double A_L => 120;
        public override double A_L_tollerance => 0.25;

        public override double A_e => 626.8/ Math.Pow(10, 6);

        public override double B_sat => 0.380;/*at 100 degrees c*/

        public override double RelativePermeability => 2000;
        public override CoreCrossSection CrossSection => new CircularCoreCrossSection(radius: 0.014125);

        public override string DataSheetUrl => throw new NotImplementedException();
    }
}
