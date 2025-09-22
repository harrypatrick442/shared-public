using CircuitAnalysis.ComponentSpecs;

namespace CircuitAnalysis
{
    public class SM108033006FE : ResistorSpecs
    {
        public override string Name => "SM108033006FE";

        public override double Resistance => 300e6;

        public override double Tolerance => 0.01;

        public override double PowerRating => 2.5;

        public override string DataSheetUrl => "https://www.ohmite.com/assets/images/res-slimmox.pdf";

        public override double ParasiticCapacitance => 0.6e-12;
    }
}
