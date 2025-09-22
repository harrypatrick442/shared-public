using CircuitAnalysis.ComponentSpecs;
using System.Text;

namespace CircuitAnalysis
{
    public class RCDSnubberSpecs
    {
        /// <summary>
        /// Energy stored in leakage inductance
        /// </summary>
        public double E_leak { get; }
        /// <summary>
        /// Estimated capacitor size required
        /// </summary>
        public double C { get; }
        /// <summary>
        ///Resistor calculation (critical damping approximation)
        /// </summary>
        public double R { get; }
        /// <summary>
        /// Resistor power dissipation
        /// </summary>
        public double P_R { get; }
        /// <summary>
        /// Diode peak current
        /// </summary>
        public double I_diode { get; }
        /// <summary>
        /// Diode voltage rating
        /// </summary>
        public double V_clamp { get; }
        public RCDSnubberSpecs(
            double E_leak, double C, double R,
            double P_R, double I_diode, double V_clamp
            )
        { 
            this.E_leak = E_leak;
            this.C = C;
            this.R = R;
            this.P_R = P_R;
            this.I_diode = I_diode;
            this.V_clamp = V_clamp;
        }
        public void Print() { 
            Console.WriteLine(ToString());
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Estimated capacitor (C): {C} F");
            sb.AppendLine($"Estimated resistor (R): {R} Ω");
            sb.AppendLine($"Resistor power dissipation: {P_R} W");
            sb.AppendLine($"Estimated diode peak current: {I_diode} A");
            sb.AppendLine($"Diode voltage rating: {V_clamp} V");
            return sb.ToString();
        }
    }
}
