using CircuitAnalysis.ComponentSpecs;

namespace CircuitAnalysis
{
    public class Ohmite13FR200E : ResistorSpecs
    {
        public override string Name => "Ohmite_13FR200E";

        public override double Resistance => 2e-1;

        public override double Tolerance => 0.01;

        public override double PowerRating => 3;

        public override string DataSheetUrl => "https://www.ohmite.com/assets/images/res-10.pdf";

        public override double ParasiticCapacitance => throw new NotImplementedException();
    }
}
