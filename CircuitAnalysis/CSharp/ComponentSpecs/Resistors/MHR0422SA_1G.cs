using CircuitAnalysis.ComponentSpecs;

namespace CircuitAnalysis
{
    public class MHR0422SA_1G : ResistorSpecs
    {
        public override string Name => "MHR0422SA_1G";

        public override double Resistance => 1e9;

        public override double Tolerance => 0.01;

        public override double PowerRating => 1.3d;

        public override string DataSheetUrl => "https://www.mouser.co.uk/ProductDetail/Murata-Power-Solutions/MHR0422SA108F70?qs=T3oQrply3y%252BIxqV4afI14A%3D%3D";

        public override double ParasiticCapacitance => throw new NotImplementedException();
    }
}
