namespace CircuitAnalysis.ComponentSpecs
{
    public abstract class DiodeSpecs : SpecsBase
    {
        /// <summary>
        /// Vrrm stands for "Repetitive Peak Reverse Voltage." 
        /// It is the maximum reverse voltage a diode can withstand repeatedly without breaking down.
        /// </summary>
        public abstract double V_rrm { get; }

        /// <summary>
        /// trr stands for "Reverse Recovery Time."
        /// It is the time required for the diode to switch from conducting in the forward direction to blocking in the reverse direction.
        /// </summary>
        public abstract double t_rr { get; }

        /// <summary>
        /// Tjmax stands for "Maximum Junction Temperature."
        /// It is the highest allowable temperature of the diode's semiconductor junction during operation.
        /// </summary>
        public abstract double T_jmax { get; }

        /// <summary>
        /// VF stands for "Forward Voltage."
        /// It is the voltage drop across the diode when it is conducting in the forward direction.
        /// </summary>
        public abstract double V_F { get; }

        /// <summary>
        /// IFAV stands for "Max Average Forward Current" in the context of a diode's specifications. 
        /// It is the maximum average current that a diode can conduct in the forward direction over time without exceeding its thermal limits.
        /// </summary>
        public abstract double I_FAV { get; }

        /// <summary>
        /// Peak forward surge current.
        /// </summary>
        public abstract double I_FSM { get; }

        /// <summary>
        /// Repetitive peak forward current.
        /// </summary>
        public abstract double I_FRM { get; }

        /// <summary>
        /// Rtha stands for "Thermal Resistance, Junction to Ambient."
        /// It represents the diode's ability to dissipate heat, measured as the thermal resistance between the junction and the surrounding environment.
        /// </summary>
        public abstract double R_tha { get; }
    }
}
