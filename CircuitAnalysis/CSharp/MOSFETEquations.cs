using CircuitAnalysis.ComponentSpecs;

namespace CircuitAnalysis
{
    using System;

    public static class MOSFETEquations
    {
        /// <summary>
        /// Calculates the optimal gate resistor for a MOSFET driver.
        /// </summary>
        /// <param name="gateCapacitance">Gate capacitance in Farads (F)</param>
        /// <param name="gateDriveVoltage">Gate drive voltage VDD in Volts (V)</param>
        /// <param name="gateThresholdVoltage">MOSFET gate threshold voltage VGS(th) in Volts (V)</param>
        /// <param name="desiredSwitchingTime">Desired switching time in Seconds (s)</param>
        /// <returns>Calculated gate resistor value in Ohms (Ω)</returns>
        public static double CalculateGateResistor(
            double gateCapacitance,
            double gateDriveVoltage,
            double gateThresholdVoltage,
            double desiredSwitchingTime)
        {
            if (gateCapacitance <= 0 || gateDriveVoltage <= 0 || desiredSwitchingTime <= 0)
            {
                throw new ArgumentException("Input values must be greater than zero.");
            }

            if (gateThresholdVoltage >= gateDriveVoltage)
            {
                throw new ArgumentException("Gate threshold voltage must be less than gate drive voltage.");
            }

            // Calculate the natural logarithm term
            double lnTerm = Math.Log(gateDriveVoltage / (gateDriveVoltage - gateThresholdVoltage));

            // Calculate gate resistor (Rg)
            double gateResistor = desiredSwitchingTime / (gateCapacitance * lnTerm);

            return Math.Round(gateResistor, 2); // Rounded to 2 decimal places for readability
        }
        public static double ClampGateDriveResistor(
            double switchingFrequency, 
            double proposedRg, 
            out bool? wasClampedMaxElseMin)
        {
            // Define reasonable Rg ranges based on switching frequency
            double minRg, maxRg;

            if (switchingFrequency >= 1_000_000) // 1 MHz or higher (RF, GaN)
            {
                minRg = 0.5;
                maxRg = 2;
            }
            else if (switchingFrequency >= 100_000) // 100 kHz – 1 MHz (SMPS, Class D, fast switching)
            {
                minRg = 1;
                maxRg = 10;
            }
            else if (switchingFrequency >= 20_000) // 20 kHz – 100 kHz (PWM inverters, mid-range applications)
            {
                minRg = 4.7;
                maxRg = 22;
            }
            else if (switchingFrequency >= 1_000) // 1 kHz – 20 kHz (low-frequency power switching)
            {
                minRg = 10;
                maxRg = 100;
            }
            else // Below 1 kHz (high-voltage switching, large MOSFETs)
            {
                minRg = 10;
                maxRg = 200;
            }
            // Determine if the resistor value is being clamped
            wasClampedMaxElseMin = null;

            if (proposedRg < minRg)
            {
                wasClampedMaxElseMin = false; // Clamped to min
                proposedRg = minRg;
            }
            else if (proposedRg > maxRg)
            {
                wasClampedMaxElseMin = true; // Clamped to max
                proposedRg = maxRg;
            }

            return proposedRg;
        }

        /// <summary>
        /// Calculates the peak current the gate driver needs to supply.
        /// </summary>
        /// <param name="Vgs">Gate drive voltage in Volts (V)</param>
        /// <param name="Rg">Gate resistor value in Ohms (Ω)</param>
        /// <returns>Peak gate drive current in Amps (A)</returns>
        public static double CalculatePeakGateDriveCurrent(double Vgs, double Rg)
        {
            if (Vgs <= 0 || Rg <= 0)
            {
                throw new ArgumentException("Gate voltage and resistor value must be greater than zero.");
            }

            return Vgs / Rg;
        }
        /// <summary>
        /// Calculates the maximum instantaneous power dissipation.
        /// </summary>
        public static double CalculateMaximumInstantaneousPower(double Vds, double IdMax)
        {
            if (Vds <= 0 || IdMax <= 0)
            {
                throw new ArgumentException("Voltage and current must be greater than zero.");
            }
            return Vds * IdMax;
        }

