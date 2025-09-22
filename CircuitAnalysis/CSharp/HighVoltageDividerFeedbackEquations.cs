using CircuitAnalysis.ComponentSpecs;

namespace CircuitAnalysis
{
    public class HighVoltageDividerFeedbackEquations
    {
        public static void Design(
            ZenerDiodeSpecs zenerDiodeSpecs,
            double signalVoltageClampRequired,
            double maximumOperatingSignalVoltage,
            double measuredVoltageMax,
            double measuredVoltageMin,
            double nIntervalsMin,
            double powerConsumptionMax,
            double maximumSlewRateMeasuredVoltage,
            double safetyFactor,
            ResistorSpecs? highSideResistor = null,
            ResistorSpecs? lowSideResistor = null) {
            if (zenerDiodeSpecs.VzMin < maximumOperatingSignalVoltage) {
                throw new Exception($"Zener diode clamp had too high {nameof(zenerDiodeSpecs.VzMax)}");
            }
            double minimumResistanceRequired = Math.Pow(measuredVoltageMax, 2) / powerConsumptionMax;
            Console.WriteLine($"The minimum divider series resistance required to fulfill {nameof(powerConsumptionMax)}" +
                $" is {minimumResistanceRequired} ohms.");
            if(highSideResistor==null^ lowSideResistor == null) {
                throw new Exception("If providing resistor specs provide both high side and low side");
            }
            if (highSideResistor == null)
            {
                ForSeriesResistance(
                     seriesResistanceRequired: minimumResistanceRequired,
                     measuredVoltageMax: measuredVoltageMax,
                     measuredVoltageMin: measuredVoltageMin,
                     maximumOperatingSignalVoltage: maximumOperatingSignalVoltage,
                     zenerDiodeSpecs: zenerDiodeSpecs);
            }
            else {
                ForResistorValues(
                    highSideResistor.MinimumResistance,
                    highSideResistor.MaximumResistance,
                    lowSideResistor!.MinimumResistance,
                    lowSideResistor!.MaximumResistance,
                    measuredVoltageMin: measuredVoltageMin,
                    measuredVoltageMax: measuredVoltageMax,
                    zenerDiodeSpecs
                );
                CheckResistorPower(safetyFactor, measuredVoltageMax, highSideResistor!, lowSideResistor!);
                CheckVoltageDropFromParasiticCapacitance(
                    maximumSlewRateMeasuredVoltage,
                    highSideResistor.ParasiticCapacitance);
            }
        }
        private static void ForSeriesResistance(
            double seriesResistanceRequired,
            double measuredVoltageMax,
            double measuredVoltageMin,
            double maximumOperatingSignalVoltage,
            ZenerDiodeSpecs zenerDiodeSpecs)
        {
            double maximumCurrent = measuredVoltageMax / seriesResistanceRequired;
            double highSideResistance = (measuredVoltageMax - maximumOperatingSignalVoltage) / maximumCurrent;
            double lowSideResistance = maximumOperatingSignalVoltage / maximumCurrent;
            Console.WriteLine($"Potential divider values of {highSideResistance}/{lowSideResistance} ohms gives a maximum current of {maximumCurrent} A");
            ForResistorValues(highSideResistance, highSideResistance, 
                lowSideResistance, lowSideResistance,
                measuredVoltageMin, measuredVoltageMax, zenerDiodeSpecs);
        }
        private static void ForResistorValues(
            double highSideResistanceMin,
            double highSideResistanceMax,
            double lowSideResistanceMin,
            double lowSideResistanceMax,
            double measuredVoltageMin,
            double measuredVoltageMax,
            ZenerDiodeSpecs zenerDiodeSpecs) {

            double maximumCurrent = measuredVoltageMax / (highSideResistanceMin+lowSideResistanceMin);
            double minimumCurrent = measuredVoltageMin / (highSideResistanceMax + lowSideResistanceMax);
            Console.WriteLine($"Maximum current: {maximumCurrent} A");
            Console.WriteLine($"Minimum current: {minimumCurrent} A");
            double minimumCurrentCanAllowWithZener = zenerDiodeSpecs.IrMax * 2;
            if (minimumCurrent < minimumCurrentCanAllowWithZener)
            {
                throw new Exception($"The current at {nameof(measuredVoltageMin)} ({measuredVoltageMin} volts) was {minimumCurrent} amps, which is less than {nameof(minimumCurrentCanAllowWithZener)}*2 ({minimumCurrentCanAllowWithZener} amps)");
            }
            Console.WriteLine($"The minimum current corresponding to {nameof(measuredVoltageMin)} is {minimumCurrent} which is {minimumCurrent / zenerDiodeSpecs.IrMax} times the zener diode {nameof(zenerDiodeSpecs.IrMax)} value of {zenerDiodeSpecs.IrMax}");
        }
        private static void CheckResistorPower(
            double safetyFactor, 
            double measuredVoltageMax, 
            ResistorSpecs highSideResistor, 
            ResistorSpecs lowSideResistor) {

            double current = measuredVoltageMax / (highSideResistor.MinimumResistance + lowSideResistor.MinimumResistance);
            double highSideResistorPower = highSideResistor.MinimumResistance * Math.Pow(current, 2);
            if (highSideResistorPower > safetyFactor * highSideResistor.PowerRating) {
                throw new Exception($"The high side resistor maximum power ({highSideResistorPower} watts) exceeded the power rating ({highSideResistor.PowerRating} watts) X the safety factor ({safetyFactor})");
            }
            Console.WriteLine($"High side resistor power was {highSideResistorPower} watts," +
                $" {100d * highSideResistorPower / highSideResistor.PowerRating}% of the " +
                $"resistor power rating {highSideResistor.PowerRating} watts");
            double lowSideResistorPower = lowSideResistor.MinimumResistance * Math.Pow(current, 2);
            if (lowSideResistorPower > safetyFactor * lowSideResistor.PowerRating)
            {
                throw new Exception($"The low side resistor maximum power ({highSideResistorPower} watts) exceeded the power rating ({highSideResistor.PowerRating} watts) X the safety factor ({safetyFactor})");
            }
            Console.WriteLine($"Low side resistor power was {lowSideResistorPower} watts," +
                $" {100d * lowSideResistorPower / lowSideResistor.PowerRating}% of the " +
                $"resistor power rating {lowSideResistor.PowerRating} watts");

        }
        private static void CheckVoltageDropFromParasiticCapacitance(
            double maximumSlewRateMeasuredVoltage,
            double parasiticCapacitanceHighSideResistor) {
            double maximumBiasCurrent = maximumSlewRateMeasuredVoltage * parasiticCapacitanceHighSideResistor;

        }
    }
}
