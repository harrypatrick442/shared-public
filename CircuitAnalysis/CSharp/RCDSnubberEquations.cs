using Core.Enums;

namespace CircuitAnalysis
{
    public static class RCDSnubberEquations
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="L_leak">Leakage inductance in henries</param>
        /// <param name="I_peak">Peak primary current in amperes</param>
        /// <param name="V_clamp">Clamping voltage in volts</param>
        /// <param name="f_s">Switching frequency</param>
        /// <returns></returns>
        public static RCDSnubberSpecs SizeSnubber(
            double L_leak,
            double I_peak,
            double V_clamp,
            double f_s,
            double? C = null)
        {
            // Energy stored in leakage inductance
            double E_leak = 0.5 * L_leak * Math.Pow(I_peak, 2); // Joules

            // Capacitor calculation
            double minimumC = 2 * E_leak / Math.Pow(V_clamp, 2); // Farad
            if (C == null)
            {
                C = minimumC;
            }
            else { 
                if((double)C< minimumC) {
                    throw new Exception($"Preselected capacitor needs to be at least {minimumC} {UnitsType.Capacitance.GetString()}");
                }
            }
            // Resistor calculation (critical damping approximation)
            double R = Math.Sqrt(L_leak / C.Value); // Ohms

            // Resistor power dissipation
            double P_R = E_leak * f_s; // Watts

            // Diode peak current
            double I_diode = V_clamp / R; // Amperes
            return new RCDSnubberSpecs(E_leak, C.Value, R, P_R, I_diode, V_clamp);
        }
        public static RCDSnubberSpecs SizeSnubber2(
            double L_lk1,
            double I_peak,
            double n,//np over ns?
            double V_o,//maximum desired output voltage upon secondary output capacitor
            double f_s)
        {
            double V_sn = 2.5 * n * V_o;//2 to 2.5 x n V_o. very small value results in severe loss in the snubber circuit

            double t_s = L_lk1 * I_peak / (V_sn - n * V_o);
            if (t_s <= 0) 
                throw new Exception("t_s should be positive");
            double P_sn = V_sn * (I_peak * t_s) * f_s / 2d;
            double R_sn = Math.Pow(V_sn, 2) / P_sn;
            double desired_dV_sn = 0.5;
            //5-10% ripple is reasonable.
            double C_sn = V_sn / (desired_dV_sn * R_sn * f_s);
            //double dV_sn = V_sn / (C_sn * R_sn * f_s);//voltage ripple

            return new RCDSnubberSpecs(-1d, C_sn, R_sn, P_sn,  I_peak, V_sn);
        }
        public static RCDSnubberSpecs SizeSnubber3(
    double L_lk1,   // Leakage inductance [H]
    double I_peak,  // Peak primary current [A]
    double n,       // Turns ratio (np/ns)
    double V_o,     // Output voltage on secondary side [V]
    double f_s      // Switching frequency [Hz]
)
        {
            // Step 1: Choose clamp voltage (2.0–2.5× reflected voltage is typical)
            double V_sn = 2.5 * n * V_o;

            // Step 2: Compute effective voltage across the leakage inductance during snubber conduction
            double V_reflected = n * V_o;
            double V_effective = V_sn - V_reflected;

            if (V_effective <= 0)
                throw new Exception("Clamp voltage must exceed reflected output voltage");

            // Step 3: Estimate conduction time of snubber path
            double t_s = (L_lk1 * I_peak) / V_effective;

            // Step 4: Calculate nominal power dissipated in the snubber resistor
            double P_sn_nominal = V_sn * (I_peak * t_s) * f_s / 2.0;

            // Step 5: Calculate resistor value using *nominal* power (correct approach)
            double R_sn = Math.Pow(V_sn, 2) / P_sn_nominal;

            // Step 6: Add 25% power margin for real-world resistor rating
            double P_sn = 1.25 * P_sn_nominal;

            // Step 7: Capacitor sizing based on allowed ripple (e.g., 0.5 V ripple)
            double desired_dV_sn = 0.5; // Acceptable voltage ripple across C [V]
            double C_sn = V_sn / (desired_dV_sn * R_sn * f_s);

            // Final result packaged into return type
            throw new NotImplementedException("Issue with tau");
            /*
            return new RCDSnubberSpecs(
                tau: -1.0,      // Optional, if you want to compute RC time constant elsewhere
                C_sn: C_sn,
                R_sn: R_sn,
                P_sn: P_sn,     // Power rating needed for R
                I_peak: I_peak,
                V_sn: V_sn
            );*/
        }

    }
}
