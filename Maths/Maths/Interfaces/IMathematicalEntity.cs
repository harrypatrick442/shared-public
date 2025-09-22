using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace Maths.Interfaces;
public interface IMathematicalEntity
{
    string EquationFor { get; }
    string BriefDescription { get; }
    string[] Characteristics { get; }
    string Origin { get; }
    string[] Applications { get; }
    Image<Rgba32> GraphicalRepresentation { get; }
}