        /// <summary>
        /// Calculates the junction temperature rise due to an instantaneous power pulse.
        /// </summary>
        public static double CalculateJunctionTemperatureRise(double maxPower, double transientThermalImpedance)
        {
            if (maxPower <= 0 || transientThermalImpedance <= 0)
            {
                throw new ArgumentException("Power and thermal impedance must be greater than zero.");
            }
            return maxPower * transientThermalImpedance;
        }
    /// <summary>
    /// Calculates the effective gate capacitance for a MOSFET.
    /// Includes both standard calculation and optional Miller effect correction.
    /// </summary>
    /// <param name="C_iss">Input capacitance in Farads (F)</param>
    /// <param name="C_rss">Reverse transfer capacitance in Farads (F)</param>
    /// <param name="Vds">Drain-to-source voltage in Volts (V)</param>
    /// <param name="Vgs">Gate drive voltage in Volts (V)</param>
    /// <returns>Effective gate capacitance in Farads (F)</returns>
    public static double CalculateEffectiveGateCapacitance(
            double C_iss, 
            double C_rss, 
            double Vds, 
            double Vgs)
        {
            if (C_iss <= 0 || C_rss <= 0 || Vds <= 0 || Vgs <= 0)
            {
                throw new ArgumentException("Capacitance and voltage values must be greater than zero.");
            }

            if (Vgs >= Vds)
            {
                throw new ArgumentException("Gate drive voltage (Vgs) must be less than Vds for proper Miller effect calculation.");
            }

            // Calculate standard effective gate capacitance
            double standardCapacitance = C_iss + C_rss;

            // Miller effect correction term
            double millerCorrection = C_rss * (Vds / Vgs);

            // Final effective gate capacitance including Miller effect
            double effectiveGateCapacitance = standardCapacitance + millerCorrection;

            return effectiveGateCapacitance;
        }
        /// <summary>
         /// Calculates the continuous gate drive current based on capacitance values.
         /// </summary>
         /// <param name="C_iss">Input capacitance in Farads (F) from the MOSFET datasheet</param>
         /// <param name="C_rss">Reverse transfer capacitance in Farads (F) from the MOSFET datasheet</param>
         /// <param name="Vgs">Gate drive voltage in Volts (V)</param>
         /// <param name="switchingFrequency">Switching frequency in Hertz (Hz)</param>
         /// <returns>Continuous gate drive current in Amps (A)</returns>
        public static double CalculateGateDriveCurrent(double C_iss, double C_rss, double Vgs, double switchingFrequency)
        {
            if (C_iss <= 0 || C_rss < 0 || Vgs <= 0 || switchingFrequency <= 0)
            {
                throw new ArgumentException("Capacitance, voltage, and frequency values must be greater than zero.");
            }

            // Calculate continuous gate drive current
            double gateDriveCurrent = (C_iss + C_rss) * Vgs * switchingFrequency;

            return gateDriveCurrent;
        }
        /// <summary>
        /// Calculates conduction losses in the MOSFET.
        /// </summary>
        public static double CalculateConductionLoss(double drainCurrent, double RdsOn)
        {
            return Math.Pow(drainCurrent, 2) * RdsOn;
        }

        /// <summary>
        /// Calculates switching losses in the MOSFET.
        /// </summary>
        public static double CalculateSwitchingLoss(double Vds, double drainCurrent, double riseTime, double fallTime, double switchingFrequency)
        {
            return 0.5 * Vds * drainCurrent * (riseTime + fallTime) * switchingFrequency;
        }

        /// <summary>
        /// Calculates gate drive losses.
        /// </summary>
        public static double CalculateGateDriveLossFromEffectiveCapacitance(double effectiveGateCapacitance, double gateVoltage, double switchingFrequency)
        {
            return effectiveGateCapacitance * Math.Pow(gateVoltage, 2) * switchingFrequency;
        }

        /// <summary>
        /// Calculates gate drive losses.
        /// </summary>
        public static double CalculateGateDriveLoss(double gateCharge, double gateVoltage, double switchingFrequency)
        {
            return gateCharge * gateVoltage * switchingFrequency;
        }

        /// <summary>
        /// Calculates total power dissipation in the MOSFET.
        /// </summary>
        public static double CalculateTotalPowerLoss(double drainCurrent, double RdsOn,
            double Vds, double riseTime, double fallTime, double switchingFrequency,
            double effectiveGateCapacitance, double gateDriveVoltage, 
            out double proportionConduction, 
            out double proportionSwitching, 
            out double proportionGateDrive)
        {

            double conductionLoss = CalculateConductionLoss(drainCurrent, RdsOn);
            double switchingLoss = CalculateSwitchingLoss(Vds, drainCurrent, riseTime, fallTime, switchingFrequency);
            double gateDriveLoss = CalculateGateDriveLossFromEffectiveCapacitance(effectiveGateCapacitance, gateDriveVoltage, switchingFrequency);
            double totalLoss = conductionLoss + switchingLoss + gateDriveLoss;
            proportionConduction = conductionLoss / totalLoss;
            proportionSwitching = switchingLoss / totalLoss;
            proportionGateDrive = gateDriveLoss / totalLoss;
            return totalLoss;
        }
        /// <summary>
        /// Calculates the MOSFET junction temperature.
        /// </summary>
        public static double CalculateJunctionTemperature(double ambientTemp, double powerDissipation, double thermalResistanceJA)
        {
            return ambientTemp + (powerDissipation * thermalResistanceJA);
        }

        /// <summary>
        /// Calculates the MOSFET case temperature.
        /// </summary>
        public static double CalculateCaseTemperature(double ambientTemp, double powerDissipation, double thermalResistanceJC)
        {
            return ambientTemp + (powerDissipation * thermalResistanceJC);
        }

        /// <summary>
        /// Calculates the required heatsink thermal resistance.
        /// </summary>
        public static double CalculateRequiredHeatsinkResistance(double caseTemp, double ambientTemp, double powerDissipation)
        {
            if (powerDissipation <= 0)
            {
                throw new ArgumentException("Power dissipation must be greater than zero.");
            }
            return (caseTemp - ambientTemp) / powerDissipation;
        }
        /// <summary>
        /// Calculates the MOSFET junction temperature with a heatsink.
        /// </summary>
        public static double CalculateJunctionTemperatureWithHeatsink(
            double ambientTemp, double powerDissipation, double thermalResistanceJC, double thermalResistanceHS)
        {
            return ambientTemp + (powerDissipation * (thermalResistanceJC + thermalResistanceHS));
        }

        /// <summary>
        /// Calculates PCB copper trace resistance.
        /// </summary>
        public static double CalculatePCBTraceResistance(double resistivity, double traceLength, double traceArea)
        {
            if (traceLength <= 0 || traceArea <= 0)
            {
                throw new ArgumentException("Trace length and area must be greater than zero.");
            }
            return resistivity * (traceLength / traceArea);
        }

    }
}
