using CircuitAnalysis.ComponentSpecs;

namespace CircuitAnalysis
{
    public class SM108035005FE : ResistorSpecs
    {
        public override string Name => "SM108035005FE";

        public override double Resistance => 50e6;

        public override double Tolerance => 0.01;

        public override double PowerRating => 2.5;

        public override string DataSheetUrl => "https://www.ohmite.com/assets/images/res-slimmox.pdf";

        public override double ParasiticCapacitance => 0.60e-12;
    }
}
/*4x SM104035005FE 50M gives 200 ohms, 40kv tolerance which is excessive but yields a 0.7/4 pico farad capacitance.
This combined with the lower 200 ohms waste a bit more power but gives better capacitance and stability

3 SM108031006FE gives 300 ohms and 0.2pf capacitance for better price. More cost effective but higher capacitance

a single SM108032006FE gives 0.6pf and slightly more power wastage for slightly improved stability with 20kv rating.
* 
* 4 SM108035005FE in series gives 200M and 0.65/4 pf capacitance. Probably the best option. cost £16
*/
