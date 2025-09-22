using CircuitAnalysis.ComponentSpecs;

namespace CircuitAnalysis
{
    public class MFR_25FBF52_45K3 : ResistorSpecs
    {
        public override string Name => "MFR-25FBF52-45K3";

        public override double Resistance => 45300;

        public override double Tolerance => 0.01;

        public override double PowerRating => 0.25;

        public override string DataSheetUrl => "https://www.mouser.co.uk/datasheet/2/447/YAGEO_MFR_datasheet_2023v3-3324391.pdf";

        public override double ParasiticCapacitance => throw new NotImplementedException();
    }
}
