using CircuitAnalysis.ComponentSpecs;

namespace CircuitAnalysis
{
    public class CompositeSeriesResistor : ResistorSpecs
    {
        public int NUnits { get; }
        private ResistorSpecs Unit { get; }
        public override string Name => Unit.Name;

        public override double Resistance => Unit.Resistance*NUnits;

        public override double Tolerance => Unit.Tolerance;

        public override double PowerRating => Unit.PowerRating * (double)NUnits;

        public override string DataSheetUrl => Unit.DataSheetUrl;

        public override double ParasiticCapacitance => Unit.ParasiticCapacitance / (double)NUnits;

        public CompositeSeriesResistor(ResistorSpecs unit, int nUnits) {  
            Unit = unit; 
            NUnits = nUnits;
        }
    }
}
