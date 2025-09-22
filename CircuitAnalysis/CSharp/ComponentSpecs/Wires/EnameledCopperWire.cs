namespace CircuitAnalysis.ComponentSpecs
{
    public class EnameledCopperWire : WireSpecs
    {
        public int AWG { get; }
        public override double FullDiameterMeters { get; }
        public override double ConductingDiameterMeters { get; }
        public override double Resistivity => Resistivities.Copper;

        public override string DataSheetUrl => throw new NotImplementedException();

        public EnameledCopperWire(int awg)
        {
            AWG = awg;
            ConductingDiameterMeters = WireGaugeHelper.GetConductingDiameter(awg);
            FullDiameterMeters = ConductingDiameterMeters + 2 * WireGaugeHelper.GetEnamelThickness(awg);
        }
    }

}
