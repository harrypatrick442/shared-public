using CircuitAnalysis.ComponentSpecs.Cores.CrossSection;
namespace CircuitAnalysis.ComponentSpecs
{
    public class PC40_T80_20_50 : CoreSpecs
    {
        public override double A_L => 4320;
        public override double A_L_tollerance => 0.25;

        public override double A_e => 295 / Math.Pow(10, 6);

        public override double B_sat => 0.380;/*at 100 degrees c*/

        public override double RelativePermeability => 2000;
        public override CoreCrossSection CrossSection => throw new NotImplementedException();

        public override string DataSheetUrl => throw new NotImplementedException();
    }
}
