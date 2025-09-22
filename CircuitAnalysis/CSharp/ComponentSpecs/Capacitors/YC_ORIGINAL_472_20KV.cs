namespace CircuitAnalysis.ComponentSpecs
{
    public class YC_ORIGINAL_472_20KV : CapacitorSpecs
    {
        public override double Capacitance => 1e-9;
        public override double VoltageRating => 20000;
        public override double Tolerance => 0.2;
        public override string DataSheetUrl => throw new NotImplementedException();
    }
}
