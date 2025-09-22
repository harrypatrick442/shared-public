namespace Maths.Interfaces;
public interface IMathematicalConstant: IMathematicalEntity
{
    double Value { get; }
    string Approximation { get; }
    char AsciiCharacter { get; }
    string HexCharacterCode { get; }

    IMathematicalConstant[] RelatedConstants { get; }
}
