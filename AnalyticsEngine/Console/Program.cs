using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnalyticsEngine;
using QuantLib;
using System.Xml;
using System.Xml.Linq;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var tenor = new Period(Frequency.Semiannual);
            var calendar = new UnitedKingdom(UnitedKingdom.Market.Settlement);
            var bonds = new List<Tuple<string, string, FixedRateBond, double>>();

            XElement xml = XElement.Load("Bonds.xml");
            IEnumerable<XElement> bondElements =    from element in xml.Descendants("Bond")
                                                    select element;

            foreach (var element in bondElements)
            {
                string issue = element.Element("IssueDate").Value;
                string maturity = element.Element("MaturityDate").Value;
                var schedule = new Schedule(new Date(issue, "dd/mm/yyyy"),
                    new Date(maturity, "dd/mm/yyyy"),
                    tenor,
                    calendar,
                    BusinessDayConvention.Unadjusted,
                    BusinessDayConvention.Unadjusted,
                    DateGeneration.Rule.Backward,
                    false);

                double coupon = double.Parse(element.Element("Coupon").Value);
                var bond = new FixedRateBond(1,
                    100.0,
                    schedule,
                    new DoubleVector(1) { coupon },
                    new ActualActual(ActualActual.Convention.Bond),
                    BusinessDayConvention.Unadjusted,
                    100.0,
                    new Date(issue, "dd/mm/yyyy"));

                string isin = element.Attribute("ISIN").Value;
                string description = element.Element("Description").Value;
                double price = double.Parse(element.Element("CleanPrice").Value);
                bonds.Add(new Tuple<string, string, FixedRateBond, double>(isin, description, bond, price));
            }

            var reference = new Date(19, Month.September, 2014);

            foreach (var tuple in bonds)
            {
                var isin = tuple.Item1;
                var description = tuple.Item2;
                var bond = tuple.Item3;
                var price = tuple.Item4;

                Console.WriteLine("ISIN:        {0}", isin);
                Console.WriteLine("Description: {0}", description);
                Console.WriteLine("Start:       {0} {1} {2} {3} ", bond.startDate().weekday(), bond.startDate().dayOfMonth(), bond.startDate().month(), bond.startDate().year());
                Console.WriteLine("Maturity:    {0} {1} {2} {3} ", bond.maturityDate().weekday(), bond.maturityDate().dayOfMonth(), bond.maturityDate().month(), bond.maturityDate().year());

                var settlement = bond.settlementDate(reference);
                Console.WriteLine("Reference:   {0} {1} {2} {3} ", reference.weekday(), reference.dayOfMonth(), reference.month(), reference.year());
                Console.WriteLine("Settlement:  {0} {1} {2} {3} ", settlement.weekday(), settlement.dayOfMonth(), settlement.month(), settlement.year());

                Console.WriteLine("AI:          {0}", bond.accruedAmount(settlement));
                Console.WriteLine("Yield:       {0}", bond.yield(price, bond.dayCounter(), Compounding.Compounded, Frequency.Semiannual, settlement) * 100.0);

                Console.WriteLine();
            }

            Console.Read();
        }
    }
}
