using QuantLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AnalyticsEngine
{
    public class BondAnalytics : IBondAnalytics
    {
        private Dictionary<string, FixedRateBond> bonds = new Dictionary<string, FixedRateBond>();
        private PiecewiseFlatForward yieldCurve;
        private DiscountingBondEngine pricingEngine;

        public BondAnalytics(string refDate = null)
        {
            if (refDate != null)
                Settings.instance().setEvaluationDate(new Date(refDate, "dd/mm/yyyy"));
            else
                Settings.instance().setEvaluationDate(Date.todaysDate());

            BuildYieldCurve();

            pricingEngine = new DiscountingBondEngine(new YieldTermStructureHandle(yieldCurve));
        }

        public void Add(string isin, string issueDate, string maturityDate, double coupon)
        {
            if (bonds.ContainsKey(isin))
                return;

            var schedule = new Schedule(new Date(issueDate, "dd/mm/yyyy"),
                                        new Date(maturityDate, "dd/mm/yyyy"),
                                        new Period(Frequency.Semiannual),
                                        new UnitedKingdom(UnitedKingdom.Market.Settlement),
                                        BusinessDayConvention.Unadjusted,
                                        BusinessDayConvention.Unadjusted,
                                        DateGeneration.Rule.Backward,
                                        false);

            var bond = new FixedRateBond(1,
                                            100.0,
                                            schedule,
                                            new DoubleVector(1) { coupon },
                                            new ActualActual(ActualActual.Convention.Bond),
                                            BusinessDayConvention.Unadjusted,
                                            100.0,
                                            new Date(issueDate, "dd/mm/yyyy"));

            bond.setPricingEngine(pricingEngine);

            bonds.Add(isin, bond);
        }

        public double Yield(string isin, double cleanPrice, string refDate = null)
        {
            if (!bonds.ContainsKey(isin))
                return 0.0;

            Date settlementDate;
            if (refDate != null)
                settlementDate = bonds[isin].settlementDate(new Date(refDate, "dd/mm/yyyy"));
            else
                settlementDate = bonds[isin].settlementDate();

            return bonds[isin].yield(cleanPrice, bonds[isin].dayCounter(), Compounding.Compounded, Frequency.Semiannual, settlementDate);
        }

        public double NetPresentValue(string isin)
        {
            if (!bonds.ContainsKey(isin))
                return 0.0;

            return bonds[isin].NPV();
        }

        public double ModifiedDuration(string isin, double yield, string refDate = null)
        {
            if (!bonds.ContainsKey(isin))
                return 0.0;

            Date settlementDate;
            if (refDate != null)
                settlementDate = bonds[isin].settlementDate(new Date(refDate, "dd/mm/yyyy"));
            else
                settlementDate = bonds[isin].settlementDate();

            return CashFlows.duration(bonds[isin].cashflows(), 
                                        yield, 
                                        bonds[isin].dayCounter(), 
                                        Compounding.Compounded, 
                                        Frequency.Semiannual, 
                                        Duration.Type.Modified, 
                                        true, 
                                        settlementDate);
        }

        private void BuildYieldCurve()
        {
            //Extract the rates from the xml
            var xDoc = XElement.Load("Curve.xml");

            var helpers = new RateHelperVector();

            AddDeposits(xDoc, helpers);
            AddBonds(xDoc, helpers);
            AddSwaps(xDoc, helpers);

            yieldCurve = new PiecewiseFlatForward(Settings.instance().getEvaluationDate(), helpers, new ActualActual(ActualActual.Convention.Bond));
            yieldCurve.enableExtrapolation();
        }

        private void AddDeposits(XElement xCurve, RateHelperVector helpers)
        {
            var xDeposits = from xDeposit in xCurve.Descendants("Deposit")
                            select xDeposit;

            foreach (var xDeposit in xDeposits)
            {
                helpers.Add(new DepositRateHelper(new QuoteHandle(new SimpleQuote(double.Parse(xDeposit.Value))),
                                                    new Period(xDeposit.Attribute("Tenor").Value),
                                                    3,
                                                    new UnitedKingdom(UnitedKingdom.Market.Settlement),
                                                    BusinessDayConvention.ModifiedFollowing,
                                                    true,
                                                    new ActualActual(ActualActual.Convention.Bond)));
            }
        }

        private void AddBonds(XElement xCurve, RateHelperVector helpers)
        {
            var xBonds = from xBond in xCurve.Descendants("Bond")
                         select xBond;

            foreach (var xBond in xBonds)
            {
                var schedule = new Schedule(new Date(xBond.Attribute("IssueDate").Value, "dd/mm/yyyy"),
                                            new Date(xBond.Attribute("MaturityDate").Value, "dd/mm/yyyy"),
                                            new Period(Frequency.Semiannual),
                                            new UnitedKingdom(UnitedKingdom.Market.Settlement),
                                            BusinessDayConvention.Unadjusted,
                                            BusinessDayConvention.Unadjusted,
                                            DateGeneration.Rule.Backward,
                                            false);

                helpers.Add(new FixedRateBondHelper(new QuoteHandle(new SimpleQuote(double.Parse(xBond.Value))),
                                                    1,
                                                    100.0,
                                                    schedule,
                                                    new DoubleVector(1) { double.Parse(xBond.Attribute("Coupon").Value) },
                                                    new ActualActual(ActualActual.Convention.Bond),
                                                    BusinessDayConvention.Unadjusted,
                                                    100.0,
                                                    new Date(xBond.Attribute("IssueDate").Value, "dd/mm/yyyy")));
            }
        }

        private void AddSwaps(XElement xCurve, RateHelperVector helpers)
        {
            var xSwaps = from xSwap in xCurve.Descendants("Swap")
                         select xSwap;

            foreach (var xSwap in xSwaps)
            {
                helpers.Add(new SwapRateHelper(new QuoteHandle(new SimpleQuote(double.Parse(xSwap.Value))),
                                                new Period(xSwap.Attribute("Tenor").Value),
                                                new UnitedKingdom(UnitedKingdom.Market.Settlement),
                                                Frequency.Annual,
                                                BusinessDayConvention.Unadjusted,
                                                new Thirty360(Thirty360.Convention.European),
                                                new Euribor6M(),
                                                new QuoteHandle(new SimpleQuote(0.0)),
                                                new Period(1, TimeUnit.Days)));
            }
        }
    }
}
