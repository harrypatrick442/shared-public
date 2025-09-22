using System;
using UnitsType = Core.Enums.UnitsType;
namespace Core.Enums
{
    public static class UnitsTypeExtensions
    {
        public static string? GetString(this UnitsType unitsType)
        {
            switch (unitsType)
            {
                case UnitsType.Current:
                    return "A";
                case UnitsType.Voltage:
                    return "V";
                case UnitsType.Charge:
                    return "C";
                case UnitsType.Power:
                    return "W";
                case UnitsType.Resistance:
                    return "Ω";
                case UnitsType.Time:
                    return "s";
                case UnitsType.Frequency:
                    return "Hz";
                case UnitsType.Capacitance:
                    return "F";
                case UnitsType.Inductance:
                    return "H";
                case UnitsType.Radians:
                    return "rad";
                case UnitsType.None:
                    return null;
                default:
                    throw new NotImplementedException(Enum.GetName(typeof(UnitsType), unitsType));
            }
        }
    }
}