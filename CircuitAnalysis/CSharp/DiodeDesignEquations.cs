using CircuitAnalysis.ComponentSpecs;

namespace CircuitAnalysis
{
    public class DiodeDesignEquations
        {
        public static void CalculatePTotalAndOperatingTemperature(
            DiodeSpecs diodeSpecs,
            double I_f, double I_fb,
            double D, double V_r, double f_s,
            double T_ambient,  double safetyFactor,
            out double P_total, out double T_junction) {
            P_total = CalculatePTotal(diodeSpecs,
                safetyFactor,
             I_f, I_fb, D, V_r, f_s);
            T_junction = CalculateTDiode(P_total, T_ambient, diodeSpecs.R_tha);
            if (T_junction*safetyFactor >= diodeSpecs.T_jmax) {
                throw new Exception($"The junction temperature with the safety factor exceeded {nameof(diodeSpecs.T_jmax)}");
            }
        }
        public static double CalculateTDiode(double P_total, double T_ambient, double R_th) {
            return (P_total * R_th) + T_ambient;
        }
        public static double CalculatePTotal(DiodeSpecs diodeSpecs,
            double safetyFactor,
            double I_f, double I_fb, double D, double V_r, double f_s)
        {
            if (I_f * safetyFactor > diodeSpecs.I_FAV)
            {
                throw new Exception($"The junction temperature with the safety factor exceeded {nameof(diodeSpecs.T_jmax)}");
            }
            if (V_r*safetyFactor > diodeSpecs.V_rrm)
            {
                throw new Exception($"The reverse voltage across the diode ({V_r}V) with the safety factor cannot be greater than the diode rated V_rrm ({diodeSpecs.V_rrm}V)");
            }
            return CalculatePConduction(diodeSpecs.V_F, I_f, D) 
                + CalculatePSwitching(V_r, I_fb, diodeSpecs.t_rr, f_s);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="V_f"></param>
        /// <param name="I_f"></param>
        /// <param name="I_fb">forward current through diode before it starts to reverse</param>
        /// <param name="D"></param>
        /// <param name="V_r"></param>
        /// <param name="t_rr"></param>
        /// <param name="f_s"></param>
        /// <returns></returns>
        public static double CalculatePTotal(double V_f, double I_f, double I_fb, double D, double V_r, double t_rr, double f_s)
        {
            return CalculatePConduction(V_f, I_f, D) + CalculatePSwitching(V_r, I_fb, t_rr, f_s);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="V_f">forward voltage drop accross diode</param>
        /// <param name="I_f">average forward current through diode</param>
        /// <param name="D">Duty cycle</param>
        /// <returns></returns>
        private static double CalculatePConduction(double V_f, double I_f, double D) {
            return V_f * I_f * D;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="V_r">reverse voltage across the diode</param>
        /// <param name="I_f">forward current through the diode before it starts to reverse</param>
        /// <param name="t_rr">is the reverse recovery time (time taken for the diode to switch off and stop conducting)</param>
        /// <param name="f_s">switching frequency</param>
        /// <returns></returns>
        private static double CalculatePSwitching(double V_r, double I_f, double t_rr, double f_s) {
            return 0.5 * V_r * I_f * t_rr * f_s;
        }
    }
}
