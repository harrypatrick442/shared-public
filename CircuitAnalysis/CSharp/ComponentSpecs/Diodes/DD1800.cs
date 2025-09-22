using CircuitAnalysis.ComponentSpecs;

namespace CircuitAnalysis
{
    public class DD1800 : DiodeSpecs
    {
        public override double V_rrm => 18000;

        public override double t_rr => 1501e-9;

        public override double T_jmax => 423.15;

        public override double V_F => 40;

        public override double I_FAV => 0.020;

        public override double I_FSM => 0.5;

        public override double R_tha => 60;

        public override double I_FRM => 0.3d;

        public override string DataSheetUrl => throw new NotImplementedException();
    }
}
