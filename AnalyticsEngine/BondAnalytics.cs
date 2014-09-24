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
            //Ultra short
            var schedule = new Schedule(new Date(4, Month.November, 2009),
                new Date(22, Month.January, 2015),
                new Period(Frequency.Semiannual),
                new UnitedKingdom(UnitedKingdom.Market.Settlement),
                BusinessDayConvention.ModifiedFollowing,
                BusinessDayConvention.ModifiedFollowing,
                DateGeneration.Rule.Backward,
                false);

            var bond = new FixedRateBond(2,
                100.0,
                schedule,
                new DoubleVector(1) { 0.0275 },
                new ActualActual(),
                BusinessDayConvention.ModifiedFollowing,
                100.0,
                new Date(4, Month.November, 2009));

            bonds.Add("GB00B4LFZR36", bond);

            //Long
            var schedule2 = new Schedule(new Date(3, Month.October, 2007),
                new Date(7, Month.December, 2030),
                new Period(Frequency.Semiannual),
                new UnitedKingdom(UnitedKingdom.Market.Settlement),
                BusinessDayConvention.Preceding,
                BusinessDayConvention.Preceding,
                DateGeneration.Rule.Backward,
                false);

            var bond2 = new FixedRateBond(2,
                100.0,
                schedule2,
                new DoubleVector(1) { 0.0475 },
                new ActualActual(ActualActual.Convention.Bond),
                BusinessDayConvention.Preceding,
                100.0,
                new Date(3, Month.October, 2007));

            var leg = bond2.cashflows();
            foreach (var cashflow in leg)
            {
                Console.WriteLine("{0}/{1}/{2} = {3}", cashflow.date().dayOfMonth(), cashflow.date().month(), cashflow.date().year(), cashflow.amount());
            }

            bonds.Add("GB00B24FF097", bond2);
        }

        public double CalculateYield(string name, double price, Date settlementDate)
        {
            if (!bonds.ContainsKey(name))
                return 0.0;

            Console.WriteLine("Accrued = {0}", bonds[name].accruedAmount(settlementDate));

            return bonds[name].yield(price, new ActualActual(ActualActual.Convention.Bond), Compounding.Compounded, Frequency.Semiannual, settlementDate);
        }
    }
}
