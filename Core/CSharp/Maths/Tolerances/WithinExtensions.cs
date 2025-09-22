using System;

namespace Core.Maths.Tolerances
{
    public static class WithinExtensions {
        public static bool Within(this double value, double proportionTolerance, double of) {
            return (value - of) / of <= proportionTolerance;
        }
    }
}
