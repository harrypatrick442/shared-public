using CircuitAnalysis.ComponentSpecs;

namespace CircuitAnalysis
{
    public class MFR_25FBF52_30K1 : ResistorSpecs
    {
        public override string Name => "MFR-25FBF52-30K1";

        public override double Resistance => 30100;

        public override double Tolerance => 0.01;

        public override double PowerRating => 0.25;

        public override string DataSheetUrl => "https://www.mouser.co.uk/datasheet/2/447/YAGEO_MFR_datasheet_2023v3-3324391.pdf";

        public override double ParasiticCapacitance => throw new NotImplementedException();
    }
}
