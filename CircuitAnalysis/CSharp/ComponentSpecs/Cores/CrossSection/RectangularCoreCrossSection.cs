namespace CircuitAnalysis.ComponentSpecs.Cores.CrossSection
{
    public class RectangularCoreCrossSection:CoreCrossSection
    {
        public double Width { get; }
        public double Thickness { get; }
        public RectangularCoreCrossSection(double width, double thickness) { 
            Width = width;
            Thickness = thickness;
        }
    }
}
