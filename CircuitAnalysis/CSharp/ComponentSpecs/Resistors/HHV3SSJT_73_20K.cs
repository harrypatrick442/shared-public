using CircuitAnalysis.ComponentSpecs;

namespace CircuitAnalysis
{
    public class HHV3SSJT_73_20K : ResistorSpecs
    {
        public override string Name => "HHV3SSJT=73-20K";

        public override double Resistance => 2e4;

        public override double Tolerance => 0.05;

        public override double PowerRating => 3;

        public override string DataSheetUrl => "https://www.mouser.co.uk/ProductDetail/YAGEO/HHV3SSJT-73-20K?qs=xZ%2FP%252Ba9zWqatcwW0APOgAA%3D%3D";

        public override double ParasiticCapacitance => throw new NotImplementedException();
    }
}
