namespace CircuitAnalysis.ComponentSpecs.Cores.CrossSection
{
    public class CircularCoreCrossSection : CoreCrossSection
    {
        public double Radius { get; }

        public CircularCoreCrossSection(double radius) {
            Radius = radius;
        }
    }
}
