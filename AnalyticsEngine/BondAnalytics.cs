using QuantLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine
{
    public class BondAnalytics : IBondAnalytics
    {
        Dictionary<string, FixedRateBond> bonds = new Dictionary<string,FixedRateBond>();

        public BondAnalytics()
        {
            //Set up bond data
            var schedule = new Schedule(new Date(15, Month.May, 2007),
                new Date(15, Month.May, 2017),
                new Period(Frequency.Semiannual),
                new UnitedStates(UnitedStates.Market.GovernmentBond),
                BusinessDayConvention.Unadjusted,
                BusinessDayConvention.Unadjusted,
                DateGeneration.Rule.Backward,
                false);

            var bond = new FixedRateBond(2,
                100000000.0,
                schedule,
                new DoubleVector(1) { 0.04 },
                new ActualActual(ActualActual.Convention.Bond),
                BusinessDayConvention.ModifiedFollowing,
                100.0,
                new Date(15, Month.May, 2007));

            bonds.Add("Test", bond);
        }

        public double CalculateYield(string name, double price)
        {
            if (!bonds.ContainsKey(name))
                return 0.0;

            return bonds[name].yield(price, new Actual360(), Compounding.Compounded, Frequency.Annual);
        }
    }
}
